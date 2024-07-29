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
            series.Points.Add(new DataPoint(45, 11));
            series.Points.Add(new DataPoint(90, 10));
            series.Points.Add(new DataPoint(135, 11));
            series.Points.Add(new DataPoint(180, 9));
            series.Points.Add(new DataPoint(225, 9));
            series.Points.Add(new DataPoint(270, 10));
            series.Points.Add(new DataPoint(315, 9));
            series.Points.Add(new DataPoint(355, 9));

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
            series.Points.Add(new DataPoint(0, 0));
            series.Points.Add(new DataPoint(11, 30));
            series.Points.Add(new DataPoint(12, 45));
            series.Points.Add(new DataPoint(10, 90));
            series.Points.Add(new DataPoint(8, 135));
            series.Points.Add(new DataPoint(9, 180));
            series.Points.Add(new DataPoint(11, 225));
            series.Points.Add(new DataPoint(10, 270));
            series.Points.Add(new DataPoint(11, 315));
            series.Points.Add(new DataPoint(12, 355));

            model.Series.Add(series);

            return model;
        }
    }
}
