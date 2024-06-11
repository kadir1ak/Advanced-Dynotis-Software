using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advanced_Dynotis_Software.Services.Logger
{
    public static class Logger
    {
        private static string logFilePath = "ApplicationLog.txt";

        public static void Log(string message)
        {
            try
            {
                using (var writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now}: {message}");
                }
            }
            catch (Exception ex)
            {
                // Log dosyasına yazma işlemi başarısız olursa burada ele alınabilir.
                Console.WriteLine($"Unable to log message to file. Error: {ex.Message}");
            }
        }
    }
}
