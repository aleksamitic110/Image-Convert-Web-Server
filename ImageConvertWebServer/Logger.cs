using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageConvertWebServer
{
    internal class Logger
    {
        private static readonly string logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        private static readonly string logFile = Path.Combine(logFolder, $"log_{DateTime.Now:yyyy-MM-dd}.txt");
        private static readonly object _lock = new object();

        static Logger()
        {
            if (!Directory.Exists(logFolder))
                Directory.CreateDirectory(logFolder);
        }

        public static void LogInfo(string message)
        {
            Log("INFO", message);
        }

        public static void LogError(string message)
        {
            Log("ERROR", message);
        }

        public static void LogRequest(string request)
        {
            Log("REQUEST", request);
        }

        private static void Log(string type, string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string fullMessage = $"[{timestamp}] [{type}] {message}";

            lock (_lock)
            {
                Console.WriteLine(fullMessage);
                File.AppendAllText(logFile, fullMessage + Environment.NewLine);
            }
        }
    }
}
