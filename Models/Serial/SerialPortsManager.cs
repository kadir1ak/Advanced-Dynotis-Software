using System;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Advanced_Dynotis_Software.Models.Serial
{
    public class SerialPortsManager : IDisposable
    {
        private readonly ManagementEventWatcher _serialPortsPhysicallyRemovedEvents;
        private readonly ManagementEventWatcher _serialPortsCreationDeletionEvents;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public ObservableCollection<string> SerialPorts { get; private set; }
        public event Action<string> SerialPortAdded;
        public event Action<string> SerialPortRemoved;

        public SerialPortsManager()
        {
            SerialPorts = new ObservableCollection<string>();
            _cancellationTokenSource = new CancellationTokenSource();
            _serialPortsPhysicallyRemovedEvents = CreateEventWatcher("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3", SerialPortsPhysicallyRemovedEventHandler);
            _serialPortsCreationDeletionEvents = CreateEventWatcher("SELECT * FROM __InstanceOperationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_SerialPort'", SerialPortsCreationDeletionEventHandler);
            ScanSerialPorts();
        }

        private ManagementEventWatcher CreateEventWatcher(string query, EventArrivedEventHandler eventHandler)
        {
            var watcher = new ManagementEventWatcher(new ManagementScope("root\\CIMV2"), new WqlEventQuery(query));
            watcher.EventArrived += eventHandler;
            watcher.Start();
            return watcher;
        }

        private void SerialPortsPhysicallyRemovedEventHandler(object sender, EventArrivedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ScanSerialPorts();
            });
        }

        private void SerialPortsCreationDeletionEventHandler(object sender, EventArrivedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ScanSerialPorts();
            });
        }

        public void ScanSerialPorts()
        {
            try
            {
                var existingPorts = SerialPorts.ToList();
                var currentPorts = SerialPort.GetPortNames().ToList();

                foreach (var port in currentPorts)
                {
                    if (!SerialPorts.Contains(port))
                    {
                        SerialPorts.Add(port);
                        SerialPortAdded?.Invoke(port);
                    }
                }

                foreach (var port in existingPorts)
                {
                    if (!currentPorts.Contains(port))
                    {
                        SerialPorts.Remove(port);
                        SerialPortRemoved?.Invoke(port);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ScanSerialPorts: {ex.Message}");
            }
        }

        public IEnumerable<string> GetSerialPorts()
        {
            return SerialPorts;
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _serialPortsPhysicallyRemovedEvents?.Stop();
            _serialPortsCreationDeletionEvents?.Stop();
            _serialPortsPhysicallyRemovedEvents?.Dispose();
            _serialPortsCreationDeletionEvents?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }
}
