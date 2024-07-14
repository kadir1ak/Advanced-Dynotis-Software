﻿using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.Services;
using Advanced_Dynotis_Software.Services.Helpers;
using Advanced_Dynotis_Software.ViewModels.Main;
using Advanced_Dynotis_Software.ViewModels.Managers;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Advanced_Dynotis_Software.ViewModels.Pages
{
    public class CoaxialTestViewModel : INotifyPropertyChanged
    {
        private DeviceViewModel _selectedDeviceOne;
        private DeviceViewModel _selectedDeviceTwo;
        private DeviceViewModel _connectedOneDevice;
        private DeviceViewModel _connectedTwoDevice;

        private EquipmentParametersManager _equipmentParametersManagerOne;
        private EquipmentParametersManager _equipmentParametersManagerTwo;
        private ESCParametersManager _escParametersManagerOne;
        private ESCParametersManager _escParametersManagerTwo;
        private BatterySecurityLimitsManager _batterySecurityLimitsManagerOne;
        private BatterySecurityLimitsManager _batterySecurityLimitsManagerTwo;
        private TareManager _tareManagerOne;
        private TareManager _tareManagerTwo;

        private EquipmentParametersViewModel _currentEquipmentParametersOne;
        private EquipmentParametersViewModel _currentEquipmentParametersTwo;
        private ESCParametersViewModel _currentESCParametersOne;
        private ESCParametersViewModel _currentESCParametersTwo;
        private BatterySecurityLimitsViewModel _currentBatterySecurityLimitsOne;
        private BatterySecurityLimitsViewModel _currentBatterySecurityLimitsTwo;
        private TareViewModel _currentTareOne;
        private TareViewModel _currentTareTwo;

        public ObservableCollection<DeviceViewModel> AvailableDevicesOne { get; private set; }
        public ObservableCollection<DeviceViewModel> AvailableDevicesTwo { get; private set; }

        public DeviceViewModel SelectedDeviceOne
        {
            get => _selectedDeviceOne;
            set
            {
                if (_selectedDeviceTwo == null || _selectedDeviceTwo.Device.PortName != value?.Device.PortName)
                {
                    _selectedDeviceOne = value;
                    OnPropertyChanged();
                }
                else
                {
                    MessageBox.Show("Bu cihaz zaten 2. cihaz olarak seçildi. Lütfen farklı bir cihaz seçin.");
                }
            }
        }

        public DeviceViewModel SelectedDeviceTwo
        {
            get => _selectedDeviceTwo;
            set
            {
                if (_selectedDeviceOne == null || _selectedDeviceOne.Device.PortName != value?.Device.PortName)
                {
                    _selectedDeviceTwo = value;
                    OnPropertyChanged();
                }
                else
                {
                    MessageBox.Show("Bu cihaz zaten 1. cihaz olarak seçildi. Lütfen farklı bir cihaz seçin.");
                }
            }
        }

        public DeviceViewModel ConnectedOneDevice
        {
            get => _connectedOneDevice;
            set
            {
                if (_connectedOneDevice != value)
                {
                    _connectedOneDevice = value;
                    OnPropertyChanged();
                }
            }
        }

        public DeviceViewModel ConnectedTwoDevice
        {
            get => _connectedTwoDevice;
            set
            {
                if (_connectedTwoDevice != value)
                {
                    _connectedTwoDevice = value;
                    OnPropertyChanged();
                }
            }
        }
        public TareViewModel CurrentTareOne
        {
            get => _currentTareOne;
            set => SetProperty(ref _currentTareOne, value);
        }

        public TareViewModel CurrentTareTwo
        {
            get => _currentTareTwo;
            set => SetProperty(ref _currentTareTwo, value);
        }

        public EquipmentParametersViewModel CurrentEquipmentParametersOne
        {
            get => _currentEquipmentParametersOne;
            set => SetProperty(ref _currentEquipmentParametersOne, value);
        }

        public EquipmentParametersViewModel CurrentEquipmentParametersTwo
        {
            get => _currentEquipmentParametersTwo;
            set => SetProperty(ref _currentEquipmentParametersTwo, value);
        }

        public ESCParametersViewModel CurrentESCParametersOne
        {
            get => _currentESCParametersOne;
            set => SetProperty(ref _currentESCParametersOne, value);
        }

        public ESCParametersViewModel CurrentESCParametersTwo
        {
            get => _currentESCParametersTwo;
            set => SetProperty(ref _currentESCParametersTwo, value);
        }

        public BatterySecurityLimitsViewModel CurrentBatterySecurityLimitsOne
        {
            get => _currentBatterySecurityLimitsOne;
            set => SetProperty(ref _currentBatterySecurityLimitsOne, value);
        }

        public BatterySecurityLimitsViewModel CurrentBatterySecurityLimitsTwo
        {
            get => _currentBatterySecurityLimitsTwo;
            set => SetProperty(ref _currentBatterySecurityLimitsTwo, value);
        }

        public ICommand ConnectCommand { get; }

        public CoaxialTestViewModel()
        {
            AvailableDevicesOne = new ObservableCollection<DeviceViewModel>(DeviceManager.Instance.GetAllDevices());
            AvailableDevicesTwo = new ObservableCollection<DeviceViewModel>(DeviceManager.Instance.GetAllDevices());
            ConnectCommand = new RelayCommand(ConnectToDevice);

            _equipmentParametersManagerOne = new EquipmentParametersManager();
            _equipmentParametersManagerTwo = new EquipmentParametersManager();
            _escParametersManagerOne = new ESCParametersManager();
            _escParametersManagerTwo = new ESCParametersManager();
            _batterySecurityLimitsManagerOne = new BatterySecurityLimitsManager();
            _batterySecurityLimitsManagerTwo = new BatterySecurityLimitsManager();
            _tareManagerOne = new TareManager();
            _tareManagerTwo = new TareManager();

            DeviceManager.Instance.DeviceDisconnected += OnDeviceDisconnected;
            DeviceManager.Instance.DeviceConnected += OnDeviceConnected;
        }

        private async void ConnectToDevice(object parameter)
        {
            if (parameter is string device)
            {
                switch (device)
                {
                    case "One":
                        if (SelectedDeviceOne != null)
                        {
                            await ConnectToDeviceAsync(SelectedDeviceOne);
                        }
                        break;
                    case "Two":
                        if (SelectedDeviceTwo != null)
                        {
                            await ConnectToDeviceAsync(SelectedDeviceTwo);
                        }
                        break;
                }
            }
        }

        private async Task ConnectToDeviceAsync(DeviceViewModel deviceViewModel)
        {
            await DeviceManager.Instance.ConnectToDeviceAsync(deviceViewModel.Device.PortName);
            DeviceManager.Instance.AddConnectedDevice(deviceViewModel);
            if (deviceViewModel == SelectedDeviceOne)
            {
                ConnectedOneDevice = SelectedDeviceOne;
                CurrentEquipmentParametersOne = _equipmentParametersManagerOne.GetEquipmentParametersViewModel(SelectedDeviceOne.Device.PortName, SelectedDeviceOne.Device.DynotisData);
                CurrentESCParametersOne = _escParametersManagerOne.GetESCParametersViewModel(SelectedDeviceOne.Device.PortName, SelectedDeviceOne.Device.DynotisData);
                CurrentBatterySecurityLimitsOne = _batterySecurityLimitsManagerOne.GetBatterySecurityLimitsViewModel(SelectedDeviceOne.Device.PortName, SelectedDeviceOne.Device.DynotisData);
                CurrentTareOne = _tareManagerOne.GetTareViewModel(SelectedDeviceOne.Device.PortName, SelectedDeviceOne.Device.DynotisData);
            }
            else if (deviceViewModel == SelectedDeviceTwo)
            {
                ConnectedTwoDevice = SelectedDeviceTwo;
                CurrentEquipmentParametersTwo = _equipmentParametersManagerTwo.GetEquipmentParametersViewModel(SelectedDeviceTwo.Device.PortName, SelectedDeviceTwo.Device.DynotisData);
                CurrentESCParametersTwo = _escParametersManagerTwo.GetESCParametersViewModel(SelectedDeviceTwo.Device.PortName, SelectedDeviceTwo.Device.DynotisData);
                CurrentBatterySecurityLimitsTwo = _batterySecurityLimitsManagerTwo.GetBatterySecurityLimitsViewModel(SelectedDeviceTwo.Device.PortName, SelectedDeviceTwo.Device.DynotisData);
                CurrentTareTwo = _tareManagerTwo.GetTareViewModel(SelectedDeviceTwo.Device.PortName, SelectedDeviceTwo.Device.DynotisData);
            }
        }

        private void UpdateAvailableDevices()
        {
            var allDevices = DeviceManager.Instance.GetAllDevices().ToList();

            var selectedDeviceOnePortName = SelectedDeviceOne?.Device.PortName;
            var selectedDeviceTwoPortName = SelectedDeviceTwo?.Device.PortName;

            AvailableDevicesOne.Clear();
            AvailableDevicesTwo.Clear();

            foreach (var device in allDevices)
            {
                AvailableDevicesOne.Add(device);
                AvailableDevicesTwo.Add(device);
            }

            OnPropertyChanged(nameof(AvailableDevicesOne));
            OnPropertyChanged(nameof(AvailableDevicesTwo));

            SelectedDeviceOne = AvailableDevicesOne.FirstOrDefault(d => d.Device.PortName == selectedDeviceOnePortName);
            SelectedDeviceTwo = AvailableDevicesTwo.FirstOrDefault(d => d.Device.PortName == selectedDeviceTwoPortName);
        }

        private void OnDeviceDisconnected(DeviceViewModel device)
        {
            if (ConnectedOneDevice == device)
            {
                ConnectedOneDevice = null;
                CurrentEquipmentParametersOne = null;
                CurrentESCParametersOne = null;
                CurrentBatterySecurityLimitsOne = null;
                CurrentTareOne = null;
            }
            if (ConnectedTwoDevice == device)
            {
                ConnectedTwoDevice = null;
                CurrentEquipmentParametersTwo = null;
                CurrentESCParametersTwo = null;
                CurrentBatterySecurityLimitsTwo = null;
                CurrentTareTwo = null;
            }

            if (SelectedDeviceOne == device)
            {
                SelectedDeviceOne = null;
            }
            if (SelectedDeviceTwo == device)
            {
                SelectedDeviceTwo = null;
            }

            UpdateAvailableDevices();
        }

        private void OnDeviceConnected(DeviceViewModel device)
        {
            UpdateAvailableDevices();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
