using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;
using Topshelf.Logging;
using WS_Logging;

namespace taks_windows_services
{
	[Logger]
	public sealed class ImageService : ServiceControl
	{
		private readonly LogWriter _logger;

		private FileUtility FileSystem { get; }

		private readonly FileSystemWatcher _watcher;
		private readonly Task _worker;
		private readonly CancellationTokenSource _tokenSource;
		private readonly AutoResetEvent _newFileEvent;

		public ImageService(string inDirectory, string outDirectory, string tempDirectory)
		{
			_logger = HostLogger.Get<ImageService>();

			FileSystem = new FileUtility(inDirectory, outDirectory, tempDirectory);

			_watcher = new FileSystemWatcher();
			_watcher.Created += (sender, args) => _newFileEvent.Set();
			_tokenSource = new CancellationTokenSource();
			_worker = new Task(() => ProcessImage(_tokenSource.Token));
			_newFileEvent = new AutoResetEvent(false);
		}

		public bool Start(HostControl hostControl)
		{
			_worker.Start();
			_watcher.EnableRaisingEvents = true;
			return true;
		}

		public bool Stop(HostControl hostControl)
		{
			_watcher.EnableRaisingEvents = false;
			_tokenSource.Cancel();
			_worker.Wait();
			return true;
		}

		private static int GetImageIndex(string fileName)
		{
			Match match = Regex.Match(fileName, @"[0-9]{3}");

			return match.Success ? int.Parse(match.Value) : -1;
		}

		public void ProcessImage(CancellationToken token)
		{
			int currentImageIndex = -1;
			int imageCount = 0;
			bool isNextPageWaiting = false;

			PdfUtility pdf = new PdfUtility();

			do
			{
				foreach (var file in FileSystem.InputDirectory.GetFiles().Skip(imageCount))
				{
					string fileName = file.Name;
					bool isValid = Regex.IsMatch(fileName, @"^img_[0-9]{3}.(jpg|png|jpeg)$");
					if (isValid)
					{
						int imageIndex = GetImageIndex(fileName);
						if (imageIndex != currentImageIndex + 1 && currentImageIndex != -1 && isNextPageWaiting)
						{
							Save(ref pdf, out isNextPageWaiting);
						}

						if (FileSystem.TryOpenFile(file, 3))
						{
							pdf.AddImage(file.FullName);
							imageCount++;
							currentImageIndex = imageIndex;
							isNextPageWaiting = true;
						}
					}
					else
					{
						string outFile = Path.Combine(FileSystem.TempDirectory.FullName, fileName);
						if (File.Exists(outFile))
						{
							FileSystem.DeleteFile(file);
						}
						else
						{
							file.MoveTo(outFile);
						}
					}
				}

				if (!_newFileEvent.WaitOne(5000) && isNextPageWaiting)
				{
					Save(ref pdf, out isNextPageWaiting);
				}

				if (token.IsCancellationRequested)
				{
					if (isNextPageWaiting)
					{
						pdf.Save(FileSystem.GetNextFilename());
					}

					foreach (var file in FileSystem.InputDirectory.GetFiles())
					{
						FileSystem.DeleteFile(file);
					}
				}
			} while (!token.IsCancellationRequested);
		}

		private void Save(ref PdfUtility pdf, out bool isNextPageWaiting)
		{
			pdf.Save(FileSystem.GetNextFilename());
			pdf = new PdfUtility();
			isNextPageWaiting = false;
		}
	}
}
