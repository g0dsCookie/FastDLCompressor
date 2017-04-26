using CookieProjects.FastDLCompressor.Configuration;
using CookieProjects.FastDLCompressor.Uploader;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CookieProjects.FastDLCompressor
{
	class Program
	{
		static void Help(OptionSet p)
		{
			Console.WriteLine("Usage: FastDLCompressor.exe [OPTIONS]");
			Console.WriteLine(" Compress resource files and move them into your FastDL directory.\n");
			Console.WriteLine("Options:");
			p.WriteOptionDescriptions(Console.Out);
		}

		static void Compress(JsonConfiguration config)
		{
			JsonConfiguration.Configuration = config;

			if (config.CleanupTargetDirectory)
			{
				if (Directory.Exists(config.TargetDirectory))
				{
					Console.WriteLine("Removing old directory...");
					Directory.Delete(config.TargetDirectory, true);
				}
			}

			var sw = new Stopwatch();

			foreach (var s in config.SourceDirectories)
			{
				sw.Reset();
				Console.WriteLine($"Building file list for {s.Directory}...");
				sw.Start();
				var fl = FileList.Build(s);
				sw.Stop();
				Console.WriteLine($"File list created in {sw.ElapsedMilliseconds} ms.");

				sw.Reset();
				Console.WriteLine("Compressing files...");
				sw.Start();
				fl.Compress(config.TargetDirectory);
				sw.Stop();
				Console.WriteLine($"Compressing done in {sw.ElapsedMilliseconds} ms.");
			}
		}

		static void Main(string[] args)
		{
			string configFile = string.Empty, targetDir = string.Empty;
			var sourceDirs = new List<string>();
			bool showHelp = false, cleanupTarget = true;
			var threads = 0;

			var p = new OptionSet()
			{
				{ "c|config=", "the configuration file to use.", c => configFile = c },
				{ "t|target=", "the destination directory (target), where to store the compressed files.", t => targetDir = t },
				{ "s|source=", "the source directory to compress. You can specify multiple source directories.", s => sourceDirs.Add(s) },
				{ "r|cleanupTarget", "clean the destination directory before compressing.", c => cleanupTarget = c == null },
				{ "threads=", "the maximum number of threads to use.", t => threads = int.Parse(t) },
				{ "h|help", "show this help.", b => showHelp = b != null }
			};

			try
			{
				p.Parse(args);
			}
			catch (OptionException ex)
			{
				Console.Error.WriteLine("Could not parse command line: {0}", ex.Message);
				Environment.Exit(1);
			}

			if (showHelp)
			{
				Help(p);
				return;
			}

			JsonConfiguration conf;

			if (!string.IsNullOrWhiteSpace(configFile))
				conf = JsonConfiguration.Load(configFile);
			else
				conf = JsonConfiguration.Generate(targetDir, sourceDirs, cleanupTarget, threads);

			Compress(conf);

			if (conf.FtpConfiguration != null)
			{
				var ftp = new FtpUploader(conf.FtpConfiguration);
				ftp.DeleteDirectory(conf.FtpConfiguration.Destination);
				ftp.UploadDirectory(conf.TargetDirectory, conf.FtpConfiguration.Destination);
			}
		}
	}
}
