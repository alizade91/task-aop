using System;
using System.IO;
using Topshelf;

namespace taks_windows_services
{
	class Program
	{
		static void Main(string[] args)
		{
			string currentDir = AppDomain.CurrentDomain.BaseDirectory;
			string inputDir = Path.Combine(currentDir, "input");
			string outDir = Path.Combine(currentDir, "output");
			string tempDir = Path.Combine(currentDir, "temp");

			HostFactory.Run(x =>
			{
				x.Service(() => new ImageService(inputDir, outDir, tempDir));

				x.SetServiceName("Image Service");
				x.StartAutomaticallyDelayed();
				x.RunAsLocalSystem();
			});
		}
	}
}
