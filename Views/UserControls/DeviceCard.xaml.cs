using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Advanced_Dynotis_Software.Views.UserControls
{
    /// <summary>
    /// DeviceCard.xaml etkileşim mantığı
    /// </summary>
    public partial class DeviceCard : UserControl
    {
        public DeviceCard()
        {
            InitializeComponent();
            // Yeni bir PlotController oluştur
            var controller = new PlotController();

            // Fare tekerleği olaylarını devre dışı bırak
            controller.UnbindMouseWheel();

            // Tüm PlotView kontrolleri için kontrolörü ayarla
            MotorSpeedPlot.Controller = controller;
            ThrustPlot.Controller = controller;
            TorquePlot.Controller = controller;
            VoltagePlot.Controller = controller;
            CurrentPlot.Controller = controller;
            VibrationPlot.Controller = controller;
        }
    }
}
