using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using System.Collections.Generic;

namespace Advanced_Dynotis_Software.ViewModels.Managers
{
    public class BatterySecurityLimitsManager
    {
        private readonly Dictionary<string, BatterySecurityLimitsViewModel> _batterySecurityLimits;

        public BatterySecurityLimitsManager()
        {
            _batterySecurityLimits = new Dictionary<string, BatterySecurityLimitsViewModel>();
        }

        public BatterySecurityLimitsViewModel GetBatterySecurityLimitsViewModel(string devicePortName, DynotisData dynotisData)
        {
            if (_batterySecurityLimits.ContainsKey(devicePortName))
            {
                return _batterySecurityLimits[devicePortName];
            }
            else
            {
                var viewModel = new BatterySecurityLimitsViewModel(dynotisData);
                _batterySecurityLimits[devicePortName] = viewModel;
                return viewModel;
            }
        }

        public void RemoveBatterySecurityLimitsViewModel(string devicePortName)
        {
            if (_batterySecurityLimits.ContainsKey(devicePortName))
            {
                _batterySecurityLimits.Remove(devicePortName);
            }
        }
    }
}
