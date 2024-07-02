using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using System.Collections.Generic;

namespace Advanced_Dynotis_Software.ViewModels.Managers
{
    public class EquipmentParametersManager
    {
        private readonly Dictionary<string, EquipmentParametersViewModel> _equipmentParameters;

        public EquipmentParametersManager()
        {
            _equipmentParameters = new Dictionary<string, EquipmentParametersViewModel>();
        }

        public EquipmentParametersViewModel GetEquipmentParametersViewModel(string devicePortName, DynotisData dynotisData)
        {
            if (_equipmentParameters.ContainsKey(devicePortName))
            {
                return _equipmentParameters[devicePortName];
            }
            else
            {
                var viewModel = new EquipmentParametersViewModel(dynotisData);
                _equipmentParameters[devicePortName] = viewModel;
                return viewModel;
            }
        }

        public void RemoveEquipmentParametersViewModel(string devicePortName)
        {
            if (_equipmentParameters.ContainsKey(devicePortName))
            {
                _equipmentParameters.Remove(devicePortName);
            }
        }
    }
}
