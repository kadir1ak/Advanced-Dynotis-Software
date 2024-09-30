using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advanced_Dynotis_Software.ViewModels.Managers
{
    public class FirmwareUpdateManager
    {
        private readonly Dictionary<string, FirmwareUpdateViewModel> _firmwareUpdate;

        public FirmwareUpdateManager()
        {
            _firmwareUpdate = new Dictionary<string, FirmwareUpdateViewModel>();
        }

        public FirmwareUpdateViewModel GetFirmwareUpdateViewModel(Dynotis device)
        {
            if (_firmwareUpdate.ContainsKey(device.PortName))
            {
                return _firmwareUpdate[device.PortName];
            }
            else
            {
                var viewModel = new FirmwareUpdateViewModel(device);
                _firmwareUpdate[device.PortName] = viewModel;
                return viewModel;
            }
        }

        public void RemoveFirmwareUpdateViewModel(string devicePortName)
        {
            if (_firmwareUpdate.ContainsKey(devicePortName))
            {
                _firmwareUpdate.Remove(devicePortName);
            }
        }
    }
}
