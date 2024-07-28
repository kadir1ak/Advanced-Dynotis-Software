using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advanced_Dynotis_Software.ViewModels.Managers
{
    public class BalancedPropellersManager
    {
        private readonly Dictionary<string, BalancedPropellersViewModel> _balancedPropellers;

        public BalancedPropellersManager()
        {
            _balancedPropellers = new Dictionary<string, BalancedPropellersViewModel>();
        }

        public BalancedPropellersViewModel GetBalancedPropellersViewModel(string devicePortName, DynotisData dynotisData, InterfaceVariables interfaceVariables)
        {
            if (_balancedPropellers.ContainsKey(devicePortName))
            {
                return _balancedPropellers[devicePortName];
            }
            else
            {
                var viewModel = new BalancedPropellersViewModel(interfaceVariables);
                _balancedPropellers[devicePortName] = viewModel;
                return viewModel;
            }
        }

        public void RemoveBalancedPropellersViewModel(string devicePortName)
        {
            if (_balancedPropellers.ContainsKey(devicePortName))
            {
                _balancedPropellers.Remove(devicePortName);
            }
        }
    }
}
