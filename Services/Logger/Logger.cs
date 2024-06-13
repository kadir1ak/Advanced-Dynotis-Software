using System;
using System.IO;

namespace Advanced_Dynotis_Software.Services.Logger
{
    public static class Logger
    {
        private static readonly string LogFilePath = "log.txt";

        public static void Log(string message)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(LogFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now:G}: {message}");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions related to logging
                Console.WriteLine($"Failed to log message: {ex.Message}");
            }
        }
    }
}
