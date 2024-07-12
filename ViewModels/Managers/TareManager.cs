using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using System.Collections.Generic;

namespace Advanced_Dynotis_Software.ViewModels.Managers
{
    public class TareManager
    {
        private readonly Dictionary<string, TareViewModel> _tare;

        public TareManager()
        {
            _tare = new Dictionary<string, TareViewModel>();
        }

        public TareViewModel GetTareViewModel(string devicePortName, DynotisData dynotisData)
        {
            if (_tare.ContainsKey(devicePortName))
            {
                return _tare[devicePortName];
            }
            else
            {
                var viewModel = new TareViewModel();
                _tare[devicePortName] = viewModel;
                return viewModel;
            }
        }

        public void RemoveTareViewModel(string devicePortName)
        {
            if (_tare.ContainsKey(devicePortName))
            {
                _tare.Remove(devicePortName);
            }
        }
    }
}
