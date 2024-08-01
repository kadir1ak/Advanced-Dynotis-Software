using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.ViewModels.Managers;
using Advanced_Dynotis_Software.ViewModels.Pages;
using Advanced_Dynotis_Software.ViewModels.UserControls;

namespace Advanced_Dynotis_Software.ViewModels.Main
{
    public class DeviceViewModel : INotifyPropertyChanged, IDisposable
    {
        public InterfaceVariables DeviceInterfaceVariables { get; private set; }

        public ChartViewModel ChartViewModel { get; }

        public string DeviceDisplayName => $"{Device.Model} - {Device.SeriNo}";

        private Dynotis _device;
        private DynotisData _latestDynotisData;
        private readonly object _dataLock = new();
        private CancellationTokenSource _cancellationTokenSource;
        private int UpdateTimeMillisecond = 2; // 500 Hz (2ms)
        private int ChartUpdateTimeMillisecond = 20; // 50 Hz (20ms)

        public Dynotis Device
        {
            get => _device;
            set
            {
                if (_device != value)
                {
                    _device = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Device.DynotisData)); // Notify when the device changes
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
                                e.PropertyName == nameof(BalancedPropellersViewModel.BalancedPropellerArea) ||
                                e.PropertyName == nameof(BalancedPropellersViewModel.BalancingTestDates) ||
                                e.PropertyName == nameof(BalancedPropellersViewModel.VibrationLevels))
                            {
                                DeviceInterfaceVariables.BalancedPropellersID = _currentBalancedPropellers.BalancedPropellerID;
                                DeviceInterfaceVariables.BalancedPropellersArea = _currentBalancedPropellers.BalancedPropellerArea;
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
                                e.PropertyName == nameof(BalancedPropellersViewModel.BalancedPropellerArea) ||
                                e.PropertyName == nameof(BalancedPropellersViewModel.BalancingTestDates) ||
                                e.PropertyName == nameof(BalancedPropellersViewModel.VibrationLevels))
                            {
                                DeviceInterfaceVariables.BalancedPropellersID = _currentBalancedPropellers.BalancedPropellerID;
                                DeviceInterfaceVariables.BalancedPropellersArea = _currentBalancedPropellers.BalancedPropellerArea;
                                DeviceInterfaceVariables.BalancedPropellersTestDates = _currentBalancedPropellers.BalancingTestDates;
                                DeviceInterfaceVariables.BalancedPropellersVibrations = _currentBalancedPropellers.VibrationLevels;
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
                            if (e.PropertyName == nameof(BalancerRoutingStepsVibrationLevelsViewModel.BalancerIterationVibrations) ||
                                e.PropertyName == nameof(BalancerRoutingStepsVibrationLevelsViewModel.BalancerIterationStep))
                            {
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
                                e.PropertyName == nameof(BalancerRoutingStepsViewModel.ESCValue) ||
                                e.PropertyName == nameof(BalancerRoutingStepsViewModel.ESCStatus) ||
                                e.PropertyName == nameof(BalancerRoutingStepsViewModel.TestStepsPropellerVibrations))
                            {
                                DeviceInterfaceVariables.BalancerIterationStep = _currentBalancerRoutingSteps.BalancerIterationStep;
                                DeviceInterfaceVariables.BalancerIterationVibrations = _currentBalancerRoutingSteps.TestStepsPropellerVibrations;
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
                                e.PropertyName == nameof(BalancerParametersViewModel.ReferenceWeight) ||
                                e.PropertyName == nameof(BalancerParametersViewModel.BalancerIterationStep) ||
                                e.PropertyName == nameof(BalancerParametersViewModel.BalancerIterationVibrations))
                            {
                                DeviceInterfaceVariables.ReferenceMotorSpeed = _currentBalancerParameters.ReferenceMotorSpeed;
                                DeviceInterfaceVariables.ReferenceWeight = _currentBalancerParameters.ReferenceWeight;
                                DeviceInterfaceVariables.BalancerIterationStep = _currentBalancerParameters.BalancerIterationStep;
                                DeviceInterfaceVariables.BalancerIterationVibrations = _currentBalancerParameters.BalancerIterationVibrations;
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
                            if (e.PropertyName == nameof(EquipmentParametersViewModel.UserPropellerArea) ||
                                e.PropertyName == nameof(EquipmentParametersViewModel.UserMotorInner) ||
                                e.PropertyName == nameof(EquipmentParametersViewModel.UserNoLoadCurrents))
                            {
                                Device.DynotisData.PropellerArea = _currentEquipmentParameters.UserPropellerArea;
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

        public DeviceViewModel(string portName)
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                DeviceInterfaceVariables = new InterfaceVariables();
                Device = new Dynotis(portName);
                ChartViewModel = new ChartViewModel();
                _cancellationTokenSource = new CancellationTokenSource();

                Device.PropertyChanged += Device_PropertyChanged;

                InitializeDeviceAsync();
                Task.Run(() => UpdateDataLoop(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
                Task.Run(() => UpdateChartLoop(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
            }
        }

        private async void InitializeDeviceAsync()
        {
            await Device.OpenPortAsync();
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
                        
                        DeviceInterfaceVariables.VibrationBuffer.Add(DeviceInterfaceVariables.Vibration);
                        if (DeviceInterfaceVariables.VibrationBuffer.Count > 100) 
                        {
                            DeviceInterfaceVariables.HighVibrationAVG = CalculateHighVibrations(DeviceInterfaceVariables.VibrationBuffer);
                            DeviceInterfaceVariables.VibrationBuffer.Clear();
                        }                        

                        DeviceInterfaceVariables.UpdateFrom(latestData);
                    });
                }
            }
        }

        private double CalculateHighVibrations(List<double> buffer)
        {
            // Buffer içerisindeki en yüksek 10 verinin ortalamasını threshold olarak belirle
            double threshold = CalculateThreshold(buffer);

            double highVibrations = 0;
            int highVibrationCount = 0;

            // Yüksek titreşim değerlerinin ortalamasını hesapla
            foreach (double value in buffer)
            {
                if (value > threshold)
                {
                    highVibrations += value;
                    highVibrationCount++;
                }
            }

            // Eğer yüksek titreşim bulunamazsa, ortalama 0 olacak
            if (highVibrationCount > 0)
            {
                highVibrations /= highVibrationCount;
            }

            return highVibrations;
        }
        private double CalculateThreshold(List<double> buffer)
        {
            // En yüksek 10 değeri bul
            var topValues = buffer.OrderByDescending(x => x).Take(10);
            // En yüksek 10 değerin ortalamasını hesapla
            double threshold = topValues.Average();
            return threshold;
        }

        private async Task UpdateChartLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(ChartUpdateTimeMillisecond, token);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ChartViewModel.UpdateChartData(DeviceInterfaceVariables);
                });
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
