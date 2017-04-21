using System;

namespace CookieProjects.FastDLCompressor
{
	public struct FileEntry
	{
		public string Path
		{
			get;
		}

		public FileEntry(string path)
		{
			Path = path;
		}
	}
}
