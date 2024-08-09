using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.Services.Controllers;
using Advanced_Dynotis_Software.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class BalancerRoutingStepsViewModel : INotifyPropertyChanged
    {
        private InterfaceVariables _interfaceVariables;
        private DynotisData _dynotisData;

        // Counts
        private double _testTimeCount;
        private double _motorReadyTimeCount;

        // Status 
        private bool _motorReadyStatus;
        private bool _testReadyStatus;

        // Steps Buttons
        public ICommand RepeatStepButtonCommand { get; }
        public ICommand ApprovalStepButtonCommand { get; }
        public ICommand NextStepButtonCommand { get; }

        private Visibility _repeatStepButtonVisibility;
        private Visibility _approvalStepButtonVisibility;
        private Visibility _nextStepButtonVisibility;

        // Main Buttons
        public ICommand RunButtonCommand { get; }
        public ICommand StopButtonCommand { get; }
        public ICommand NewBalanceTestButtonCommand { get; }

        private bool _runButtonIsEnabled;

        //StatusBar
        private double _testTimeStatusBar;

        private string _statusMessage;
        private string _testResult;

        //Iteration
        private string _iterationHeader;
        private string _iteration;
        private int _balancerIterationStep;
        private int _currentStepIndex;
        public List<string> IterationHeaders { get; set; }
        public List<string> Iterations { get; set; }

        //ESC
        private bool _escStatus;
        private int _escValue;

        //PID
        private int smoothTransitionStep;
        private PIDController _pidController;
        private DispatcherTimer _pidTimer;

        //Progress Bar Time
        private DispatcherTimer _progressTimer;

        //AVG
        private DispatcherTimer _avgTimer;
        private double _highVibration;
        private List<double> _testVibrationsDataBuffer;
        private List<double> _testStepsPropellerVibrations;

        //StepChart
        private ObservableCollection<int> _balancerIterationStepChart;
        private ObservableCollection<double> _balancerIterationVibrationsChart;

        // Step Indicators
        private ObservableCollection<Brush> _stepIndicators;

        // Constructor
        public BalancerRoutingStepsViewModel(DynotisData dynotisData, InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
            _dynotisData = dynotisData;

            _interfaceVariables.PropertyChanged += InterfaceVariables_PropertyChanged;

            TestVibrationsDataBuffer = new List<double>();
            TestStepsPropellerVibrations = new List<double>();
            BalancerIterationStepChart = new ObservableCollection<int>();
            BalancerIterationVibrationsChart = new ObservableCollection<double>();


            // Initialize PID Controller
            _pidController = new PIDController(1.5, 0.03, 0.05);

            RunButtonCommand = new RelayCommand(param => RunCommand());
            StopButtonCommand = new RelayCommand(param => StopCommand());
            NewBalanceTestButtonCommand = new RelayCommand(param => NewTestCommand());

            RepeatStepButtonCommand = new RelayCommand(param => RepeatStepCommand());
            ApprovalStepButtonCommand = new RelayCommand(param => ApprovalCommand());
            NextStepButtonCommand = new RelayCommand(param => NextStepCommand());

            IterationHeaders = new List<string>
            {
                "Welcome Balancer Test",
                "Cihazın Hazırlanması",
                "Cihaz Bağlantısının ve Arayüz Parametrelerinin Ayarlanması",
                "Ortam Titreşimlerinin Hesaplanması ve Filtrelenmesi\r\n(Dara İşlemi)",
                "Motor Titreşimin Hesaplanması",
                "Pervane Montajı",
                "Pervane Titreşimin Hesaplanması",
                "Birim Referans Düzeltici Ağırlık Değerinin Pervane Boyutuna Göre Hesaplanması",
                "Pervanenin Her İki Kanadına Birim Referans Düzeltici Ağırlığın Eklenmesi",
                "Düzeltici Ağırlık Değerinin ve Düzeltici Yönün Hesaplanması",
                "Tayin Edilen Yöne Düzeltici Ağırlığın Eklenmesi"
            };

            StepIndicators = new ObservableCollection<Brush>();
            for (int i = 0; i < IterationHeaders.Count; i++)
            {
                StepIndicators.Add((SolidColorBrush)Application.Current.Resources["BalancerRoutingStepsPassive"]);
            }

            _progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _progressTimer.Tick += ProgressTimer_Tick;

            _pidTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            _pidTimer.Tick += PIDTimer_Tick;

            _avgTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1)
            };
            _avgTimer.Tick += AVGTimer_Tick;

            BalanceTestInitialConfig();
        }

        private void BalanceTestInitialConfig()
        {
            // 1. Step
            IterationHeader = IterationHeaders[0];
            Iteration = "";

            // Counts Zero
            TestTimeCount = 0;
            MotorReadyTimeCount = 0;
            TestTimeStatusBar = 0;

            BalancerIterationStep = 0;
            CurrentStepIndex = 0;

            ESCValue = 0;

            // Clear Buffer
            TestVibrationsDataBuffer.Clear();
            TestStepsPropellerVibrations.Clear();
            BalancerIterationStepChart.Clear();
            BalancerIterationVibrationsChart.Clear();

            // Buttons Visibility
            RepeatStepButtonVisibility = Visibility.Hidden;
            ApprovalStepButtonVisibility = Visibility.Hidden;
            NextStepButtonVisibility = Visibility.Hidden;

            // Status False
            MotorReadyStatus = false;
            TestReadyStatus = false;
            ESCStatus = false;

            // Status True
            RunButtonIsEnabled = true;

            // Output Message Clear
            StatusMessage = "";
            TestResult = "";

            // Timers Stop
            _progressTimer.Stop();
            _pidTimer.Stop();
            _avgTimer.Stop();

            // Indicators Clear
            for (int i = 0; i < IterationHeaders.Count; i++)
            {
                StepIndicators[i] = (SolidColorBrush)Application.Current.Resources["BalancerRoutingStepsPassive"];
            }

        }

        private void RunCommand()
        {
            if (_interfaceVariables.ReferenceMotorSpeed <= 0 && _interfaceVariables.ReferencePropelleDiameter <= 0)
            {
                MessageBoxResult result = MessageBox.Show(
                "Please enter the reference motor speed value and propelle parameters!",
                "Missing Value Warning",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            }
            else
            {
                NextStepCommand();
                RunButtonIsEnabled = false;
                RepeatStepButtonVisibility = Visibility.Hidden;
                ApprovalStepButtonVisibility = Visibility.Hidden;
                NextStepButtonVisibility = Visibility.Visible;
            }            
        }
        private void StopCommand()
        {

        }

        private void NewTestCommand()
        {
            MessageBoxResult result = MessageBox.Show(
               "Test data will be erased. Are you sure you want to start a new test?",
               "Confirm New Test",
               MessageBoxButton.YesNo,
               MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                BalanceTestInitialConfig();
            }
        }

        private void RepeatStepCommand()
        {

        }
        private void ApprovalCommand()
        {

        }
        private void NextStepCommand()
        {
            if (_interfaceVariables.ReferenceMotorSpeed <= 0 && _interfaceVariables.ReferencePropelleDiameter <= 0)
            {
                MessageBoxResult result = MessageBox.Show(
                "Please enter the reference motor speed value and propelle parameters!",
                "Missing Value Warning",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            }
            else
            {
                if (CurrentStepIndex < IterationHeaders.Count - 1)
                {
                    CurrentStepIndex++;

                    // Genişletilmiş Algoritma formülünü yazacağım.

                    IterationHeader = IterationHeaders[CurrentStepIndex];
                    // Adım geçişlerinde renk güncellemeleri yapılıyor
                    StepIndicators[CurrentStepIndex] = (SolidColorBrush)Application.Current.Resources["BalancerRoutingStepsActive"];
                    for (int i = 1; i < CurrentStepIndex; i++)
                    {
                        StepIndicators[i] = (SolidColorBrush)Application.Current.Resources["BalancerRoutingStepsOK"];
                    }
                }
                else
                {
                    BalancingTestFinished();
                }
            }
        }
        private void BalancingTestFinished()
        {
            MessageBoxResult result = MessageBox.Show(
               "Balancing Test Finished?",
               "Confirm Test Finished",
               MessageBoxButton.YesNo,
               MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                BalanceTestInitialConfig();
                RunButtonIsEnabled = false;
                IterationHeader = "Balancing Test Finished";
            }
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {

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
            // PID çıktısını 0-100 aralığından 800-2200 aralığına dönüştürme
            double minOutput = 0;
            double maxOutput = 100;
            double minESC = 800;
            double maxESC = 2200;
            pidOutput = (pidOutput - minOutput) * (maxESC - minESC) / (maxOutput - minOutput) + minESC;

            pidOutput = Math.Clamp(pidOutput, 800, 2200);

            ESCValue = SmoothTransition(ESCValue, (int)pidOutput);
        }

        private int SmoothTransition(int currentValue, int targetValue)
        {
            double speedDifference = Math.Abs(_interfaceVariables.MotorSpeed.Value - _interfaceVariables.ReferenceMotorSpeed);

            if (speedDifference > 10)
            {

                if (speedDifference >= 400) { smoothTransitionStep = 5; }
                else if ((speedDifference <= 400) && (speedDifference > 300)) { smoothTransitionStep = 4; }
                else if ((speedDifference <= 300) && (speedDifference > 200)) { smoothTransitionStep = 3; }
                else if ((speedDifference <= 200) && (speedDifference > 100)) { smoothTransitionStep = 2; }
                else if ((speedDifference <= 100) && (speedDifference > 50)) { smoothTransitionStep = 1; }
                else if ((speedDifference <= 50) && (speedDifference > 10)) { smoothTransitionStep = 0; }
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

        private void InterfaceVariables_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_interfaceVariables.Vibration.HighVibration))
            {
                HighVibration = _interfaceVariables.Vibration.HighVibration;
            }
        }
        public string IterationHeader
        {
            get => _iterationHeader;
            set
            {
                if (SetProperty(ref _iterationHeader, value))
                {
                    OnPropertyChanged(nameof(IterationHeader));
                }
            }
        }
        public string Iteration
        {
            get => _iteration;
            set
            {
                if (SetProperty(ref _iteration, value))
                {
                    OnPropertyChanged(nameof(Iteration));
                }
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

        public double TestTimeStatusBar
        {
            get => _testTimeStatusBar;
            set
            {
                if (SetProperty(ref _testTimeStatusBar, value))
                {
                    OnPropertyChanged(nameof(TestTimeStatusBar));
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
        public bool TestReadyStatus
        {
            get => _testReadyStatus;
            set
            {
                if (SetProperty(ref _testReadyStatus, value))
                {
                    OnPropertyChanged(nameof(TestReadyStatus));
                }
            }
        }
        public Visibility RepeatStepButtonVisibility
        {
            get => _repeatStepButtonVisibility;
            set
            {
                if (SetProperty(ref _repeatStepButtonVisibility, value))
                {
                    OnPropertyChanged(nameof(RepeatStepButtonVisibility));
                }
            }
        }
        public Visibility ApprovalStepButtonVisibility
        {
            get => _approvalStepButtonVisibility;
            set
            {
                if (SetProperty(ref _approvalStepButtonVisibility, value))
                {
                    OnPropertyChanged(nameof(ApprovalStepButtonVisibility));
                }
            }
        }
        public Visibility NextStepButtonVisibility
        {
            get => _nextStepButtonVisibility;
            set
            {
                if (SetProperty(ref _nextStepButtonVisibility, value))
                {
                    OnPropertyChanged(nameof(NextStepButtonVisibility));
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
        public bool RunButtonIsEnabled
        {
            get => _runButtonIsEnabled;
            set
            {
                if (SetProperty(ref _runButtonIsEnabled, value))
                {
                    OnPropertyChanged(nameof(RunButtonIsEnabled));
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

        public ObservableCollection<Brush> StepIndicators
        {
            get => _stepIndicators;
            set
            {
                if (SetProperty(ref _stepIndicators, value))
                {
                    OnPropertyChanged(nameof(StepIndicators));
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
