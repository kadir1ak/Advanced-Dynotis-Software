using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.Services.Helpers;
using Advanced_Dynotis_Software.Services.Logger;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class BalancerParametersViewModel : INotifyPropertyChanged
    {
        private int _referenceMotorSpeed;
        private double _referencePropellerDiameter;
        private double _unitTapeSize;
        private double _equalizerTapeSize;
        private string _equalizerDirection;
        private string _motorSpeedUnitSymbol;

        private int _balancerIterationStep;
        private ObservableCollection<int> _balancerIterationStepChart;
        private ObservableCollection<double> _balancerIterationVibrationsChart;
        private InterfaceVariables _interfaceVariables;
        private DynotisData _dynotisData;

        private double _tareVibration;
        private double _tareVibrationX;
        private double _tareVibrationY;
        private double _tareVibrationZ;   

        private ObservableCollection<BalancerIteration> _balancingIterations;
        public ICommand TareCommand { get; }
        public BalancerParametersViewModel(DynotisData dynotisData, InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
            _dynotisData = dynotisData;
            // Subscribe to the PropertyChanged event of InterfaceVariables
            _interfaceVariables.PropertyChanged += InterfaceVariables_PropertyChanged;

            TareCommand = new RelayCommand(param => VibrationTare());

            _balancerIterationStepChart = new ObservableCollection<int>();
            _balancerIterationVibrationsChart = new ObservableCollection<double>();
            _balancingIterations = new ObservableCollection<BalancerIteration>();
        }

        private void InterfaceVariables_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_interfaceVariables.BalancerIterationStep) ||
                e.PropertyName == nameof(_interfaceVariables.BalancerIterationStepChart) ||
                e.PropertyName == nameof(_interfaceVariables.ReferencePropellerDiameter) ||
                e.PropertyName == nameof(_interfaceVariables.BalancerIterationVibrationsChart))
            {
                BalancerIterationStep = _interfaceVariables.BalancerIterationStep;
                BalancerIterationStepChart = _interfaceVariables.BalancerIterationStepChart;
                BalancerIterationVibrationsChart = _interfaceVariables.BalancerIterationVibrationsChart;
                MotorSpeedUnitSymbol = _interfaceVariables.MotorSpeed.UnitSymbol;
                ReferencePropellerDiameter = _interfaceVariables.ReferencePropellerDiameter;
                UpdateBalancingIterations();
            }
        }
        private void VibrationTare()
        {
            if (_interfaceVariables != null && _dynotisData != null)
            {
                TareVibration = _interfaceVariables.Vibration.TareCurrentVibration;
                TareVibrationX = _interfaceVariables.Vibration.TareCurrentVibrationX;
                TareVibrationY = _interfaceVariables.Vibration.TareCurrentVibrationY;
                TareVibrationZ = _interfaceVariables.Vibration.TareCurrentVibrationZ;
            }
        }

        public double TareVibration
        {
            get => _tareVibration;
            set
            {
                if (SetProperty(ref _tareVibration, value))
                {
                    _interfaceVariables.Vibration.TareVibration = value;
                    OnPropertyChanged(nameof(TareVibration));
                }
            }
        } 
        public double TareVibrationX
        {
            get => _tareVibrationX;
            set
            {
                if (SetProperty(ref _tareVibrationX, value))
                {
                    _interfaceVariables.Vibration.TareVibrationX = value;
                    OnPropertyChanged(nameof(TareVibrationX));
                }
            }
        } 
        public double TareVibrationY
        {
            get => _tareVibrationY;
            set
            {
                if (SetProperty(ref _tareVibrationY, value))
                {
                    _interfaceVariables.Vibration.TareVibrationY = value;
                    OnPropertyChanged(nameof(TareVibrationY));
                }
            }
        } 
        public double TareVibrationZ
        {
            get => _tareVibrationZ;
            set
            {
                if (SetProperty(ref _tareVibrationZ, value))
                {
                    _interfaceVariables.Vibration.TareVibrationZ = value;
                    OnPropertyChanged(nameof(TareVibrationZ));
                }
            }
        }    
        public int ReferenceMotorSpeed
        {
            get => _referenceMotorSpeed;
            set
            {
                if (SetProperty(ref _referenceMotorSpeed, value))
                {
                    _interfaceVariables.ReferenceMotorSpeed = value;
                    OnPropertyChanged(nameof(ReferenceMotorSpeed));
                }
            }
        }
        public double ReferencePropellerDiameter
        {
            get => _referencePropellerDiameter;
            set
            {
                if (SetProperty(ref _referencePropellerDiameter, value))
                {
                    _interfaceVariables.ReferencePropellerDiameter = value;
                    OnPropertyChanged(nameof(ReferencePropellerDiameter));
                }
            }
        }
        public double UnitTapeSize
        {
            get => _unitTapeSize;
            set
            {
                if (SetProperty(ref _unitTapeSize, value))
                {
                    _interfaceVariables.UnitTapeSize = value;
                    OnPropertyChanged(nameof(UnitTapeSize));
                }
            }
        }
        public double EqualizerTapeSize
        {
            get => _equalizerTapeSize;
            set
            {
                if (SetProperty(ref _equalizerTapeSize, value))
                {
                    _interfaceVariables.EqualizerTapeSize = value;
                    OnPropertyChanged(nameof(EqualizerTapeSize));
                }
            }
        }
        public string EqualizerDirection
        {
            get => _equalizerDirection;
            set
            {
                if (SetProperty(ref _equalizerDirection, value))
                {
                    _interfaceVariables.EqualizerDirection = value;
                    OnPropertyChanged(nameof(EqualizerDirection));
                }
            }
        }
        public string MotorSpeedUnitSymbol
        {
            get => _motorSpeedUnitSymbol;
            set
            {
                if (SetProperty(ref _motorSpeedUnitSymbol, value))
                {
                    OnPropertyChanged(nameof(MotorSpeedUnitSymbol));
                }
            }
        }
        public int BalancerIterationStep
        {
            get => _balancerIterationStep;
            set
            {
                if (SetProperty(ref _balancerIterationStep, value))
                {
                    _interfaceVariables.BalancerIterationStep = value;
                    OnPropertyChanged(nameof(BalancerIterationStep));
                }
            }
        }

        public ObservableCollection<int> BalancerIterationStepChart
        {
            get => _balancerIterationStepChart;
            set
            {
                if (SetProperty(ref _balancerIterationStepChart, value))
                {
                    _interfaceVariables.BalancerIterationStepChart = value;
                    OnPropertyChanged(nameof(BalancerIterationStepChart));
                    UpdateBalancingIterations();
                }
            }
        }

        public ObservableCollection<double> BalancerIterationVibrationsChart
        {
            get => _balancerIterationVibrationsChart;
            set
            {
                if (SetProperty(ref _balancerIterationVibrationsChart, value))
                {
                    _interfaceVariables.BalancerIterationVibrationsChart = value;
                    OnPropertyChanged(nameof(BalancerIterationVibrationsChart));
                    UpdateBalancingIterations();
                }
            }
        }
        public ObservableCollection<BalancerIteration> BalancingIterations
        {
            get => _balancingIterations;
            set
            {
                if (SetProperty(ref _balancingIterations, value))
                {
                    OnPropertyChanged(nameof(BalancingIterations));
                }
            }
        }

        private void UpdateBalancingIterations()
        {
            // Clear the BalancingIterations collection to prevent data stacking
            BalancingIterations.Clear();

            // Iterate through the BalancerIterationStepChart and BalancerIterationVibrationsChart collections
            for (int i = 0; i < BalancerIterationStepChart.Count && i < BalancerIterationVibrationsChart.Count; i++)
            {
                // Add each step and corresponding vibration to the BalancingIterations collection
                BalancingIterations.Add(new BalancerIteration
                {
                    IterationStep = BalancerIterationStepChart[i],
                    Vibrations = Math.Round(BalancerIterationVibrationsChart[i], 3)
                });
            }

            // Notify that the BalancingIterations collection has been updated
            OnPropertyChanged(nameof(BalancingIterations));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BalancerIteration
    {
        public int IterationStep { get; set; }
        public double Vibrations { get; set; }
    }
}
