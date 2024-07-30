using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advanced_Dynotis_Software.ViewModels.Managers
{
    public class BalancerRoutingStepsVibrationLevelsManager
    {
        private readonly Dictionary<string, BalancerRoutingStepsVibrationLevelsViewModel> _balancerRoutingStepsVibrationLevels;

        public BalancerRoutingStepsVibrationLevelsManager()
        {
            _balancerRoutingStepsVibrationLevels = new Dictionary<string, BalancerRoutingStepsVibrationLevelsViewModel>();
        }

        public BalancerRoutingStepsVibrationLevelsViewModel GetBalancerRoutingStepsVibrationLevelsViewModel(string devicePortName, DynotisData dynotisData, InterfaceVariables interfaceVariables)
        {
            if (_balancerRoutingStepsVibrationLevels.ContainsKey(devicePortName))
            {
                return _balancerRoutingStepsVibrationLevels[devicePortName];
            }
            else
            {
                var viewModel = new BalancerRoutingStepsVibrationLevelsViewModel(dynotisData, interfaceVariables);
                _balancerRoutingStepsVibrationLevels[devicePortName] = viewModel;
                return viewModel;
            }
        }

        public void RemoveBalancerRoutingStepsVibrationLevelsViewModel(string devicePortName)
        {
            if (_balancerRoutingStepsVibrationLevels.ContainsKey(devicePortName))
            {
                _balancerRoutingStepsVibrationLevels.Remove(devicePortName);
            }
        }
    }
}
