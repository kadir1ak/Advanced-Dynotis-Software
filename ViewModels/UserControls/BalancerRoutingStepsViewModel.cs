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
        private List<IterationStep> _iterationSteps;
        private string _iterationHeader;
        private string _iteration;
        private int _balancerIterationStep;
        private int _headerStepIndex;
        private int _iterationStepIndex;

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

            // Initialize the IterationSteps with static data
            IterationStep.Init(); // Initialize the static data
            IterationSteps = IterationStep.IterationSteps;

            // Initialize step indicators
            StepIndicators = new ObservableCollection<Brush>();
            for (int i = 0; i < IterationSteps.Count; i++)
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
            // Set the first iteration header and clear iteration steps
            IterationHeader = IterationSteps[0].Header;
            Iteration = "";

            // Reset Counts and Status
            TestTimeCount = 0;
            MotorReadyTimeCount = 0;
            TestTimeStatusBar = 0;

            BalancerIterationStep = 0;
            HeaderStepIndex = 0;
            IterationStepIndex = 0;

            ESCValue = 0;

            // Clear Buffers and Charts
            TestVibrationsDataBuffer.Clear();
            TestStepsPropellerVibrations.Clear();
            BalancerIterationStepChart.Clear();
            BalancerIterationVibrationsChart.Clear();

            // Set Buttons Visibility
            RepeatStepButtonVisibility = Visibility.Hidden;
            ApprovalStepButtonVisibility = Visibility.Hidden;
            NextStepButtonVisibility = Visibility.Hidden;

            // Set Status Flags
            MotorReadyStatus = false;
            TestReadyStatus = false;
            ESCStatus = false;
            RunButtonIsEnabled = true;

            // Clear Output Messages
            StatusMessage = "";
            TestResult = "";

            // Stop all timers
            _progressTimer.Stop();
            _pidTimer.Stop();
            _avgTimer.Stop();

            // Reset Step Indicators
            StepIndicatorSet(0);
        }

        private void RunCommand()
        {
            BalancingIteration();
        }

        private void StopCommand()
        {
            // Stop logic implementation here
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
            // Logic for repeating the current step
        }

        private void ApprovalCommand()
        {
            // Logic for approving the current step
        }

        private void NextStepCommand()
        {
            if (_interfaceVariables.ReferenceMotorSpeed <= 0 && _interfaceVariables.ReferencePropelleDiameter <= 0)
            {
                MessageBox.Show("Please enter the reference motor speed value and propeller parameters!", "Missing Value Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                BalancingIteration();
            }
        }

        private void BalancingIteration()
        {
            switch (HeaderStepIndex)
            {
                case 0: // Run Button
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
                            RunButtonIsEnabled = false;
                            RepeatStepButtonVisibility = Visibility.Hidden;
                            ApprovalStepButtonVisibility = Visibility.Hidden;
                            NextStepButtonVisibility = Visibility.Visible;
                            HeaderStepIndex++;
                            BalancingIteration();
                        }
                    }
                    break;
                case 1:
                    {
                        IterationHeader = IterationSteps[HeaderStepIndex].Header;
                        Iteration = IterationSteps[HeaderStepIndex].Steps[IterationStepIndex];
                        StepIndicatorSet(HeaderStepIndex);

                        if (IterationStepIndex < IterationSteps[HeaderStepIndex].Steps.Count - 1) { IterationStepIndex++;}  else{ HeaderStepIndex++; IterationStepIndex = 0; }
                        


                    }
                    break;
                case 2:
                    {
                        IterationHeader = IterationSteps[HeaderStepIndex].Header;
                        Iteration = IterationSteps[HeaderStepIndex].Steps[IterationStepIndex];
                        StepIndicatorSet(HeaderStepIndex);

                        if (IterationStepIndex < IterationSteps[HeaderStepIndex].Steps.Count - 1) { IterationStepIndex++;}  else{ HeaderStepIndex++; IterationStepIndex = 0; }
                        
                    }
                    break;
                case 3:
                    {
                        IterationHeader = IterationSteps[HeaderStepIndex].Header;
                        Iteration = IterationSteps[HeaderStepIndex].Steps[IterationStepIndex];
                        StepIndicatorSet(HeaderStepIndex);

                        if (IterationStepIndex < IterationSteps[HeaderStepIndex].Steps.Count - 1) { IterationStepIndex++;}  else{ HeaderStepIndex++; IterationStepIndex = 0; }
                        


                    }
                    break;
                case 4:
                    {
                        IterationHeader = IterationSteps[HeaderStepIndex].Header;
                        Iteration = IterationSteps[HeaderStepIndex].Steps[IterationStepIndex];
                        StepIndicatorSet(HeaderStepIndex);

                        if (IterationStepIndex < IterationSteps[HeaderStepIndex].Steps.Count - 1) { IterationStepIndex++;}  else{ HeaderStepIndex++; IterationStepIndex = 0; }
                       


                    }
                    break;
                case 5:
                    {
                        IterationHeader = IterationSteps[HeaderStepIndex].Header;
                        Iteration = IterationSteps[HeaderStepIndex].Steps[IterationStepIndex];
                        StepIndicatorSet(HeaderStepIndex);

                        if (IterationStepIndex < IterationSteps[HeaderStepIndex].Steps.Count - 1) { IterationStepIndex++;}  else{ HeaderStepIndex++; IterationStepIndex = 0; }
                       


                    }
                    break;
                case 6:
                    {
                        IterationHeader = IterationSteps[HeaderStepIndex].Header;
                        Iteration = IterationSteps[HeaderStepIndex].Steps[IterationStepIndex];
                        StepIndicatorSet(HeaderStepIndex);

                        if (IterationStepIndex < IterationSteps[HeaderStepIndex].Steps.Count - 1) { IterationStepIndex++;}  else{ HeaderStepIndex++; IterationStepIndex = 0; }
                     


                    }
                    break;
                case 7:
                    {
                        IterationHeader = IterationSteps[HeaderStepIndex].Header;
                        Iteration = IterationSteps[HeaderStepIndex].Steps[IterationStepIndex];
                        StepIndicatorSet(HeaderStepIndex);

                        if (IterationStepIndex < IterationSteps[HeaderStepIndex].Steps.Count - 1) { IterationStepIndex++;}  else{ HeaderStepIndex++; IterationStepIndex = 0; }
                      


                    }
                    break;
                case 8:
                    {
                        IterationHeader = IterationSteps[HeaderStepIndex].Header;
                        Iteration = IterationSteps[HeaderStepIndex].Steps[IterationStepIndex];
                        StepIndicatorSet(HeaderStepIndex);

                        if (IterationStepIndex < IterationSteps[HeaderStepIndex].Steps.Count - 1) { IterationStepIndex++;}  else{ HeaderStepIndex++; IterationStepIndex = 0; }
                       


                    }
                    break;
                case 9:
                    {
                        IterationHeader = IterationSteps[HeaderStepIndex].Header;
                        Iteration = IterationSteps[HeaderStepIndex].Steps[IterationStepIndex];
                        StepIndicatorSet(HeaderStepIndex);

                        if (IterationStepIndex < IterationSteps[HeaderStepIndex].Steps.Count - 1) { IterationStepIndex++;}  else{ HeaderStepIndex++; IterationStepIndex = 0; }
                    


                    }
                    break;
                case 10:
                    {
                        IterationHeader = IterationSteps[HeaderStepIndex].Header;
                        Iteration = IterationSteps[HeaderStepIndex].Steps[IterationStepIndex];
                        StepIndicatorSet(HeaderStepIndex);

                        if (IterationStepIndex < IterationSteps[HeaderStepIndex].Steps.Count - 1) { IterationStepIndex++;}  else{ HeaderStepIndex++; IterationStepIndex = 0; }
                      


                    }
                    break;
                case 11:
                    {
                        BalancingTestFinished();
                    }
                    break;
            }
        }
        private void BalancingTestFinished()
        {
            MessageBoxResult result = MessageBox.Show("Balancing Test Finished?", "Confirm Test Finished", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                BalanceTestInitialConfig();
                RunButtonIsEnabled = false;
                IterationHeader = "Balancing Test Finished";
            }
        }
        private void StepIndicatorSet(int index)
        {
            StepIndicators[index] = (SolidColorBrush)Application.Current.Resources["BalancerRoutingStepsActive"];
            for (int i = 0; i < index; i++)
            {
                StepIndicators[i] = (SolidColorBrush)Application.Current.Resources["BalancerRoutingStepsOK"];
            }
            for (int i = index + 1; i < IterationSteps.Count; i++)
            {
                StepIndicators[i] = (SolidColorBrush)Application.Current.Resources["BalancerRoutingStepsPassive"];
            }
        }
        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            // Timer logic implementation here
        }

        private void AVGTimer_Tick(object sender, EventArgs e)
        {
            TestVibrationsDataBuffer.Add(HighVibration);
        }

        private void PIDTimer_Tick(object sender, EventArgs e)
        {
            double currentSpeed = _interfaceVariables.MotorSpeed.Value;
            double pidOutput = _pidController.Calculate(_interfaceVariables.ReferenceMotorSpeed, currentSpeed);

            // Map PID output to ESC range
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
                else if (speedDifference > 300) { smoothTransitionStep = 4; }
                else if (speedDifference > 200) { smoothTransitionStep = 3; }
                else if (speedDifference > 100) { smoothTransitionStep = 2; }
                else if (speedDifference > 50) { smoothTransitionStep = 1; }
                else { smoothTransitionStep = 0; }

                if (Math.Abs(currentValue - targetValue) <= smoothTransitionStep)
                {
                    return targetValue;
                }

                currentValue += currentValue < targetValue ? smoothTransitionStep : -smoothTransitionStep;
                MotorReadyStatus = speedDifference < 80;
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

        public List<IterationStep> IterationSteps
        {
            get => _iterationSteps;
            set
            {
                if (SetProperty(ref _iterationSteps, value))
                {
                    OnPropertyChanged(nameof(IterationSteps));
                }
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

        public int HeaderStepIndex
        {
            get => _headerStepIndex;
            set
            {
                if (SetProperty(ref _headerStepIndex, value))
                {
                    OnPropertyChanged(nameof(HeaderStepIndex));
                }
            }
        }
        public int IterationStepIndex
        {
            get => _iterationStepIndex;
            set
            {
                if (SetProperty(ref _iterationStepIndex, value))
                {
                    OnPropertyChanged(nameof(IterationStepIndex));
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
                    OnPropertyChanged(nameof(TestVibrationsDataBuffer));
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

    public class IterationStep
    {
        public string Header { get; set; }
        public List<string> Steps { get; set; }

        // Static list to hold iteration steps
        public static List<IterationStep> IterationSteps { get; private set; }

        // Static initializer for the default steps
        public static void Init()
        {
            IterationSteps = new List<IterationStep>
            {
                new IterationStep
                {
                    Header = "Welcome Balancer Test",
                    Steps = new List<string> {}
                },
                new IterationStep
                {
                    Header = "Cihazın Hazırlanması",
                    Steps = new List<string> { "Step 1.0", "Step 1.1", "Step 1.2" }
                },
                new IterationStep
                {
                    Header = "Cihaz Bağlantısının ve Arayüz Parametrelerinin Ayarlanması",
                    Steps = new List<string> { "Step 2.0", "Step 2.1", "Step 2.2", "Step 2.3", "Step 2.4", "Step 2.5" }
                },
                new IterationStep
                {
                    Header = "Ortam Titreşimlerinin Hesaplanması ve Filtrelenmesi (Dara İşlemi)",
                    Steps = new List<string> { "Step 3.0", "Step 3.1", "Step 3.2", "Step 3.3", "Step 3.4" }
                },
                new IterationStep
                {
                    Header = "Motor Titreşimin Hesaplanması",
                    Steps = new List<string> { "Step 4.0", "Step 4.1" }
                },
                new IterationStep
                {
                    Header = "Pervane Montajı",
                    Steps = new List<string> { "Step 5.0", "Step 5.1" }
                },
                new IterationStep
                {
                    Header = "Pervane Titreşimin Hesaplanması",
                    Steps = new List<string> { "Step 6.0", "Step 6.1" }
                },
                new IterationStep
                {
                    Header = "Birim Referans Düzeltici Ağırlık Değerinin Pervane Boyutuna Göre Hesaplanması",
                    Steps = new List<string> { "Step 7.0", "Step 7.1", "Step 7.2" }
                },
                new IterationStep
                {
                    Header = "Pervanenin Her İki Kanadına Birim Referans Düzeltici Ağırlığın Eklenmesi",
                    Steps = new List<string> { "Step 8.0", "Step 8.1" }
                },
                new IterationStep
                {
                    Header = "Düzeltici Ağırlık Değerinin ve Düzeltici Yönün Hesaplanması",
                    Steps = new List<string> { "Step 9.0", "Step 9.1" }
                },
                new IterationStep
                {
                    Header = "Tayin Edilen Yöne Düzeltici Ağırlığın Eklenmesi",
                    Steps = new List<string> { "Step 10.0", "Step 10.1" }
                }
            };
        }
    }
}
