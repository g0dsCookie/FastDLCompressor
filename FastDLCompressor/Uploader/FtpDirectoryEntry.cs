using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookieProjects.FastDLCompressor.Uploader
{
	public struct FtpDirectoryEntry
	{
		public string Name
		{
			get;
		}

		public bool IsDirectory
		{
			get;
		}

		public string Owner
		{
			get;
		}

		public string Group
		{
			get;
		}

		public FtpDirectoryEntry(string name, string owner, string group, bool isDir)
		{
			Name = name;
			Owner = owner;
			Group = group;
			IsDirectory = isDir;
		}

		public static FtpDirectoryEntry Parse(string dir, string entry)
		{
			var tokens = entry.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			var isDir = tokens[0][0];
			var owner = tokens[2];
			var group = tokens[3];
			var name = Combine(dir, tokens[8]);

			return new FtpDirectoryEntry(name, owner, group, isDir == 'd');
		}

		public static string Combine(params string[] paths)
		{
			var sb = new StringBuilder();

			for (int i = 0; i < paths.Length; i++)
			{
				if (i > 0 && !string.IsNullOrWhiteSpace(paths[i-1]) && !paths[i - 1].EndsWith("/"))
					sb.Append("/");
				sb.Append(paths[i]);
			}

			return sb.ToString();
		}
	}
}
