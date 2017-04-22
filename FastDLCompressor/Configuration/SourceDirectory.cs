using Newtonsoft.Json;

namespace CookieProjects.FastDLCompressor.Configuration
{
	public class SourceDirectory
	{
		[JsonProperty("dir")]
		public string Directory
		{
			get;
			set;
		}

		[JsonProperty("includes")]
		public string[] Includes
		{
			get;
			set;
		}

		[JsonProperty("filters")]
		public string[] Filters
		{
			get;
			set;
		}
	}
}
