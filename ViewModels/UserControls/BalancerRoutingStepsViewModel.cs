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
using DocumentFormat.OpenXml.Drawing;
using System.Collections.ObjectModel;
using DocumentFormat.OpenXml.Office2016.Excel;

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

        private ObservableCollection<int> _balancerIterationStepChart;
        private ObservableCollection<double> _balancerIterationVibrationsChart;

        private double _highVibration;
        private double _testTimeCount;
        private double _motorReadyTimeCount;
        private bool _motorReadyStatus;
        private bool _testReadyStatus;
        private string _statusMessage;
        private string _testResult;
        private string _balancerPage_RunButton;

        public ICommand RunCommand { get; }
        public ICommand ApprovalCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand NewTestCommand { get; }

        private int _balancerIterationStep;
        private int _currentStepIndex;

        private readonly List<string> _steps;
        public List<string> Steps => _steps;

        public string IterationHeader {  get; set; }
        public string Iteration {  get; set; }

        private bool _isRunButtonEnabled;
        private bool _isApprovalButtonEnabled;

        private bool _escStatus;
        private int _escValue;

        private int smoothTransitionStep;

        private List<double> _testVibrationsDataBuffer;
        private List<double> _testStepsPropellerVibrations;

        public BalancerRoutingStepsViewModel(DynotisData dynotisData, InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
            _dynotisData = dynotisData;
            // Subscribe to the PropertyChanged event of InterfaceVariables
            _interfaceVariables.PropertyChanged += InterfaceVariables_PropertyChanged;

            ESCStatus = dynotisData.ESCStatus;
            ESCValue = dynotisData.ESCValue;

            // Initialize the PID Controller with tuned parameters (Kp, Ki, Kd)
            //_pidController = new PIDController(0.2, 0.01, 0.05);
            _pidController = new PIDController(1.5, 0.03, 0.05);

            RunCommand = new RelayCommand(param => Run(), param => IsRunButtonEnabled);
            ApprovalCommand = new RelayCommand(param => Approval(), param => IsApprovalButtonEnabled);
            StopCommand = new RelayCommand(param => Stop());
            NewTestCommand = new RelayCommand(param => NewTest());

            IterationHeader = "Cihazın Hazırlanması";
            Iteration = "\r\n" + "1.) Cihazın uygun şekilde sabitleyiniz. " +
                        "\r\n" + "Çevresel dengesizlik veya belirsizliğe sebep olabilecek koşullardan arındırdığınızdan emin olunuz. " +
                        "\r\n" + "2.) Motor montajını yapınız. " +
                        "\r\n" + "3.) Elektronik bağlantıları kontrol ediniz. ";

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
            TestResult = " ";

            _testVibrationsDataBuffer = new List<double>();
            _testStepsPropellerVibrations = new List<double>();

            _balancerIterationStepChart = new ObservableCollection<int>();
            _balancerIterationVibrationsChart = new ObservableCollection<double>();

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
            _pidTimer.Interval = TimeSpan.FromMilliseconds(50);
            _pidTimer.Tick += PIDTimer_Tick;

            _avgTimer = new DispatcherTimer();
            _avgTimer.Interval = TimeSpan.FromMilliseconds(1);
            _avgTimer.Tick += AVGTimer_Tick;

        }

        private void InterfaceVariables_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_interfaceVariables.Vibration.HighVibration))
            {
                HighVibration = _interfaceVariables.Vibration.HighVibration;
            }
        }

        private void Run()
        {
            if (_interfaceVariables.ReferenceMotorSpeed <= 0)
            {
                MessageBoxResult result = MessageBox.Show(
                "Please enter the reference motor speed value!",
                "Missing Value Warning",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            }
            else
            {
                TestResult = " ";
                TestTimeCount = 0;
                MotorReadyTimeCount = 0;

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
                    MotorReadyTimeCount += 10;
                }
                else
                {
                    if (TestTimeCount < 100)
                    {
                        StatusMessage = Resources.BalancerPage_StatusMessage3;
                        if (_avgTimer.IsEnabled == false)
                        {
                            _avgTimer.Start();
                        }                     
                        TestTimeCount += 20;
                    }
                    else
                    {
                        BalancerPage_RunButton = Resources.BalancerPage_RunButton3;
                        StatusMessage = "";

                        IsApprovalButtonEnabled = true;
                        IsRunButtonEnabled = true;                       
                        MotorReadyStatus = false;
                        TestStatus = false;
                        ESCStatus = false;                        
                        ESCValue = 800;

                        _avgTimer.Stop();
                        _progressTimer.Stop();
                        _pidTimer.Stop();
                       

                        TestStepsPropellerVibrations.Add(TestVibrationsDataBuffer.Average());
                        TestResult = "Test Result: " + TestVibrationsDataBuffer.Average().ToString("0.000") + " g";
                    
                    }
                }
            }
        }

        private void AVGTimer_Tick(object sender, EventArgs e)
        {
            TestVibrationsDataBuffer.Add(HighVibration); // - DareVibration);
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

            if (speedDifference > 10)
            {
                
                if (speedDifference >= 400)                                     { smoothTransitionStep = 5; }
                else if ((speedDifference <= 400) && (speedDifference > 300))   { smoothTransitionStep = 4; }
                else if ((speedDifference <= 300) && (speedDifference > 200))   { smoothTransitionStep = 3; }
                else if ((speedDifference <= 200) && (speedDifference > 100))   { smoothTransitionStep = 2; }
                else if ((speedDifference <= 100) && (speedDifference > 50))    { smoothTransitionStep = 1; }
                else if ((speedDifference <= 50) && (speedDifference > 10))     { smoothTransitionStep = 0; }
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
                if (speedDifference < 80)
                {
                    MotorReadyStatus = true;
                }
            }
            else
            {
                MotorReadyStatus = true;
            }
            return currentValue;
        }

        private void Approval()
        {
            BalancerPage_RunButton = Resources.BalancerPage_RunButton1;
            if (CurrentStepIndex < Steps.Count - 1)
            {
                CurrentStepIndex++;
            }
            else
            {
                CurrentStepIndex = 0;
            }
            BalancerIterationVibrationsChart.Add(TestVibrationsDataBuffer.Average());
            BalancerIterationStepChart.Add(BalancerIterationVibrationsChart.Count);
            TestVibrationsDataBuffer.Clear();

            BalancerIterationStep = CurrentStepIndex;

            TestResult = " ";
            IsRunButtonEnabled = true;
            IsApprovalButtonEnabled = false;
            TestTimeCount = 0;
            MotorReadyTimeCount = 0;
        }

        private void Stop()
        {
            BalancerPage_RunButton = Resources.BalancerPage_RunButton1;
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

        private void NewTest()
        {
            MessageBoxResult result = MessageBox.Show(
               "Test data will be erased. Are you sure you want to start a new test?",
               "Confirm New Test",
               MessageBoxButton.YesNo,
               MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                BalancerPage_RunButton = Resources.BalancerPage_RunButton1;
                StatusMessage = "";
                TestResult = " ";
                MotorReadyStatus = false;
                TestStatus = false;
                ESCStatus = false;
                ESCValue = 800;
                CurrentStepIndex = 0;
                _progressTimer.Stop();
                _pidTimer.Stop();
                _avgTimer.Stop();

                IsRunButtonEnabled = true;
                IsApprovalButtonEnabled = false;
                TestTimeCount = 0;
                MotorReadyTimeCount = 0;
                TestVibrationsDataBuffer.Clear();
                BalancerIterationVibrationsChart.Clear();
                BalancerIterationStepChart.Clear();
                BalancerIterationStep = 0;
            }
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
        public string TestResult
        {
            get => _testResult;
            set
            {
                if (SetProperty(ref _testResult, value))
                {
                    OnPropertyChanged(nameof(TestResult));
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
        public double HighVibration
        {
            get => _highVibration;
            set
            {
                if (_highVibration != value)
                {
                    _highVibration = value;
                    OnPropertyChanged(nameof(HighVibration));
                }
            }
        }
        public List<double> TestVibrationsDataBuffer
        {
            get => _testVibrationsDataBuffer;
            set
            {
                if (_testVibrationsDataBuffer != value)
                {
                    _testVibrationsDataBuffer = value;
                    OnPropertyChanged(nameof(TestVibrationsDataBuffer)); ;
                }
            }
        }
        public List<double> TestStepsPropellerVibrations
        {
            get => _testStepsPropellerVibrations;
            set
            {
                if (SetProperty(ref _testStepsPropellerVibrations, value))
                {
                    _interfaceVariables.BalancerIterationVibrations = value;
                    OnPropertyChanged(nameof(TestStepsPropellerVibrations));
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
