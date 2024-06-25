using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using Advanced_Dynotis_Software.Services.Helpers;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Newtonsoft.Json;

namespace Advanced_Dynotis_Software.ViewModels.Pages
{
    public class AutomateTestViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<SequenceItem> _sequenceItems;
        private ObservableCollection<string> _savedTests;
        private SeriesCollection _chartSeries;

        private string _selectedTest;
        public string SelectedTest
        {
            get => _selectedTest;
            set
            {
                _selectedTest = value;
                OnPropertyChanged();
                // LoadSequenceCommand.Execute(_selectedTest); // Bu satırı kaldırıyoruz
            }
        }
        public ObservableCollection<SequenceItem> SequenceItems
        {
            get => _sequenceItems;
            set
            {
                if (_sequenceItems != value)
                {
                    if (_sequenceItems != null)
                    {
                        foreach (var item in _sequenceItems)
                        {
                            item.PropertyChanged -= OnSequenceItemPropertyChanged;
                        }
                    }

                    _sequenceItems = value;

                    if (_sequenceItems != null)
                    {
                        foreach (var item in _sequenceItems)
                        {
                            item.PropertyChanged += OnSequenceItemPropertyChanged;
                        }
                    }

                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> SavedTests
        {
            get => _savedTests;
            set
            {
                _savedTests = value;
                OnPropertyChanged();
            }
        }

        public SeriesCollection ChartSeries
        {
            get => _chartSeries;
            set
            {
                _chartSeries = value;
                OnPropertyChanged();
            }
        }
        public Func<double, string> XFormatter { get; set; }
        public Func<double, string> YFormatter { get; set; }
        public ICommand AddRowCommand { get; }
        public ICommand RemoveRowCommand { get; }
        public ICommand ClearAllPointsCommand { get; }
        public ICommand SaveSequenceCommand { get; }
        public ICommand LoadSequenceCommand { get; }
        public ICommand UpdateChartCommand { get; }
        public ICommand CellEditEndingCommand { get; }
        public ICommand KeyDownCommand { get; }

        public AutomateTestViewModel()
        {

            LoadSequenceCommand = new RelayCommand(param => LoadSequence(param as string));

            SequenceItems = new ObservableCollection<SequenceItem>
            {
                new SequenceItem { Time = 0, ThrottleOutput = 0 }
            };

            LoadSavedTests();

            ChartSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Values = new ChartValues<ObservablePoint>(),
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 10,
                    FontSize = 12,
                    PointForeground = new SolidColorBrush(Colors.Black),
                    LineSmoothness = 0, // This makes the line straight
                    Stroke = new SolidColorBrush(Colors.Orange),
                    StrokeThickness = 2,
                    Fill = new SolidColorBrush(Color.FromArgb(30, Colors.Orange.R, Colors.Orange.G, Colors.Orange.B)),
                    LabelPoint = point => $"ESC Throttle (μs): {point.Y}"
                }
            };

            XFormatter = value => value.ToString("0.00");
            YFormatter = value => value.ToString("0");

            AddRowCommand = new RelayCommand(param => AddRow(param));
            RemoveRowCommand = new RelayCommand(param => RemoveRow(param));
            ClearAllPointsCommand = new RelayCommand(param => ClearAllPoints());
            SaveSequenceCommand = new RelayCommand(param => SaveSequence(param as string));
            LoadSequenceCommand = new RelayCommand(param => LoadSequence(param as string));
            UpdateChartCommand = new RelayCommand(param => UpdateChart());
            CellEditEndingCommand = new RelayCommand(param => OnCellEditEnding(param));
            KeyDownCommand = new RelayCommand(param => OnKeyDown(param));

            SequenceItems.CollectionChanged += (sender, args) =>
            {
                if (args.NewItems != null)
                {
                    foreach (SequenceItem item in args.NewItems)
                    {
                        item.PropertyChanged += OnSequenceItemPropertyChanged;
                    }
                }

                if (args.OldItems != null)
                {
                    foreach (SequenceItem item in args.OldItems)
                    {
                        item.PropertyChanged -= OnSequenceItemPropertyChanged;
                    }
                }

                SortAndRefreshSequenceItems();
                UpdateChart();
            };
        }

        private void LoadSavedTests()
        {
            try
            {
                if (!Directory.Exists("TestSequences"))
                {
                    Directory.CreateDirectory("TestSequences");
                }

                SavedTests = new ObservableCollection<string>(Directory.GetFiles("TestSequences", "*.json").Select(Path.GetFileNameWithoutExtension));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading saved tests: {ex.Message}");
                SavedTests = new ObservableCollection<string>();
            }
        }

        private void AddRow(object parameter)
        {
            try
            {
                if (double.TryParse(parameter as string, out double time))
                {
                    SequenceItems.Add(new SequenceItem { Time = time, ThrottleOutput = 0 });
                    SortAndRefreshSequenceItems();
                    UpdateChart();
                }
                else
                {
                    throw new ArgumentException("Invalid time value.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding row: {ex.Message}");
            }
        }

        private void RemoveRow(object parameter)
        {
            try
            {
                if (parameter is SequenceItem item)
                {
                    SequenceItems.Remove(item);
                    UpdateChart();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing row: {ex.Message}");
            }
        }

        private void ClearAllPoints()
        {
            SequenceItems.Clear();
            SequenceItems.Add(new SequenceItem { Time = 0, ThrottleOutput = 0 });
            UpdateChart();
        }

        private void SaveSequence(string fileName)
        {
            try
            {
                var json = JsonConvert.SerializeObject(SequenceItems);
                Directory.CreateDirectory("TestSequences");
                File.WriteAllText(Path.Combine("TestSequences", fileName + ".json"), json);
                if (!SavedTests.Contains(fileName))
                {
                    SavedTests.Add(fileName);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving sequence: {ex.Message}");
            }
        }

        private void LoadSequence(string fileName)
        {
            try
            {
                var json = File.ReadAllText(Path.Combine("TestSequences", fileName + ".json"));
                var items = JsonConvert.DeserializeObject<ObservableCollection<SequenceItem>>(json);
                if (items != null)
                {
                    SequenceItems = items;
                }
                UpdateChart();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading sequence: {ex.Message}");
            }
        }

        private void UpdateChart()
        {
            var lineSeries = ChartSeries[0] as LineSeries;
            lineSeries.Values.Clear();

            for (int i = 0; i < SequenceItems.Count - 1; i++)
            {
                var current = SequenceItems[i];
                var next = SequenceItems[i + 1];

                lineSeries.Values.Add(new ObservablePoint(current.Time, current.ThrottleOutput));
                lineSeries.Values.Add(new ObservablePoint(next.Time, current.ThrottleOutput));
            }

            if (SequenceItems.Count > 0)
            {
                var last = SequenceItems[^1];
                lineSeries.Values.Add(new ObservablePoint(last.Time, last.ThrottleOutput));
            }
        }

        private void OnSequenceItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SortAndRefreshSequenceItems();
            UpdateChart();
        }

        private void OnCellEditEnding(object parameter)
        {
            SortAndRefreshSequenceItems();
            UpdateChart();
        }

        private void OnKeyDown(object parameter)
        {
            if (parameter is KeyEventArgs e && e.Key == Key.Enter)
            {
                SortAndRefreshSequenceItems();
                UpdateChart();
            }
        }

        public void SortAndRefreshSequenceItems()
        {
            SequenceItems = new ObservableCollection<SequenceItem>(SequenceItems.OrderBy(item => item.Time));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class SequenceItem : INotifyPropertyChanged
        {
            private double _time;
            private double _throttleOutput;

            public double Time
            {
                get => _time;
                set
                {
                    if (_time != value)
                    {
                        _time = value;
                        OnPropertyChanged();
                    }
                }
            }

            public double ThrottleOutput
            {
                get => _throttleOutput;
                set
                {
                    if (_throttleOutput != value)
                    {
                        _throttleOutput = value;
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
