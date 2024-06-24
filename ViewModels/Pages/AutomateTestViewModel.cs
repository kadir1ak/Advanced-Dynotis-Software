using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Advanced_Dynotis_Software.Services.Helpers;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;

namespace Advanced_Dynotis_Software.ViewModels.Pages
{
    public class AutomateTestViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<SequenceItem> _sequenceItems;
        private SeriesCollection _chartSeries;

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

        public SeriesCollection ChartSeries
        {
            get => _chartSeries;
            set
            {
                _chartSeries = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddRowCommand { get; }
        public ICommand RemoveRowCommand { get; }
        public ICommand UpdateChartCommand { get; }
        public ICommand CellEditEndingCommand { get; }
        public ICommand KeyDownCommand { get; }

        public AutomateTestViewModel()
        {
            SequenceItems = new ObservableCollection<SequenceItem>
            {
                new SequenceItem { Time = 0, ThrottleOutput = 0 }
            };

            ChartSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Values = new ChartValues<ObservablePoint>(),
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 15,
                    LineSmoothness = 0, // This makes the line straight
                }
            };

            AddRowCommand = new RelayCommand(AddRow);
            RemoveRowCommand = new RelayCommand(RemoveRow);
            UpdateChartCommand = new RelayCommand(_ => UpdateChart());
            CellEditEndingCommand = new RelayCommand(OnCellEditEnding);
            KeyDownCommand = new RelayCommand(OnKeyDown);

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
