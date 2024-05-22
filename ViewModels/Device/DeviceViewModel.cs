using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Advanced_Dynotis_Software.Models.Dynotis;
using System.Management;
using System.Windows;
using LiveCharts.Wpf;
using LiveCharts;


namespace Advanced_Dynotis_Software.ViewModels.Device
{
    public class DeviceViewModel : INotifyPropertyChanged
    {
        private Dynotis device;
        public Dynotis Device
        {
            get => device;
            set
            {
                device = value;
                OnPropertyChanged(nameof(Device));
            }
        }

        //@@@@@@@@@@@@@@@@@@@@@@@
        private readonly object _dataLock = new object();
        private readonly Queue<SensorData> _dataQueue = new Queue<SensorData>();
        private Timer _updateTimer;

        private SeriesCollection seriesCollection;
        public SeriesCollection SeriesCollection
        {
            get => seriesCollection;
            set
            {
                seriesCollection = value;
                OnPropertyChanged(nameof(SeriesCollection));
            }
        }

        private ObservableCollection<string> labels;
        public ObservableCollection<string> Labels
        {
            get => labels;
            set
            {
                labels = value;
                OnPropertyChanged(nameof(Labels));
            }
        }
        //@@@@@@@@@@@@@@@@@@@@@@@

        public DeviceViewModel(string port)
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Device = new Dynotis(port);
                Device.OpenPort();
                Device.devicePortsEvent += DevicePortsEvent;
                //@@@@@@@@@@@@@@@@@@@@@@@
                // Initialize SeriesCollection and Labels
                SeriesCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Ambient Temp",
                        Values = new ChartValues<double>()
                    }
                };
                Labels = new ObservableCollection<string>();

                Device.PropertyChanged += Device_PropertyChanged;

                // Setup a timer to debounce UI updates
                _updateTimer = new Timer(UpdateChart, null, 100, 100);
                //@@@@@@@@@@@@@@@@@@@@@@@

            }
            else
            {
                // Tasarım zamanı için örnek veri
                //Device = new Dynotis("DesignModePort");
            }
        }
        //@@@@@@@@@@@@@@@@@@@@@@@

        private void Device_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Device.SensorData))
            {
                lock (_dataLock)
                {
                    _dataQueue.Enqueue(Device.SensorData);
                }
            }
        }

        private void UpdateChart(object state)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                lock (_dataLock)
                {
                    while (_dataQueue.Count > 0)
                    {
                        var sensorData = _dataQueue.Dequeue();

                        // Add new data points to the chart
                        ((LineSeries)SeriesCollection[0]).Values.Add(sensorData.AmbientTemp);
                        Labels.Add(sensorData.Time.ToString());

                        // Keep only the latest 100 data points
                        if (((LineSeries)SeriesCollection[0]).Values.Count > 100)
                        {
                            ((LineSeries)SeriesCollection[0]).Values.RemoveAt(0);
                            Labels.RemoveAt(0);
                        }
                    }

                    OnPropertyChanged(nameof(SeriesCollection));
                    OnPropertyChanged(nameof(Labels));
                }
            });
        }

        //@@@@@@@@@@@@@@@@@@@@@@@
        public async void DevicePortsEvent()
        {
            try
            {
                // Sorun yaşanan portu kapat
                Device.ClosePort();

                // Belirli bir süre sonra portu tekrar açmaya çalış
                await Task.Delay(1000);

                // Cihazı tekrar aç ve güncelle
                Device.OpenPort();

                // Eğer hala bağlantı sorunu devam ediyorsa, kullanıcıya bilgi ver
                if (!Device.Port.IsOpen)
                {
                    // MessageBox.Show("DevicePortsEvent");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed DevicePortsEvent: {ex.Message}");
 
            }

        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
