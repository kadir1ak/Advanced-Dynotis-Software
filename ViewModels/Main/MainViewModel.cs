using Advanced_Dynotis_Software.Models.Device;
using Advanced_Dynotis_Software.Models.Interface;
using Advanced_Dynotis_Software.Models.Serial;
using Advanced_Dynotis_Software.Services.BindableBase;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Advanced_Dynotis_Software.ViewModels.Main
{
    public class MainViewModel : BindableBase
    {
        public MainViewModel()
        {
            Device = new Device();
            Interface = new InterfaceData();
            Interface.InitializeCharts(Device);

            serialPortsManager = new SerialPortsManager();

            // ComboBox'ı SerialPorts koleksiyonuna bağla
            //portComboBoxItemsSource = serialPortsManager.SerialPorts;
            //Closed += (s, e) => serialPortsManager.Dispose();

            StartUpdateDataLoop();
            StartUpdatePlotLoop();
        }

        private readonly SerialPortsManager serialPortsManager;
        public Device Device { get; set; }
        public InterfaceData Interface { get; set; }

        #region Update Data Loop (Veri Güncelleme Döngüsü)

        private CancellationTokenSource _updateDataLoopCancellationTokenSource;
        private int DataUpdateTimeMillisecond = 100; // 10 Hz (100ms)

        private async Task UpdateDataLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(DataUpdateTimeMillisecond, token);

                    // Verileri güncelle
                    Interface.UpdateDeviceData(Device);
                }
            }
            catch (TaskCanceledException)
            {
                // Döngü iptal edildiğinde hata atılmaz
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Data update loop error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void StartUpdateDataLoop()
        {
            StopUpdateDataLoop();
            _updateDataLoopCancellationTokenSource = new CancellationTokenSource();
            var token = _updateDataLoopCancellationTokenSource.Token;
            _ = UpdateDataLoop(token);
        }

        public void StopUpdateDataLoop()
        {
            if (_updateDataLoopCancellationTokenSource != null && !_updateDataLoopCancellationTokenSource.IsCancellationRequested)
            {
                _updateDataLoopCancellationTokenSource.Cancel();
                _updateDataLoopCancellationTokenSource.Dispose();
                _updateDataLoopCancellationTokenSource = null;
            }
        }

        #endregion

        #region Update Plot Loop (Grafik Güncelleme Döngüsü)

        private CancellationTokenSource _updatePlotLoopCancellationTokenSource;
        private int PlotUpdateTimeMillisecond = 10; // 100 Hz (10ms)

        private async Task UpdatePlotLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(PlotUpdateTimeMillisecond, token);

                    // Grafik güncellemeleri
                    Interface.UpdateCharts(Device);
                }
            }
            catch (TaskCanceledException)
            {
                // Döngü iptal edildiğinde hata atılmaz
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Plot update loop error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void StartUpdatePlotLoop()
        {
            StopUpdatePlotLoop();
            _updatePlotLoopCancellationTokenSource = new CancellationTokenSource();
            var token = _updatePlotLoopCancellationTokenSource.Token;
            _ = UpdatePlotLoop(token);
        }

        public void StopUpdatePlotLoop()
        {
            if (_updatePlotLoopCancellationTokenSource != null && !_updatePlotLoopCancellationTokenSource.IsCancellationRequested)
            {
                _updatePlotLoopCancellationTokenSource.Cancel();
                _updatePlotLoopCancellationTokenSource.Dispose();
                _updatePlotLoopCancellationTokenSource = null;
            }
        }

        #endregion
    }
}
