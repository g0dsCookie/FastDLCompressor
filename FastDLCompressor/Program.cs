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

		static void Compress()
		{
			var config = JsonConfiguration.Configuration;

			if (config.CleanupTargetDirectory && Directory.Exists(config.TargetDirectory))
				Directory.Delete(config.TargetDirectory, true);

			foreach (var s in config.SourceDirectories)
			{
				var fl = FileList.Build(s);
				fl.Compress(config.TargetDirectory);
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

			if (!string.IsNullOrWhiteSpace(configFile))
				JsonConfiguration.Configuration = JsonConfiguration.Load(configFile);
			else
				JsonConfiguration.Configuration = JsonConfiguration.Generate(targetDir, sourceDirs, cleanupTarget, threads);
			var conf = JsonConfiguration.Configuration;

			Logger.Global = new Logger(
				conf.LogConfiguration.Severity,
				conf.LogConfiguration.Logfile,
				conf.LogConfiguration.Append);

			Compress();

			if (conf.FtpConfiguration != null)
			{
				var ftp = new FtpUploader(conf.FtpConfiguration);
				ftp.DeleteDirectory(conf.FtpConfiguration.Destination);
				ftp.UploadDirectory(conf.TargetDirectory, conf.FtpConfiguration.Destination);
			}
		}
	}
}
