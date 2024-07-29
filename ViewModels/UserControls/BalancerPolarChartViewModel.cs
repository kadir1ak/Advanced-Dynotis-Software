using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class BalancerPolarChartViewModel
    {
        public PlotModel CartesianPlotModel { get; set; }
        public PlotModel PolarPlotModel { get; set; }

        public BalancerPolarChartViewModel()
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
            series.Points.Add(new DataPoint(0, 10));
            series.Points.Add(new DataPoint(45, 8));
            series.Points.Add(new DataPoint(90, 9));
            series.Points.Add(new DataPoint(135, 10));
            series.Points.Add(new DataPoint(180, 12));
            series.Points.Add(new DataPoint(225, 11));
            series.Points.Add(new DataPoint(270, 9));
            series.Points.Add(new DataPoint(315, 10));

            model.Series.Add(series);

            return model;
        }

        private PlotModel CreatePolarPlotModel()
        {
            var model = new PlotModel {};

            var angleAxis = new AngleAxis
            {
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                StartAngle = 0,
                EndAngle = 360
            };
            model.Axes.Add(angleAxis);

            var magnitudeAxis = new MagnitudeAxis
            {
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
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
            series.Points.Add(new DataPoint(0, 10));
            series.Points.Add(new DataPoint(45, 8));
            series.Points.Add(new DataPoint(90, 9));
            series.Points.Add(new DataPoint(135, 10));
            series.Points.Add(new DataPoint(180, 12));
            series.Points.Add(new DataPoint(225, 11));
            series.Points.Add(new DataPoint(270, 9));
            series.Points.Add(new DataPoint(315, 10));

            model.Series.Add(series);

            return model;
        }
    }
}
