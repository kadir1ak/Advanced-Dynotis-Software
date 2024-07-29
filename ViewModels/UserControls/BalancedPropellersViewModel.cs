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

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class BalancedPropellersViewModel : INotifyPropertyChanged
    {
        private string _balancedPropellerID;
        private double _balancedPropellerArea;
        private ObservableCollection<DateTime> _balancingTestDate;
        private ObservableCollection<double> _vibrationLevel;
        private string _selectedPropeller;
        private ObservableCollection<string> _savedBalancedPropellers;
        private ObservableCollection<BalancingTestResult> _balancingTestResults;
        private InterfaceVariables _interfaceVariables;

        public string BalancedPropellerID
        {
            get => _balancedPropellerID;
            set
            {
                if (SetProperty(ref _balancedPropellerID, value))
                {
                    _interfaceVariables.BalancedPropeller.BalancedPropellerID = value;
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
                    _interfaceVariables.BalancedPropeller.BalancedPropellerArea = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<DateTime> BalancingTestDate
        {
            get => _balancingTestDate;
            set
            {
                if (SetProperty(ref _balancingTestDate, value))
                {
                    _interfaceVariables.BalancedPropeller.BalancingTestDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<double> VibrationLevel
        {
            get => _vibrationLevel;
            set
            {
                if (SetProperty(ref _vibrationLevel, value))
                {
                    _interfaceVariables.BalancedPropeller.VibrationLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> SavedBalancedPropellers
        {
            get => _savedBalancedPropellers;
            set
            {
                _savedBalancedPropellers = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<BalancingTestResult> BalancingTestResults
        {
            get => _balancingTestResults;
            set
            {
                _balancingTestResults = value;
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

        public ICommand SaveCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand DeleteCommand { get; }

        public BalancedPropellersViewModel(InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
            _savedBalancedPropellers = new ObservableCollection<string>();
            _balancingTestResults = new ObservableCollection<BalancingTestResult>();
            _balancingTestDate = new ObservableCollection<DateTime>();
            _vibrationLevel = new ObservableCollection<double>();

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

                SavedBalancedPropellers = new ObservableCollection<string>(Directory.GetFiles("BalancedPropellers", "*.json").Select(Path.GetFileNameWithoutExtension));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading saved balanced propellers: {ex.Message}");
                SavedBalancedPropellers = new ObservableCollection<string>();
            }
        }

        private void SaveBalancedPropeller()
        {
            try
            {
                var balancedPropellerData = new BalancedPropeller
                {
                    BalancedPropellerID = BalancedPropellerID,
                    BalancedPropellerArea = BalancedPropellerArea,
                    BalancingTestDate = BalancingTestDate.ToList(),
                    VibrationLevel = VibrationLevel.ToList()
                };

                var json = JsonConvert.SerializeObject(balancedPropellerData, Formatting.Indented);
                Directory.CreateDirectory("BalancedPropellers");
                File.WriteAllText(Path.Combine("BalancedPropellers", BalancedPropellerID + ".json"), json);

                if (!SavedBalancedPropellers.Contains(BalancedPropellerID))
                {
                    SavedBalancedPropellers.Add(BalancedPropellerID);
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
                var json = File.ReadAllText(Path.Combine("BalancedPropellers", SelectedPropeller + ".json"));
                var data = JsonConvert.DeserializeObject<BalancedPropeller>(json);

                if (data == null)
                {
                    throw new Exception("Deserialized data is null");
                }

                BalancedPropellerID = data.BalancedPropellerID;
                BalancedPropellerArea = data.BalancedPropellerArea;
                BalancingTestDate = new ObservableCollection<DateTime>(data.BalancingTestDate ?? new List<DateTime>());
                VibrationLevel = new ObservableCollection<double>(data.VibrationLevel ?? new List<double>());

                BalancingTestResults.Clear();
                for (int i = 0; i < BalancingTestDate.Count; i++)
                {
                    BalancingTestResults.Add(new BalancingTestResult
                    {
                        TestDate = BalancingTestDate[i],
                        Vibration = VibrationLevel[i]
                    });
                }
            }
            catch (Exception ex)
            {
               // MessageBox.Show($"Error loading balanced propeller: {ex.Message}");
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
                        SavedBalancedPropellers.Remove(SelectedPropeller);
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

        public class BalancedPropeller
        {
            public string BalancedPropellerID { get; set; }
            public double BalancedPropellerArea { get; set; }
            public List<DateTime> BalancingTestDate { get; set; }
            public List<double> VibrationLevel { get; set; }
        }

        public class BalancingTestResult : INotifyPropertyChanged
        {
            private DateTime _testDate;
            private double _vibration;

            public DateTime TestDate
            {
                get => _testDate;
                set
                {
                    if (_testDate != value)
                    {
                        _testDate = value;
                        OnPropertyChanged();
                    }
                }
            }

            public double Vibration
            {
                get => _vibration;
                set
                {
                    if (_vibration != value)
                    {
                        _vibration = value;
                        OnPropertyChanged();
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
