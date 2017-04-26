using ICSharpCode.SharpZipLib.BZip2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using CookieProjects.FastDLCompressor.Configuration;
using CookieProjects.FastDLCompressor.Extensions;

namespace CookieProjects.FastDLCompressor
{
	public class FileList
	{
		public string BaseDirectory
		{
			get;
		}

		public List<FileEntry> Files
		{
			get;
			private set;
		} = new List<FileEntry>();

		private FileList(string baseDir)
		{
			BaseDirectory = baseDir;
		}

		public void Compress(string targetDirectory)
		{
			var config = JsonConfiguration.Configuration;
			Parallel.ForEach(Files, new ParallelOptions()
			{
				MaxDegreeOfParallelism = config.MaxThreads != 0 ? config.MaxThreads : Environment.ProcessorCount
			},
			f => {
				var relPath = BaseDirectory.RelativePath(f.Path);
				var targetFileName = Path.Combine(targetDirectory, relPath + ".bz2");
				Directory.CreateDirectory(Path.GetDirectoryName(targetFileName));

				var fi = new FileInfo(f.Path);
				if (fi.Length <= config.CompressionOptions.MinimumSize)
				{
					if (config.Verbose)
						Console.WriteLine($"Moving file {relPath} because the size is below the MinimumSize.");
					File.Move(f.Path, targetFileName);
					return;
				}

				using (var sourceFile = new FileStream(f.Path, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					using (var targetFile = new FileStream(targetFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
					{
						try
						{
							if (config.Verbose)
								Console.WriteLine($"Compressing file {relPath}");
							BZip2.Compress(sourceFile, targetFile, true, config.CompressionOptions.Level);
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Could not compress {relPath}: {ex.Message}");
						}
					}
				}
			});
		}

		static bool MatchFilters(string relFile, string[] filters)
		{
			if (filters == null)
				return false;

			foreach (var f in filters)
			{
				if (Regex.IsMatch(relFile, f))
					return true;
			}
			return false;
		}

		private static List<FileEntry> Walk(string directory, SourceDirectory sDir)
		{
			var entries = new List<FileEntry>();
			var config = JsonConfiguration.Configuration;
			
			Parallel.ForEach(Directory.EnumerateFileSystemEntries(directory), new ParallelOptions()
			{
				MaxDegreeOfParallelism = config.MaxThreads != 0 ? config.MaxThreads : Environment.ProcessorCount
			},
			f => {
				var relPath = sDir.Directory.RelativePath(f);
				if (!MatchFilters(relPath, sDir.Includes) && MatchFilters(relPath, sDir.Filters))
					return;

				var attr = File.GetAttributes(f);
				if (attr.HasFlag(FileAttributes.Directory))
				{
					var t = Walk(f, sDir);
					if (t.Count > 0)
					{
						lock (entries)
						{
							entries.AddRange(t);
						}
					}
					t = null;
				}
				else
				{
					lock (entries)
					{
						entries.Add(new FileEntry(f));
					}
				}
			});

			return entries;
		}

		public static FileList Build(SourceDirectory directory)
		{
			if (directory.Directory[directory.Directory.Length - 1] != Path.DirectorySeparatorChar)
				directory.Directory += Path.DirectorySeparatorChar;

			var fl = new FileList(directory.Directory);

			var files = Walk(directory.Directory, directory);
			if (files.Count > 0)
				fl.Files.AddRange(files);
			
			return fl;
		}
	}
}
