using System;

namespace CookieProjects.FastDLCompressor.Extensions
{
	public static class StringExtension
	{
		public static string RelativePath(this string baseDir, string subDir)
		{
			return new Uri(baseDir).MakeRelativeUri(new Uri(subDir)).ToString();
		}
	}
}
