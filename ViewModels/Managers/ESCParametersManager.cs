using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using System.Collections.Generic;

namespace Advanced_Dynotis_Software.ViewModels.Managers
{
    public class ESCParametersManager
    {
        private readonly Dictionary<string, ESCParametersViewModel> _escParameters;

        public ESCParametersManager()
        {
            _escParameters = new Dictionary<string, ESCParametersViewModel>();
        }

        public ESCParametersViewModel GetESCParametersViewModel(string devicePortName, DynotisData dynotisData)
        {
            if (_escParameters.ContainsKey(devicePortName))
            {
                return _escParameters[devicePortName];
            }
            else
            {
                var viewModel = new ESCParametersViewModel(dynotisData);
                _escParameters[devicePortName] = viewModel;
                return viewModel;
            }
        }

        public void RemoveESCParametersViewModel(string devicePortName)
        {
            if (_escParameters.ContainsKey(devicePortName))
            {
                _escParameters.Remove(devicePortName);
            }
        }
    }
}
