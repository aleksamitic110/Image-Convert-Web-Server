using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageConvertWebServer
{
	internal class Logger
	{
		private static readonly string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\.."));
		private static readonly string logFolder = Path.Combine(projectRoot, "Logs");
		private static readonly string logFile = Path.Combine(logFolder, $"log_{DateTime.Now:yyyy-MM-dd}.txt");

		// Umesto _lock, koristimo SemaphoreSlim(1, 1) koji se ponaša kao asinhroni lock
		private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

		static Logger()
		{
			if (!Directory.Exists(logFolder))
				Directory.CreateDirectory(logFolder);
		}

		// Sve metode su sada asihrone
		public static async Task LogInfoAsync(string message)
		{
			await LogAsync("INFO", message);
		}
		public static async Task LogErrorAsync(string message)
		{
			await LogAsync("ERROR", message);
		}

		public static async Task LogRequestAsync(string request)
		{
			await LogAsync("REQUEST", request);
		}

		private static async Task LogAsync(string type, string message)
		{
			string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			string fullMessage = $"[{timestamp}] [{type}] {message}" + Environment.NewLine;

			// Čekamo da "uđemo" u semafor (ekvivalent početka lock bloka)
			await _semaphore.WaitAsync();
			try
			{
				Console.Write(fullMessage);
				// Koristimo asinhronu metodu za pisanje u fajl
				//await File.AppendAllTextAsync(logFile, fullMessage); nije nam radilo
				using (var sw = new StreamWriter(logFile, append: true))
				{
					await sw.WriteAsync(fullMessage);
				}
			}
			finally
			{
				// Oslobađamo semafor (ekvivalent kraja lock bloka)
				_semaphore.Release();
			}
		}
	}
}
