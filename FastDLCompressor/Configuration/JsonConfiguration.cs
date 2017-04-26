using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace CookieProjects.FastDLCompressor.Configuration
{
	public class JsonConfiguration
	{
		static JsonConfiguration _globalConf;

		public static JsonConfiguration Configuration
		{
			get
			{
				return _globalConf;
			}
			set
			{
				_globalConf = value;
				
				if (_globalConf.MaxThreads < 0)
					_globalConf.MaxThreads = 0;

				if (_globalConf.CompressionOptions == null)
					_globalConf.CompressionOptions = new CompressionOptions();

				if (_globalConf.CompressionOptions.MinimumSize < 0)
					_globalConf.CompressionOptions.MinimumSize = 0;
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
		[DefaultValue(null)]
		public CompressionOptions CompressionOptions
		{
			get;
			set;
		}

		[JsonProperty("ftp")]
		[DefaultValue(null)]
		public FtpConfiguration FtpConfiguration
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

		public static JsonConfiguration Generate(string targetDir, List<string> sourceDirs, bool cleanupTarget, int threads)
		{
			if (string.IsNullOrWhiteSpace(targetDir))
			{
				Console.Error.WriteLine("Target directory not specified.");
				Environment.Exit(1);
			}
			if (sourceDirs.Count == 0)
			{
				Console.Error.WriteLine("No source directory specified.");
				Environment.Exit(1);
			}

			var conf = new JsonConfiguration()
			{
				TargetDirectory = targetDir,
				CleanupTargetDirectory = cleanupTarget,
				MaxThreads = threads
			};

			var sDirs = new List<SourceDirectory>();
			sourceDirs.ForEach(s =>
			{
				sDirs.Add(new SourceDirectory()
				{
					Directory = s,
					Filters = new string[0],
					Includes = new string[0]
				});
			});
			conf.SourceDirectories = sDirs.ToArray();

			return conf;
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
