using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using Advanced_Dynotis_Software.Models.Dynotis;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class BalancerPolarChartViewModel
    {
        public PlotModel CartesianPlotModel { get; set; }
        public PlotModel PolarPlotModel { get; set; }



        private InterfaceVariables _interfaceVariables;
        private DynotisData _dynotisData;

        public BalancerPolarChartViewModel(DynotisData dynotisData, InterfaceVariables interfaceVariables)
        {
            CartesianPlotModel = CreateCartesianPlotModel();
            PolarPlotModel = CreatePolarPlotModel();

            _interfaceVariables = interfaceVariables;
            _dynotisData = dynotisData;
            // Subscribe to the PropertyChanged event of InterfaceVariables
            _interfaceVariables.PropertyChanged += InterfaceVariables_PropertyChanged;
        }
        private void InterfaceVariables_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_interfaceVariables.VibrationDynamicBalancer360))
            {
                for (int i = 0; i < _interfaceVariables.VibrationDynamicBalancer360.Length; i++)
                {
                    VibrationDynamicBalancer360[i] = _interfaceVariables.VibrationDynamicBalancer360[i];
                }
                UpdateVibrationPolarChart();
            }
        }
        private void UpdateVibrationPolarChart()
        {
            // Get the series from the PolarPlotModel
            var series = PolarPlotModel.Series.FirstOrDefault() as LineSeries;

            // Ensure the series exists before proceeding
            if (VibrationDynamicBalancer360 != null && VibrationDynamicBalancer360.Length == 12) // Assuming 12 data points expected
            {

                // Clear existing points
                series.Points.Clear();

                // Populate the points based on the Vibration360 values
                series.Points.Add(new DataPoint(VibrationDynamicBalancer360[0], 0));    // 0 degrees
                series.Points.Add(new DataPoint(VibrationDynamicBalancer360[1], 30));   // 30 degrees
                series.Points.Add(new DataPoint(VibrationDynamicBalancer360[2], 60));   // 60 degrees
                series.Points.Add(new DataPoint(VibrationDynamicBalancer360[3], 90));   // 90 degrees
                series.Points.Add(new DataPoint(VibrationDynamicBalancer360[4], 120));  // 120 degrees
                series.Points.Add(new DataPoint(VibrationDynamicBalancer360[5], 150));  // 150 degrees
                series.Points.Add(new DataPoint(VibrationDynamicBalancer360[6], 180));  // 180 degrees
                series.Points.Add(new DataPoint(VibrationDynamicBalancer360[7], 210));  // 210 degrees
                series.Points.Add(new DataPoint(VibrationDynamicBalancer360[8], 240));  // 240 degrees
                series.Points.Add(new DataPoint(VibrationDynamicBalancer360[9], 270));  // 270 degrees
                series.Points.Add(new DataPoint(VibrationDynamicBalancer360[10], 300)); // 300 degrees
                series.Points.Add(new DataPoint(VibrationDynamicBalancer360[11], 330)); // 330 degrees

                // Refresh the chart to display the updated points
                PolarPlotModel.InvalidatePlot(true);
            }
            else
            {
                // Handle the case where the Vibration360 list is null or has an unexpected number of points
                Console.WriteLine("Error: Vibration360 array does not contain the expected number of data points.");
            }
        }

        private PlotModel CreateCartesianPlotModel()
        {
            var model = new PlotModel {};

            var phaseAxis = new LinearAxis { Position = AxisPosition.Bottom, Title = "Phase Position (deg)" };
            var vibrationAxis = new LinearAxis { Position = AxisPosition.Left, Title = "Vibration" };

            model.Axes.Add(phaseAxis);
            model.Axes.Add(vibrationAxis);

            var series = new LineSeries
            {
                Title = "Vibration Data",
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStroke = OxyColors.Blue
            };

            // Sabit veriler ekleniyor
            series.Points.Add(new DataPoint(0.10, 0));
            series.Points.Add(new DataPoint(0.11, 30));
            series.Points.Add(new DataPoint(0.10, 60));
            series.Points.Add(new DataPoint(0.11, 90));
            series.Points.Add(new DataPoint(0.9, 120));
            series.Points.Add(new DataPoint(0.9, 150));
            series.Points.Add(new DataPoint(0.10, 180));
            series.Points.Add(new DataPoint(0.9, 210));
            series.Points.Add(new DataPoint(0.13, 240));
            series.Points.Add(new DataPoint(0.10, 270));
            series.Points.Add(new DataPoint(0.9, 330));

            model.Series.Add(series);

            return model;
        }

        private PlotModel CreatePolarPlotModel()
        {
            var model = new PlotModel {};

            var angleAxis = new AngleAxis
            {
                Minimum = 0,
                Maximum = 360,
                MajorStep = 30,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                StartAngle = 0,
                EndAngle = 360,
                Title = "Angle (degrees)"
            };
            model.Axes.Add(angleAxis);

            var magnitudeAxis = new MagnitudeAxis
            {
                Minimum = 0,
                Maximum = 1,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Title = "Magnitude"
            };
            model.Axes.Add(magnitudeAxis);

            var series = new LineSeries
            {
                Title = "Vibration Data",
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStroke = OxyColors.Blue
                
            };

            // Sabit veriler ekleniyor

            series.Points.Add(new DataPoint(0.10, 0));
            series.Points.Add(new DataPoint(0.10, 30));
            series.Points.Add(new DataPoint(0.10, 60));
            series.Points.Add(new DataPoint(0.10, 90));
            series.Points.Add(new DataPoint(0.10, 120));
            series.Points.Add(new DataPoint(0.10, 150));
            series.Points.Add(new DataPoint(0.10, 180));
            series.Points.Add(new DataPoint(0.10, 210));
            series.Points.Add(new DataPoint(0.10, 240));
            series.Points.Add(new DataPoint(0.10, 270));
            series.Points.Add(new DataPoint(0.10, 300));
            series.Points.Add(new DataPoint(0.10, 330));
            series.Points.Add(new DataPoint(0.10, 360));

            model.Series.Add(series);

            return model;
        }

        private double[] _vibrationDynamicBalancer360 = new double[12];
        public double[] VibrationDynamicBalancer360
        {
            get => _vibrationDynamicBalancer360;
            set => SetProperty(ref _vibrationDynamicBalancer360, value);
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
}
