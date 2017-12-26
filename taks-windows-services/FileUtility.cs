using System.IO;
using System.Threading;

namespace taks_windows_services
{
	public class FileUtility
	{
		private readonly string _outputDirectory;

		public DirectoryInfo InputDirectory { get; }
		public DirectoryInfo TempDirectory { get; }

		public FileUtility(string inputDirectory, string outputDirectory, string tempDirectory)
		{
			_outputDirectory = outputDirectory;

			InputDirectory = !Directory.Exists(inputDirectory) ? Directory.CreateDirectory(inputDirectory) : new DirectoryInfo(inputDirectory);
			TempDirectory = !Directory.Exists(tempDirectory) ? Directory.CreateDirectory(tempDirectory) : new DirectoryInfo(tempDirectory);
			if (!Directory.Exists(outputDirectory))
			{
				Directory.CreateDirectory(outputDirectory);
			}
		}

		public void DeleteFile(FileInfo file)
		{
			if (TryOpenFile(file, 3))
			{
				file.Delete();
			}
		}

		public bool TryOpenFile(FileInfo fileInfo, int tryCount)
		{
			for (int i = 0; i < tryCount; i++)
			{
				try
				{
					var fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None);
					fileStream.Close();

					return true;
				}
				catch (IOException)
				{
					Thread.Sleep(5000);
				}
			}

			return false;
		}

		public string GetNextFilename()
		{
			int documentIndex = Directory.GetFiles(_outputDirectory).Length + 1;
			return Path.Combine(_outputDirectory, $"document_{documentIndex}.pdf");
		}
	}
}
