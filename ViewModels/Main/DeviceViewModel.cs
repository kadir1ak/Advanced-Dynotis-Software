using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Advanced_Dynotis_Software.Models.Dynotis;

namespace Advanced_Dynotis_Software.ViewModels.Main
{
    public class DeviceViewModel : INotifyPropertyChanged, IDisposable
    {
        public InterfaceVariables InterfaceVariables { get; private set; }
        private bool _isUpdatingInterfaceVariables;

        public ChartViewModel ChartViewModel { get; }

        public string DeviceDisplayName => $"{Device.Model} - {Device.SeriNo}";

        private Dynotis _device;
        private Queue<DynotisData> _dynotisDataBuffer;
        private readonly object _bufferLock = new();
        private const int BufferLimit = 10;
        private CancellationTokenSource _cancellationTokenSource;

        public Dynotis Device
        {
            get => _device;
            set
            {
                if (_device != value)
                {
                    _device = value;
                    OnPropertyChanged();
                }
            }
        }

        public DeviceViewModel(string portName)
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                InterfaceVariables = new InterfaceVariables();
                Device = new Dynotis(portName);
                ChartViewModel = new ChartViewModel();
                _dynotisDataBuffer = new Queue<DynotisData>();
                _cancellationTokenSource = new CancellationTokenSource();

                Device.PropertyChanged += Device_PropertyChanged;

                InitializeDeviceAsync();
                InitializeTasks(_cancellationTokenSource.Token);
            }
        }

        private async void InitializeDeviceAsync()
        {
            await Device.OpenPortAsync();
        }

        private void InitializeTasks(CancellationToken token)
        {
            Task.Run(() => UpdateChartDataLoop(token), token);
            Task.Run(() => UpdateInterfaceVariablesLoop(token), token);
        }

        private async Task UpdateChartDataLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(1);

                DynotisData dynotisData = null;
                lock (_bufferLock)
                {
                    if (_dynotisDataBuffer.Count > 0)
                    {
                        dynotisData = _dynotisDataBuffer.Dequeue();
                    }
                }

                if (dynotisData != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ChartViewModel.UpdateChartData(dynotisData);
                    });
                }
            }
        }

        private async Task UpdateInterfaceVariablesLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(20); // Interface update interval (50Hz)

                DynotisData sensorData = null;
                lock (_bufferLock)
                {
                    if (_dynotisDataBuffer.Count > 0)
                    {
                        sensorData = _dynotisDataBuffer.Dequeue();
                    }
                }

                if (sensorData != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        InterfaceVariables.UpdateFrom(sensorData);
                    });
                }
            }
        }

        private void Device_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Dynotis.DynotisData))
            {
                lock (_bufferLock)
                {
                    if (_dynotisDataBuffer.Count >= BufferLimit)
                    {
                        _dynotisDataBuffer.Dequeue();
                    }
                    _dynotisDataBuffer.Enqueue(Device.DynotisData);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
