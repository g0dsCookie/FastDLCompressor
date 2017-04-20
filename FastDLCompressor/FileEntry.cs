using System;

namespace CookieProjects.FastDLCompressor
{
	public class FileEntry
	{
		public string Path
		{
			get;
		}

		public string RelativePath
		{
			get
			{
				var path = new Uri(Path);
				var reference = new Uri(_parent.BaseDirectory);
				return reference.MakeRelativeUri(path).ToString();
			}
		}

		FileList _parent;

		public FileEntry(string path, FileList parent)
		{
			Path = path;
			_parent = parent;
		}
	}
}
