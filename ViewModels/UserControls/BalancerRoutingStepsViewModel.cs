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
using Advanced_Dynotis_Software.Properties;
using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.Services.Controllers;
using Advanced_Dynotis_Software.Services.Helpers;
using Advanced_Dynotis_Software.Services.Logger;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.VisualBasic;
using DocumentFormat.OpenXml.ExtendedProperties;
using Advanced_Dynotis_Software.Views.UserControls;
using System.IO;
using Newtonsoft.Json;

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



        private Visibility _iterationWarningMessageVisibility;
        private Visibility _autoProgressCountVisibility;
        private Visibility _recommendedTableVisibility;
        private Visibility _repeatStepButtonVisibility;
        private Visibility _approvalStepButtonVisibility;
        private Visibility _nextStepButtonVisibility;
        private Visibility _runButtonVisibility;

        // Main Buttons
        public ICommand AddTestButtonCommand { get; }
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
        private double _highIPSVibration;
        private List<double> _vibrationsDataBuffer;
        private List<double> _vibrationsIPSDataBuffer;


        private readonly object _balancerProgressTimerLock = new object();
        private readonly object _autoProgressCountTimerLock = new object();
        private readonly object _highVibrationDataCollectionTimerLock = new object();
        private readonly object _pidTimerLock = new object();

        // Propeller Vibration 
        private List<double> _testStepsPropellerVibrations;

        //StepChart
        private ObservableCollection<double> _balancerIterationStepChart;
        private ObservableCollection<double> _balancerIterationVibrationsChart;
        private ObservableCollection<string> _balancerIterationDescription;

        // Step Indicators
        private ObservableCollection<System.Windows.Media.Brush> _stepIndicators;

        // File
        private string _balancedPropellersID;
        private double _balancedPropellersDiameter;
        private ObservableCollection<DateTime> _balancedPropellersTestDates;
        private ObservableCollection<double> _balancedPropellersVibrations;

        // Constructor
        public BalancerRoutingStepsViewModel(DynotisData dynotisData, InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
            _dynotisData = dynotisData;

            _interfaceVariables.PropertyChanged += InterfaceVariables_PropertyChanged;
            _vibrationsDataBuffer = new List<double>();
            _vibrationsIPSDataBuffer = new List<double>();
            _testStepsPropellerVibrations = new List<double>();
            _balancerIterationStepChart = new ObservableCollection<double>();
            _balancerIterationVibrationsChart = new ObservableCollection<double>();
            _balancerIterationDescription = new ObservableCollection<string>();
            _balancedPropellersTestDates = new ObservableCollection<DateTime>();
            _balancedPropellersVibrations = new ObservableCollection<double>();

            // Initialize PID Controller
            PIDController = new PIDController(1.5, 0.03, 0.05);

            AddTestButtonCommand = new RelayCommand(param => AddTestFileCommand());
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
                StepIndicators.Add((SolidColorBrush)System.Windows.Application.Current.Resources["BalancerRoutingStepsPassive"]);
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
            DeviceBaseStaticVibration = 0;
            MotorBaseRunningVibration = 0;
            PropellerBaseRunningVibration = 0;
            FirstBladeVibration = 0;
            SecondBladeVibration = 0;
            EqualizerTapeCoefficient = 0;
            EqualizerDirection = "";
            BalancedPropellerRunningVibration = 0;

            // Clear Buffers and Charts
            VibrationsDataBuffer.Clear();
            VibrationsIPSDataBuffer.Clear();
            TestStepsPropellerVibrations.Clear();
            BalancerIterationStepChart.Clear();
            BalancerIterationVibrationsChart.Clear();

            // Set Buttons Visibility
            IterationWarningMessageVisibility = Visibility.Collapsed;
            AutoProgressCountVisibility = Visibility.Collapsed;
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

        private void AddTestFileCommand()
        {
            MessageBoxResult result = MessageBox.Show(
           "The last test data will be saved. Are you sure you want to save the test results?", 
           "Confirm Result Save",
           MessageBoxButton.YesNo,
           MessageBoxImage.Information);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    BalancedPropellersID = _interfaceVariables.BalancedPropellersID;
                    BalancedPropellersDiameter = _interfaceVariables.BalancedPropellersDiameter;
                    var filePath = Path.Combine("BalancedPropellers", BalancedPropellersID + ".json");
                    // Dosya varsa, içeriğini yükle, yoksa yeni bir obje oluştur
                    BalancedDataset propellerData;
                    if (File.Exists(filePath))
                    {
                        var json = File.ReadAllText(filePath);
                        propellerData = JsonConvert.DeserializeObject<BalancedDataset>(json);
                    }
                    else
                    {
                        propellerData = new BalancedDataset
                        {
                            PropellerID = BalancedPropellersID,
                            PropellerDiameter = BalancedPropellersDiameter,
                            TestDates = new ObservableCollection<DateTime>(),
                            Vibrations = new ObservableCollection<double>()
                        };
                    }
                    var currentDate = DateTime.Now;
                    var VibrationData = BalancedPropellerRunningVibration; // Dengeli Pervane Çalışma Titreşimi

                    propellerData.TestDates.Add(currentDate);
                    propellerData.Vibrations.Add(Math.Round(VibrationData, 3));
                    // Dosyayı tekrar kaydet
                    var updatedJson = JsonConvert.SerializeObject(propellerData, Formatting.Indented);
                    File.WriteAllText(filePath, updatedJson);

                    // Bilgilendirme mesajı
                    MessageBox.Show("Test Mesaj: " + BalancedPropellersID + " " + propellerData.PropellerDiameter
                                    + "\nTest Date: " + currentDate + "\nVibration: " + Math.Round(VibrationData, 3) + " IPS");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    System.Diagnostics.Debug.WriteLine($"Error in AddTestCommand: {ex.Message}");
                }
            }
        }

        private void RunCommand()
        {
            BalancingIteration();
        }

        private void StopCommand()
        {
            AutoProgressCountTimer.Stop();
            BalancerProgressTimer.Stop();
            HighVibrationDataCollectionTimer.Stop();
            IterationStepIndex = 0;
            BalancingIteration();
            MotorStop();
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
            switch (HeaderStepIndex)
            {
                case 1: // Ortam Titreşimlerinin Hesaplanması
                    {                      
                        switch (IterationStepIndex)
                        {
                            case 6:  // Sonuçları kontrol edin.
                                {
                                    BalancerIterationVibrationsChart.Add(MotorBaseRunningVibration);
                                    BalancerIterationStepChart.Add(BalancerIterationVibrationsChart.Count);
                                    BalancerIterationDescription.Add("Motor Base Running Vibration");
                                    BalancerIterationStep = BalancerIterationVibrationsChart.Count;

                                    TestStepsPropellerVibrations.Add(MotorBaseRunningVibration);
                                }
                                break;
                        }
                        IterationStepIndex = 0;
                    }
                    break;
                case 2: // Pervane Titreşiminin Hesaplanması
                    {                       
                        switch (IterationStepIndex)
                        {
                            case 3:  // Sonuçları kontrol edin.
                                {
                                    BalancerIterationVibrationsChart.Add(PropellerBaseRunningVibration);
                                    BalancerIterationStepChart.Add(BalancerIterationVibrationsChart.Count);
                                    BalancerIterationDescription.Add("Propeller Base Running Vibration");
                                    BalancerIterationStep = BalancerIterationVibrationsChart.Count;

                                    TestStepsPropellerVibrations.Add(PropellerBaseRunningVibration);
                                }
                                break;
                        }
                        IterationStepIndex = 1;
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
                            case 6:  // Sonuçları kontrol edin.
                                {
                                    BalancerIterationVibrationsChart.Add(FirstBladeVibration);
                                    BalancerIterationStepChart.Add(BalancerIterationVibrationsChart.Count);
                                    BalancerIterationDescription.Add("First Blade Vibration");
                                    BalancerIterationStep = BalancerIterationVibrationsChart.Count;

                                    BalancerIterationVibrationsChart.Add(SecondBladeVibration);
                                    BalancerIterationStepChart.Add(BalancerIterationVibrationsChart.Count);
                                    BalancerIterationDescription.Add("Second Blade Vibration");
                                    BalancerIterationStep = BalancerIterationVibrationsChart.Count;

                                    TestStepsPropellerVibrations.Add(FirstBladeVibration);
                                    TestStepsPropellerVibrations.Add(SecondBladeVibration);
                                }
                                break;
                        }
                        IterationStepIndex = 0;
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
                            case 2:  // Sonuçları kontrol edin.
                                {
                                    BalancerIterationVibrationsChart.Add(BalancedPropellerRunningVibration);
                                    BalancerIterationStepChart.Add(BalancerIterationVibrationsChart.Count);
                                    BalancerIterationDescription.Add("Balanced Propeller Running Vibration");
                                    BalancerIterationStep = BalancerIterationVibrationsChart.Count;

                                    TestStepsPropellerVibrations.Add(BalancedPropellerRunningVibration);
                                }
                                break;
                        }
                        IterationStepIndex = 0;
                    }
                    break;
            }
            // Set Buttons Visibility
            RepeatStepButtonVisibility = Visibility.Collapsed;
            ApprovalStepButtonVisibility = Visibility.Collapsed;
            NextStepButtonVisibility = Visibility.Visible;
            BalancingIteration();
        }

        private void ApprovalCommand()
        {
            switch (HeaderStepIndex)
            {
                case 1: // Ortam Titreşimlerinin Hesaplanması
                    {
                        switch (IterationStepIndex)
                        {
                            case 6:  // Sonuçları kontrol edin.
                                {
                                    BalancerIterationVibrationsChart.Add(MotorBaseRunningVibration);                                    
                                    BalancerIterationStepChart.Add(BalancerIterationVibrationsChart.Count);
                                    BalancerIterationDescription.Add("Motor Base Running Vibration");
                                    BalancerIterationStep = BalancerIterationVibrationsChart.Count;

                                    TestStepsPropellerVibrations.Add(MotorBaseRunningVibration);
                                }
                                break;
                        }
                    }
                    break;
                case 2: // Pervane Titreşiminin Hesaplanması
                    {
                        switch (IterationStepIndex)
                        {
                            case 3:  // Sonuçları kontrol edin.
                                {
                                    BalancerIterationVibrationsChart.Add(PropellerBaseRunningVibration);                                    
                                    BalancerIterationStepChart.Add(BalancerIterationVibrationsChart.Count);                             
                                    BalancerIterationDescription.Add("Propeller Base Running Vibration");
                                    BalancerIterationStep = BalancerIterationVibrationsChart.Count;

                                    TestStepsPropellerVibrations.Add(PropellerBaseRunningVibration);
                                }
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
                            case 6:  // Sonuçları kontrol edin.
                                {
                                    BalancerIterationVibrationsChart.Add(FirstBladeVibration);
                                    BalancerIterationStepChart.Add(BalancerIterationVibrationsChart.Count);                                    
                                    BalancerIterationDescription.Add("First Blade Vibration");
                                    BalancerIterationStep = BalancerIterationVibrationsChart.Count;

                                    BalancerIterationVibrationsChart.Add(SecondBladeVibration);
                                    BalancerIterationStepChart.Add(BalancerIterationVibrationsChart.Count);
                                    BalancerIterationDescription.Add("Second Blade Vibration");
                                    BalancerIterationStep = BalancerIterationVibrationsChart.Count;

                                    TestStepsPropellerVibrations.Add(FirstBladeVibration);
                                    TestStepsPropellerVibrations.Add(SecondBladeVibration);
                                }
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
                            case 2:  // Sonuçları kontrol edin.
                                {
                                    BalancerIterationVibrationsChart.Add(BalancedPropellerRunningVibration);
                                    BalancerIterationStepChart.Add(BalancerIterationVibrationsChart.Count);
                                    BalancerIterationDescription.Add("Balanced Propeller Running Vibration");
                                    BalancerIterationStep = BalancerIterationVibrationsChart.Count;

                                    TestStepsPropellerVibrations.Add(BalancedPropellerRunningVibration);
                                }
                                break;
                        }
                    }
                    break;
            }
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
                IterationWarningMessageVisibility = Visibility.Collapsed;
            }
            else if (status == "AllCollapsed")
            {
                AutoProgressCountVisibility = Visibility.Collapsed;
                RepeatStepButtonVisibility = Visibility.Collapsed;
                ApprovalStepButtonVisibility = Visibility.Collapsed;
                NextStepButtonVisibility = Visibility.Collapsed;
                IterationWarningMessageVisibility = Visibility.Visible;
            }
            else if (status == "AllButtonVisible")
            {
                AutoProgressCountVisibility = Visibility.Collapsed;
                RepeatStepButtonVisibility = Visibility.Visible;
                ApprovalStepButtonVisibility = Visibility.Visible;
                NextStepButtonVisibility = Visibility.Visible;
                IterationWarningMessageVisibility = Visibility.Collapsed;
            }
            else if (status == "ApprovalRepeatStepVisible")
            {
                AutoProgressCountVisibility = Visibility.Collapsed;
                RepeatStepButtonVisibility = Visibility.Visible;
                ApprovalStepButtonVisibility = Visibility.Visible;
                NextStepButtonVisibility = Visibility.Collapsed;
                IterationWarningMessageVisibility = Visibility.Collapsed;
            }
            else if (status == "NextStepVisible")
            {
                AutoProgressCountVisibility = Visibility.Collapsed;
                RepeatStepButtonVisibility = Visibility.Collapsed;
                ApprovalStepButtonVisibility = Visibility.Collapsed;
                NextStepButtonVisibility = Visibility.Visible;
                IterationWarningMessageVisibility = Visibility.Collapsed;
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
                                                "Motor Base Running Vibration: " + MotorBaseRunningVibration.ToString("0.000") + " IPS";
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
                            case 3:  // Sonuçları kontrol edin.
                                {
                                    SetVisibility("ApprovalRepeatStepVisible");
                                    Iteration = Iteration + "\r\n" +
                                                "Propeller Base Running Vibration: " + PropellerBaseRunningVibration.ToString("0.000") + " IPS";
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
                                    if(FirstBladeVibration>SecondBladeVibration)
                                    {
                                        Iteration = Iteration + "\r\n" +
                                                    "Measured vibration: " + SecondBladeVibration.ToString("0.000") + " IPS";
                                    }
                                    else if (SecondBladeVibration > FirstBladeVibration)
                                    {
                                        Iteration = Iteration + "\r\n" +
                                                    "Measured vibration: " + FirstBladeVibration.ToString("0.000") + " IPS";
                                    }
                                    else
                                    {
                                        Iteration = Iteration + "\r\n" +
                                                    "Measured vibration: " + FirstBladeVibration.ToString("0.000") + " IPS" + "\r\n" +
                                                    "Measured vibration: " + SecondBladeVibration.ToString("0.000") + " IPS";
                                    }
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

                        EqualizerTapeCoefficient = (((1 / Ratio) - 1) / 2.0); // (-1) şuan var olan referans bantı temsil ediyor.

                        if (EqualizerDirection == "First Blade")
                        {
                            Iteration = IterationSteps[HeaderStepIndex].Steps[0] + "\r\n" +
                                        IterationSteps[HeaderStepIndex].Steps[1] + "\r\n" +
                                        IterationSteps[HeaderStepIndex].Steps[2] + " " + EqualizerTapeCoefficient.ToString("0.0");
                        }
                        else if (EqualizerDirection == "Second Blade")
                        {
                            Iteration = IterationSteps[HeaderStepIndex].Steps[0] + "\r\n" +
                                        IterationSteps[HeaderStepIndex].Steps[2] + " " + EqualizerTapeCoefficient.ToString("0.0");
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
                                                "Balanced Propeller Running Vibration: " + BalancedPropellerRunningVibration.ToString("0.000") + " IPS";
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
                        AddTestFileCommand();
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
            StepIndicators[index] = (SolidColorBrush)System.Windows.Application.Current.Resources["BalancerRoutingStepsActive"];
            for (int i = 0; i < index; i++)
            {
                StepIndicators[i] = (SolidColorBrush)System.Windows.Application.Current.Resources["BalancerRoutingStepsOK"];
            }
            for (int i = index + 1; i < IterationSteps.Count; i++)
            {
                StepIndicators[i] = (SolidColorBrush)System.Windows.Application.Current.Resources["BalancerRoutingStepsPassive"];
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
        private void HighVibrationDataCollectionTimer_Tick(object sender, EventArgs e)
        {
            lock (_highVibrationDataCollectionTimerLock)
            {
                if (_interfaceVariables.Vibration.HighIPSVibration > 0) 
                {                   
                    HighIPSVibration = _interfaceVariables.Vibration.HighIPSVibration;                 
                    VibrationsIPSDataBuffer.Add(HighIPSVibration);                   
                    //Logger.Log($"HighIPSVibration:  {HighIPSVibration.ToString("0.000")}");
                }

                HighVibration = _interfaceVariables.Vibration.HighVibration;
                VibrationsDataBuffer.Add(HighVibration);
                //Logger.Log($"HighVibration:     {HighVibration.ToString("0.000")}");
            }
        }
        private double CalculateHighIPSVibrations(List<double> buffer)
        {
            double average = buffer.Average();
            var aboveAverageValues = buffer.Where(x => x > average);
            if (!aboveAverageValues.Any()) { return 0; }
            return aboveAverageValues.Average();
        }
        private void CalculateDeviceBaseStaticVibrationVibration()
        {
            DeviceBaseStaticVibration = VibrationsDataBuffer.Sum() / VibrationsDataBuffer.Count;
            VibrationsDataBuffer.Clear();
        }

        private void CalculateMotorBaseRunningVibration()
        {
            MotorBaseRunningVibration = CalculateHighIPSVibrations(VibrationsIPSDataBuffer);
            VibrationsIPSDataBuffer.Clear();
        }
        private void CalculatePropellerBaseRunningVibration()
        {
            PropellerBaseRunningVibration = CalculateHighIPSVibrations(VibrationsIPSDataBuffer);
            VibrationsIPSDataBuffer.Clear();
        }
        private void CalculateBalancedPropellerRunningVibration()
        {
            BalancedPropellerRunningVibration = CalculateHighIPSVibrations(VibrationsIPSDataBuffer);
            VibrationsIPSDataBuffer.Clear();
        }
        private void CalculateFirstBladeVibration()
        {
            FirstBladeVibration = CalculateHighIPSVibrations(VibrationsIPSDataBuffer);
            VibrationsIPSDataBuffer.Clear();
        }
        private void CalculateSecondBladeVibration()
        {
            SecondBladeVibration = CalculateHighIPSVibrations(VibrationsIPSDataBuffer);
            VibrationsIPSDataBuffer.Clear();
        }

        private void MotorStop()
        {
            PIDTimer.Stop();
            ESCStatus = false;
            ESCValue = 800;
            MotorReadyStatus = false;
        }

        private async Task MotorStopAsync()
        {
            // PID kontrolü durduruluyor
            PIDTimer.Stop();

            // Gecikme eklemek için belirli bir süre bekleme
            await Task.Delay(2000); // 1000 ms = 1 saniye

            // Motor durdurma işlemleri
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
        int timeTest = 2; 
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
                                            if (TestTimeStatusBar > 100) // 10 Sn
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
                                                TestTimeStatusBar += timeTest;
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
                                        if (TestTimeCount >= 30) // 3 Sn
                                        {

                                            HighVibrationDataCollectionTimer.Start();
                                            if (TestTimeStatusBar > 100) // 10 Sn
                                            {
                                                HighVibrationDataCollectionTimer.Stop();
                                                BalancerProgressTimer.Stop();
                                                TestTimeCount = 0;
                                                TestTimeStatusBar = 0;
                                                CalculateDeviceBaseStaticVibrationVibration();
                                                IterationStepIndex++;
                                                BalancingIteration();
                                            }
                                            else
                                            {
                                                TestTimeStatusBar += timeTest;
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
                                            if (TestTimeCount >= 100) // 10 Sn
                                            {

                                                HighVibrationDataCollectionTimer.Start();
                                                if (TestTimeStatusBar > 100) // 100 Sn
                                                {
                                                    HighVibrationDataCollectionTimer.Stop();
                                                    BalancerProgressTimer.Stop();                                                   
                                                    TestTimeCount = 0;
                                                    TestTimeStatusBar = 0;
                                                    CalculateMotorBaseRunningVibration();
                                                    IterationStepIndex++;
                                                    BalancingIteration();
                                                    MotorStopAsync();
                                                }
                                                else
                                                {
                                                    TestTimeStatusBar += timeTest;
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
                                            if (TestTimeCount >= 100) // 10 Sn
                                            {

                                                HighVibrationDataCollectionTimer.Start();
                                                if (TestTimeStatusBar > 100) // 10 Sn
                                                {
                                                    HighVibrationDataCollectionTimer.Stop();
                                                    BalancerProgressTimer.Stop();
                                                    TestTimeCount = 0;
                                                    TestTimeStatusBar = 0;
                                                    CalculatePropellerBaseRunningVibration();
                                                    IterationStepIndex++;
                                                    BalancingIteration();
                                                    MotorStopAsync();
                                                }
                                                else
                                                {
                                                    TestTimeStatusBar += timeTest;
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
                                            if (TestTimeCount >= 100) // 10 Sn
                                            {

                                                HighVibrationDataCollectionTimer.Start();
                                                if (TestTimeStatusBar > 100) // 10 Sn
                                                {
                                                    HighVibrationDataCollectionTimer.Stop();
                                                    BalancerProgressTimer.Stop();
                                                    TestTimeCount = 0;
                                                    TestTimeStatusBar = 0;
                                                    CalculateFirstBladeVibration();
                                                    IterationStepIndex++;
                                                    BalancingIteration();
                                                    MotorStopAsync();
                                                }
                                                else
                                                {
                                                    TestTimeStatusBar += timeTest;
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
                                            if (TestTimeCount >= 100) // 10 Sn
                                            {

                                                HighVibrationDataCollectionTimer.Start();
                                                if (TestTimeStatusBar > 100) // 10 Sn
                                                {
                                                    HighVibrationDataCollectionTimer.Stop();
                                                    BalancerProgressTimer.Stop();
                                                    TestTimeCount = 0;
                                                    TestTimeStatusBar = 0;
                                                    CalculateSecondBladeVibration();
                                                    IterationStepIndex++;
                                                    BalancingIteration();
                                                    MotorStopAsync();
                                                }
                                                else
                                                {
                                                    TestTimeStatusBar += timeTest;
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
                                            if (TestTimeCount >= 100) // 10 Sn
                                            {

                                                HighVibrationDataCollectionTimer.Start();
                                                if (TestTimeStatusBar > 100) // 10 Sn
                                                {
                                                    HighVibrationDataCollectionTimer.Stop();
                                                    BalancerProgressTimer.Stop();
                                                    TestTimeCount = 0;
                                                    TestTimeStatusBar = 0;
                                                    CalculateBalancedPropellerRunningVibration();
                                                    IterationStepIndex++;
                                                    BalancingIteration();
                                                    MotorStopAsync();
                                                }
                                                else
                                                {
                                                    TestTimeStatusBar += timeTest;
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
            if (e.PropertyName == nameof(_interfaceVariables.Vibration.HighVibration) ||
                e.PropertyName == nameof(_interfaceVariables.Vibration.HighVibration))
            {
                HighVibration = _interfaceVariables.Vibration.HighVibration;
                HighIPSVibration = _interfaceVariables.Vibration.HighIPSVibration;
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
        public double HighIPSVibration
        {
            get => _highIPSVibration;
            set
            {
                if (_highIPSVibration != value)
                {
                    _highIPSVibration = value;
                    _interfaceVariables.Vibration.HighIPSVibration = value;
                    OnPropertyChanged(nameof(HighIPSVibration));
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
        public List<double> VibrationsIPSDataBuffer
        {
            get => _vibrationsIPSDataBuffer;
            set
            {
                if (_vibrationsIPSDataBuffer != value)
                {
                    _vibrationsIPSDataBuffer = value;
                    OnPropertyChanged(nameof(VibrationsIPSDataBuffer));
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
        public ObservableCollection<double> BalancerIterationStepChart
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

        public ObservableCollection<string> BalancerIterationDescription
        {
            get => _balancerIterationDescription;
            set
            {
                if (SetProperty(ref _balancerIterationDescription, value))
                {
                    _interfaceVariables.BalancerIterationDescription = value;
                    OnPropertyChanged(nameof(BalancerIterationDescription));
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
        public string BalancedPropellersID
        {
            get => _balancedPropellersID;
            set
            {
                if (SetProperty(ref _balancedPropellersID, value))
                {
                    _interfaceVariables.BalancedPropellersID = value;
                    OnPropertyChanged(nameof(BalancedPropellersID));
                }
            }
        }
        public double BalancedPropellersDiameter
        {
            get => _balancedPropellersDiameter;
            set
            {
                if (SetProperty(ref _balancedPropellersDiameter, value))
                {
                    _interfaceVariables.BalancedPropellersDiameter = value;
                    OnPropertyChanged(nameof(BalancedPropellersDiameter));
                }
            }
        }
        public ObservableCollection<DateTime> BalancedPropellersTestDates
        {
            get => _balancedPropellersTestDates;
            set
            {
                if (SetProperty(ref _balancedPropellersTestDates, value))
                {
                    _interfaceVariables.BalancedPropellersTestDates = value;
                    OnPropertyChanged(nameof(BalancedPropellersTestDates));
                }
            }
        }

        public ObservableCollection<double> BalancedPropellersVibrations
        {
            get => _balancedPropellersVibrations;
            set
            {
                if (SetProperty(ref _balancedPropellersVibrations, value))
                {
                    _interfaceVariables.BalancedPropellersVibrations = value;
                    OnPropertyChanged(nameof(BalancedPropellersVibrations));
                }
            }
        }

        //  Testing in progress, do not touch the device anytime during measurement process
        public Visibility IterationWarningMessageVisibility
        {
            get => _iterationWarningMessageVisibility;
            set
            {
                if (SetProperty(ref _iterationWarningMessageVisibility, value))
                {
                    OnPropertyChanged(nameof(IterationWarningMessageVisibility));
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
                Header = Resources.BalancerPage_IterationStep0Header,   //  Preparing the Device
                Steps = new List<string>
                {
                    Resources.BalancerPage_IterationStep0Message1,      //  1.) Mount the Dynotis on the ground as level as possible and make sure the surrounding area is clear
                    Resources.BalancerPage_IterationStep0Message2,      //  2.) Mount the motor only
                    Resources.BalancerPage_IterationStep0Message3,      //  3.) Check the electrical connections
                    Resources.BalancerPage_IterationStep0Message4,      //  4.) Enter propeller details
                    Resources.BalancerPage_IterationStep0Message5       //  5.) Set reference RPM               
                }
            },
            new IterationStep // 1
            {
                Header = Resources.BalancerPage_IterationStep1Header,   //  Initialization and measurement of motor only vibration
                Steps = new List<string>
                {
                    Resources.BalancerPage_IterationStep1Message1,      //  The sensor will be calibrated + Testing in progress, do not touch the device anytime during measurement process
                    Resources.BalancerPage_IterationStep1Message2,      //  The sensor is being calibrated
                    Resources.BalancerPage_IterationStep1Message3,      //  Vibration noise will be calculated + Testing in progress, do not touch the device anytime during measurement process
                    Resources.BalancerPage_IterationStep1Message4,      //  Vibration noise value is being measured
                    Resources.BalancerPage_IterationStep1Message5,      //  Electric motor's vibration will be measured + Testing in progress, do not touch the device anytime during measurement process
                    Resources.BalancerPage_IterationStep1Message6,      //  Electric motor's vibration is being measured
                    Resources.BalancerPage_IterationStep1Message7       //  Please check the measurement
                }
            },
            new IterationStep // 2
            {
                Header = Resources.BalancerPage_IterationStep2Header,   //  Calculation of propulsion system's vibration
                Steps = new List<string>
                {
                    Resources.BalancerPage_IterationStep2Message1,      //  Attach the propeller to the motor
                    Resources.BalancerPage_IterationStep2Message2,      //  Propulsion system's vibration will be measured + Testing in progress, do not touch the device anytime during measurement process
                    Resources.BalancerPage_IterationStep2Message3,      //  Propulsion system's vibration is being measured
                    Resources.BalancerPage_IterationStep2Message4       //  Check the measured value
                }
            },
            new IterationStep // 3
            {
                Header = Resources.BalancerPage_IterationStep3Header,   //  Selection of initial tape length
                Steps = new List<string>
                {
                    Resources.BalancerPage_IterationStep3Message1       //  Use a tape with 15-20mm width
                }
            },
            new IterationStep // 4
            {
                Header = Resources.BalancerPage_IterationStep4Header,   //  Adding reference weight to the lighter blade
                Steps = new List<string>
                {
                    Resources.BalancerPage_IterationStep4Message1,      //  Place the reference tape onto %20 to 30% of the radius of any propeller blade
                    Resources.BalancerPage_IterationStep4Message2,      //  Propulsion system's vibration will be measured + Testing in progress, do not touch the device anytime during measurement process
                    Resources.BalancerPage_IterationStep4Message3,      //  Propulsion system's vibration is being measured
                    Resources.BalancerPage_IterationStep4Message4,      //  Remove the tape from the blade onto the other blade's 20% to 30% radius
                    Resources.BalancerPage_IterationStep4Message5,      //  Propulsion system's vibration will be measured + Testing in progress, do not touch the device anytime during measurement process
                    Resources.BalancerPage_IterationStep4Message6,      //  Propulsion system's vibration is being measured
                    Resources.BalancerPage_IterationStep4Message7       //  Check the measured value
                }
            },
            new IterationStep // 5
            {
                Header = Resources.BalancerPage_IterationStep5Header,   //  Calculation of correction band quantity/length
                Steps = new List<string>
                {
                     Resources.BalancerPage_IterationStep5Message1,     //  Correction band length has been calculated
                     Resources.BalancerPage_IterationStep5Message2,     //  Please paste the selected tape back to its first location
                     Resources.BalancerPage_IterationStep5Message3      //  Please add X times more additional length of tape onto the existing tape
                }
            },
            new IterationStep // 6
            {
                Header = Resources.BalancerPage_IterationStep6Header,   //  Testing and Control of Results
                Steps = new List<string>
                {
                    Resources.BalancerPage_IterationStep6Message1,      //  Propulsion system's vibration will be measured + Testing in progress, do not touch the device anytime during measurement process
                    Resources.BalancerPage_IterationStep6Message2,      //  Propulsion system's vibration is being measured
                    Resources.BalancerPage_IterationStep6Message3       //  Check the measured value
                }
            }
            };
        }
    }
}
