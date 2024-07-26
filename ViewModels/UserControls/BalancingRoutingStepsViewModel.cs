using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Advanced_Dynotis_Software.Properties;
using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.Services.Controllers;
using Advanced_Dynotis_Software.Services.Helpers;
using Advanced_Dynotis_Software.Services.Logger;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class BalancingRoutingStepsViewModel : INotifyPropertyChanged
    {
        private InterfaceVariables _interfaceVariables;
        private DynotisData _dynotisData;
        private PIDController _pidController;

        private DispatcherTimer _progressTimer;
        private DispatcherTimer _pidTimer;
        private double _progressValue;
        private bool _progressStatus;

        public ICommand RunCommand { get; }
        public ICommand SaveCommand { get; }

        private int _balancingIterationStep;
        private int _currentStepIndex;

        private readonly List<string> _steps;
        public List<string> Steps => _steps;

        private bool _isRunButtonEnabled;
        private bool _isSaveButtonEnabled;

        private bool _escStatus;
        private int _escValue;

        public BalancingRoutingStepsViewModel(DynotisData dynotisData, InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
            _dynotisData = dynotisData;
            ESCStatus = dynotisData.ESCStatus;
            ESCValue = dynotisData.ESCValue;

            // Initialize the PID Controller with tuned parameters (Kp, Ki, Kd)
            _pidController = new PIDController(0.2, 0.01, 0.05, integralMax: 500.0, integralMin: -500.0, alpha: 0.1);


            RunCommand = new RelayCommand(param => Run(), param => IsRunButtonEnabled);
            SaveCommand = new RelayCommand(param => Save(), param => IsSaveButtonEnabled);

            _steps = new List<string>
            {
                Resources.BalancerPage_Group1,
                Resources.BalancerPage_Group2,
                Resources.BalancerPage_Group3,
                Resources.BalancerPage_Group4,
                Resources.BalancerPage_Group5,
                Resources.BalancerPage_Group6,
                Resources.BalancerPage_Group7,
            };

            _currentStepIndex = 0;
            _progressValue = 0;
            _isRunButtonEnabled = true;
            _isSaveButtonEnabled = false;
            _progressStatus = false;

            _progressTimer = new DispatcherTimer();
            _progressTimer.Interval = TimeSpan.FromSeconds(1); // Adjusted for finer control
            _progressTimer.Tick += ProgressTimer_Tick;

            _pidTimer = new DispatcherTimer();
            _pidTimer.Interval = TimeSpan.FromSeconds(1); // Adjust the interval as needed
            _pidTimer.Tick += PIDTimer_Tick;
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
            if(ProgressStatus)
            {            
                if (ProgressValue < 100)
                {
                    ProgressValue += 5;
                }
                else
                {
                    ProgressStatus = false;
                    ESCStatus = false;
                    ESCValue = 0;
                    _progressTimer.Stop();
                    _pidTimer.Stop();
                    IsSaveButtonEnabled = true;
                }
            }
        }

        private void PIDTimer_Tick(object sender, EventArgs e)
        {
            double currentSpeed = _interfaceVariables.MotorSpeed.Value;
            double pidOutput = _pidController.Calculate(_interfaceVariables.ReferenceMotorSpeed, currentSpeed);

            pidOutput = Math.Clamp(pidOutput, 0, 100);

            ESCValue = SmoothTransition(ESCValue, (int)pidOutput);
        }

        private int SmoothTransition(int currentValue, int targetValue)
        {
            if (Math.Abs(_interfaceVariables.MotorSpeed.Value - _interfaceVariables.ReferenceMotorSpeed) > 500)
            {
                int step = 1; 
                if (Math.Abs(currentValue - targetValue) <= step)
                {
                    return targetValue; 
                }
                if (currentValue < targetValue)
                {
                    currentValue = Math.Min(currentValue + step, targetValue);
                }
                else if (currentValue > targetValue)
                {
                    currentValue = Math.Max(currentValue - step, targetValue);
                }
                ProgressStatus = false;
            }
            else
            {
                ProgressStatus = true;
            }
            return currentValue;
        }


        private void Save()
        {         
            if (CurrentStepIndex < Steps.Count-1)
            {
                CurrentStepIndex++;
            }
            else
            {
                CurrentStepIndex = 0;
            }
            BalancingIterationStep = CurrentStepIndex;

            IsRunButtonEnabled = true;
            IsSaveButtonEnabled = false;
            ProgressValue = 0;
        }

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
        public bool ProgressStatus
        {
            get => _progressStatus;
            set
            {
                if (SetProperty(ref _progressStatus, value))
                {
                    OnPropertyChanged(nameof(ProgressStatus));
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

        public int ESCValue
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

