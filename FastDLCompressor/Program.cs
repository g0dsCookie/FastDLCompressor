using System;
using System.Diagnostics;
using System.IO;

namespace CookieProjects.FastDLCompressor
{
	class Program
	{
		static void Help(int exitCode = 0)
		{
			Console.WriteLine("fastdlcompressor <source> <target>");
			Console.WriteLine(" <source>\tDirectory to compress.");
			Console.WriteLine(" <target>\tDirectory where to store the compressed files.");
			Environment.Exit(exitCode);
		}

		static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("Not enough arguments!\n");
				Help(1);
			}

			if (Directory.Exists(args[1]))
			{
				Console.WriteLine("Removing old directory...");
				Directory.Delete(args[1], true);
			}

			var sw = new Stopwatch();

			Console.WriteLine("Building file list...");
			sw.Start();
			var fl = FileList.Build(args[0]);
			sw.Stop();
			Console.WriteLine($"File list created in {sw.ElapsedMilliseconds} ms.");

			sw.Reset();

			Console.WriteLine("Compressing files...");
			sw.Start();
			fl.Compress(args[1]);
			sw.Stop();

			Console.WriteLine($"Compressing done in {sw.ElapsedMilliseconds} ms.");
		}
	}
}
