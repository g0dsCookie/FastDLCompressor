using CookieProjects.FastDLCompressor.Configuration;
using CookieProjects.FastDLCompressor.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CookieProjects.FastDLCompressor.Uploader
{
	public class FtpUploader
	{
		public const int MAX_CHUNK_SIZE = 64 * 1024 * 1024;

		public Uri Hostname
		{
			get;
		}

		public string Username
		{
			get;
		}

		public string Password
		{
			get;
		}

		public bool ReuseConnection
		{
			get;
		}

		public int Threads
		{
			get;
		}

		public FtpUploader(FtpConfiguration conf)
			: this(conf.Hostname, conf.Username, conf.Password, true, conf.UploadThreads)
		{ }

		public FtpUploader(string hostname, string username, string password, bool reuseConnection, int threads)
		{
			Hostname = new Uri("ftp://" + hostname);
			Username = username;
			Password = password;
			ReuseConnection = reuseConnection;
			Threads = threads;
		}

		FtpWebRequest GetNewRequest(Uri uri, string method)
		{
			var request = (FtpWebRequest)WebRequest.Create(uri);
			request.Credentials = new NetworkCredential(Username, Password);
			request.KeepAlive = ReuseConnection;
			request.Method = method;
			return request;
		}

		public bool CreateDirectory(string dir, bool recursive = false)
		{
			if (recursive)
			{
				var lastSlash = dir.LastIndexOf('/');

				if (lastSlash != -1)
				{
					CreateDirectory(dir.Substring(0, lastSlash), recursive);
				}
			}

			var request = GetNewRequest(Hostname.Concat(dir), WebRequestMethods.Ftp.MakeDirectory);
			try
			{
				request.GetResponse();
			}
			catch (WebException ex)
			{
				Debug.WriteLine(ex.Message);
				return false;
			}

			return true;
		}

		public bool UploadFile(string file, string destination)
		{
			var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
			var buf = new byte[MAX_CHUNK_SIZE];

			var request = GetNewRequest(Hostname.Concat(destination), WebRequestMethods.Ftp.UploadFile);
			var requestStream = request.GetRequestStream();

			while (fs.Position < fs.Length)
			{
				var length = fs.Read(buf, 0, buf.Length);
				requestStream.Write(buf, 0, length);
			}
			fs.Close();
			requestStream.Close();

			request.GetResponse();
			return true;
		}

		public bool UploadDirectory(string directory, string destination)
		{
			if (directory[directory.Length - 1] != Path.DirectorySeparatorChar)
				directory += Path.DirectorySeparatorChar;

			var fileList = FileList.Build(new Configuration.SourceDirectory() { Directory = directory });
			var logger = Logger.Global;

			Parallel.ForEach(fileList.Files,
				new ParallelOptions() { MaxDegreeOfParallelism = Threads },
				f =>
				{
					var relPath = directory.RelativePath(f.Path).Replace('\\', '/');
					var relPathDir = directory.RelativePath(Path.GetDirectoryName(f.Path)).Replace('\\', '/');

					CreateDirectory(FtpDirectoryEntry.Combine(destination, relPathDir), true);

					logger.Write($"Uploading file {relPath}.");
					UploadFile(f.Path, FtpDirectoryEntry.Combine(destination, relPath));
				});

			return false;
		}

		public List<FtpDirectoryEntry> ListDirectory(string dir, bool recursive = false)
		{
			var retVal = new List<FtpDirectoryEntry>();

			var request = GetNewRequest(Hostname.Concat(dir), WebRequestMethods.Ftp.ListDirectoryDetails);

			using (var response = request.GetResponse())
			{
				using (var stream = new StreamReader(response.GetResponseStream()))
				{
					while (!stream.EndOfStream)
					{
						var line = stream.ReadLine();
						var entry = FtpDirectoryEntry.Parse(dir, line);
						retVal.Add(entry);
					}
				}
			}

			if (recursive)
			{
				var add = new List<FtpDirectoryEntry>();
				foreach (var d in retVal.Where(e => e.IsDirectory))
				{
					var list = ListDirectory(d.Name, recursive);
					add.AddRange(list);
				}
				retVal.AddRange(add);
			}

			return retVal;
		}

		public bool DeleteFile(string file)
		{
			var request = GetNewRequest(Hostname.Concat(file), WebRequestMethods.Ftp.DeleteFile);
			request.GetResponse();
			return true;
		}

		public List<FtpDirectoryEntry> DeleteDirectory(string dir)
		{
			var entries = ListDirectory(dir);

			var addEntries = new List<FtpDirectoryEntry>();
			Parallel.ForEach(entries, new ParallelOptions() { MaxDegreeOfParallelism = Threads },
				e =>
				{
					if (e.IsDirectory)
					{
						var d = DeleteDirectory(e.Name);
						lock (addEntries)
						{
							addEntries.AddRange(d);
						}
					}
					else
					{
						DeleteFile(e.Name);
					}
				});

			Parallel.Invoke(() =>
			{
				entries.AddRange(addEntries);
			}, () =>
			{
				var request = GetNewRequest(Hostname.Concat(dir), WebRequestMethods.Ftp.RemoveDirectory);
				request.GetResponse();
			});

			return entries;
		}

		public void Close()
		{
			var request = GetNewRequest(Hostname, WebRequestMethods.Ftp.PrintWorkingDirectory);
			request.KeepAlive = false;
			request.GetResponse();
		}
	}
}
