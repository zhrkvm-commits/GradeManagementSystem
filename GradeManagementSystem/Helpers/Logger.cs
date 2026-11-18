using System;
using System.Configuration;
using System.IO;
using System.Text;

namespace GradeManagementSystem.Helpers
{
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }

    public static class Logger
    {
        private static readonly object lockObject = new object();
        private static string logDirectory;
        private static LogLevel minimumLevel = LogLevel.Info;

        public static void Initialize()
        {
            logDirectory = ConfigurationManager.AppSettings["LogDirectory"];

            if (string.IsNullOrEmpty(logDirectory))
                logDirectory = "Logs";

            if (!Path.IsPathRooted(logDirectory))
                logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logDirectory);

            string levelSetting = ConfigurationManager.AppSettings["LogLevel"];
            if (!string.IsNullOrEmpty(levelSetting))
            {
                LogLevel parsed;
                if (Enum.TryParse(levelSetting, true, out parsed))
                    minimumLevel = parsed;
            }

            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);

            Info("Приложение запущено");
        }

        public static void Debug(string message)
        {
            Write(LogLevel.Debug, message);
        }

        public static void Info(string message)
        {
            Write(LogLevel.Info, message);
        }

        public static void Warning(string message)
        {
            Write(LogLevel.Warning, message);
        }

        public static void Error(string message, Exception ex = null)
        {
            string text = ex == null
                ? message
                : $"{message} | {ex.GetType().Name}: {ex.Message}";
            Write(LogLevel.Error, text);
        }

        private static void Write(LogLevel level, string message)
        {
            if (level < minimumLevel)
                return;

            try
            {
                if (string.IsNullOrEmpty(logDirectory))
                    logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);

                string fileName = $"log_{DateTime.Now:yyyy-MM-dd}.txt";
                string fullPath = Path.Combine(logDirectory, fileName);
                string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level.ToString().ToUpper()}] {message}";

                lock (lockObject)
                {
                    File.AppendAllText(fullPath, line + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch
            {
            }
        }
    }
}
