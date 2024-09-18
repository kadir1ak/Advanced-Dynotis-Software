using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using Advanced_Dynotis_Software.Models.Dynotis;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class BalancerPolarChartViewModel
    {
        public PlotModel CartesianPlotModel { get; set; }
        public PlotModel PolarPlotModel { get; set; }

        public BalancerPolarChartViewModel(DynotisData dynotisData, InterfaceVariables interfaceVariables)
        {
            CartesianPlotModel = CreateCartesianPlotModel();
            PolarPlotModel = CreatePolarPlotModel();
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
            series.Points.Add(new DataPoint(10, 0));
            series.Points.Add(new DataPoint(11, 30));
            series.Points.Add(new DataPoint(10, 60));
            series.Points.Add(new DataPoint(11, 90));
            series.Points.Add(new DataPoint(9, 120));
            series.Points.Add(new DataPoint(9, 150));
            series.Points.Add(new DataPoint(10, 180));
            series.Points.Add(new DataPoint(9, 210));
            series.Points.Add(new DataPoint(13, 240));
            series.Points.Add(new DataPoint(10, 270));
            series.Points.Add(new DataPoint(9, 330));

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
                Maximum = 20,
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

            series.Points.Add(new DataPoint(10, 0));
            series.Points.Add(new DataPoint(10, 30));
            series.Points.Add(new DataPoint(10, 60));
            series.Points.Add(new DataPoint(10, 90));
            series.Points.Add(new DataPoint(10, 120));
            series.Points.Add(new DataPoint(10, 150));
            series.Points.Add(new DataPoint(10, 180));
            series.Points.Add(new DataPoint(10, 210));
            series.Points.Add(new DataPoint(10, 240));
            series.Points.Add(new DataPoint(10, 270));
            series.Points.Add(new DataPoint(10, 300));
            series.Points.Add(new DataPoint(10, 330));
            series.Points.Add(new DataPoint(10, 360));



            model.Series.Add(series);

            return model;
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
