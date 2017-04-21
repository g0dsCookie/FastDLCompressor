using Newtonsoft.Json;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace CookieProjects.FastDLCompressor
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
		[DefaultValue(5*1024)]
		public int MinimumSize
		{
			get;
			set;
		}
	}

	public class JsonConfiguration
	{
		static JsonConfiguration _globalConf;
		static System.Threading.Mutex _globalConfMux = new System.Threading.Mutex();

		public static JsonConfiguration Configuration
		{
			get
			{
				return _globalConf;
			}
			set
			{
				_globalConfMux.WaitOne();
				_globalConf = value;

				if (_globalConf.MaxThreads < 0)
					_globalConf.MaxThreads = 0;

				if (_globalConf.CompressionOptions == null)
					_globalConf.CompressionOptions = new CompressionOptions();

				if (_globalConf.CompressionOptions.MinimumSize < 0)
					_globalConf.CompressionOptions.MinimumSize = 0;

				_globalConfMux.ReleaseMutex();
			}
		}

		[JsonProperty("sources")]
		public SourceDirectory[] SourceDirectories
		{
			get;
			set;
		}

		[JsonProperty("target")]
		public string TargetDirectory
		{
			get;
			set;
		}

		[JsonProperty("cleanupTarget")]
		[DefaultValue(true)]
		public bool CleanupTargetDirectory
		{
			get;
			set;
		}

		[JsonProperty("threads")]
		[DefaultValue(0)]
		public int MaxThreads
		{
			get;
			set;
		}

		[JsonProperty("verbose")]
		[DefaultValue(false)]
		public bool Verbose
		{
			get;
			set;
		}

		[JsonProperty("compression")]
		public CompressionOptions CompressionOptions
		{
			get;
			set;
		}

		public static JsonConfiguration Load(string file)
		{
			using (var reader = new StreamReader(file, Encoding.UTF8))
			{
				using (var jsonReader = new JsonTextReader(reader))
				{
					var json = new JsonSerializer();

					json.DefaultValueHandling = DefaultValueHandling.Populate;

					return json.Deserialize<JsonConfiguration>(jsonReader);
				}
			}
		}

		public void Save(string file)
		{
			using (var writer = new StreamWriter(file, false, Encoding.UTF8))
			{
				using (var jsonWriter = new JsonTextWriter(writer))
				{
					var json = new JsonSerializer();

					jsonWriter.Formatting = Formatting.Indented;
					jsonWriter.Indentation = 4;
					jsonWriter.IndentChar = '\t';

					json.Serialize(jsonWriter, this);
				}
			}
		}
	}
}
