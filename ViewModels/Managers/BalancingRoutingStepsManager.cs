using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advanced_Dynotis_Software.ViewModels.Managers
{
    public class BalancingRoutingStepsManager
    {
        private readonly Dictionary<string, BalancingRoutingStepsViewModel> _balancingRoutingSteps;

        public BalancingRoutingStepsManager()
        {
            _balancingRoutingSteps = new Dictionary<string, BalancingRoutingStepsViewModel>();
        }

        public BalancingRoutingStepsViewModel GetBalancingRoutingStepsViewModel(string devicePortName, DynotisData dynotisData, InterfaceVariables interfaceVariables)
        {
            if (_balancingRoutingSteps.ContainsKey(devicePortName))
            {
                return _balancingRoutingSteps[devicePortName];
            }
            else
            {
                var viewModel = new BalancingRoutingStepsViewModel(interfaceVariables);
                _balancingRoutingSteps[devicePortName] = viewModel;
                return viewModel;
            }
        }

        public void RemoveBalancingRoutingStepsViewModel(string devicePortName)
        {
            if (_balancingRoutingSteps.ContainsKey(devicePortName))
            {
                _balancingRoutingSteps.Remove(devicePortName);
            }
        }
    }
}
