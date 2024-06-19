using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Windows;

namespace Advanced_Dynotis_Software.Models.Serial
{
    public class SerialPortsManager
    {
        public ObservableCollection<string> SerialPorts { get; private set; }
        public event Action<string> SerialPortAdded;
        public event Action<string> SerialPortRemoved;

        public SerialPortsManager()
        {
            SerialPorts = new ObservableCollection<string>();
            SerialPortsChangedEvents();
            ScanSerialPorts();
        }

        private void SerialPortsChangedEvents()
        {
            try
            {
                WqlEventQuery physicallyRemovedQuery = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3");
                var serialPortsPhysicallyRemovedEvents = new ManagementEventWatcher(new ManagementScope("root\\CIMV2"), physicallyRemovedQuery);
                serialPortsPhysicallyRemovedEvents.EventArrived += SerialPortsPhysicallyRemovedEventHandler;
                serialPortsPhysicallyRemovedEvents.Start();

                WqlEventQuery query = new WqlEventQuery("SELECT * FROM __InstanceOperationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_SerialPort'");
                var serialPortsCreationDeletionEvents = new ManagementEventWatcher(query);
                serialPortsCreationDeletionEvents.EventArrived += SerialPortsCreationDeletionEventHandler;
                serialPortsCreationDeletionEvents.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SerialPortsChangedEvents: {ex.Message}");
            }
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
    }
}
