using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using System.Collections.Generic;

namespace Advanced_Dynotis_Software.ViewModels.Managers
{
    public class RecordManager
    {
        private readonly Dictionary<string, RecordViewModel> _tare;

        public RecordManager()
        {
            _tare = new Dictionary<string, RecordViewModel>();
        }

        public RecordViewModel GetRecordViewModel(string devicePortName, DynotisData dynotisData, InterfaceVariables interfaceVariables)
        {
            if (_tare.ContainsKey(devicePortName))
            {
                return _tare[devicePortName];
            }
            else
            {
                var viewModel = new RecordViewModel(interfaceVariables);
                _tare[devicePortName] = viewModel;
                return viewModel;
            }
        }

        public void RemoveRecordViewModel(string devicePortName)
        {
            if (_tare.ContainsKey(devicePortName))
            {
                _tare.Remove(devicePortName);
            }
        }
    }
}
