using Newtonsoft.Json;
using System.ComponentModel;

namespace CookieProjects.FastDLCompressor.Configuration
{
	public class FtpConfiguration
	{
		[JsonProperty("username")]
		public string Username
		{
			get;
			set;
		}

		[JsonProperty("password")]
		public string Password
		{
			get;
			set;
		}

		[JsonProperty("hostname")]
		public string Hostname
		{
			get;
			set;
		}

		[JsonProperty("destination")]
		[DefaultValue("fastdl")]
		public string Destination
		{
			get;
			set;
		}

		[JsonProperty("threads")]
		[DefaultValue(2)]
		public int UploadThreads
		{
			get;
			set;
		}
	}
}
