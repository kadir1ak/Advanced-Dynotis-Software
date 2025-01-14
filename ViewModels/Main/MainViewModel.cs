using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using Advanced_Dynotis_Software.Models.Devices;
using Advanced_Dynotis_Software.Models.Interface;
using Advanced_Dynotis_Software.Models.Serial;
using Advanced_Dynotis_Software.Services.BindableBase;

namespace Advanced_Dynotis_Software.ViewModels.Main
{
    public class MainViewModel : BindableBase
    {
        public MainViewModel()
        {
            Devices = new DevicesModel();

            serialPortsManager = new SerialPortsManager();

        }

        private readonly SerialPortsManager serialPortsManager;
        public DevicesModel Devices { get; set; }

    }
}
