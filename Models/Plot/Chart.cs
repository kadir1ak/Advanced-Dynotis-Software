using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Legends;
using System;
using System.Windows;
using Advanced_Dynotis_Software.Services.BindableBase;

namespace Advanced_Dynotis_Software.Models.Plot
{
    public class Chart : BindableBase
    {
        // Sensör Adı
        private string _sensorName;
        public string SensorName
        {
            get => _sensorName;
            set => SetProperty(ref _sensorName, value);
        }

        // Sensör Birimi (örneğin, "m/s²", "°C")
        private string _unit;
        public string Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }

        // Sensör Rengi
        private OxyColor _color;
        public OxyColor Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        // PlotModel Özelliği
        private PlotModel _plotModel;
        public PlotModel PlotModel
        {
            get => _plotModel;
            set => SetProperty(ref _plotModel, value);
        }

        // Kilit Mekanizması
        private readonly object _plotDataLock = new();

        // Constructor
        public Chart(string sensorName, string unit, OxyColor color)
        {
            SensorName = sensorName;
            Unit = unit;
            Color = color;

            InitializePlotModel(sensorName);
        }

        // PlotModel'i Başlatma
        private void InitializePlotModel(string title)
        {
            PlotModel = new PlotModel { Title = $"{title} ({Unit})" };

            // Legend Ekleme
            PlotModel.Legends.Add(new Legend
            {
                LegendTitle = "Sensor Data",
                LegendPosition = LegendPosition.LeftTop,
                LegendTextColor = OxyColors.Black
            });

            // Seriyi Başlatma
            var series = new LineSeries
            {
                Title = SensorName,
                Color = Color,
                StrokeThickness = 2,
                LineStyle = LineStyle.Solid
            };
            PlotModel.Series.Add(series);
        }

        // Sensör Verisini Güncelleme
        public void UpdateSensorData(double time, double value)
        {
            lock (_plotDataLock)
            {
                var series = PlotModel.Series[0] as LineSeries;
                if (series != null)
                {
                    series.Points.Add(new DataPoint(time, value));

                    // Eski verileri temizleme
                    if (series.Points.Count > 1000)
                    {
                        series.Points.RemoveAt(0);
                    }
                }
            }
        }

        // Grafiği Güncelle
        public void RefreshPlot()
        {
            lock (_plotDataLock)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    PlotModel.InvalidatePlot(true);
                });
            }
        }

        // Tüm Verileri Temizle
        public void ClearData()
        {
            lock (_plotDataLock)
            {
                var series = PlotModel.Series[0] as LineSeries;
                if (series != null)
                {
                    series.Points.Clear();
                }
                RefreshPlot();
            }
        }

        // Yeni Veri Noktası Ekleme (İsteğe Bağlı)
        public void AddDataPoint(DataPoint point)
        {
            lock (_plotDataLock)
            {
                var series = PlotModel.Series[0] as LineSeries;
                if (series != null)
                {
                    series.Points.Add(point);

                    // Eski verileri temizleme
                    if (series.Points.Count > 1000)
                    {
                        series.Points.RemoveAt(0);
                    }
                }
            }
        }
    }
}
