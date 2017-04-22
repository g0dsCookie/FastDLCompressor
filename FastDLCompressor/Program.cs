using CookieProjects.FastDLCompressor.Configuration;
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

			if (!string.IsNullOrWhiteSpace(configFile))
				Compress(JsonConfiguration.Load(configFile));
			else
			{
				if (string.IsNullOrWhiteSpace(targetDir))
				{
					Console.Error.WriteLine("Target directory not specified.");
					Environment.Exit(1);
				}
				if (sourceDirs.Count == 0)
				{
					Console.Error.WriteLine("No source directory specified.");
					Environment.Exit(1);
				}

				var conf = new JsonConfiguration()
				{
					CleanupTargetDirectory = cleanupTarget,
					TargetDirectory = targetDir,
					MaxThreads = threads
				};

				var sDirs = new List<SourceDirectory>();
				sourceDirs.ForEach(s =>
				{
					sDirs.Add(new SourceDirectory()
					{
						Directory = s,
						Filters = new string[0],
						Includes = new string[0]
					});
				});
				conf.SourceDirectories = sDirs.ToArray();

				Compress(conf);
			}
		}
	}
}
