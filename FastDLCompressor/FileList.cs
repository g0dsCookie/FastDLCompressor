using ICSharpCode.SharpZipLib.BZip2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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
			Parallel.ForEach(Files, f =>
			{
				var targetFileName = Path.Combine(targetDirectory, f.RelativePath + ".bz2");
				Directory.CreateDirectory(Path.GetDirectoryName(targetFileName));
				using (var sourceFile = new FileStream(f.Path, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					using (var targetFile = new FileStream(targetFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
					{
						try
						{
							BZip2.Compress(sourceFile, targetFile, true, 9);
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Could not compress {f.RelativePath}: {ex.Message}");
						}
					}
				}
			});
		}

		private static FileEntry[] Walk(string directory, FileList parent)
		{
			var entries = new List<FileEntry>();

			Parallel.ForEach(Directory.EnumerateDirectories(directory), d =>
			{
				var t = Walk(d, parent);
				lock (entries)
				{
					entries.AddRange(t);
				}
			});

			foreach (var f in Directory.EnumerateFiles(directory))
			{
				var entry = new FileEntry(f, parent);
				entries.Add(entry);
			}

			return entries.ToArray();
		}

		public static FileList Build(string baseDir)
		{
			if (baseDir[baseDir.Length-1] != Path.DirectorySeparatorChar)
				baseDir += Path.DirectorySeparatorChar;

			var fl = new FileList(baseDir);
			var files = Walk(baseDir, fl);
			fl.Files.AddRange(files);
			return fl;
		}
	}
}
