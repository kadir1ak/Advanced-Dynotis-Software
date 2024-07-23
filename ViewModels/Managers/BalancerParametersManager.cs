using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.ViewModels.Pages;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using Advanced_Dynotis_Software.Views.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advanced_Dynotis_Software.ViewModels.Managers
{
    public class BalancerParametersManager
    {
        private readonly Dictionary<string, BalancerParametersViewModel> _balancerParameters;

        public BalancerParametersManager()
        {
            _balancerParameters = new Dictionary<string, BalancerParametersViewModel>();
        }

        public BalancerParametersViewModel GetBalancerParametersViewModel(string devicePortName, DynotisData dynotisData, InterfaceVariables interfaceVariables)
        {
            if (_balancerParameters.ContainsKey(devicePortName))
            {
                return _balancerParameters[devicePortName];
            }
            else
            {
                var viewModel = new BalancerParametersViewModel(interfaceVariables);
                _balancerParameters[devicePortName] = viewModel;
                return viewModel;
            }
        }

        public void RemoveBalancerParametersViewModel(string devicePortName)
        {
            if (_balancerParameters.ContainsKey(devicePortName))
            {
                _balancerParameters.Remove(devicePortName);
            }
        }
    }
}
