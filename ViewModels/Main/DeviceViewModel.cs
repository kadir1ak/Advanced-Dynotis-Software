﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.ViewModels.Managers;
using Advanced_Dynotis_Software.ViewModels.Pages;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using Advanced_Dynotis_Software.Services.Helpers;

namespace Advanced_Dynotis_Software.ViewModels.Main
{
    public class DeviceViewModel : INotifyPropertyChanged, IDisposable
    {
        public InterfaceVariables DeviceInterfaceVariables { get; private set; }
        public SaveVariables DeviceSaveVariables { get; private set; }
        public ChartViewModel ChartViewModel { get; }

        public CsvHelper CsvHelper { get; private set; }

        public string DeviceDisplayName => $"{Device.Model} - {Device.SeriNo}";

        private Dynotis _device;
        private DynotisData _latestDynotisData;
        private readonly object _dataLock = new();
        private CancellationTokenSource _cancellationTokenSource;
        private int SaveTimeMillisecond = 10; // 100 Hz (10ms)
        private int UpdateTimeMillisecond = 20; // 50 Hz (20ms)
        private int ChartUpdateTimeMillisecond = 5; // 200 Hz (2ms)
        public Dynotis Device
        {
            get => _device;
            set
            {
                if (_device != value)
                {
                    _device = value;
                    OnPropertyChanged(nameof(Device.DynotisData)); // Notify when the device changes
                }
            }
        }
        public DeviceViewModel(string portName)
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                DeviceInterfaceVariables = new InterfaceVariables();
                DeviceSaveVariables = new SaveVariables();
                Device = new Dynotis(portName);
                ChartViewModel = new ChartViewModel();
                _cancellationTokenSource = new CancellationTokenSource();

                Device.PropertyChanged += Device_PropertyChanged;

                InitializeDeviceAsync();
                Task.Run(() => SaveDataLoop(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
                Task.Run(() => UpdateDataLoop(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
                Task.Run(() => UpdateChartLoop(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
            }
        }

        private async void InitializeDeviceAsync()
        {
            await Device.OpenPortAsync();
        }

        private async Task SaveDataLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(SaveTimeMillisecond, token);

                DynotisData latestData;
                lock (_dataLock)
                {
                    latestData = _latestDynotisData;
                    _latestDynotisData = null;
                }

                if (latestData != null)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (latestData.RecordStatus)
                        {
                            DeviceSaveVariables.UpdateSaveVariables(latestData);
                            if (!DeviceSaveVariables.RecordFileCreate && DeviceSaveVariables.RecordFilePath != null)
                            {
                                // CSV şablonu oluştur
                                CsvHelper.CreateCsvTemplate(DeviceSaveVariables);
                                DeviceSaveVariables.RecordFileCreate = true;
                                DeviceSaveVariables.RecordStatus = true;
                            }
                            else
                            {
                                // CSV dosyasına satır ekle
                                CsvHelper.AppendCsvRow(DeviceSaveVariables.RecordFilePath, DeviceSaveVariables.DataRow);
                            }
                        }
                    });
                }
            }
        }
        private async Task UpdateDataLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(UpdateTimeMillisecond, token);

                DynotisData latestData;
                lock (_dataLock)
                {
                    latestData = _latestDynotisData;
                    _latestDynotisData = null;
                }

                if (latestData != null)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        DeviceInterfaceVariables.UpdateFrom(latestData);
                    });
                }
            }
        }

        private async Task UpdateChartLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(ChartUpdateTimeMillisecond, token);

                // Veriler DynotisData dan alınacak şekilde düzenlenecek!
                DynotisData latestData;
                lock (_dataLock)
                {
                    latestData = _latestDynotisData;
                    _latestDynotisData = null;
                }

                if (latestData != null)
                {
                    try
                    {
                        // ChartViewModel'deki UpdateChartData metodunu çağır
                        await ChartViewModel.UpdateChartData(latestData, DeviceInterfaceVariables);
                    }
                    catch (Exception ex)
                    {
                        // Hata durumunda bir loglama mekanizması eklenebilir
                        MessageBox.Show($"Grafik güncellenirken bir hata oluştu: {ex.Message}");
                    }
                }
            }
        }
        private void Device_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Dynotis.DynotisData))
            {
                lock (_dataLock)
                {
                    _latestDynotisData = Device.DynotisData;
                }
            }
        }


        private BalancerPolarChartViewModel _currentBalancerPolarChart;
        public BalancerPolarChartViewModel CurrentBalancerPolarChart
        {
            get => _currentBalancerPolarChart;
            set
            {
                if (SetProperty(ref _currentBalancerPolarChart, value))
                {
                    if (_currentBalancerPolarChart != null)
                    {
                        _currentBalancerPolarChart.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == nameof(BalancerPolarChartViewModel.CartesianPlotModel) ||
                                e.PropertyName == nameof(BalancerPolarChartViewModel.PolarPlotModel) ||
                                 e.PropertyName == nameof(BalancerPolarChartViewModel.VibrationDynamicBalancer360))
                            {

                                _currentBalancerPolarChart.VibrationDynamicBalancer360 = DeviceInterfaceVariables.VibrationDynamicBalancer360;


                                OnPropertyChanged(nameof(DeviceInterfaceVariables));
                            }
                        };
                    }
                }
            }
        }


        private BalancedPropellersViewModel _currentBalancedPropellers;
        public BalancedPropellersViewModel CurrentBalancedPropellers
        {
            get => _currentBalancedPropellers;
            set
            {
                if (SetProperty(ref _currentBalancedPropellers, value))
                {
                    if (_currentBalancedPropellers != null)
                    {
                        _currentBalancedPropellers.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == nameof(BalancedPropellersViewModel.BalancedPropellerID) ||
                                e.PropertyName == nameof(BalancedPropellersViewModel.BalancedPropellerDiameter) ||
                                e.PropertyName == nameof(BalancedPropellersViewModel.BalancingTestDates) ||
                                e.PropertyName == nameof(BalancedPropellersViewModel.VibrationLevels))
                            {
                                DeviceInterfaceVariables.BalancedPropellersID = _currentBalancedPropellers.BalancedPropellerID;
                                DeviceInterfaceVariables.BalancedPropellersDiameter = _currentBalancedPropellers.BalancedPropellerDiameter;
                                DeviceInterfaceVariables.ReferencePropellerDiameter = _currentBalancedPropellers.BalancedPropellerDiameter;
                                DeviceInterfaceVariables.BalancedPropellersTestDates = _currentBalancedPropellers.BalancingTestDates;
                                DeviceInterfaceVariables.BalancedPropellersVibrations = _currentBalancedPropellers.VibrationLevels;
                                OnPropertyChanged(nameof(DeviceInterfaceVariables));
                            }
                        };
                    }
                }
            }
        }

        private BalancedPropellerTestsChartViewModel _currentBalancedPropellerTestsChart;
        public BalancedPropellerTestsChartViewModel CurrentBalancedPropellerTestsChart
        {
            get => _currentBalancedPropellerTestsChart;
            set
            {
                if (SetProperty(ref _currentBalancedPropellerTestsChart, value))
                {
                    if (_currentBalancedPropellerTestsChart != null)
                    {
                        _currentBalancedPropellerTestsChart.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == nameof(BalancedPropellersViewModel.BalancedPropellerID) ||
                                e.PropertyName == nameof(BalancedPropellersViewModel.BalancedPropellerDiameter) ||
                                e.PropertyName == nameof(BalancedPropellersViewModel.BalancingTestDates) ||
                                e.PropertyName == nameof(BalancedPropellersViewModel.VibrationLevels))
                            {
                                DeviceInterfaceVariables.BalancedPropellersID = _currentBalancedPropellers.BalancedPropellerID;
                                DeviceInterfaceVariables.BalancedPropellersDiameter = _currentBalancedPropellers.BalancedPropellerDiameter;
                                DeviceInterfaceVariables.BalancedPropellersTestDates = _currentBalancedPropellers.BalancingTestDates;
                                DeviceInterfaceVariables.BalancedPropellersVibrations = _currentBalancedPropellers.VibrationLevels;

                                Device.DynotisData.PropellerDiameter = DeviceInterfaceVariables.BalancedPropellersDiameter;

                                OnPropertyChanged(nameof(DeviceInterfaceVariables));
                                OnPropertyChanged(nameof(Device.DynotisData));
                            }
                        };
                    }
                }
            }
        }

        private BalancerRoutingStepsVibrationLevelsViewModel _currentBalancerRoutingStepsVibrationLevels;
        public BalancerRoutingStepsVibrationLevelsViewModel CurrentBalancerRoutingStepsVibrationLevels
        {
            get => _currentBalancerRoutingStepsVibrationLevels;
            set
            {
                if (SetProperty(ref _currentBalancerRoutingStepsVibrationLevels, value))
                {
                    if (_currentBalancerRoutingStepsVibrationLevels != null)
                    {
                        _currentBalancerRoutingStepsVibrationLevels.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == nameof(BalancerRoutingStepsVibrationLevelsViewModel.BalancerIterationStepChart) ||
                                e.PropertyName == nameof(BalancerRoutingStepsVibrationLevelsViewModel.BalancerIterationVibrationsChart) ||
                                e.PropertyName == nameof(BalancerRoutingStepsVibrationLevelsViewModel.BalancerIterationStep))
                            {
                                _currentBalancerRoutingStepsVibrationLevels.BalancerIterationStep = DeviceInterfaceVariables.BalancerIterationStep;
                                _currentBalancerRoutingStepsVibrationLevels.BalancerIterationStepChart = DeviceInterfaceVariables.BalancerIterationStepChart;
                                _currentBalancerRoutingStepsVibrationLevels.BalancerIterationVibrationsChart = DeviceInterfaceVariables.BalancerIterationVibrationsChart;
                                OnPropertyChanged(nameof(DeviceInterfaceVariables));
                                OnPropertyChanged(nameof(Device.DynotisData));
                            }
                        };
                    }
                }
            }
        }

        private BalancerRoutingStepsViewModel _currentBalancerRoutingSteps;
        public BalancerRoutingStepsViewModel CurrentBalancerRoutingSteps
        {
            get => _currentBalancerRoutingSteps;
            set
            {
                if (SetProperty(ref _currentBalancerRoutingSteps, value))
                {
                    if (_currentBalancerRoutingSteps != null)
                    {
                        _currentBalancerRoutingSteps.PropertyChanged += (sender, e) =>
                        {

                            if (e.PropertyName == nameof(BalancerRoutingStepsViewModel.BalancerIterationStep) ||
                                e.PropertyName == nameof(BalancerRoutingStepsViewModel.BalancerIterationStepChart) ||
                                e.PropertyName == nameof(BalancerRoutingStepsViewModel.BalancerIterationVibrationsChart) ||
                                e.PropertyName == nameof(BalancerRoutingStepsViewModel.BalancerIterationDescription) ||
                                e.PropertyName == nameof(BalancerRoutingStepsViewModel.TestStepsPropellerVibrations) ||
                                e.PropertyName == nameof(BalancerRoutingStepsViewModel.DeviceBaseStaticVibration) ||
                                e.PropertyName == nameof(BalancerRoutingStepsViewModel.MotorBaseRunningVibration) ||
                                e.PropertyName == nameof(BalancerRoutingStepsViewModel.PropellerBaseRunningVibration) ||
                                e.PropertyName == nameof(BalancerRoutingStepsViewModel.BalancedPropellerRunningVibration) ||
                                e.PropertyName == nameof(BalancerRoutingStepsViewModel.FirstBladeVibration) ||
                                e.PropertyName == nameof(BalancerRoutingStepsViewModel.SecondBladeVibration) ||
                                e.PropertyName == nameof(BalancerRoutingStepsViewModel.TareVibration) ||
                                e.PropertyName == nameof(BalancerRoutingStepsViewModel.TareVibrationX) ||
                                e.PropertyName == nameof(BalancerRoutingStepsViewModel.TareVibrationY) ||
                                e.PropertyName == nameof(BalancerRoutingStepsViewModel.TareVibrationZ) ||
                                e.PropertyName == nameof(BalancerRoutingStepsViewModel.ESCValue) ||
                                e.PropertyName == nameof(BalancerRoutingStepsViewModel.ESCStatus) ||
                                e.PropertyName == nameof(BalancerRoutingStepsViewModel.HighIPSVibration) ||
                                e.PropertyName == nameof(BalancerRoutingStepsViewModel.HighVibration))
                            {

                                DeviceInterfaceVariables.BalancerIterationStep = _currentBalancerRoutingSteps.BalancerIterationStep;
                                DeviceInterfaceVariables.BalancerIterationVibrations = _currentBalancerRoutingSteps.TestStepsPropellerVibrations;
                                DeviceInterfaceVariables.BalancerIterationStepChart = _currentBalancerRoutingSteps.BalancerIterationStepChart;
                                DeviceInterfaceVariables.BalancerIterationVibrationsChart = _currentBalancerRoutingSteps.BalancerIterationVibrationsChart;
                                DeviceInterfaceVariables.BalancerIterationDescription = _currentBalancerRoutingSteps.BalancerIterationDescription;
                                DeviceInterfaceVariables.DeviceBaseStaticVibration = _currentBalancerRoutingSteps.DeviceBaseStaticVibration;
                                DeviceInterfaceVariables.MotorBaseRunningVibration = _currentBalancerRoutingSteps.MotorBaseRunningVibration;
                                DeviceInterfaceVariables.PropellerBaseRunningVibration = _currentBalancerRoutingSteps.PropellerBaseRunningVibration;
                                DeviceInterfaceVariables.BalancedPropellerRunningVibration = _currentBalancerRoutingSteps.BalancedPropellerRunningVibration;
                                DeviceInterfaceVariables.FirstBladeVibration = _currentBalancerRoutingSteps.FirstBladeVibration;
                                DeviceInterfaceVariables.SecondBladeVibration = _currentBalancerRoutingSteps.SecondBladeVibration;
                                DeviceInterfaceVariables.Vibration.TareVibration = _currentBalancerRoutingSteps.TareVibration;
                                DeviceInterfaceVariables.Vibration.TareVibrationX = _currentBalancerRoutingSteps.TareVibrationX;
                                DeviceInterfaceVariables.Vibration.TareVibrationY = _currentBalancerRoutingSteps.TareVibrationY;
                                DeviceInterfaceVariables.Vibration.TareVibrationZ = _currentBalancerRoutingSteps.TareVibrationZ;
                                _currentBalancerRoutingSteps.HighVibration = DeviceInterfaceVariables.Vibration.HighVibration;
                                _currentBalancerRoutingSteps.HighIPSVibration = DeviceInterfaceVariables.Vibration.HighIPSVibration;

                                OnPropertyChanged(nameof(DeviceInterfaceVariables));

                                Device.DynotisData.ESCValue = _currentBalancerRoutingSteps.ESCValue;
                                Device.DynotisData.ESCStatus = _currentBalancerRoutingSteps.ESCStatus;
                                OnPropertyChanged(nameof(Device.DynotisData));

                            }
                        };
                    }
                }
            }
        }

        private BalancerParametersViewModel _currentBalancerParameters;
        public BalancerParametersViewModel CurrentBalancerParameters
        {
            get => _currentBalancerParameters;
            set
            {
                if (SetProperty(ref _currentBalancerParameters, value))
                {
                    if (_currentBalancerParameters != null)
                    {
                        _currentBalancerParameters.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == nameof(BalancerParametersViewModel.ReferenceMotorSpeed) ||
                                e.PropertyName == nameof(BalancerParametersViewModel.ReferencePropellerDiameter) ||
                                e.PropertyName == nameof(BalancerParametersViewModel.BalancerIterationStep) ||
                                e.PropertyName == nameof(BalancerParametersViewModel.BalancerIterationStepChart) ||
                                e.PropertyName == nameof(BalancerParametersViewModel.BalancerIterationVibrationsChart) ||
                                e.PropertyName == nameof(BalancerParametersViewModel.BalancerIterationDescription))
                            {
                                DeviceInterfaceVariables.ReferenceMotorSpeed = _currentBalancerParameters.ReferenceMotorSpeed;
                                DeviceInterfaceVariables.TotalWeight = _currentBalancerParameters.TotalWeight;
                                DeviceInterfaceVariables.UnitReferenceWeight = _currentBalancerParameters.UnitReferenceWeight;
                                _currentBalancerParameters.ReferencePropellerDiameter = DeviceInterfaceVariables.ReferencePropellerDiameter;
                                _currentBalancerParameters.BalancerIterationStep = DeviceInterfaceVariables.BalancerIterationStep;
                                _currentBalancerParameters.BalancerIterationStepChart = DeviceInterfaceVariables.BalancerIterationStepChart;
                                _currentBalancerParameters.BalancerIterationDescription = DeviceInterfaceVariables.BalancerIterationDescription;
                                _currentBalancerParameters.BalancerIterationVibrationsChart = DeviceInterfaceVariables.BalancerIterationVibrationsChart;
                                OnPropertyChanged(nameof(DeviceInterfaceVariables));
                            }
                        };
                    }
                }
            }
        }

        private RecordViewModel _currentRecord;
        public RecordViewModel CurrentRecord
        {
            get => _currentRecord;
            set
            {
                if (SetProperty(ref _currentRecord, value))
                {
                    if (_currentRecord != null)
                    {
                        _currentRecord.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == nameof(RecordViewModel.FileName) ||
                                e.PropertyName == nameof(RecordViewModel.IsRecording) ||
                                e.PropertyName == nameof(RecordViewModel.TestMode) ||
                                e.PropertyName == nameof(RecordViewModel.Duration))
                            {
                                DeviceInterfaceVariables.FileName = _currentRecord.FileName;
                                DeviceInterfaceVariables.IsRecording = _currentRecord.IsRecording;
                                DeviceInterfaceVariables.TestMode = _currentRecord.TestMode;
                                DeviceInterfaceVariables.Duration = _currentRecord.Duration;
                                OnPropertyChanged(nameof(DeviceInterfaceVariables));
                            }
                        };
                    }
                }
            }
        }

        private TareViewModel _currentTare;
        public TareViewModel CurrentTare
        {
            get => _currentTare;
            set
            {
                if (SetProperty(ref _currentTare, value))
                {
                    if (_currentTare != null)
                    {
                        _currentTare.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == nameof(TareViewModel.TareThrustBaseValue) ||
                                e.PropertyName == nameof(TareViewModel.TareTorqueBaseValue) ||
                                e.PropertyName == nameof(TareViewModel.TareCurrentBaseValue) ||
                                e.PropertyName == nameof(TareViewModel.TareMotorSpeedBaseValue))
                            {
                                DeviceInterfaceVariables.TareThrustBaseValue = _currentTare.TareThrustBaseValue;
                                DeviceInterfaceVariables.TareTorqueBaseValue = _currentTare.TareTorqueBaseValue;
                                DeviceInterfaceVariables.TareCurrentBaseValue = _currentTare.TareCurrentBaseValue;
                                DeviceInterfaceVariables.TareMotorSpeedBaseValue = _currentTare.TareMotorSpeedBaseValue;
                                OnPropertyChanged(nameof(DeviceInterfaceVariables));
                            }
                        };
                    }
                }
            }
        }

        private FirmwareUpdateViewModel _currentFirmwareUpdate;
        public FirmwareUpdateViewModel CurrentFirmwareUpdate
        {
            get => _currentFirmwareUpdate;
            set
            {
                if (SetProperty(ref _currentFirmwareUpdate, value))
                {
                    if (_currentFirmwareUpdate != null)
                    {
                        _currentFirmwareUpdate.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == nameof(FirmwareUpdateViewModel.Bootloader_Mode))
                            {
                                Device.Bootloader_Mode = _currentFirmwareUpdate.Bootloader_Mode;
                                Device.Mode = _currentFirmwareUpdate.Mode;
                                OnPropertyChanged(nameof(Device.DynotisData));
                            }
                        };
                    }
                }
            }
        }

        private EquipmentParametersViewModel _currentEquipmentParameters;
        public EquipmentParametersViewModel CurrentEquipmentParameters
        {
            get => _currentEquipmentParameters;
            set
            {
                if (SetProperty(ref _currentEquipmentParameters, value))
                {
                    if (_currentEquipmentParameters != null)
                    {
                        _currentEquipmentParameters.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == nameof(EquipmentParametersViewModel.UserPropellerDiameter) ||
                                e.PropertyName == nameof(EquipmentParametersViewModel.UserMotorInner) ||
                                e.PropertyName == nameof(EquipmentParametersViewModel.UserNoLoadCurrents))
                            {
                                Device.DynotisData.PropellerDiameter = _currentEquipmentParameters.UserPropellerDiameter;
                                Device.DynotisData.MotorInner = _currentEquipmentParameters.UserMotorInner;
                                Device.DynotisData.NoLoadCurrents = _currentEquipmentParameters.UserNoLoadCurrents;
                                OnPropertyChanged(nameof(Device.DynotisData));
                            }
                        };
                    }
                }
            }
        }

        private ESCParametersViewModel _currentESCParameters;
        public ESCParametersViewModel CurrentESCParameters
        {
            get => _currentESCParameters;
            set
            {
                if (SetProperty(ref _currentESCParameters, value))
                {
                    if (_currentESCParameters != null)
                    {
                        _currentESCParameters.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == nameof(ESCParametersViewModel.ESCValue) ||
                                e.PropertyName == nameof(ESCParametersViewModel.ESCStatus))
                            {
                                Device.DynotisData.ESCValue = _currentESCParameters.ESCValue;
                                Device.DynotisData.ESCStatus = _currentESCParameters.ESCStatus;
                                OnPropertyChanged(nameof(Device.DynotisData));
                            }
                        };
                    }
                }
            }
        }

        private BatterySecurityLimitsViewModel _currentBatterySecurityLimits;
        public BatterySecurityLimitsViewModel CurrentBatterySecurityLimits
        {
            get => _currentBatterySecurityLimits;
            set
            {
                if (SetProperty(ref _currentBatterySecurityLimits, value))
                {
                    if (_currentBatterySecurityLimits != null)
                    {
                        _currentBatterySecurityLimits.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == nameof(BatterySecurityLimitsViewModel.MaxCurrent) ||
                                e.PropertyName == nameof(BatterySecurityLimitsViewModel.BatteryLevel) ||
                                e.PropertyName == nameof(BatterySecurityLimitsViewModel.SecurityStatus))
                            {
                                Device.DynotisData.MaxCurrent = _currentBatterySecurityLimits.MaxCurrent;
                                Device.DynotisData.BatteryLevel = _currentBatterySecurityLimits.BatteryLevel;
                                Device.DynotisData.SecurityStatus = _currentBatterySecurityLimits.SecurityStatus;
                                OnPropertyChanged(nameof(Device.DynotisData));
                            }
                        };
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
