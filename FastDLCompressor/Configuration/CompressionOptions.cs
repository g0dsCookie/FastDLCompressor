using Newtonsoft.Json;
using System.ComponentModel;

namespace CookieProjects.FastDLCompressor.Configuration
{
	public class CompressionOptions
	{
		[JsonProperty("level")]
		[DefaultValue(9)]
		public int Level
		{
			get;
			set;
		}

		[JsonProperty("minimumSize")]
		[DefaultValue(5 * 1024)]
		public int MinimumSize
		{
			get;
			set;
		}
	}
}
