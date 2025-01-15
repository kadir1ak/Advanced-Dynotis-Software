using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Advanced_Dynotis_Software.Models.Dynotis;
//using LiveCharts;
//using LiveCharts.Defaults;
//using LiveCharts.Wpf;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Advanced_Dynotis_Software.ViewModels.Main
{
    public class ChartViewModel : INotifyPropertyChanged
    {
        public PlotModel VibrationPlotModel { get; private set; }
        public PlotModel CurrentPlotModel { get; private set; }
        public PlotModel VoltagePlotModel { get; private set; }
        public PlotModel TorquePlotModel { get; private set; }
        public PlotModel ThrustPlotModel { get; private set; }
        public PlotModel MotorSpeedPlotModel { get; private set; }
        // Her bir PlotModel için özel padding ve ölçekleme faktörü
        private readonly Dictionary<PlotModel, double> ModelSettings;

        private const int MaxDataPoints = 100;
        private readonly object _dataLock = new();

        public ChartViewModel()
        {
            VibrationPlotModel = CreatePlotModel("Vibration (g)", OxyColors.Red);
            CurrentPlotModel = CreatePlotModel("Current (A)", OxyColors.Blue);
            VoltagePlotModel = CreatePlotModel("Voltage (V)", OxyColors.Purple);
            TorquePlotModel = CreatePlotModel("Torque (Nm)", OxyColors.Green);
            ThrustPlotModel = CreatePlotModel("Thrust (N)", OxyColors.Orange);
            MotorSpeedPlotModel = CreatePlotModel("Motor Speed (RPM)", OxyColors.SkyBlue);
            // Her bir model için özel ayarları tanımla
            ModelSettings = new Dictionary<PlotModel, double>
            {
                { VibrationPlotModel, 1.0 },
                { CurrentPlotModel, 20.0 },
                { VoltagePlotModel, 20.0 },
                { TorquePlotModel, 500.0 },
                { ThrustPlotModel, 500.0 },
                { MotorSpeedPlotModel, 500.0 }
            };
        }

        private PlotModel CreatePlotModel(string title, OxyColor color)
        {
            var plotModel = new PlotModel { };

            // Veri serisi
            var series = new LineSeries
            {
                Title = title,                
                TrackerFormatString = "{0}\nTime: {2:0.000}\nValue: {4:0.000}",
                Color = color
            };
            plotModel.Series.Add(series);

            /*
            plotModel.Legends.Add(new OxyPlot.Legends.Legend
            {
                LegendPosition = OxyPlot.Legends.LegendPosition.TopCenter,
                LegendTextColor = color
            });
            */

            return plotModel;
        }
        private void SetYAxis(PlotModel plotModel, double min, double max, string title = "Value")
        {
            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left, // Sol tarafta Y ekseni
                Minimum = min,                // Minimum değer
                Maximum = max,                // Maksimum değer
                Title = title,                // Y ekseni başlığı (isteğe bağlı)
                IsZoomEnabled = true,         // Zoom özelliği etkin
                IsPanEnabled = true           // Pan özelliği etkin
            };
            plotModel.Axes.Add(yAxis);
        }
        private void AdjustYAxis(PlotModel plotModel)
        {
            // Veri serisini kontrol et
            if (plotModel.Series.Count > 0 && plotModel.Series[0] is LineSeries series)
            {
                // Veri serisindeki minimum ve maksimum değerleri bul
                double seriesMin = series.Points.Min(p => p.Y);
                double seriesMax = series.Points.Max(p => p.Y);

                // Biraz boşluk bırakmak için sınırları genişlet
                double padding = Math.Abs(seriesMax - seriesMin) * 0.10; // %10 boşluk ekle
                
                // ModelSettings'ten MinPadding değerini al
                double minPadding = ModelSettings.TryGetValue(plotModel, out double paddingValue) ? paddingValue : 50.0;
                padding = Math.Max(padding, minPadding);

                double newMin = seriesMin - padding;
                double newMax = seriesMax + padding;

                // Mevcut Y eksenini kontrol et
                if (plotModel.Axes.Count > 0 && plotModel.Axes[0] is LinearAxis yAxis)
                {
                    // Eğer sınırlar değiştiyse, Y eksenini yeniden ayarla
                    if (newMin != yAxis.Minimum || newMax != yAxis.Maximum)
                    {
                        yAxis.Minimum = newMin;
                        yAxis.Maximum = newMax;
                    }
                }
                else
                {
                    // Eğer Y ekseni yoksa, yeni bir Y ekseni ekle
                    SetYAxis(plotModel, newMin, newMax);
                }
            }
        }
        private void AdjustYAxisToEightDivisions(PlotModel plotModel)
        {
            // Mevcut Y eksenini bul
            if (plotModel.Axes.Count > 0 && plotModel.Axes[0] is LinearAxis yAxis)
            {
                // Minimum ve maksimum değerleri al
                double yMin = yAxis.Minimum;
                double yMax = yAxis.Maximum;

                // Eğer minimum ve maksimum değerler uygunsa, MajorStep'i hesapla
                if (yMax > yMin)
                {
                    double range = yMax - yMin;
                    yAxis.MajorStep = range / 8.0; // 8 parçaya böl
                }

                // Gereksiz küçük separatorları gizlemek için MinorStep'i ayarla
                yAxis.MinorStep = yAxis.MajorStep / 2.0;

                // Eksen etiketlerini güncelle
                yAxis.IntervalLength = 50; // Her bir separatorun uzunluğu (isteğe bağlı)
                                           
                yAxis.StringFormat = "F2"; // Ondalık basamak sayısını 2 olarak ayarla
            }
            else
            {
                // Eğer Y ekseni yoksa, hata mesajı yazdırabilir veya yeni bir eksen ekleyebilirsiniz
                Console.WriteLine("Y ekseni bulunamadı!");
            }
        }

        public async Task UpdateChartData(DynotisData data, InterfaceVariables interfaceData)
        {
            // Verilerin işlenmesi ve grafiklere eklenmesi
            lock (_dataLock)
            {
                UpdatePlotData(VibrationPlotModel, data.Time / 1000.0, data.Vibration.Value);
                UpdatePlotData(CurrentPlotModel, data.Time / 1000.0, data.Current);
                UpdatePlotData(VoltagePlotModel, data.Time / 1000.0, data.Voltage);
                UpdatePlotData(TorquePlotModel, data.Time / 1000.0, data.Torque.Value);
                UpdatePlotData(ThrustPlotModel, data.Time / 1000.0, data.Thrust.Value);
                UpdatePlotData(MotorSpeedPlotModel, data.Time / 1000.0, data.MotorSpeed.Value);

                // Dinamik olarak Y ekseni sınırlarını ayarla
                AdjustYAxis(VibrationPlotModel);
                AdjustYAxis(CurrentPlotModel);
                AdjustYAxis(VoltagePlotModel);
                AdjustYAxis(TorquePlotModel);
                AdjustYAxis(ThrustPlotModel);
                AdjustYAxis(MotorSpeedPlotModel);
                AdjustYAxisToEightDivisions(VibrationPlotModel);
                AdjustYAxisToEightDivisions(CurrentPlotModel);
                AdjustYAxisToEightDivisions(VoltagePlotModel);
                AdjustYAxisToEightDivisions(TorquePlotModel);
                AdjustYAxisToEightDivisions(ThrustPlotModel);
                AdjustYAxisToEightDivisions(MotorSpeedPlotModel);
            }

            // UI thread üzerinde grafiklerin yeniden çizimi
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                VibrationPlotModel.InvalidatePlot(true);
                CurrentPlotModel.InvalidatePlot(true);
                VoltagePlotModel.InvalidatePlot(true);
                TorquePlotModel.InvalidatePlot(true);
                ThrustPlotModel.InvalidatePlot(true);
                MotorSpeedPlotModel.InvalidatePlot(true);
            });
        }
        private void UpdatePlotData(PlotModel plotModel, double time, double value)
        {
            try
            {
                // Veri serisini kontrol et
                if (plotModel.Series.Count > 0 && plotModel.Series[0] is LineSeries series)
                {
                    // Yeni veri noktası ekle
                    series.Points.Add(new DataPoint(time, value));

                    // Maksimum veri noktası sayısını kontrol et
                    if (series.Points.Count > MaxDataPoints)
                    {
                        series.Points.RemoveAt(0);
                    }

                    // Grafiği yenile
                    plotModel.InvalidatePlot(true);
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda loglama yapabilirsiniz
                Console.WriteLine($"Hata: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //public SeriesCollection VibrationSeriesCollection { get; private set; }
        //public SeriesCollection VibrationIPSSeriesCollection { get; private set; }
        //public SeriesCollection VibrationXSeriesCollection { get; private set; }
        //public SeriesCollection VibrationYSeriesCollection { get; private set; }
        //public SeriesCollection VibrationZSeriesCollection { get; private set; }
        //public SeriesCollection VibrationHighSeriesCollection { get; private set; }
        //public SeriesCollection MotorSpeedSeriesCollection { get; private set; }
        //public SeriesCollection VoltageSeriesCollection { get; private set; }
        //public SeriesCollection CurrentSeriesCollection { get; private set; }
        //public SeriesCollection ThrustSeriesCollection { get; private set; }
        //public SeriesCollection TorqueSeriesCollection { get; private set; }

        //public ObservableCollection<string> TimeLabels { get; private set; }

        //public Func<double, string> CurrentXAxisFormatter { get; private set; }
        //public Func<double, string> CurrentYAxisFormatter { get; private set; }   

        //public Func<double, string> VoltageXAxisFormatter { get; private set; }
        //public Func<double, string> VoltageYAxisFormatter { get; private set; }  

        //public Func<double, string> VibrationXAxisFormatter { get; private set; }
        //public Func<double, string> VibrationYAxisFormatter { get; private set; } 

        //public Func<double, string> MotorSpeedXAxisFormatter { get; private set; }
        //public Func<double, string> MotorSpeedYAxisFormatter { get; private set; } 

        //public Func<double, string> ThrustXAxisFormatter { get; private set; }
        //public Func<double, string> ThrustYAxisFormatter { get; private set; }

        //public Func<double, string> TorqueXAxisFormatter { get; private set; }
        //public Func<double, string> TorqueYAxisFormatter { get; private set; }

        //public double CurrentYAxisMin { get; private set; }
        //public double CurrentYAxisMax { get; private set; }

        //public double VoltageYAxisMin { get; private set; }
        //public double VoltageYAxisMax { get; private set; }

        //public double VibrationIPSYAxisMin { get; private set; }
        //public double VibrationIPSYAxisMax { get; private set; }       

        //public double VibrationYAxisMin { get; private set; }
        //public double VibrationYAxisMax { get; private set; }

        //public double VibrationXYAxisMin { get; private set; }
        //public double VibrationXYAxisMax { get; private set; }

        //public double VibrationYYAxisMin { get; private set; }
        //public double VibrationYYAxisMax { get; private set; }

        //public double VibrationZYAxisMin { get; private set; }
        //public double VibrationZYAxisMax { get; private set; }

        //public double VibrationHighYAxisMin { get; private set; }
        //public double VibrationHighYAxisMax { get; private set; }

        //public double MotorSpeedYAxisMin { get; private set; }
        //public double MotorSpeedYAxisMax { get; private set; }

        //public double ThrustYAxisMin { get; private set; }
        //public double ThrustYAxisMax { get; private set; }

        //public double TorqueYAxisMin { get; private set; }
        //public double TorqueYAxisMax { get; private set; }

        //public double CurrentXAxisStep { get; private set; }
        //public double CurrentYAxisStep { get; private set; }
        //public double VoltageXAxisStep { get; private set; }
        //public double VoltageYAxisStep { get; private set; }
        //public double VibrationIPSXAxisStep { get; private set; }
        //public double VibrationIPSYAxisStep { get; private set; }
        //public double VibrationXAxisStep { get; private set; }
        //public double VibrationYAxisStep { get; private set; }
        //public double VibrationXXAxisStep { get; private set; }
        //public double VibrationXYAxisStep { get; private set; }
        //public double VibrationYXAxisStep { get; private set; }
        //public double VibrationYYAxisStep { get; private set; }
        //public double VibrationZXAxisStep { get; private set; }
        //public double VibrationZYAxisStep { get; private set; }
        //public double VibrationHighXAxisStep { get; private set; }
        //public double VibrationHighYAxisStep { get; private set; }
        //public double MotorSpeedXAxisStep { get; private set; }
        //public double MotorSpeedYAxisStep { get; private set; }
        //public double ThrustXAxisStep { get; private set; }
        //public double ThrustYAxisStep { get; private set; }
        //public double TorqueXAxisStep { get; private set; }
        //public double TorqueYAxisStep { get; private set; }


        //private int seriesBufferSize = 100;

        //const double defaultValue = 100;

        //public ChartViewModel()
        //{
        //    InitializeCharts();
        //}

        //private void InitializeCharts()
        //{
        //    VibrationSeriesCollection = CreateSeriesCollection("Vibration", Colors.IndianRed);

        //    VibrationIPSSeriesCollection = CreateSeriesCollection("Vibration IPS", Colors.Orange);

        //    VibrationXSeriesCollection = CreateSeriesCollection("VibrationX", Colors.Blue);
        //    VibrationYSeriesCollection = CreateSeriesCollection("VibrationY", Colors.Green);
        //    VibrationZSeriesCollection = CreateSeriesCollection("VibrationZ", Colors.Purple);
        //    VibrationHighSeriesCollection = CreateSeriesCollection("VibrationHigh", Colors.Orange);

        //    CurrentSeriesCollection = CreateSeriesCollection("Current", Colors.DarkOliveGreen);
        //    MotorSpeedSeriesCollection = CreateSeriesCollection("Motor Speed", Colors.PaleVioletRed);
        //    VoltageSeriesCollection = CreateSeriesCollection("Voltage", Colors.Orange);
        //    ThrustSeriesCollection = CreateSeriesCollection("Thrust", Colors.DarkOliveGreen);
        //    TorqueSeriesCollection = CreateSeriesCollection("Torque", Colors.HotPink);

        //    TimeLabels = new ObservableCollection<string>();

        //    InitializeDefaultChartData();

        //    CurrentXAxisFormatter = value => value.ToString("0.0");
        //    CurrentYAxisFormatter = value => value.ToString("0.00");

        //    VoltageXAxisFormatter = value => value.ToString("0.0");
        //    VoltageYAxisFormatter = value => value.ToString("0.00");

        //    VibrationXAxisFormatter = value => value.ToString("0.0");
        //    VibrationYAxisFormatter = value => value.ToString("0.000");

        //    MotorSpeedXAxisFormatter = value => value.ToString("0.0");
        //    MotorSpeedYAxisFormatter = value => value.ToString("0");

        //    ThrustXAxisFormatter = value => value.ToString("0.0");
        //    ThrustYAxisFormatter = value => value.ToString("0.000");

        //    TorqueXAxisFormatter = value => value.ToString("0.0");
        //    TorqueYAxisFormatter = value => value.ToString("0.000");
        //}

        //private void InitializeDefaultChartData()
        //{
        //    for (int i = 0; i < seriesBufferSize; i++)
        //    {
        //        TimeLabels.Add(i.ToString());
        //        UpdateSeries(VibrationIPSSeriesCollection, defaultValue);
        //        UpdateSeries(VibrationSeriesCollection, defaultValue);
        //        UpdateSeries(VibrationXSeriesCollection, defaultValue);
        //        UpdateSeries(VibrationYSeriesCollection, defaultValue);
        //        UpdateSeries(VibrationZSeriesCollection, defaultValue);
        //        UpdateSeries(VibrationHighSeriesCollection, defaultValue);
        //        UpdateSeries(CurrentSeriesCollection, defaultValue);
        //        UpdateSeries(MotorSpeedSeriesCollection, defaultValue);
        //        UpdateSeries(VoltageSeriesCollection, defaultValue);
        //        UpdateSeries(ThrustSeriesCollection, defaultValue);
        //        UpdateSeries(TorqueSeriesCollection, defaultValue);
        //    }

        //    UpdateChartSteps();
        //}       

        //private static SeriesCollection CreateSeriesCollection(string title, Color color)
        //{
        //    return new SeriesCollection
        //    {
        //        new LineSeries
        //        {
        //            Title = title,
        //            FontSize = 14,
        //            Values = new ChartValues<double>(),
        //            PointGeometrySize = 0,
        //            LineSmoothness = 0,
        //            Stroke = new SolidColorBrush(color),
        //            StrokeThickness = 1,
        //            Fill = new SolidColorBrush(Color.FromArgb(10, color.R, color.G, color.B)),
        //            PointForeground = Brushes.Transparent,
        //            LabelPoint = point => point.Y.ToString("N1")
        //        }
        //    };
        //}

        //public void UpdateChartData(DynotisData data, InterfaceVariables interfaceData)
        //{
        //    if (TimeLabels.Count >= seriesBufferSize)
        //    {
        //        TimeLabels.RemoveAt(0);
        //    }
        //    TimeLabels.Add(interfaceData.Time.ToString());
        //    UpdateSeries(VibrationSeriesCollection, interfaceData.Vibration.Value);
        //    UpdateSeries(VibrationIPSSeriesCollection, interfaceData.Theoric.IPS);
        //    UpdateSeries(VibrationXSeriesCollection, interfaceData.Vibration.VibrationX);
        //    UpdateSeries(VibrationYSeriesCollection, interfaceData.Vibration.VibrationY);
        //    UpdateSeries(VibrationZSeriesCollection, interfaceData.Vibration.VibrationZ);
        //    UpdateSeries(VibrationHighSeriesCollection, interfaceData.Vibration.HighVibration);
        //    UpdateSeries(CurrentSeriesCollection, interfaceData.Current);
        //    UpdateSeries(MotorSpeedSeriesCollection, interfaceData.MotorSpeed.Value);
        //    UpdateSeries(VoltageSeriesCollection, interfaceData.Voltage);
        //    UpdateSeries(ThrustSeriesCollection, interfaceData.Thrust.Value);
        //    UpdateSeries(TorqueSeriesCollection, interfaceData.Torque.Value);
        //    UpdateChartSteps();
        //    UpdateChartLimits();
        //}


        //private void UpdateSeries(SeriesCollection seriesCollection, double value)
        //{
        //    var values = ((LineSeries)seriesCollection[0]).Values;
        //    if (values.Count >= seriesBufferSize)
        //    {
        //        values.RemoveAt(0);
        //    }
        //    values.Add(value);
        //}

        //private void UpdateChartSteps()
        //{
        //    CurrentXAxisStep = ((LineSeries)CurrentSeriesCollection[0]).Values.Count / 6.0;
        //    CurrentYAxisStep = (CurrentYAxisMax == CurrentYAxisMin) ? 1 : (CurrentYAxisMax - CurrentYAxisMin) / 10.0;

        //    VoltageXAxisStep = ((LineSeries)VoltageSeriesCollection[0]).Values.Count / 6.0;
        //    VoltageYAxisStep = (VoltageYAxisMax == VoltageYAxisMin) ? 1 : (VoltageYAxisMax - VoltageYAxisMin) / 10.0;

        //    MotorSpeedXAxisStep = ((LineSeries)MotorSpeedSeriesCollection[0]).Values.Count / 6.0;
        //    MotorSpeedYAxisStep = (MotorSpeedYAxisMax == MotorSpeedYAxisMin) ? 1 : (MotorSpeedYAxisMax - MotorSpeedYAxisMin) / 10.0;

        //    ThrustXAxisStep = ((LineSeries)ThrustSeriesCollection[0]).Values.Count / 6.0;
        //    ThrustYAxisStep = (ThrustYAxisMax == ThrustYAxisMin) ? 1 : (ThrustYAxisMax - ThrustYAxisMin) / 20.0;

        //    TorqueXAxisStep = ((LineSeries)TorqueSeriesCollection[0]).Values.Count / 6.0;
        //    TorqueYAxisStep = (TorqueYAxisMax == TorqueYAxisMin) ? 1 : (TorqueYAxisMax - TorqueYAxisMin) / 20.0;

        //    VibrationIPSXAxisStep = ((LineSeries)VibrationIPSSeriesCollection[0]).Values.Count / 6.0;
        //    VibrationIPSYAxisStep = (VibrationIPSYAxisMax == VibrationIPSYAxisMin) ? 1 : (VibrationIPSYAxisMax - VibrationIPSYAxisMin) / 10.0;

        //    VibrationXAxisStep = ((LineSeries)VibrationSeriesCollection[0]).Values.Count / 6.0;
        //    VibrationYAxisStep = (VibrationYAxisMax == VibrationYAxisMin) ? 1 : (VibrationYAxisMax - VibrationYAxisMin) / 10.0;

        //    VibrationXXAxisStep = ((LineSeries)VibrationXSeriesCollection[0]).Values.Count / 2.0;
        //    VibrationXYAxisStep = (VibrationXYAxisMax == VibrationXYAxisMin) ? 1 : (VibrationXYAxisMax - VibrationXYAxisMin) / 3.0;

        //    VibrationYXAxisStep = ((LineSeries)VibrationYSeriesCollection[0]).Values.Count / 2.0;
        //    VibrationYYAxisStep = (VibrationYYAxisMax == VibrationYYAxisMin) ? 1 : (VibrationYYAxisMax - VibrationYYAxisMin) / 3.0;

        //    VibrationZXAxisStep = ((LineSeries)VibrationZSeriesCollection[0]).Values.Count / 2.0;
        //    VibrationZYAxisStep = (VibrationZYAxisMax == VibrationZYAxisMin) ? 1 : (VibrationZYAxisMax - VibrationZYAxisMin) / 3.0;

        //    VibrationHighXAxisStep = ((LineSeries)VibrationHighSeriesCollection[0]).Values.Count / 2.0;
        //    VibrationHighYAxisStep = (VibrationHighYAxisMax == VibrationHighYAxisMin) ? 1 : (VibrationHighYAxisMax - VibrationHighYAxisMin) / 3.0;

        //    OnPropertyChanged(nameof(CurrentXAxisStep));
        //    OnPropertyChanged(nameof(CurrentYAxisStep));
        //    OnPropertyChanged(nameof(VoltageXAxisStep));
        //    OnPropertyChanged(nameof(VoltageYAxisStep));
        //    OnPropertyChanged(nameof(VibrationIPSXAxisStep));
        //    OnPropertyChanged(nameof(VibrationIPSYAxisStep));
        //    OnPropertyChanged(nameof(VibrationXAxisStep));
        //    OnPropertyChanged(nameof(VibrationYAxisStep));
        //    OnPropertyChanged(nameof(VibrationXXAxisStep));
        //    OnPropertyChanged(nameof(VibrationXYAxisStep));
        //    OnPropertyChanged(nameof(VibrationYXAxisStep));
        //    OnPropertyChanged(nameof(VibrationYYAxisStep));
        //    OnPropertyChanged(nameof(VibrationZXAxisStep));
        //    OnPropertyChanged(nameof(VibrationZYAxisStep));
        //    OnPropertyChanged(nameof(VibrationHighXAxisStep));
        //    OnPropertyChanged(nameof(VibrationHighYAxisStep));
        //    OnPropertyChanged(nameof(MotorSpeedXAxisStep));
        //    OnPropertyChanged(nameof(MotorSpeedYAxisStep));
        //    OnPropertyChanged(nameof(ThrustXAxisStep));
        //    OnPropertyChanged(nameof(ThrustYAxisStep));
        //    OnPropertyChanged(nameof(TorqueXAxisStep));
        //    OnPropertyChanged(nameof(TorqueYAxisStep));
        //}
        //private void UpdateChartLimits()
        //{
        //    CurrentYAxisMin = CurrentSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Min() / 2.0;
        //    CurrentYAxisMax = Math.Max(CurrentSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Max() * 1.5, 5);

        //    VoltageYAxisMin = VoltageSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Min() / 2.0;
        //    VoltageYAxisMax = Math.Max(VoltageSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Max() * 1.5, 10);

        //    MotorSpeedYAxisMin = MotorSpeedSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Min() / 2.0;
        //    MotorSpeedYAxisMax = Math.Max(MotorSpeedSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Max() * 1.5, 1000);

        //    ThrustYAxisMin = ThrustSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Min() / 2.0;
        //    ThrustYAxisMax = Math.Max(ThrustSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Max() * 4.5, 500);

        //    TorqueYAxisMin = TorqueSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Min() / 2.0;
        //    TorqueYAxisMax = Math.Max(TorqueSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Max() * 4.5, 500);

        //    VibrationYAxisMin = VibrationSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Min() / 2.0;
        //    VibrationYAxisMax = Math.Max(VibrationSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Max() * 3, 2);

        //    VibrationIPSYAxisMin = VibrationIPSSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Min() / 2.0;
        //    VibrationIPSYAxisMax = Math.Max(VibrationIPSSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Max() * 3, 2);

        //    VibrationXYAxisMin = VibrationXSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Min() / 2.0;
        //    VibrationXYAxisMax = Math.Max(VibrationXSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Max() * 10, 2);

        //    VibrationYYAxisMin = VibrationYSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Min() / 2.0;
        //    VibrationYYAxisMax = Math.Max(VibrationYSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Max() * 3, 2);

        //    VibrationZYAxisMin = VibrationZSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Min() / 2.0;
        //    VibrationZYAxisMax = Math.Max(VibrationZSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Max() * 3, 2);

        //    VibrationHighYAxisMin = VibrationHighSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Min() / 2.0;
        //    VibrationHighYAxisMax = Math.Max(VibrationHighSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Max() * 3, 2);

        //    OnPropertyChanged(nameof(CurrentYAxisMin));
        //    OnPropertyChanged(nameof(CurrentYAxisMax));
        //    OnPropertyChanged(nameof(VoltageYAxisMin));
        //    OnPropertyChanged(nameof(VoltageYAxisMax));
        //    OnPropertyChanged(nameof(MotorSpeedYAxisMin));
        //    OnPropertyChanged(nameof(MotorSpeedYAxisMax));
        //    OnPropertyChanged(nameof(ThrustYAxisMin));
        //    OnPropertyChanged(nameof(ThrustYAxisMax));
        //    OnPropertyChanged(nameof(TorqueYAxisMin));
        //    OnPropertyChanged(nameof(TorqueYAxisMax));     
        //    OnPropertyChanged(nameof(VibrationIPSYAxisMin));
        //    OnPropertyChanged(nameof(VibrationIPSYAxisMax));  
        //    OnPropertyChanged(nameof(VibrationYAxisMin));
        //    OnPropertyChanged(nameof(VibrationYAxisMax));  
        //    OnPropertyChanged(nameof(VibrationXYAxisMin));
        //    OnPropertyChanged(nameof(VibrationXYAxisMax));  
        //    OnPropertyChanged(nameof(VibrationYYAxisMin));
        //    OnPropertyChanged(nameof(VibrationYYAxisMax));  
        //    OnPropertyChanged(nameof(VibrationZYAxisMin));
        //    OnPropertyChanged(nameof(VibrationZYAxisMax));  
        //    OnPropertyChanged(nameof(VibrationHighYAxisMin));
        //    OnPropertyChanged(nameof(VibrationHighYAxisMax));
        //}

        //    public event PropertyChangedEventHandler PropertyChanged;
        //    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //    {
        //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //    }

    }
}
