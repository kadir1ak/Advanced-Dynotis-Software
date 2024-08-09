using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using LiveCharts;
using LiveCharts.Wpf;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class BalancedPropellerTestsChartViewModel : INotifyPropertyChanged
    {
        private string _balancedPropellerID;
        private double _balancedPropellerArea;
        private ObservableCollection<DateTime> _balancingTestDates;
        private ObservableCollection<double> _vibrationLevels;
        private InterfaceVariables _interfaceVariables;
        private SeriesCollection _seriesCollection;
        public Func<double, string> YFormatter { get; set; }

        private string[] _testDatesLabels;

        public BalancedPropellerTestsChartViewModel(InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
            _seriesCollection = new SeriesCollection();

            // Subscribe to the PropertyChanged event of InterfaceVariables
            _interfaceVariables.PropertyChanged += InterfaceVariables_PropertyChanged;

            YFormatter = value => value.ToString("0.000");

            // Initialize properties with current values
            UpdatePropertiesFromInterfaceVariables();
            UpdateChart();
        }

        private void InterfaceVariables_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Check if any of the relevant properties have changed
            if (e.PropertyName == nameof(InterfaceVariables.BalancedPropellersID) ||
                e.PropertyName == nameof(InterfaceVariables.BalancedPropellersDiameter) ||
                e.PropertyName == nameof(InterfaceVariables.BalancedPropellersTestDates) ||
                e.PropertyName == nameof(InterfaceVariables.BalancedPropellersVibrations))
            {
                UpdatePropertiesFromInterfaceVariables();
                UpdateChart();
            }
        }

        private void UpdatePropertiesFromInterfaceVariables()
        {
            BalancedPropellerID = _interfaceVariables.BalancedPropellersID;
            BalancedPropellerArea = _interfaceVariables.BalancedPropellersDiameter;
            BalancingTestDates = _interfaceVariables.BalancedPropellersTestDates ?? new ObservableCollection<DateTime>();
            VibrationLevels = _interfaceVariables.BalancedPropellersVibrations ?? new ObservableCollection<double>();
        }

        private void UpdateChart()
        {
            _seriesCollection.Clear();

            var lineSeries = new LineSeries
            {
                Title = "Vibration Levels",
                Values = new ChartValues<double>(_vibrationLevels)
            };

            _seriesCollection.Add(lineSeries);
            TestDatesLabels = _balancingTestDates.Select(date => date.ToString("MM/dd/yyyy")).ToArray();
        }

        public string BalancedPropellerID
        {
            get => _balancedPropellerID;
            set => SetProperty(ref _balancedPropellerID, value);
        }

        public double BalancedPropellerArea
        {
            get => _balancedPropellerArea;
            set => SetProperty(ref _balancedPropellerArea, value);
        }

        public ObservableCollection<DateTime> BalancingTestDates
        {
            get => _balancingTestDates;
            set => SetProperty(ref _balancingTestDates, value);
        }

        public ObservableCollection<double> VibrationLevels
        {
            get => _vibrationLevels;
            set => SetProperty(ref _vibrationLevels, value);
        }

        public SeriesCollection SeriesCollection
        {
            get => _seriesCollection;
            set => SetProperty(ref _seriesCollection, value);
        }

        public string[] TestDatesLabels
        {
            get => _testDatesLabels;
            set => SetProperty(ref _testDatesLabels, value);
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
