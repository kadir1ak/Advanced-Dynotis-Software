using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advanced_Dynotis_Software.ViewModels.Managers
{
    public class BalancerPolarChartManager
    {
        private readonly Dictionary<string, BalancerPolarChartViewModel> _balancerPolarChart;

        public BalancerPolarChartManager()
        {
            _balancerPolarChart = new Dictionary<string, BalancerPolarChartViewModel>();
        }

        public BalancerPolarChartViewModel GetBalancerPolarChartViewModel(string devicePortName, DynotisData dynotisData, InterfaceVariables interfaceVariables)
        {
            if (_balancerPolarChart.ContainsKey(devicePortName))
            {
                return _balancerPolarChart[devicePortName];
            }
            else
            {
                var viewModel = new BalancerPolarChartViewModel(dynotisData, interfaceVariables);
                _balancerPolarChart[devicePortName] = viewModel;
                return viewModel;
            }
        }

        public void RemoveBalancerPolarChartViewModel(string devicePortName)
        {
            if (_balancerPolarChart.ContainsKey(devicePortName))
            {
                _balancerPolarChart.Remove(devicePortName);
            }
        }
    }
}
