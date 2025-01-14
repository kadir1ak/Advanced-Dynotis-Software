using System;
using System.IO;

namespace Advanced_Dynotis_Software.Services.Logger
{
    public static class Logger
    {
        private static readonly string logFilePath = "application_log.txt";

        public static void LogError(string message, Exception ex = null)
        {
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}";
            if (ex != null) logMessage += $"\nException: {ex.Message}\nStackTrace: {ex.StackTrace}";
            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
        }

        public static void LogInfo(string message)
        {
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: {message}";
            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
        }
    }
}
