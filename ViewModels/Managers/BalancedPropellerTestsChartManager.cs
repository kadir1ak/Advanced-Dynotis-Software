using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using Advanced_Dynotis_Software.Views.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advanced_Dynotis_Software.ViewModels.Managers
{
    public class BalancedPropellerTestsChartManager
    {
        private readonly Dictionary<string, BalancedPropellerTestsChartViewModel> _balancedPropellerTestsChart;

        public BalancedPropellerTestsChartManager()
        {
            _balancedPropellerTestsChart = new Dictionary<string, BalancedPropellerTestsChartViewModel>();
        }

        public BalancedPropellerTestsChartViewModel GetBalancedPropellerTestsChartViewModel(string devicePortName, DynotisData dynotisData, InterfaceVariables interfaceVariables)
        {
            if (_balancedPropellerTestsChart.ContainsKey(devicePortName))
            {
                return _balancedPropellerTestsChart[devicePortName];
            }
            else
            {
                var viewModel = new BalancedPropellerTestsChartViewModel(interfaceVariables);
                _balancedPropellerTestsChart[devicePortName] = viewModel;
                return viewModel;
            }
        }

        public void RemoveBalancedPropellerTestsChartViewModel(string devicePortName)
        {
            if (_balancedPropellerTestsChart.ContainsKey(devicePortName))
            {
                _balancedPropellerTestsChart.Remove(devicePortName);
            }
        }
    }
}
