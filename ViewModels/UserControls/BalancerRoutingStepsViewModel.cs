using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.Services.Controllers;
using Advanced_Dynotis_Software.Services.Helpers;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Numerics;
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

        private Visibility _autoProgressCountVisibility;
        private Visibility _recommendedTableVisibility;
        private Visibility _repeatStepButtonVisibility;
        private Visibility _approvalStepButtonVisibility;
        private Visibility _nextStepButtonVisibility;
        private Visibility _runButtonVisibility;

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

        // Vibration Value
        private double _deviceBaseStaticVibration;          // Cihazın durağan haldeki titreşimi
        private double _motorBaseRunningVibration;              // Motor çalışır haldeki titreşimi
        private double _propellerBaseRunningVibration;          // Pervane çalışır haldeki titreşim değeri
        private double _balancedPropellerRunningVibration;          // Pervane çalışır haldeki titreşim değeri
        private double _firstBladeVibration;               // Pervane birim referans bant ile ilk kanat titreşim değeri
        private double _secondBladeVibration;              // Pervane birim referans bant ile ikinci kanat titreşim değeri
        private double _tareVibration;
        private double _tareVibrationX;
        private double _tareVibrationY;
        private double _tareVibrationZ;

        // Referans Değerler
        private double _equalizerTapeCoefficient;
        private string _equalizerDirection;

        //ESC
        private bool _escStatus;
        private int _escValue;

        //PID
        private int smoothTransitionStep;
        private PIDController PIDController;
        private DispatcherTimer PIDTimer;

        //Progress Bar Time
        private DispatcherTimer BalancerProgressTimer;

        // Auto Progress Count
        private DispatcherTimer AutoProgressCountTimer;
        private string _autoProgressCountIcon;

        // Vibration 
        private DispatcherTimer HighVibrationDataCollectionTimer;
        private double _highVibration;
        private List<double> _vibrationsDataBuffer;


        private readonly object _balancerProgressTimerLock = new object();
        private readonly object _autoProgressCountTimerLock = new object();
        private readonly object _highVibrationDataCollectionTimerLock = new object();
        private readonly object _pidTimerLock = new object();

        // Propeller Vibration 
        private List<double> _testStepsPropellerVibrations;

        //StepChart
        private ObservableCollection<int> _balancerIterationStepChart;
        private ObservableCollection<double> _balancerIterationVibrationsChart;

        // Step Indicators
        private ObservableCollection<System.Windows.Media.Brush> _stepIndicators;

        // Constructor
        public BalancerRoutingStepsViewModel(DynotisData dynotisData, InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
            _dynotisData = dynotisData;

            _interfaceVariables.PropertyChanged += InterfaceVariables_PropertyChanged;

            VibrationsDataBuffer = new List<double>();
            TestStepsPropellerVibrations = new List<double>();
            BalancerIterationStepChart = new ObservableCollection<int>();
            BalancerIterationVibrationsChart = new ObservableCollection<double>();

            // Initialize PID Controller
            PIDController = new PIDController(1.5, 0.03, 0.05);

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
            StepIndicators = new ObservableCollection<System.Windows.Media.Brush>();
            for (int i = 0; i < IterationSteps.Count; i++)
            {
                // Inside your constructor or other methods
                StepIndicators.Add((System.Windows.Media.SolidColorBrush)Application.Current.Resources["BalancerRoutingStepsPassive"]);
            }

            BalancerProgressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            BalancerProgressTimer.Tick += BalancerProgressTimer_Tick;

            AutoProgressCountTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            AutoProgressCountTimer.Tick += AutoProgressCountTimer_Tick;

            PIDTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            PIDTimer.Tick += PIDTimer_Tick;

            HighVibrationDataCollectionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1)
            };
            HighVibrationDataCollectionTimer.Tick += HighVibrationDataCollectionTimer_Tick;

            BalanceTestInitialConfig();
        }

        private void BalanceTestInitialConfig()
        {
            // Set the first iteration header and clear iteration steps
            IterationHeader = IterationSteps[0].Header;
            Iteration = IterationSteps[0].Steps[0] + "\r\n" +
                        IterationSteps[0].Steps[1] + "\r\n" +
                        IterationSteps[0].Steps[2] + "\r\n" +
                        IterationSteps[0].Steps[3] + "\r\n" +
                        IterationSteps[0].Steps[4];
            // Auto Progress Count
            AutoProgressCountIcon = "_3Solid";
            AutoProgressCountTimer.Start();

            // Reset Counts and Status
            TestTimeCount = 0;
            MotorReadyTimeCount = 0;
            TestTimeStatusBar = 0;

            BalancerIterationStep = 0;
            HeaderStepIndex = 0;
            IterationStepIndex = 0;

            ESCValue = 800;

            // Clear Buffers and Charts
            VibrationsDataBuffer.Clear();
            TestStepsPropellerVibrations.Clear();
            BalancerIterationStepChart.Clear();
            BalancerIterationVibrationsChart.Clear();

            // Set Buttons Visibility
            AutoProgressCountVisibility = Visibility.Visible;
            RecommendedTableVisibility = Visibility.Collapsed;
            RepeatStepButtonVisibility = Visibility.Collapsed;
            ApprovalStepButtonVisibility = Visibility.Collapsed;
            NextStepButtonVisibility = Visibility.Collapsed;
            RunButtonVisibility = Visibility.Visible;

            // Set Status Flags
            MotorReadyStatus = false;
            TestReadyStatus = false;
            ESCStatus = false;
            RunButtonIsEnabled = true;

            // Clear Output Messages
            StatusMessage = "";
            TestResult = "";

            // Stop all timers
            BalancerProgressTimer.Stop();
            PIDTimer.Stop();
            HighVibrationDataCollectionTimer.Stop();

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
            IterationStepIndex = 0;
            // Set Buttons Visibility
            RepeatStepButtonVisibility = Visibility.Collapsed;
            ApprovalStepButtonVisibility = Visibility.Collapsed;
            NextStepButtonVisibility = Visibility.Visible;
            BalancingIteration();
        }

        private void ApprovalCommand()
        {
            IterationStepIndex = 0;
            HeaderStepIndex++;
            // Set Buttons Visibility
            RepeatStepButtonVisibility = Visibility.Collapsed;
            ApprovalStepButtonVisibility = Visibility.Collapsed;
            NextStepButtonVisibility = Visibility.Visible;
            BalancingIteration();
        }

        private void NextStepCommand()
        {
            if (_interfaceVariables.ReferenceMotorSpeed <= 0 && _interfaceVariables.ReferencePropellerDiameter <= 0)
            {
                MessageBox.Show("Please enter the reference motor speed value and propeller parameters!", "Missing Value Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                BalancingIteration();
            }
        }
        private void SetVisibility(string status)
        {
            if (status == "AutoProgress")
            {
                AutoProgressCountVisibility = Visibility.Visible;
                RepeatStepButtonVisibility = Visibility.Collapsed;
                ApprovalStepButtonVisibility = Visibility.Collapsed;
                NextStepButtonVisibility = Visibility.Collapsed;
            }
            else if (status == "AllCollapsed")
            {
                AutoProgressCountVisibility = Visibility.Collapsed;
                RepeatStepButtonVisibility = Visibility.Collapsed;
                ApprovalStepButtonVisibility = Visibility.Collapsed;
                NextStepButtonVisibility = Visibility.Collapsed;
            }
            else if (status == "AllButtonVisible")
            {
                AutoProgressCountVisibility = Visibility.Collapsed;
                RepeatStepButtonVisibility = Visibility.Visible;
                ApprovalStepButtonVisibility = Visibility.Visible;
                NextStepButtonVisibility = Visibility.Visible;
            }
            else if (status == "ApprovalRepeatStepVisible")
            {
                AutoProgressCountVisibility = Visibility.Collapsed;
                RepeatStepButtonVisibility = Visibility.Visible;
                ApprovalStepButtonVisibility = Visibility.Visible;
                NextStepButtonVisibility = Visibility.Collapsed;
            }
            else if (status == "NextStepVisible")
            {
                AutoProgressCountVisibility = Visibility.Collapsed;
                RepeatStepButtonVisibility = Visibility.Collapsed;
                ApprovalStepButtonVisibility = Visibility.Collapsed;
                NextStepButtonVisibility = Visibility.Visible;
            }


        }
        private void BalancingIteration()
        {
            switch (HeaderStepIndex)
            {
                case 0: // Run Button
                    {
                        if (_interfaceVariables.ReferenceMotorSpeed <= 0 || _interfaceVariables.ReferencePropellerDiameter <= 0)
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
                            RepeatStepButtonVisibility = Visibility.Collapsed;
                            ApprovalStepButtonVisibility = Visibility.Collapsed;
                            NextStepButtonVisibility = Visibility.Visible;
                            RunButtonVisibility = Visibility.Collapsed;
                            HeaderStepIndex++;
                            BalancingIteration();
                        }
                    }
                    break;
                case 1: // Ortam ve Motor Titreşimlerinin Hesaplanması
                    {
                        IterationHeader = IterationSteps[HeaderStepIndex].Header;
                        Iteration = IterationSteps[HeaderStepIndex].Steps[IterationStepIndex];
                        StepIndicatorSet(HeaderStepIndex);

                        switch (IterationStepIndex)
                        {
                            case 0:  // Cihaz sıfırlanacak, müdahale etmeyin.
                                {
                                    SetVisibility("AutoProgress");
                                    AutoProgressCountTimer.Start();
                                }
                                break;
                            case 1:  // Sıfırlama işlemi gerçekleştiriliyor.
                                {
                                    SetVisibility("AllCollapsed");
                                }
                                break;
                            case 2:  // Ortam titreşim değeri hesaplanacak cihazına müdahale etmeyin.
                                {
                                    SetVisibility("AutoProgress");
                                    AutoProgressCountTimer.Start();
                                }
                                break;
                            case 3:  // Ortam titreşim değeri hesaplanıyor.
                                {
                                    SetVisibility("AllCollapsed");
                                }
                                break;
                            case 4:  // Motor titreşim değeri hesaplanacak cihazına müdahale etmeyin.
                                {
                                    SetVisibility("AutoProgress");
                                    AutoProgressCountTimer.Start();
                                }
                                break;
                            case 5:  // Motor titreşim değeri hesaplanıyor.
                                {
                                    SetVisibility("AllCollapsed");
                                }
                                break;
                            case 6:  // Sonuçları kontrol edin.
                                {
                                    SetVisibility("ApprovalRepeatStepVisible");
                                    Iteration = Iteration + "\r\n" +
                                                "Device Base Static Vibration: " + DeviceBaseStaticVibration.ToString("0.000") + " g" + "\r\n" +
                                                "Motor Base Running Vibration: " + MotorBaseRunningVibration.ToString("0.000") + " g";
                                }
                                break;
                        }
                    }
                    break;
                case 2: // Pervane Titreşiminin Hesaplanması
                    {
                        IterationHeader = IterationSteps[HeaderStepIndex].Header;
                        Iteration = IterationSteps[HeaderStepIndex].Steps[IterationStepIndex];
                        StepIndicatorSet(HeaderStepIndex);

                        switch (IterationStepIndex)
                        {
                            case 0:  // Pervane montajını yapın.
                                {
                                    IterationStepIndex++;
                                }
                                break;
                            case 1:  // Pervane titreşim değeri hesaplanacak cihazına müdahale etmeyin.
                                {
                                    SetVisibility("AutoProgress");
                                    AutoProgressCountTimer.Start();
                                }
                                break;
                            case 2:  // Pervane titreşim değeri hesaplanıyor.
                                {
                                    SetVisibility("AllCollapsed");
                                }
                                break;
                            case 3:  // Sonuçları kontrol edin.
                                {
                                    SetVisibility("ApprovalRepeatStepVisible");
                                    Iteration = Iteration + "\r\n" +
                                                "Propeller Base Running Vibration: " + PropellerBaseRunningVibration.ToString("0.000") + " g";
                                }
                                break;
                        }
                    }
                    break;
                case 3: // Birim Referans Bant Uzunluğunun Seçimi
                    {
                        IterationHeader = IterationSteps[HeaderStepIndex].Header;
                        Iteration = IterationSteps[HeaderStepIndex].Steps[IterationStepIndex];
                        StepIndicatorSet(HeaderStepIndex);
                        RecommendedTableVisibility = Visibility.Visible;

                        SetVisibility("NextStepVisible");

                        if (IterationStepIndex < IterationSteps[HeaderStepIndex].Steps.Count - 1) { IterationStepIndex++; } else { HeaderStepIndex++; IterationStepIndex = 0; }

                    }
                    break;
                case 4: // Pervanenin Her İki Kanadına Birim Referans Bantın Eklenmesi
                    {
                        IterationHeader = IterationSteps[HeaderStepIndex].Header;
                        Iteration = IterationSteps[HeaderStepIndex].Steps[IterationStepIndex];
                        StepIndicatorSet(HeaderStepIndex);
                        RecommendedTableVisibility = Visibility.Collapsed;
                        switch (IterationStepIndex)
                        {
                            case 0:  // Seçtiğiniz birim referans bandı, pervanenin herhangi bir kanadının merkezinden itibaren yarıçapının yaklaşık %25-30'una denk gelecek şekilde yapıştırın.
                                {
                                    SetVisibility("NextStepVisible");
                                    IterationStepIndex++;
                                }
                                break;
                            case 1:  // Pervane titreşim değeri hesaplanacak cihazına müdahale etmeyin.
                                {
                                    SetVisibility("AutoProgress");
                                    AutoProgressCountTimer.Start();
                                }
                                break;
                            case 2:  // Pervane titreşim değeri hesaplanıyor.
                                {
                                    SetVisibility("AllCollapsed");
                                }
                                break;
                            case 3:  // Aynı bandı çıkarıp, pervanenin diğer kanadına merkezden itibaren yarıçapının yaklaşık %25-30'una denk gelecek şekilde yapıştırın.
                                {
                                    SetVisibility("NextStepVisible");
                                    IterationStepIndex++;
                                }
                                break;
                            case 4:  // Pervane titreşim değeri hesaplanacak cihazına müdahale etmeyin.
                                {
                                    SetVisibility("AutoProgress");
                                    AutoProgressCountTimer.Start();
                                }
                                break;
                            case 5:  // Pervane titreşim değeri hesaplanıyor.
                                {
                                    SetVisibility("AllCollapsed");
                                }
                                break;
                            case 6:  // Sonuçları kontrol edin.
                                {
                                    SetVisibility("ApprovalRepeatStepVisible");
                                    Iteration = Iteration + "\r\n" +
                                                "First Blade Vibration: " + FirstBladeVibration.ToString("0.000") + " g" + "\r\n" +
                                                "Second Blade Vibration: " + SecondBladeVibration.ToString("0.000") + " g";

                                }
                                break;
                        }
                    }
                    break;
                case 5: // Düzeltici Bant Adetinin ve Yönünün Belirlenmesi, Düzeltici Bantın Eklenmesi"
                    {
                        // Düzeltici yön ve bant adedi hesaplandı.
                        // Lütfen seçtiğiniz birim referans bandı ilk durumundaki yere geri yapıştırın.
                        // x adet daha ilave düzeltici bantı, aerodinamik yapıyı bozmayacak şekilde referans bantın olduğu bölgeye yapıştırın.
                        IterationHeader = IterationSteps[HeaderStepIndex].Header;
                        Iteration = IterationSteps[HeaderStepIndex].Steps[IterationStepIndex];
                        StepIndicatorSet(HeaderStepIndex);

                        SetVisibility("NextStepVisible");

                        double Ratio = 0;
                        if (FirstBladeVibration <= SecondBladeVibration)
                        {
                            EqualizerDirection = "First Blade";
                            Ratio = ((PropellerBaseRunningVibration - FirstBladeVibration) / PropellerBaseRunningVibration);
                        }
                        else
                        {
                            EqualizerDirection = "Second Blade";
                            Ratio = ((PropellerBaseRunningVibration - SecondBladeVibration) / PropellerBaseRunningVibration);
                        }

                        EqualizerTapeCoefficient = (1 / Ratio) - 1; // (-1) şuan var olan referans bantı temsil ediyor.

                        if (EqualizerDirection == "First Blade")
                        {
                            Iteration = IterationSteps[HeaderStepIndex].Steps[0] + "\r\n" +
                                        IterationSteps[HeaderStepIndex].Steps[1] + "\r\n" +
                                        "Equalizer Tape Coefficient: " + EqualizerTapeCoefficient.ToString("0.0") + " Piece" + "\r\n" +
                                        "Equalizer Direction: " + EqualizerDirection + "\r\n" +
                                         EqualizerTapeCoefficient.ToString("0.0") + IterationSteps[HeaderStepIndex].Steps[2];

                        }
                        else if (EqualizerDirection == "Second Blade")
                        {
                            Iteration = IterationSteps[HeaderStepIndex].Steps[0] + "\r\n" +
                                        "Equalizer Tape Coefficient: " + EqualizerTapeCoefficient.ToString("0.0") + " Piece" + "\r\n" +
                                        "Equalizer Direction: " + EqualizerDirection + "\r\n" +
                                         EqualizerTapeCoefficient.ToString("0.0") + IterationSteps[HeaderStepIndex].Steps[2];
                        }                    
                        
                        HeaderStepIndex++; 
                        IterationStepIndex = 0; 
                    }
                    break;
                case 6: // Test ve Sonuçların Kontrolü
                    {
                        IterationHeader = IterationSteps[HeaderStepIndex].Header;
                        Iteration = IterationSteps[HeaderStepIndex].Steps[IterationStepIndex];
                        StepIndicatorSet(HeaderStepIndex);

                        switch (IterationStepIndex)
                        {
                            case 0:  // Pervane titreşim değeri hesaplanacak cihazına müdahale etmeyin.
                                {
                                    SetVisibility("AutoProgress");
                                    AutoProgressCountTimer.Start();
                                }
                                break;
                            case 1:  // Pervane titreşim değeri hesaplanıyor.
                                {
                                    SetVisibility("AllCollapsed");
                                }
                                break;
                            case 2:  // Sonuçları kontrol edin.
                                {
                                    SetVisibility("ApprovalRepeatStepVisible");
                                    Iteration = Iteration + "\r\n" +
                                                "Balanced Propeller Running Vibration: " + BalancedPropellerRunningVibration.ToString("0.000") + " g";
                                }
                                break;
                            case 3:  // Sonuçları kontrol edin.
                                {
                                    // Bitirme algoritması yazılacak
                                    HeaderStepIndex++;
                                    IterationStepIndex = 0;
                                }
                                break;
                        }
                    }
                    break;
                case 7:
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
            }
        }
        private void StepIndicatorSet(int index)
        {
            StepIndicators[index] = (System.Windows.Media.SolidColorBrush)Application.Current.Resources["BalancerRoutingStepsActive"];
            for (int i = 0; i < index; i++)
            {
                StepIndicators[i] = (System.Windows.Media.SolidColorBrush)Application.Current.Resources["BalancerRoutingStepsOK"];
            }
            for (int i = index + 1; i < IterationSteps.Count; i++)
            {
                StepIndicators[i] = (System.Windows.Media.SolidColorBrush)Application.Current.Resources["BalancerRoutingStepsPassive"];
            }
        }
        private void CalculateVibrationTare()
        {
            if (_interfaceVariables != null && _dynotisData != null)
            {
                TareVibration = _interfaceVariables.Vibration.TareCurrentVibration;
                TareVibrationX = _interfaceVariables.Vibration.TareCurrentVibrationX;
                TareVibrationY = _interfaceVariables.Vibration.TareCurrentVibrationY;
                TareVibrationZ = _interfaceVariables.Vibration.TareCurrentVibrationZ;
            }
        }

        private void CalculateDeviceBaseStaticVibrationVibration(List<double> DataBuffer)
        {
            DeviceBaseStaticVibration = DataBuffer.Sum() / DataBuffer.Count;
        }
        private void CalculateMotorBaseRunningVibration(List<double> DataBuffer)
        {
            MotorBaseRunningVibration = DataBuffer.Sum() / DataBuffer.Count;
        }
        private void CalculatePropellerBaseRunningVibration(List<double> DataBuffer)
        {
            PropellerBaseRunningVibration = DataBuffer.Sum() / DataBuffer.Count;
        }
        private void CalculateBalancedPropellerRunningVibration(List<double> DataBuffer)
        {
            BalancedPropellerRunningVibration = DataBuffer.Sum() / DataBuffer.Count;
        }
        private void CalculateFirstBladeVibration(List<double> DataBuffer)
        {
            FirstBladeVibration = DataBuffer.Sum() / DataBuffer.Count;
        }
        private void CalculateSecondBladeVibration(List<double> DataBuffer)
        {
            SecondBladeVibration = DataBuffer.Sum() / DataBuffer.Count;
        }

        private void MotorStop()
        {
            PIDTimer.Stop();
            ESCStatus = false;
            ESCValue = 800;
            MotorReadyStatus = false;
        }
        private void AutoProgressCountTimer_Tick(object sender, EventArgs e)
        {
            lock (_autoProgressCountTimerLock)
            {
                switch (HeaderStepIndex)
                {
                    case 1: // Ortam Titreşimlerinin Hesaplanması
                        {
                            switch (IterationStepIndex)
                            {
                                case 0:  // Cihaz sıfırlanacak, müdahale etmeyin.
                                    {
                                        switch (AutoProgressCountIcon)
                                        {
                                            case "_3Solid": AutoProgressCountIcon = "_2Solid"; break;
                                            case "_2Solid": AutoProgressCountIcon = "_1Solid"; break;
                                            case "_1Solid": AutoProgressCountIcon = "_0Solid"; break;
                                            case "_0Solid":
                                                {
                                                    AutoProgressCountVisibility = Visibility.Collapsed;
                                                    AutoProgressCountIcon = "_3Solid";
                                                    IterationStepIndex++;
                                                    BalancingIteration();
                                                    BalancerProgressTimer.Start();
                                                    AutoProgressCountTimer.Stop();
                                                }
                                                break;
                                        }
                                    }
                                    break;
                                case 1:  // Sıfırlama işlemi gerçekleştiriliyor.
                                    { }
                                    break;
                                case 2:  // Ortam titreşim değeri hesaplanacak cihazına müdahale etmeyin.
                                    {
                                        switch (AutoProgressCountIcon)
                                        {
                                            case "_3Solid": AutoProgressCountIcon = "_2Solid"; break;
                                            case "_2Solid": AutoProgressCountIcon = "_1Solid"; break;
                                            case "_1Solid": AutoProgressCountIcon = "_0Solid"; break;
                                            case "_0Solid":
                                                {
                                                    AutoProgressCountVisibility = Visibility.Collapsed;
                                                    AutoProgressCountIcon = "_3Solid";
                                                    IterationStepIndex++;
                                                    BalancingIteration();
                                                    BalancerProgressTimer.Start();
                                                    AutoProgressCountTimer.Stop();
                                                }
                                                break;
                                        }
                                    }
                                    break;
                                case 3:  // Ortam titreşim değeri hesaplanıyor.
                                    { }
                                    break;
                                case 4:  // Motor titreşim değeri hesaplanacak cihazına müdahale etmeyin.
                                    {
                                        switch (AutoProgressCountIcon)
                                        {
                                            case "_3Solid": AutoProgressCountIcon = "_2Solid"; break;
                                            case "_2Solid": AutoProgressCountIcon = "_1Solid"; break;
                                            case "_1Solid": AutoProgressCountIcon = "_0Solid"; break;
                                            case "_0Solid":
                                                {
                                                    AutoProgressCountVisibility = Visibility.Collapsed;
                                                    AutoProgressCountIcon = "_3Solid";
                                                    IterationStepIndex++;
                                                    BalancingIteration();
                                                    PIDTimer.Start();
                                                    BalancerProgressTimer.Start();
                                                    AutoProgressCountTimer.Stop();
                                                }
                                                break;
                                        }
                                    }
                                    break;
                                case 5:  // Motor titreşim değeri hesaplanıyor.
                                    { }
                                    break;
                                case 6:  // Sonuçları kontrol edin.
                                    { }
                                    break;
                            }
                        }
                        break;
                    case 2: // Pervane Titreşiminin Hesaplanması
                        {
                            switch (IterationStepIndex)
                            {
                                case 0:  // Pervane montajını yapın.
                                    { }
                                    break;
                                case 1:  // Pervane titreşim değeri hesaplanacak cihazına müdahale etmeyin.
                                    {
                                        switch (AutoProgressCountIcon)
                                        {
                                            case "_3Solid": AutoProgressCountIcon = "_2Solid"; break;
                                            case "_2Solid": AutoProgressCountIcon = "_1Solid"; break;
                                            case "_1Solid": AutoProgressCountIcon = "_0Solid"; break;
                                            case "_0Solid":
                                                {
                                                    AutoProgressCountVisibility = Visibility.Collapsed;
                                                    AutoProgressCountIcon = "_3Solid";
                                                    IterationStepIndex++;
                                                    BalancingIteration();
                                                    PIDTimer.Start();
                                                    BalancerProgressTimer.Start();
                                                    AutoProgressCountTimer.Stop();
                                                }
                                                break;
                                        }
                                    }
                                    break;
                                case 2:  // Pervane titreşim değeri hesaplanıyor.
                                    { }
                                    break;
                                case 3:  // Sonuçları kontrol edin.
                                    { }
                                    break;
                            }
                        }
                        break;
                    case 3: // Birim Referans Bant Uzunluğunun Seçimi
                        {

                        }
                        break;
                    case 4: // Pervanenin Her İki Kanadına Birim Referans Bantın Eklenmesi
                        {
                            switch (IterationStepIndex)
                            {
                                case 0:  // Seçtiğiniz birim referans bandı, pervanenin herhangi bir kanadının merkezinden itibaren yarıçapının yaklaşık %25-30'una denk gelecek şekilde yapıştırın.
                                    { }
                                    break;
                                case 1:  // Pervane titreşim değeri hesaplanacak cihazına müdahale etmeyin.
                                    {
                                        switch (AutoProgressCountIcon)
                                        {
                                            case "_3Solid": AutoProgressCountIcon = "_2Solid"; break;
                                            case "_2Solid": AutoProgressCountIcon = "_1Solid"; break;
                                            case "_1Solid": AutoProgressCountIcon = "_0Solid"; break;
                                            case "_0Solid":
                                                {
                                                    AutoProgressCountVisibility = Visibility.Collapsed;
                                                    AutoProgressCountIcon = "_3Solid";
                                                    IterationStepIndex++;
                                                    BalancingIteration();
                                                    PIDTimer.Start();
                                                    BalancerProgressTimer.Start();
                                                    AutoProgressCountTimer.Stop();
                                                }
                                                break;
                                        }
                                    }
                                    break;
                                case 2:  // Pervane titreşim değeri hesaplanıyor.
                                    { }
                                    break;
                                case 3:  // Aynı bandı çıkarıp, pervanenin diğer kanadına merkezden itibaren yarıçapının yaklaşık %25-30'una denk gelecek şekilde yapıştırın.
                                    { }
                                    break;
                                case 4:  // Pervane titreşim değeri hesaplanacak cihazına müdahale etmeyin.
                                    {
                                        switch (AutoProgressCountIcon)
                                        {
                                            case "_3Solid": AutoProgressCountIcon = "_2Solid"; break;
                                            case "_2Solid": AutoProgressCountIcon = "_1Solid"; break;
                                            case "_1Solid": AutoProgressCountIcon = "_0Solid"; break;
                                            case "_0Solid":
                                                {
                                                    AutoProgressCountVisibility = Visibility.Collapsed;
                                                    AutoProgressCountIcon = "_3Solid";
                                                    IterationStepIndex++;
                                                    BalancingIteration();
                                                    PIDTimer.Start();
                                                    BalancerProgressTimer.Start();
                                                    AutoProgressCountTimer.Stop();
                                                }
                                                break;
                                        }
                                    }
                                    break;
                                case 5:  // Pervane titreşim değeri hesaplanıyor.
                                    { }
                                    break;
                                case 6:  // Sonuçları kontrol edin.
                                    { }
                                    break;
                            }
                        }
                        break;
                    case 5: // Düzeltici Bant Adetinin ve Yönünün Belirlenmesi, Düzeltici Bantın Eklenmesi"
                        {

                        }
                        break;
                    case 6: // Test ve Sonuçların Kontrolü
                        {
                            switch (IterationStepIndex)
                            {
                                case 0:  // Pervane titreşim değeri hesaplanacak cihazına müdahale etmeyin.
                                    {
                                        switch (AutoProgressCountIcon)
                                        {
                                            case "_3Solid": AutoProgressCountIcon = "_2Solid"; break;
                                            case "_2Solid": AutoProgressCountIcon = "_1Solid"; break;
                                            case "_1Solid": AutoProgressCountIcon = "_0Solid"; break;
                                            case "_0Solid":
                                                {
                                                    AutoProgressCountVisibility = Visibility.Collapsed;
                                                    AutoProgressCountIcon = "_3Solid";
                                                    IterationStepIndex++;
                                                    BalancingIteration();
                                                    PIDTimer.Start();
                                                    BalancerProgressTimer.Start();
                                                    AutoProgressCountTimer.Stop();
                                                }
                                                break;
                                        }
                                    }
                                    break;
                                case 1:  // Pervane titreşim değeri hesaplanıyor.
                                    { }
                                    break;
                                case 2:  // Sonuçları kontrol edin.
                                    { }
                                    break;
                            }
                        }
                        break;
                }
            }
        }
        private void BalancerProgressTimer_Tick(object sender, EventArgs e)
        {
            lock (_balancerProgressTimerLock)
            {
                switch (HeaderStepIndex)
                {
                    case 1: // Ortam Titreşimlerinin Hesaplanması
                        {
                            switch (IterationStepIndex)
                            {
                                case 0:  // Cihaz sıfırlanacak, müdahale etmeyin.
                                    { }
                                    break;
                                case 1:  // Sıfırlama işlemi gerçekleştiriliyor.
                                    {
                                        if (TestTimeCount >= 20) // 2 Sn
                                        {

                                            HighVibrationDataCollectionTimer.Start();
                                            if (TestTimeStatusBar > 50 * 2) // 5 Sn
                                            {

                                                HighVibrationDataCollectionTimer.Stop();
                                                BalancerProgressTimer.Stop();
                                                TestTimeCount = 0;
                                                TestTimeStatusBar = 0;
                                                CalculateVibrationTare();
                                                VibrationsDataBuffer.Clear();
                                                IterationStepIndex++;
                                                BalancingIteration();
                                            }
                                            else
                                            {
                                                TestTimeStatusBar += 2;
                                            }

                                        }
                                        else
                                        {
                                            TestTimeCount++;
                                        }
                                    }
                                    break;
                                case 2:  // Ortam titreşim değeri hesaplanacak cihazına müdahale etmeyin.
                                    { }
                                    break;
                                case 3:  // Ortam titreşim değeri hesaplanıyor.
                                    {
                                        if (TestTimeCount >= 20) // 2 Sn
                                        {

                                            HighVibrationDataCollectionTimer.Start();
                                            if (TestTimeStatusBar > 50 * 2) // 5 Sn
                                            {
                                                HighVibrationDataCollectionTimer.Stop();
                                                BalancerProgressTimer.Stop();
                                                TestTimeCount = 0;
                                                TestTimeStatusBar = 0;
                                                CalculateDeviceBaseStaticVibrationVibration(VibrationsDataBuffer);
                                                VibrationsDataBuffer.Clear();
                                                IterationStepIndex++;
                                                BalancingIteration();
                                            }
                                            else
                                            {
                                                TestTimeStatusBar += 2;
                                            }

                                        }
                                        else
                                        {
                                            TestTimeCount++;
                                        }
                                    }
                                    break;
                                case 4:  // Motor titreşim değeri hesaplanacak cihazına müdahale etmeyin.
                                    { }
                                    break;
                                case 5:  // Motor titreşim değeri hesaplanıyor.
                                    {
                                        if (MotorReadyStatus) // Motor Hazırsa
                                        {
                                            if (TestTimeCount >= 50) // 5 Sn
                                            {

                                                HighVibrationDataCollectionTimer.Start();
                                                if (TestTimeStatusBar > 50 * 2) // 5 Sn
                                                {
                                                    HighVibrationDataCollectionTimer.Stop();
                                                    BalancerProgressTimer.Stop();
                                                    MotorStop();
                                                    TestTimeCount = 0;
                                                    TestTimeStatusBar = 0;
                                                    CalculateMotorBaseRunningVibration(VibrationsDataBuffer);
                                                    VibrationsDataBuffer.Clear();
                                                    IterationStepIndex++;
                                                    BalancingIteration();
                                                }
                                                else
                                                {
                                                    TestTimeStatusBar += 2;
                                                }

                                            }
                                            else
                                            {
                                                TestTimeCount++;
                                            }
                                        }
                                    }
                                    break;
                                case 6:  // Sonuçları kontrol edin.
                                    { }
                                    break;
                            }
                        }
                        break;
                    case 2: // Pervane Titreşiminin Hesaplanması
                        {
                            switch (IterationStepIndex)
                            {
                                case 0:  // Pervane montajını yapın.
                                    { }
                                    break;
                                case 1:  // Pervane titreşim değeri hesaplanacak cihazına müdahale etmeyin.
                                    { }
                                    break;
                                case 2:  // Pervane titreşim değeri hesaplanıyor.
                                    {
                                        if (MotorReadyStatus) // Motor Hazırsa
                                        {
                                            if (TestTimeCount >= 50) // 5 Sn
                                            {

                                                HighVibrationDataCollectionTimer.Start();
                                                if (TestTimeStatusBar > 50 * 2) // 5 Sn
                                                {
                                                    HighVibrationDataCollectionTimer.Stop();
                                                    BalancerProgressTimer.Stop();
                                                    MotorStop();
                                                    TestTimeCount = 0;
                                                    TestTimeStatusBar = 0;
                                                    CalculatePropellerBaseRunningVibration(VibrationsDataBuffer);
                                                    VibrationsDataBuffer.Clear();
                                                    IterationStepIndex++;
                                                    BalancingIteration();
                                                }
                                                else
                                                {
                                                    TestTimeStatusBar += 2;
                                                }

                                            }
                                            else
                                            {
                                                TestTimeCount++;
                                            }
                                        }
                                    }
                                    break;
                                case 3:  // Sonuçları kontrol edin.
                                    { }
                                    break;
                            }
                        }
                        break;
                    case 3: // Birim Referans Bant Uzunluğunun Seçimi
                        {

                        }
                        break;
                    case 4: // Pervanenin Her İki Kanadına Birim Referans Bantın Eklenmesi
                        {
                            switch (IterationStepIndex)
                            {
                                case 0:  // Seçtiğiniz birim referans bandı, pervanenin herhangi bir kanadının merkezinden itibaren yarıçapının yaklaşık %25-30'una denk gelecek şekilde yapıştırın.
                                    { }
                                    break;
                                case 1:  // Pervane titreşim değeri hesaplanacak cihazına müdahale etmeyin.
                                    { }
                                    break;
                                case 2:  // Pervane titreşim değeri hesaplanıyor.
                                    {
                                        if (MotorReadyStatus) // Motor Hazırsa
                                        {
                                            if (TestTimeCount >= 50) // 5 Sn
                                            {

                                                HighVibrationDataCollectionTimer.Start();
                                                if (TestTimeStatusBar > 50 * 2) // 5 Sn
                                                {
                                                    HighVibrationDataCollectionTimer.Stop();
                                                    BalancerProgressTimer.Stop();
                                                    MotorStop();
                                                    TestTimeCount = 0;
                                                    TestTimeStatusBar = 0;
                                                    CalculateFirstBladeVibration(VibrationsDataBuffer);
                                                    VibrationsDataBuffer.Clear();
                                                    IterationStepIndex++;
                                                    BalancingIteration();
                                                }
                                                else
                                                {
                                                    TestTimeStatusBar += 2;
                                                }

                                            }
                                            else
                                            {
                                                TestTimeCount++;
                                            }
                                        }
                                    }
                                    break;
                                case 3:  // Aynı bandı çıkarıp, pervanenin diğer kanadına merkezden itibaren yarıçapının yaklaşık %25-30'una denk gelecek şekilde yapıştırın.
                                    { }
                                    break;
                                case 4:  // Pervane titreşim değeri hesaplanacak cihazına müdahale etmeyin.
                                    { }
                                    break;
                                case 5:  // Pervane titreşim değeri hesaplanıyor.
                                    {
                                        if (MotorReadyStatus) // Motor Hazırsa
                                        {
                                            if (TestTimeCount >= 50) // 5 Sn
                                            {

                                                HighVibrationDataCollectionTimer.Start();
                                                if (TestTimeStatusBar > 50 * 2) // 5 Sn
                                                {
                                                    HighVibrationDataCollectionTimer.Stop();
                                                    BalancerProgressTimer.Stop();
                                                    MotorStop();
                                                    TestTimeCount = 0;
                                                    TestTimeStatusBar = 0;
                                                    CalculateSecondBladeVibration(VibrationsDataBuffer);
                                                    VibrationsDataBuffer.Clear();
                                                    IterationStepIndex++;
                                                    BalancingIteration();
                                                }
                                                else
                                                {
                                                    TestTimeStatusBar += 2;
                                                }

                                            }
                                            else
                                            {
                                                TestTimeCount++;
                                            }
                                        }
                                    }
                                    break;
                                case 6:  // Sonuçları kontrol edin.
                                    { }
                                    break;
                            }
                        }
                        break;
                    case 5: // Düzeltici Bant Adetinin ve Yönünün Belirlenmesi, Düzeltici Bantın Eklenmesi"
                        {

                        }
                        break;
                    case 6: // Test ve Sonuçların Kontrolü
                        {
                            switch (IterationStepIndex)
                            {
                                case 0:  // Pervane titreşim değeri hesaplanacak cihazına müdahale etmeyin.
                                    { }
                                    break;
                                case 1:  // Pervane titreşim değeri hesaplanıyor.
                                    {
                                        if (MotorReadyStatus) // Motor Hazırsa
                                        {
                                            if (TestTimeCount >= 50) // 5 Sn
                                            {

                                                HighVibrationDataCollectionTimer.Start();
                                                if (TestTimeStatusBar > 50 * 2) // 5 Sn
                                                {
                                                    HighVibrationDataCollectionTimer.Stop();
                                                    BalancerProgressTimer.Stop();
                                                    MotorStop();
                                                    TestTimeCount = 0;
                                                    TestTimeStatusBar = 0;
                                                    CalculateBalancedPropellerRunningVibration(VibrationsDataBuffer);
                                                    VibrationsDataBuffer.Clear();
                                                    IterationStepIndex++;
                                                    BalancingIteration();
                                                }
                                                else
                                                {
                                                    TestTimeStatusBar += 2;
                                                }

                                            }
                                            else
                                            {
                                                TestTimeCount++;
                                            }
                                        }
                                    }
                                    break;
                                case 2:  // Sonuçları kontrol edin.
                                    { }
                                    break;
                            }
                        }
                        break;
                }
            }
        }

        private void HighVibrationDataCollectionTimer_Tick(object sender, EventArgs e)
        {
            lock (_highVibrationDataCollectionTimerLock)
            {
                VibrationsDataBuffer.Add(HighVibration);
            }
        }

        private void PIDTimer_Tick(object sender, EventArgs e)
        {
            lock (_pidTimerLock)
            {
                double currentSpeed = _interfaceVariables.MotorSpeed.Value;
                double pidOutput = PIDController.Calculate(_interfaceVariables.ReferenceMotorSpeed, currentSpeed);

                // Map PID output to ESC range
                double minOutput = 0;
                double maxOutput = 100;
                double minESC = 800;
                double maxESC = 2200;
                pidOutput = (pidOutput - minOutput) * (maxESC - minESC) / (maxOutput - minOutput) + minESC;

                pidOutput = Math.Clamp(pidOutput, 800, 2200);

                ESCValue = SmoothTransition(ESCValue, (int)pidOutput);
            }
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

        public Visibility AutoProgressCountVisibility
        {
            get => _autoProgressCountVisibility;
            set
            {
                if (SetProperty(ref _autoProgressCountVisibility, value))
                {
                    OnPropertyChanged(nameof(AutoProgressCountVisibility));
                }
            }
        }
        public Visibility RecommendedTableVisibility
        {
            get => _recommendedTableVisibility;
            set
            {
                if (SetProperty(ref _recommendedTableVisibility, value))
                {
                    OnPropertyChanged(nameof(RecommendedTableVisibility));
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
        public Visibility RunButtonVisibility
        {
            get => _runButtonVisibility;
            set
            {
                if (SetProperty(ref _runButtonVisibility, value))
                {
                    OnPropertyChanged(nameof(RunButtonVisibility));
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
                    _interfaceVariables.Vibration.HighVibration = value;
                    OnPropertyChanged(nameof(HighVibration));
                }
            }
        }
        public double DeviceBaseStaticVibration
        {
            get => _deviceBaseStaticVibration;
            set
            {
                if (_deviceBaseStaticVibration != value)
                {
                    _deviceBaseStaticVibration = value;
                    _interfaceVariables.DeviceBaseStaticVibration = value;
                    OnPropertyChanged(nameof(DeviceBaseStaticVibration));
                }
            }
        }

        public double MotorBaseRunningVibration
        {
            get => _motorBaseRunningVibration;
            set
            {
                if (_motorBaseRunningVibration != value)
                {
                    _motorBaseRunningVibration = value;
                    _interfaceVariables.MotorBaseRunningVibration = value;
                    OnPropertyChanged(nameof(MotorBaseRunningVibration));
                }
            }
        }

        public double PropellerBaseRunningVibration
        {
            get => _propellerBaseRunningVibration;
            set
            {
                if (_propellerBaseRunningVibration != value)
                {
                    _propellerBaseRunningVibration = value;
                    _interfaceVariables.PropellerBaseRunningVibration = value;
                    OnPropertyChanged(nameof(PropellerBaseRunningVibration));
                }
            }
        }

        public double BalancedPropellerRunningVibration
        {
            get => _balancedPropellerRunningVibration;
            set
            {
                if (SetProperty(ref _balancedPropellerRunningVibration, value))
                {
                    _balancedPropellerRunningVibration = value;
                    _interfaceVariables.BalancedPropellerRunningVibration = value;
                    OnPropertyChanged(nameof(BalancedPropellerRunningVibration));
                }
            }
        }
        public double FirstBladeVibration
        {
            get => _firstBladeVibration;
            set
            {
                if (SetProperty(ref _firstBladeVibration, value))
                {
                    _interfaceVariables.FirstBladeVibration = value;
                    OnPropertyChanged(nameof(FirstBladeVibration));
                }
            }
        }
        public double SecondBladeVibration
        {
            get => _secondBladeVibration;
            set
            {
                if (SetProperty(ref _secondBladeVibration, value))
                {
                    _interfaceVariables.SecondBladeVibration = value;
                    OnPropertyChanged(nameof(SecondBladeVibration));
                }
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
        public double EqualizerTapeCoefficient
        {
            get => _equalizerTapeCoefficient;
            set
            {
                if (SetProperty(ref _equalizerTapeCoefficient, value))
                {
                    _interfaceVariables.EqualizerTapeCoefficient = value;
                    OnPropertyChanged(nameof(EqualizerTapeCoefficient));
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
        public string AutoProgressCountIcon
        {
            get => _autoProgressCountIcon;
            set
            {
                if (SetProperty(ref _autoProgressCountIcon, value))
                {
                    OnPropertyChanged(nameof(AutoProgressCountIcon));
                }
            }
        }

        public List<double> VibrationsDataBuffer
        {
            get => _vibrationsDataBuffer;
            set
            {
                if (_vibrationsDataBuffer != value)
                {
                    _vibrationsDataBuffer = value;
                    OnPropertyChanged(nameof(VibrationsDataBuffer));
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

        public ObservableCollection<System.Windows.Media.Brush> StepIndicators
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
                new IterationStep // 0
            {
                Header = "Cihazın Hazırlanması",
                Steps = new List<string>
                {
                    "1.) Cihazı uygun şekilde sabitleyin. Çevresel dengesizlik veya belirsizliğe sebep olabilecek koşullardan arındırdığınızdan emin olun.",
                    "2.) Motor montajını yapın.",
                    "3.) Elektronik bağlantıları kontrol edin.",
                    "4.) Pervane verilerini ayarlayın.",
                    "5.) Referans motor hız değerini ayarlayın."
                }
            },
            new IterationStep // 1
            {
                Header = "Ortam ve Motor Titreşimlerinin Hesaplanması",
                Steps = new List<string>
                {
                    "Cihaz sıfırlanacak, müdahale etmeyin.",
                    "Sıfırlama işlemi gerçekleştiriliyor.",
                    "Ortam titreşim değeri hesaplanacak cihazına müdahale etmeyin.",
                    "Ortam titreşim değeri hesaplanıyor.",
                    "Motor titreşim değeri hesaplanacak cihazına müdahale etmeyin.",
                    "Motor titreşim değeri hesaplanıyor.",
                    "Sonuçları kontrol edin."
                }
            },
            new IterationStep // 2
            {
                Header = "Pervane Titreşiminin Hesaplanması",
                Steps = new List<string>
                {
                    "Pervane montajını yapın.",
                    "Pervane titreşim değeri hesaplanacak cihazına müdahale etmeyin.",
                    "Pervane titreşim değeri hesaplanıyor.",
                    "Sonuçları kontrol edin."
                }
            },
            new IterationStep // 3
            {
                Header = "Birim Referans Bant Uzunluğunun Seçimi",
                Steps = new List<string>
                {
                    "Lütfen standart elektrik bandı kullanın veya genişliği 15-20 mm arasında olan bir bant tercih edin."
                }
            },
            new IterationStep // 4
            {
                Header = "Pervanenin Her İki Kanadına Birim Referans Bantın Eklenmesi",
                Steps = new List<string>
                {
                    "Seçtiğiniz birim referans bandı, pervanenin herhangi bir kanadının merkezinden itibaren yarıçapının yaklaşık %25-30'una denk gelecek şekilde yapıştırın.",
                    "Pervane titreşim değeri hesaplanacak cihazına müdahale etmeyin.",
                    "Pervane titreşim değeri hesaplanıyor.",
                    "Aynı bandı çıkarıp, pervanenin diğer kanadına merkezden itibaren yarıçapının yaklaşık %25-30'una denk gelecek şekilde yapıştırın.",
                    "Pervane titreşim değeri hesaplanacak cihazına müdahale etmeyin.",
                    "Pervane titreşim değeri hesaplanıyor.",
                    "Sonuçları kontrol edin."
                }
            },
            new IterationStep // 5
            {
                Header = "Düzeltici Bant Adetinin ve Yönünün Belirlenmesi, Düzeltici Bantın Eklenmesi",
                Steps = new List<string>
                {
                    "Düzeltici yön ve bant adedi hesaplandı.",
                    "Lütfen seçtiğiniz birim referans bandı ilk durumundaki yere geri yapıştırın.",
                    " adet daha ilave düzeltici bantı, aerodinamik yapıyı bozmayacak şekilde referans bantın olduğu bölgeye yapıştırın."
                }
            },
            new IterationStep // 6
            {
                Header = "Test ve Sonuçların Kontrolü",
                Steps = new List<string>
                {
                    "Pervane titreşim değeri hesaplanacak cihazına müdahale etmeyin.",
                    "Pervane titreşim değeri hesaplanıyor.",
                    "Sonuçları kontrol edin."
                }
            }
            };
        }
    }
}
