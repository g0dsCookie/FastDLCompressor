using Newtonsoft.Json;
using System.ComponentModel;

namespace CookieProjects.FastDLCompressor.Configuration
{
	public class LoggerConfiguration
	{
		[JsonProperty("file")]
		[DefaultValue("fastdlcompressor.log")]
		public string Logfile
		{
			get;
			set;
		}

		[JsonProperty("append")]
		[DefaultValue(true)]
		public bool Append
		{
			get;
			set;
		}

		[JsonProperty("severity")]
		[DefaultValue(Severity.INFO)]
		public Severity Severity
		{
			get;
			set;
		}
	}
}
