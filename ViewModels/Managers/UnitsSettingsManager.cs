using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.ViewModels.Pages;
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

        public UnitsSettingsViewModel GetUnitsSettingsViewModel(string unitType)
        {
            if (_unitsSettings.ContainsKey(unitType))
            {
                return _unitsSettings[unitType];
            }
            else
            {
                var viewModel = new UnitsSettingsViewModel(); 
                _unitsSettings[unitType] = viewModel;
                return viewModel;
            }
        }

        public void RemoveUnitsSettingsViewModel(string unitType)
        {
            if (_unitsSettings.ContainsKey(unitType))
            {
                _unitsSettings.Remove(unitType);
            }
        }

        public void UpdateUnitsSettingsViewModel(string unitType, string unitValue)
        {
            if (_unitsSettings.ContainsKey(unitType))
            {
                var viewModel = _unitsSettings[unitType];
                switch (unitType)
                {
                    case nameof(viewModel.TorqueUnit):
                        viewModel.TorqueUnit = unitValue;
                        break;
                    case nameof(viewModel.ThrustUnit):
                        viewModel.ThrustUnit = unitValue;
                        break;
                    case nameof(viewModel.MotorSpeedUnit):
                        viewModel.MotorSpeedUnit = unitValue;
                        break;
                    case nameof(viewModel.TemperatureUnit):
                        viewModel.TemperatureUnit = unitValue;
                        break;
                    case nameof(viewModel.WindSpeedUnit):
                        viewModel.WindSpeedUnit = unitValue;
                        break;
                    case nameof(viewModel.PressureUnit):
                        viewModel.PressureUnit = unitValue;
                        break;
                }
            }
        }
    }
}
