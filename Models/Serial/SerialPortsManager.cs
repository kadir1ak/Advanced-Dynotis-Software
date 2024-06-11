using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Windows;

namespace Advanced_Dynotis_Software.Models.Serial
{
    public class SerialPortsManager
    {
        public ObservableCollection<SerialPort> SerialPorts { get; private set; }
        public event Action SerialPortsEvent;

        public SerialPortsManager()
        {
            SerialPorts = new ObservableCollection<SerialPort>();
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

                WqlEventQuery query = new WqlEventQuery("SELECT * FROM __InstanceOperationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_USBControllerDevice'");
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
                try
                {
                    // Handle event
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"SerialPortsPhysicallyRemovedEventHandler: {ex.Message}");
                }
            });
        }

        private void SerialPortsCreationDeletionEventHandler(object sender, EventArrivedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ScanSerialPorts();
                SerialPortsEvent?.Invoke();
            });
        }

        public void ScanSerialPorts()
        {
            try
            {
                SerialPorts.Clear();
                string[] portNames = SerialPort.GetPortNames();
                foreach (var portName in portNames)
                {
                    var port = new SerialPort(portName);
                    SerialPorts.Add(port);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ScanSerialPorts: {ex.Message}");
            }
        }

        public IEnumerable<string> GetSerialPorts()
        {
            return SerialPorts.Select(p => p.PortName);
        }
    }
}
