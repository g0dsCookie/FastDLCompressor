using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CookieProjects.FastDLCompressor
{
	public enum Severity
	{
		DEBUG = 0,
		INFO,
		WARNING,
		ERROR
	}

	public static class SeverityExtensions
	{
		public static string GetName(this Severity severity)
		{
			switch (severity)
			{
				case Severity.DEBUG:
					return "Debug";
				case Severity.INFO:
					return "Info";
				case Severity.WARNING:
					return "Warn";
				case Severity.ERROR:
					return "ERR";
			}
			return string.Empty;
		}

		public static ConsoleColor GetColor(this Severity severity)
		{
			switch (severity)
			{
				case Severity.DEBUG:
					return ConsoleColor.Gray;
				case Severity.WARNING:
					return ConsoleColor.Yellow;
				case Severity.ERROR:
					return ConsoleColor.Red;

				default:
					return ConsoleColor.White;
			}
		}
	}

	public class Logger
	{
		static Logger _global;

		public static Logger Global
		{
			get
			{
				if (_global == null)
					return new Logger(Severity.INFO);
				return _global;
			}
			set
			{
				_global = value;
			}
		}

		public string Logfile
		{
			get;
		}

		public bool Append
		{
			get;
		}

		public Severity Severity
		{
			get;
		}

		TextWriter _file;
		TextWriter _stdout;
		TextWriter _stderr;

		Mutex _mux;

		public Logger(Severity severity)
			: this(severity, null, false)
		{ }

		public Logger(Severity severity, string logfile, bool append)
		{
			Severity = severity;
			Logfile = logfile;
			Append = append;

			_file = new StreamWriter(logfile, append, Encoding.UTF8);
			_stdout = Console.Out;
			_stderr = Console.Error;

			_mux = new Mutex();
		}

		ConsoleColor SetColor(ConsoleColor color)
		{
			var c = Console.ForegroundColor;
			Console.ForegroundColor = color;
			return c;
		}

		public bool Write(string msg, Severity severity = Severity.INFO)
		{
			if (severity < Severity)
				return false;

			var sb = new StringBuilder();
			sb.AppendFormat("[{0}] [{1}] ", DateTime.Now.ToLongTimeString(), severity.GetName());
			sb.AppendLine(msg);

			var currentColor = SetColor(severity.GetColor());

			_mux.WaitOne();

			var tasks = new List<Task>();

			if (_file != null)
				tasks.Add(_file.WriteAsync(sb.ToString()));

			if (severity == Severity.ERROR)
				tasks.Add(_stderr.WriteAsync(sb.ToString()));
			else
				tasks.Add(_stdout.WriteAsync(sb.ToString()));

			foreach (var t in tasks)
				t.Wait();

			_mux.ReleaseMutex();

			SetColor(currentColor);

			return true;
		}

		public bool Write(Exception ex, string msg = "", Severity severity = Severity.ERROR)
		{
			if (severity < Severity)
				return false;

			var sb = new StringBuilder();
			sb.Append("An exception occured: ");
			sb.AppendLine(msg);
			sb.Append("Message: ");
			sb.AppendLine(ex.Message);
			sb.Append("Source: ");
			sb.AppendLine(ex.Source);
			sb.Append("Stack: ");
			sb.AppendLine(ex.StackTrace);

			return Write(sb.ToString(), severity);
		}

		public void Close()
		{
			if (_file != null)
				_file.Close();
		}
	}
}
