using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Advanced_Dynotis_Software.Services.Helpers;
using Newtonsoft.Json;
using static Advanced_Dynotis_Software.ViewModels.Pages.AutomateTestViewModel;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class BalancedPropellersViewModel : INotifyPropertyChanged
    {
        private string _balancedPropellerID; // şuan arayüzümde gözlemlediğim pervane  ıd
        private double _balancedPropellerArea; // şuan arayüzümde gözlemlediğim pervane  boyutu
        private ObservableCollection<DateTime> _balancingTestDates; // şuan arayüzümde gözlemlediğim pervaneye ait test tarihleri
        private ObservableCollection<double> _vibrationLevels;// şuan arayüzümde gözlemlediğim pervane testlerine ait titreşim verileri

        private ObservableCollection<string> _savedPropellers; // Kalasör içerisindeki kayıtlı pervaneler
        private BalancedDataset _resultPropeller; // seçilen pervane yüklenmek istendiğinde sonuç parametreleri
        private string _selectedPropeller;

        private InterfaceVariables _interfaceVariables;
        public ObservableCollection<BalanceTestData> BalancingTestDatas { get; set; }

        private bool _isPropellerIDTextBoxReadOnly;
        private bool _isPropellerAreaTextBoxReadOnly;
        public ICommand NewCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand DeleteCommand { get; }

        public BalancedPropellersViewModel(InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
            _savedPropellers = new ObservableCollection<string>();
            _resultPropeller = new BalancedDataset();
            BalancingTestDatas = new ObservableCollection<BalanceTestData>();

            _isPropellerIDTextBoxReadOnly = true;
            _isPropellerAreaTextBoxReadOnly = true;

            NewCommand = new RelayCommand(param => NewBalancedPropeller());
            SaveCommand = new RelayCommand(param => SaveBalancedPropeller());
            LoadCommand = new RelayCommand(param => LoadBalancedPropeller());
            DeleteCommand = new RelayCommand(param => DeleteBalancedPropeller());

            LoadSavedBalancedPropellers();
        }

        private void LoadSavedBalancedPropellers()
        {
            try
            {
                if (!Directory.Exists("BalancedPropellers"))
                {
                    Directory.CreateDirectory("BalancedPropellers");
                }

                SavedPropellers = new ObservableCollection<string>(Directory.GetFiles("BalancedPropellers", "*.json").Select(Path.GetFileNameWithoutExtension));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading saved balanced propellers: {ex.Message}");
                SavedPropellers = new ObservableCollection<string>();
            }
        }

        private void NewBalancedPropeller()
        {
            try
            {
                IsPropellerIDTextBoxReadOnly = false;
                IsPropellerAreaTextBoxReadOnly = false;
                BalancedPropellerID = "";
                BalancedPropellerArea = 0;
                BalancingTestDates.Clear();
                VibrationLevels.Clear();
                BalancingTestDatas.Clear();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error new balanced propeller: {ex.Message}");
            }
        }

        private void SaveBalancedPropeller()
        {
            try
            {
                var balancedPropellerData = new BalancedDataset
                {
                    PropellerID = BalancedPropellerID,
                    PropellerArea = BalancedPropellerArea,
                    TestDates = BalancingTestDates,
                    Vibrations = VibrationLevels
                };

                var json = JsonConvert.SerializeObject(balancedPropellerData, Formatting.Indented);
                Directory.CreateDirectory("BalancedPropellers");
                File.WriteAllText(Path.Combine("BalancedPropellers", BalancedPropellerID + ".json"), json);

                if (!SavedPropellers.Contains(BalancedPropellerID))
                {
                    SavedPropellers.Add(BalancedPropellerID);
                }
                else
                {
                    MessageBox.Show("Please enter a different Id");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving balanced propeller: {ex.Message}");
            }
        }

        private void LoadBalancedPropeller()
        {
            try
            {
                IsPropellerIDTextBoxReadOnly = true;
                IsPropellerAreaTextBoxReadOnly = true;

                var json = File.ReadAllText(Path.Combine("BalancedPropellers", SelectedPropeller + ".json"));
                var items = JsonConvert.DeserializeObject<BalancedDataset>(json);
                
                if (items != null)
                {
                    ResultPropeller = items;

                    //Verileri ViewModel alanlarına aktar
                    
                    BalancedPropellerID = ResultPropeller.PropellerID;
                    BalancedPropellerArea = ResultPropeller.PropellerArea;
                    BalancingTestDates = ResultPropeller.TestDates;
                    VibrationLevels = ResultPropeller.Vibrations;
   
                    BalancingTestDatas = new ObservableCollection<BalanceTestData>();
                    for (int i = 0; i < BalancingTestDates.Count; i++)
                    {
                        BalancingTestDatas.Add(new BalanceTestData
                        {
                            TestDates = BalancingTestDates[i],
                            Vibrations = VibrationLevels[i]
                        });
                    }

                    OnPropertyChanged(nameof(BalancingTestDatas)); // BalancingTestDatas'in güncellendiğini bildir


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading balanced propeller: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Error loading balanced propeller: {ex.Message}");
            }
        }

        private void DeleteBalancedPropeller()
        {
            try
            {
                var filePath = Path.Combine("BalancedPropellers", SelectedPropeller + ".json");
                if (File.Exists(filePath))
                {
                    var result = MessageBox.Show($"Are you sure you want to delete the file \"{SelectedPropeller}\"?", "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        File.Delete(filePath);
                        SavedPropellers.Remove(SelectedPropeller);
                        MessageBox.Show($"File \"{SelectedPropeller}\" has been deleted.", "Delete Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting balanced propeller: {ex.Message}");
                MessageBox.Show($"Error deleting the file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public string BalancedPropellerID
        {
            get => _balancedPropellerID;
            set
            {
                if (SetProperty(ref _balancedPropellerID, value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public double BalancedPropellerArea
        {
            get => _balancedPropellerArea;
            set
            {
                if (SetProperty(ref _balancedPropellerArea, value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<DateTime> BalancingTestDates
        {
            get => _balancingTestDates;
            set
            {
                if (SetProperty(ref _balancingTestDates, value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<double> VibrationLevels
        {
            get => _vibrationLevels;
            set
            {
                if (SetProperty(ref _vibrationLevels, value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> SavedPropellers
        {
            get => _savedPropellers;
            set
            {
                _savedPropellers = value;
                OnPropertyChanged();
            }
        }

        public BalancedDataset ResultPropeller
        {
            get => _resultPropeller;
            set
            {
                _resultPropeller = value;
                OnPropertyChanged();
            }
        }
        public string SelectedPropeller
        {
            get => _selectedPropeller;
            set
            {
                _selectedPropeller = value;
                OnPropertyChanged();
            }
        }
        public bool IsPropellerIDTextBoxReadOnly
        {
            get => _isPropellerIDTextBoxReadOnly;
            set
            {
                if (SetProperty(ref _isPropellerIDTextBoxReadOnly, value))
                {
                    OnPropertyChanged(nameof(IsPropellerIDTextBoxReadOnly));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        public bool IsPropellerAreaTextBoxReadOnly
        {
            get => _isPropellerAreaTextBoxReadOnly;
            set
            {
                if (SetProperty(ref _isPropellerAreaTextBoxReadOnly, value))
                {
                    OnPropertyChanged(nameof(IsPropellerAreaTextBoxReadOnly));
                    CommandManager.InvalidateRequerySuggested();
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
    }
    public class BalanceTestData
    {
        public DateTime TestDates { get; set; }
        public double Vibrations { get; set; }
    }
    public class BalancedDataset : INotifyPropertyChanged
    {
        private string _propellerID;
        private double _propellerArea;
        private ObservableCollection<DateTime> _testDates;
        private ObservableCollection<double> _vibrations;

        public string PropellerID
        {
            get => _propellerID;
            set
            {
                if (SetProperty(ref _propellerID, value))
                {

                    OnPropertyChanged();
                }
            }
        }
        public double PropellerArea
        {
            get => _propellerArea;
            set
            {
                if (SetProperty(ref _propellerArea, value))
                {

                    OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<DateTime> TestDates
        {
            get => _testDates;
            set
            {
                if (SetProperty(ref _testDates, value))
                {

                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<double> Vibrations
        {
            get => _vibrations;
            set
            {
                if (SetProperty(ref _vibrations, value))
                {

                    OnPropertyChanged();
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
    }
}
