﻿using System;
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
using DocumentFormat.OpenXml.Drawing;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class BalancerRoutingStepsViewModel : INotifyPropertyChanged
    {
        private InterfaceVariables _interfaceVariables;
        private DynotisData _dynotisData;
        private PIDController _pidController;

        private DispatcherTimer _progressTimer;
        private DispatcherTimer _pidTimer;
        private DispatcherTimer _avgTimer;

        private double _testTimeCount;
        private double _motorReadyTimeCount;
        private bool _motorReadyStatus;
        private bool _testReadyStatus;
        private string _statusMessage;
        private string _balancerPage_RunButton;

        public ICommand RunCommand { get; }
        public ICommand ApprovalCommand { get; }
        public ICommand StopCommand { get; }

        private int _balancerIterationStep;
        private int _currentStepIndex;

        private readonly List<string> _steps;
        public List<string> Steps => _steps;

        private bool _isRunButtonEnabled;
        private bool _isApprovalButtonEnabled;

        private bool _escStatus;
        private int _escValue;

        private int smoothTransitionStep;

        private List<double> _vibrationDataBuffer;
        private List<double> _highVibrations;

        public BalancerRoutingStepsViewModel(DynotisData dynotisData, InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
            _dynotisData = dynotisData;
            ESCStatus = dynotisData.ESCStatus;
            ESCValue = dynotisData.ESCValue;

            // Initialize the PID Controller with tuned parameters (Kp, Ki, Kd)
            _pidController = new PIDController(0.2, 0.01, 0.05, integralMax: 50.0, integralMin: -50.0, alpha: 0.1);

            RunCommand = new RelayCommand(param => Run(), param => IsRunButtonEnabled);
            ApprovalCommand = new RelayCommand(param => Approval(), param => IsApprovalButtonEnabled);
            StopCommand = new RelayCommand(param => Stop());

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

            BalancerPage_RunButton = Resources.BalancerPage_RunButton1;

            _vibrationDataBuffer = new List<double>();
            _highVibrations = new List<double>();

            _currentStepIndex = 0;
            _motorReadyTimeCount = 0;
            _testTimeCount = 0;
            _isRunButtonEnabled = true;
            _isApprovalButtonEnabled = false;
            _testReadyStatus = false;
            _motorReadyStatus = false;

            smoothTransitionStep = 0;

            _progressTimer = new DispatcherTimer();
            _progressTimer.Interval = TimeSpan.FromSeconds(1);
            _progressTimer.Tick += ProgressTimer_Tick;

            _pidTimer = new DispatcherTimer();
            _pidTimer.Interval = TimeSpan.FromMilliseconds(1);
            _pidTimer.Tick += PIDTimer_Tick;

            _avgTimer = new DispatcherTimer();
            _avgTimer.Interval = TimeSpan.FromMilliseconds(1);
            _avgTimer.Tick += AVGTimer_Tick;

        }

        private void Run()
        {
            if (_interfaceVariables.ReferenceMotorSpeed <= 0)
            {
                MessageBox.Show("Lütfen değer giriniz");
            }
            else
            {
                StatusMessage = Resources.BalancerPage_StatusMessage1;
                BalancerPage_RunButton = Resources.BalancerPage_RunButton2;
                ESCStatus = true;
                ESCValue = 800; // Başlangıç değeri olarak 800 µs kullanıyoruz
                _pidController.Reset();

                IsRunButtonEnabled = false;
                IsApprovalButtonEnabled = false;
                _progressTimer.Start();
                _pidTimer.Start();
            }
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (MotorReadyStatus)
            {
                if (MotorReadyTimeCount < 100)
                {
                    StatusMessage = Resources.BalancerPage_StatusMessage2;
                    MotorReadyTimeCount += 20;
                }
                else
                {
                    if (TestTimeCount < 100)
                    {
                        StatusMessage = Resources.BalancerPage_StatusMessage3;
                        BalancerPage_RunButton = Resources.BalancerPage_RunButton3;
                        _avgTimer.Start();
                        TestTimeCount += 5;
                    }
                    else
                    {
                        StatusMessage = "";
                        MotorReadyStatus = false;
                        TestStatus = false;
                        ESCStatus = false;
                        IsApprovalButtonEnabled = true;
                        ESCValue = 800;
                        _progressTimer.Stop();
                        _pidTimer.Stop();

                        _avgTimer.Stop();
                        HighVibrations.Add(CalculateAverageOfHighVibrations(VibrationDataBuffer.ToArray()));
                        VibrationDataBuffer.Clear();
                    }
                }
            }
        }

        private void AVGTimer_Tick(object sender, EventArgs e)
        {
            VibrationDataBuffer.Add(_interfaceVariables.Vibration);
        }

        private void PIDTimer_Tick(object sender, EventArgs e)
        {
            double currentSpeed = _interfaceVariables.MotorSpeed.Value;
            double pidOutput = _pidController.Calculate(_interfaceVariables.ReferenceMotorSpeed, currentSpeed);

            // PID çıktısını 800-2200 aralığına uyarlama
            pidOutput = MapToESCValue(pidOutput);

            pidOutput = Math.Clamp(pidOutput, 800, 2200);

            ESCValue = SmoothTransition(ESCValue, (int)pidOutput);
        }

        private double MapToESCValue(double pidOutput)
        {
            // PID çıktısını 0-100 aralığından 800-2200 aralığına dönüştürme
            double minOutput = 0;
            double maxOutput = 100;
            double minESC = 800;
            double maxESC = 2200;
            return (pidOutput - minOutput) * (maxESC - minESC) / (maxOutput - minOutput) + minESC;
        }

        private int SmoothTransition(int currentValue, int targetValue)
        {
            double speedDifference = Math.Abs(_interfaceVariables.MotorSpeed.Value - _interfaceVariables.ReferenceMotorSpeed);

            if (speedDifference > 50)
            {
                if (speedDifference >= 1000) { smoothTransitionStep = 50; }
                else if ((speedDifference <= 1000) && (speedDifference > 500)) { smoothTransitionStep = 10; }
                else if ((speedDifference <= 500) && (speedDifference > 250)) { smoothTransitionStep = 3; }
                else if ((speedDifference <= 250) && (speedDifference > 100)) { smoothTransitionStep = 2; }
                else if ((speedDifference <= 100) && (speedDifference > 50)) { smoothTransitionStep = 1; }
                else { smoothTransitionStep = 0; }

                if (Math.Abs(currentValue - targetValue) <= smoothTransitionStep)
                {
                    return targetValue;
                }
                if (currentValue < targetValue)
                {
                    currentValue = Math.Min(currentValue + smoothTransitionStep, targetValue);
                }
                else if (currentValue > targetValue)
                {
                    currentValue = Math.Max(currentValue - smoothTransitionStep, targetValue);
                }

                MotorReadyStatus = false;
            }
            else
            {
                MotorReadyStatus = true;
            }
            return currentValue;
        }

        public double CalculateAverageOfHighVibrations(double[] vibrationData)
        {
            // Genel ortalamayı hesapla
            double overallAverage = vibrationData.Average();

            // Yüksek gürültü değerlerini belirle (genel ortalamanın %50'sinden fazla olan değerler)
            var highVibrations = vibrationData.Where(value => value > overallAverage * 1.1);

            // Eğer yüksek gürültü değerleri varsa ortalamasını hesapla, yoksa 0 döner
            double average = highVibrations.Any() ? highVibrations.Average() : 0;

            return average;
        }

        private void Approval()
        {
            if (CurrentStepIndex < Steps.Count - 1)
            {
                CurrentStepIndex++;
            }
            else
            {
                CurrentStepIndex = 0;
            }
            BalancerIterationStep = CurrentStepIndex;

            IsRunButtonEnabled = true;
            IsApprovalButtonEnabled = false;
            TestTimeCount = 0;
            MotorReadyTimeCount = 0;
        }

        private void Stop()
        {
            StatusMessage = "";
            MotorReadyStatus = false;
            TestStatus = false;
            ESCStatus = false;
            ESCValue = 800;
            _progressTimer.Stop();
            _pidTimer.Stop();
            _avgTimer.Stop();

            IsRunButtonEnabled = true;
            IsApprovalButtonEnabled = false;
            TestTimeCount = 0;
            MotorReadyTimeCount = 0;
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

        public double TestTimeCount
        {
            get => _testTimeCount;
            set
            {
                if (SetProperty(ref _testTimeCount, value))
                {
                    OnPropertyChanged(nameof(TestTimeCount));
                }
            }
        }
        public double MotorReadyTimeCount
        {
            get => _motorReadyTimeCount;
            set
            {
                if (SetProperty(ref _motorReadyTimeCount, value))
                {
                    OnPropertyChanged(nameof(MotorReadyTimeCount));
                }
            }
        }
        public bool TestStatus
        {
            get => _testReadyStatus;
            set
            {
                if (SetProperty(ref _testReadyStatus, value))
                {
                    OnPropertyChanged(nameof(TestStatus));
                }
            }
        }
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (SetProperty(ref _statusMessage, value))
                {
                    OnPropertyChanged(nameof(StatusMessage));
                }
            }
        }        
        public string BalancerPage_RunButton
        {
            get => _balancerPage_RunButton;
            set
            {
                if (SetProperty(ref _balancerPage_RunButton, value))
                {
                    OnPropertyChanged(nameof(BalancerPage_RunButton));
                }
            }
        }
        public bool MotorReadyStatus
        {
            get => _motorReadyStatus;
            set
            {
                if (SetProperty(ref _motorReadyStatus, value))
                {
                    OnPropertyChanged(nameof(MotorReadyStatus));
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

        public bool IsApprovalButtonEnabled
        {
            get => _isApprovalButtonEnabled;
            set
            {
                if (SetProperty(ref _isApprovalButtonEnabled, value))
                {
                    OnPropertyChanged(nameof(IsApprovalButtonEnabled));
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

        public List<double> VibrationDataBuffer
        {
            get => _vibrationDataBuffer;
            set
            {
                if (SetProperty(ref _vibrationDataBuffer, value))
                {
                    OnPropertyChanged(nameof(VibrationDataBuffer));
                }
            }
        }
        public List<double> HighVibrations
        {
            get => _highVibrations;
            set
            {
                if (SetProperty(ref _highVibrations, value))
                {
                    _interfaceVariables.HighVibrations = value;
                    OnPropertyChanged(nameof(HighVibrations));
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
