using Advanced_Dynotis_Software.Services.Helpers;
using System;
using System.Windows.Input;

namespace Advanced_Dynotis_Software.ViewModels.Pages
{
    public class UnitsSettingsViewModel
    {
        public ICommand TorqueUnitChangedCommand { get; }
        public ICommand ThrustUnitChangedCommand { get; }
        public ICommand MotorSpeedUnitChangedCommand { get; }
        public ICommand TemperatureUnitChangedCommand { get; }
        public ICommand WindSpeedUnitChangedCommand { get; }
        public ICommand PressureUnitChangedCommand { get; }

        public UnitsSettingsViewModel()
        {
            TorqueUnitChangedCommand = new RelayCommand(OnTorqueUnitChanged);
            ThrustUnitChangedCommand = new RelayCommand(OnThrustUnitChanged);
            MotorSpeedUnitChangedCommand = new RelayCommand(OnMotorSpeedUnitChanged);
            TemperatureUnitChangedCommand = new RelayCommand(OnTemperatureUnitChanged);
            WindSpeedUnitChangedCommand = new RelayCommand(OnWindSpeedUnitChanged);
            PressureUnitChangedCommand = new RelayCommand(OnPressureUnitChanged);
        }

        private void OnTorqueUnitChanged(object parameter)
        {
            // Handle Torque unit change
        }

        private void OnThrustUnitChanged(object parameter)
        {
            // Handle Thrust unit change
        }

        private void OnMotorSpeedUnitChanged(object parameter)
        {
            // Handle Motor Speed unit change
        }

        private void OnTemperatureUnitChanged(object parameter)
        {
            // Handle Temperature unit change
        }

        private void OnWindSpeedUnitChanged(object parameter)
        {
            // Handle Wind Speed unit change
        }

        private void OnPressureUnitChanged(object parameter)
        {
            // Handle Pressure unit change
        }
    }
}
