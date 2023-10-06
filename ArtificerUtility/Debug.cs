namespace Artificer.Utility;

public static class Debug
{
	public static bool PrintConsole = true;
		
	public enum MessageStatus {
		INFO,
		WARN,
		FAIL,
		DEBUG
	}

	public enum Sender {
		Kernel,
		Settings,
		Telegram,
		Organizations,
		Users,
		RequestManager,
		Database,
		Payments,
		Messages,
		Statistic
	}

	public static void Message(string message) 
	{ 
		Console.WriteLine(message);
	}

	private static string GetCurrentTime => $"[{DateTime.Now:HH:mm:ss}]";
	private static string GetCurrentDateLogPath => Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs"), $"{DateTime.Now.Date:dd-MM-yy}.log");
		
	private static string GetCurrentDateErrorPath => Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs"), $"{DateTime.Now.Date:dd-MM-yy}_error.log");

		public static void Log(string message)
		{
			if (PrintConsole)
			{
				Console.Write($"{GetCurrentTime} ");
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine(message);
				Console.ResetColor();
			}
			else WriteInLogFile($"{GetCurrentTime} {message}");
		}

		public static void Log(string message, Sender sender)
		{
			if (PrintConsole)
			{
				Console.Write($"{GetCurrentTime} [{sender}] ");
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine(message);
				Console.ResetColor();
			}
			else WriteInLogFile($"{GetCurrentTime} [{sender}] {message}");
		}

		public static void Log(string message, MessageStatus status)
		{
			if (PrintConsole)
			{
				Console.Write($"{GetCurrentTime} ");
				Console.ForegroundColor = status switch
				{
					MessageStatus.INFO => ConsoleColor.Green,
					MessageStatus.WARN => ConsoleColor.Yellow,
					MessageStatus.FAIL => ConsoleColor.Red,
					_ => Console.ForegroundColor
				};
				Console.WriteLine(message);
				Console.ResetColor();
			}
			else WriteInLogFile($"{GetCurrentTime} [{status}] {message}");
		}

		public static void Log(string message, Sender sender, MessageStatus status)
		{
			if (PrintConsole)
			{
				Console.Write($"{GetCurrentTime} [{sender}] ");
				Console.ForegroundColor = status switch
				{
					MessageStatus.DEBUG => ConsoleColor.Green,
					MessageStatus.WARN => ConsoleColor.Yellow,
					MessageStatus.FAIL => ConsoleColor.Red,
					_ => Console.ForegroundColor
				};
				Console.WriteLine(message);
				Console.ResetColor();
			}
			else
			{
				if (message.StartsWith("Telegram.Bot.Exceptions.RequestException: Exception during making request")) return;
				if (status == MessageStatus.FAIL)
					WriteInErrorFile($"{GetCurrentTime} [{sender}] [{status}] {message}");
				else WriteInLogFile($"{GetCurrentTime} [{status}] {message}");

			}
		}
		
		public static void Log(string message, long chatId)
		{
			if (PrintConsole)
			{
				Console.Write($"{GetCurrentTime} ");
				Console.WriteLine(message);
			}
			else
			{
				if (message.StartsWith("Telegram.Bot.Exceptions.RequestException: Exception during making request")) return;
				WriteInLogFileAsync($"[{DateTime.Now:d.MM.yyyy} {DateTime.Now:HH:mm:ss}] {message}", chatId);
			}
		}
	

		public static void NewLine()
		{
			Console.WriteLine();
		}

		private static void WriteInLogFile(string data)
		{
			try
			{
				using var file = new StreamWriter(GetCurrentDateLogPath, true);
				file.WriteLineAsync(data);
			}
			catch (Exception e)
			{
				Log($"[WriteInLogFile] {e.Message}", Sender.Kernel, MessageStatus.FAIL);
			}
		}
		
		private static void WriteInErrorFile(string data)
		{
			try
			{
				using var file = new StreamWriter(GetCurrentDateErrorPath, true);
				file.WriteLineAsync(data);
			}
			catch (Exception e)
			{
				Log($"[WriteInErrorFile] {e.Message}", Sender.Kernel, MessageStatus.FAIL);
			}
		}
		
		private static Task WriteInLogFileAsync(string data, long chatId)
		{
			try
			{
				var filePath = Path.Combine(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs"), "users"), $"{chatId}.log");
				using var file = new StreamWriter(filePath, true);
				return file.WriteLineAsync(data);
			}
			catch (Exception)
			{
				//Log($"[WriteInLogFileAsync] {e.Message} {e.StackTrace}", Sender.Kernel, MessageStatus.FAIL);
				return Task.CompletedTask;
			}
		}
	}