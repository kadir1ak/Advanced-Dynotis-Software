using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public AutomateTestViewModel()
        {
            SequenceItems = new ObservableCollection<SequenceItem>();
            ChartSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Values = new ChartValues<ObservablePoint>(),
                    PointGeometry = null
                }
            };

            AddRowCommand = new RelayCommand(AddRow);
            RemoveRowCommand = new RelayCommand(RemoveRow);
            UpdateChartCommand = new RelayCommand(_ => UpdateChart());

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

                UpdateChart();
            };
        }

        private void AddRow(object parameter)
        {
            try
            {
                if (double.TryParse(parameter as string, out double time))
                {
                    SequenceItems.Add(new SequenceItem { Time = time, ThrottleOutput = 0 }); // Default throttle output to 0
                    UpdateChart();
                }
                else
                {
                    // Handle invalid input case, if needed.
                    throw new ArgumentException("Invalid time value.");
                }
            }
            catch (Exception ex)
            {
                // Log or display error
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
                // Log or display error
                System.Diagnostics.Debug.WriteLine($"Error removing row: {ex.Message}");
            }
        }

        private void UpdateChart()
        {
            var lineSeries = ChartSeries[0] as LineSeries;
            lineSeries.Values.Clear();
            foreach (var item in SequenceItems)
            {
                lineSeries.Values.Add(new ObservablePoint(item.Time, item.ThrottleOutput));
            }
        }

        private void OnSequenceItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateChart();
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
