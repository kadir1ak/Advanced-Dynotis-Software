using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.Services.Controllers;
using Advanced_Dynotis_Software.Services.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class BalancingRoutingStepsViewModel : INotifyPropertyChanged
    {
        private InterfaceVariables _interfaceVariables;
        private DynotisData _dynotisData;
        private PIDController _pidController;

        private int _balancingIterationStep;
        private int _currentStepIndex;
        private readonly List<string> _steps;

        private DispatcherTimer _progressTimer;
        private DispatcherTimer _pidTimer;
        private double _progressValue;

        private bool _isRunButtonEnabled;
        private bool _isSaveButtonEnabled;

        private bool _escStatus;
        private double _escValue;

        public BalancingRoutingStepsViewModel(DynotisData dynotisData, InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
            _dynotisData = dynotisData;
            ESCStatus = dynotisData.ESCStatus;
            ESCValue = dynotisData.ESCValue;

            // Initialize the PID Controller with tuned parameters (Kp, Ki, Kd)
            //_pidController = new PIDController(0.3, 0.01, 0.01); // Kp değerini biraz artırabiliriz

            // Daha hassas PID parametreleri
            _pidController = new PIDController(0.5, 0.01, 0.01, integralMax: 200.0, integralMin: -200.0, alpha: 0.01);


            RunCommand = new RelayCommand(param => Run(), param => IsRunButtonEnabled);
            SaveCommand = new RelayCommand(param => Save(), param => IsSaveButtonEnabled);

            _steps = new List<string>
            {
                "Cihazın Hazırlanması ve Başlangıç Titreşiminin Ölçülmesi",
                "Pervane Titreşim Ölçümü (Ağırlıksız)",
                "Balans Yönünün Belirlenmesi (0 Derece Pozisyonu)",
                "Balans Yönünün Belirlenmesi (180 Derece Pozisyonu)",
                "Balanslama için Değerlerin Hesaplanması",
                "Test ve Değerlendirme",
                "Balanslama İyileşme Oranının Hesaplanması"
            };

            _currentStepIndex = 0;
            _progressValue = 0;
            _isRunButtonEnabled = true;
            _isSaveButtonEnabled = false;

            _progressTimer = new DispatcherTimer();
            _progressTimer.Interval = TimeSpan.FromSeconds(10); // Adjusted for finer control
            _progressTimer.Tick += ProgressTimer_Tick;

            _pidTimer = new DispatcherTimer();
            _pidTimer.Interval = TimeSpan.FromMilliseconds(100); // Adjust the interval as needed
            _pidTimer.Tick += PIDTimer_Tick;
        }

        public ICommand RunCommand { get; }
        public ICommand SaveCommand { get; }

        public List<string> Steps => _steps;

        public int CurrentStepIndex
        {
            get => _currentStepIndex;
            set
            {
                if (SetProperty(ref _currentStepIndex, value))
                {
                    OnPropertyChanged(nameof(CurrentStepIndex));
                }
            }
        }

        public int BalancingIterationStep
        {
            get => _balancingIterationStep;
            set
            {
                if (SetProperty(ref _balancingIterationStep, value))
                {
                    _interfaceVariables.BalancingIterationStep = value;
                    OnPropertyChanged(nameof(BalancingIterationStep));
                }
            }
        }

        public double ProgressValue
        {
            get => _progressValue;
            set
            {
                if (SetProperty(ref _progressValue, value))
                {
                    OnPropertyChanged(nameof(ProgressValue));
                }
            }
        }

        public bool IsRunButtonEnabled
        {
            get => _isRunButtonEnabled;
            set
            {
                if (SetProperty(ref _isRunButtonEnabled, value))
                {
                    OnPropertyChanged(nameof(IsRunButtonEnabled));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool IsSaveButtonEnabled
        {
            get => _isSaveButtonEnabled;
            set
            {
                if (SetProperty(ref _isSaveButtonEnabled, value))
                {
                    OnPropertyChanged(nameof(IsSaveButtonEnabled));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool ESCStatus
        {
            get => _escStatus;
            set
            {
                if (SetProperty(ref _escStatus, value))
                {
                    _dynotisData.ESCStatus = value;
                    OnPropertyChanged(nameof(ESCStatus));
                }
            }
        }

        public double ESCValue
        {
            get => _escValue;
            set
            {
                if (SetProperty(ref _escValue, value))
                {
                    _dynotisData.ESCValue = value;
                    OnPropertyChanged(nameof(ESCValue));
                }
            }
        }

        private void Run()
        {
            if (_interfaceVariables.ReferenceMotorSpeed <= 0)
            {
                MessageBox.Show("Lütfen değer giriniz");
            }
            else
            {
                ESCStatus = true;
                ESCValue = 10;
                _pidController.Reset();

                IsRunButtonEnabled = false;
                IsSaveButtonEnabled = false;
                _progressTimer.Start();
                _pidTimer.Start();
            }
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (ProgressValue < 100)
            {
                ProgressValue += 5;
            }
            else
            {
                ESCStatus = false;
                ESCValue = 0;

                _progressTimer.Stop();
                _pidTimer.Stop();
                IsSaveButtonEnabled = true;
            }
        }

        private void PIDTimer_Tick(object sender, EventArgs e)
        {
            double currentSpeed = _interfaceVariables.MotorSpeed.Value;
            double escValue = _pidController.Calculate(_interfaceVariables.ReferenceMotorSpeed, currentSpeed);

            // Clamp the PID output to prevent sudden jumps
            escValue = Math.Clamp(escValue, 0, 100);

            // Smooth the transition by slowly adjusting the ESC value
            ESCValue = SmoothTransition(ESCValue, escValue);
        }

        private double SmoothTransition(double currentValue, double targetValue)
        {
            double step = 0.01; // Smaller step value for higher sensitivity
            if (_interfaceVariables.MotorSpeed.Value <= 0)
            {
                step = 0.1;

            }
            else
            {
                step = 0.01;
            }

            if (Math.Abs(currentValue - targetValue) < step)
            {
                return targetValue; // Close enough to the target, set directly
            }
            if (currentValue < targetValue)
            {
                currentValue = Math.Min(currentValue + step, targetValue);
            }
            else if (currentValue > targetValue)
            {
                currentValue = Math.Max(currentValue - step, targetValue);
            }
            return currentValue;
        }

        private void Save()
        {
            MessageBox.Show("Save");
            IsRunButtonEnabled = true;
            IsSaveButtonEnabled = false;
            ProgressValue = 0;
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
}

