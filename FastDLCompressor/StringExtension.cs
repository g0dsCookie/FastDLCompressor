using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookieProjects.FastDLCompressor
{
	public static class StringExtension
	{
		public static string RelativePath(this string baseDir, string subDir)
		{
			return new Uri(baseDir).MakeRelativeUri(new Uri(subDir)).ToString();
		}
	}
}
