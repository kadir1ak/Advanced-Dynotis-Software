using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advanced_Dynotis_Software.ViewModels.Managers
{
    public class BalancerRoutingStepsManager
    {
        private readonly Dictionary<string, BalancerRoutingStepsViewModel> _balancerRoutingSteps;

        public BalancerRoutingStepsManager()
        {
            _balancerRoutingSteps = new Dictionary<string, BalancerRoutingStepsViewModel>();
        }

        public BalancerRoutingStepsViewModel GetBalancerRoutingStepsViewModel(string devicePortName, DynotisData dynotisData, InterfaceVariables interfaceVariables)
        {
            if (_balancerRoutingSteps.ContainsKey(devicePortName))
            {
                return _balancerRoutingSteps[devicePortName];
            }
            else
            {
                var viewModel = new BalancerRoutingStepsViewModel(dynotisData, interfaceVariables);
                _balancerRoutingSteps[devicePortName] = viewModel;
                return viewModel;
            }
        }

        public void RemoveBalancerRoutingStepsViewModel(string devicePortName)
        {
            if (_balancerRoutingSteps.ContainsKey(devicePortName))
            {
                _balancerRoutingSteps.Remove(devicePortName);
            }
        }
    }
}
