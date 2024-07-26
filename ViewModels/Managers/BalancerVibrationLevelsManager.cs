using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advanced_Dynotis_Software.ViewModels.Managers
{
    public class BalancerVibrationLevelsManager
    {
        private readonly Dictionary<string, BalancerVibrationLevelsViewModel> _balancerVibrationLevels;

        public BalancerVibrationLevelsManager()
        {
            _balancerVibrationLevels = new Dictionary<string, BalancerVibrationLevelsViewModel>();
        }

        public BalancerVibrationLevelsViewModel GetBalancerVibrationLevelsViewModel(string devicePortName, DynotisData dynotisData, InterfaceVariables interfaceVariables)
        {
            if (_balancerVibrationLevels.ContainsKey(devicePortName))
            {
                return _balancerVibrationLevels[devicePortName];
            }
            else
            {
                var viewModel = new BalancerVibrationLevelsViewModel(dynotisData, interfaceVariables);
                _balancerVibrationLevels[devicePortName] = viewModel;
                return viewModel;
            }
        }

        public void RemoveBalancerVibrationLevelsViewModel(string devicePortName)
        {
            if (_balancerVibrationLevels.ContainsKey(devicePortName))
            {
                _balancerVibrationLevels.Remove(devicePortName);
            }
        }
    }
}
