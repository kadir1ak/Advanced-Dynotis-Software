using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Advanced_Dynotis_Software.ViewModels.Main;

namespace Advanced_Dynotis_Software.Services.Helpers
{
    public class CsvHelper
    {
        // CSV dosyası oluşturma
        public static void CreateCsvTemplate(SaveVariables data)
        {
            StringBuilder csvContent = new StringBuilder();

            // Sabit verileri ekleyelim
            csvContent.AppendLine("Test Date");
            csvContent.AppendLine("Motor Name");
            csvContent.AppendLine("Propeller (Inch)");
            csvContent.AppendLine("Motor Inner (mR)");
            csvContent.AppendLine("Motor No Load Current (A)");
            csvContent.AppendLine();

            string separator = ";";

            // Başlık satırlarını separator değişkeni ile ekleyelim
            string headers = $"Current (A){separator}Voltage (V){separator}Motor Speed ({data.MotorSpeed.UnitName}){separator}Thrust ({data.Thrust.UnitName}){separator}Torque ({data.Torque.UnitName}){separator}Pressure (Pa){separator}Ambient Temp ({data.AmbientTemp.UnitName}){separator}Motor Temp ({data.MotorTemp.UnitName}){separator}Power (W)";
            csvContent.AppendLine(headers);


            File.WriteAllText(data.RecordFilePath, csvContent.ToString());
        }

        // CSV'ye veri ekleme
        public static void AppendCsvRow(string filePath, string[] dataRow)
        {
            string separator = ";";
            // Veri satırını verilen separator ile birleştirip dosyaya ekleme
            File.AppendAllText(filePath, string.Join(separator, dataRow) + Environment.NewLine);
        }

        // CSV dosyasına tüm verileri yazma
        public static void WriteCsv(string filePath, List<string[]> data)
        {
            StringBuilder csvContent = new StringBuilder();

            foreach (var row in data)
            {
                csvContent.AppendLine(string.Join(",", row));
            }

            File.WriteAllText(filePath, csvContent.ToString());
        }

        // CSV dosyasını okuma
        public static List<string[]> ReadCsv(string filePath)
        {
            var data = new List<string[]>();
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                data.Add(line.Split(','));
            }

            return data;
        }
    }
}
