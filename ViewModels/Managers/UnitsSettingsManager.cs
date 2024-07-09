using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.ViewModels.Pages;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using Advanced_Dynotis_Software.Views.UserControls;
using System.Collections.Generic;

namespace Advanced_Dynotis_Software.ViewModels.Managers
{
    public class UnitsSettingsManager
    {
        private readonly Dictionary<string, UnitsSettingsViewModel> _unitsSettings;

        public UnitsSettingsManager()
        {
            _unitsSettings = new Dictionary<string, UnitsSettingsViewModel>();
        }
        public UnitsSettingsViewModel GetUnitsSettingsViewModel(string devicePortName, InterfaceVariables interfaceVariables)
        {

            if (_unitsSettings.ContainsKey(devicePortName))
            {
                return _unitsSettings[devicePortName];
            }
            else
            {
                var viewModel = new UnitsSettingsViewModel(interfaceVariables);
                _unitsSettings[devicePortName] = viewModel;
                return viewModel;
            }
        }

        public void RemoveUnitsSettingsViewModel(string devicePortName)
        {
            if (_unitsSettings.ContainsKey(devicePortName))
            {
                _unitsSettings.Remove(devicePortName);
            }
        }
    }
}
