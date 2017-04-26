using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookieProjects.FastDLCompressor.Extensions
{
	public static class UriExtensions
	{
		public static Uri Concat(this Uri baseUri, string rel)
		{
			if (string.IsNullOrWhiteSpace(rel))
				return baseUri;
			return new Uri(baseUri, rel);
		}
	}
}
