using System;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Threading;
using System.Windows;

namespace Advanced_Dynotis_Software.Models.Serial
{
    public class SerialPortsManager
    {
        public ObservableCollection<SerialPort> serialPorts;
        public ManagementEventWatcher? serialPortsPhysicallyRemovedEvents;
        public ManagementEventWatcher? serialPortsCreationDeletionEvents;
        public event Action serialPortsEvent;

        public SerialPortsManager()
        {
            serialPorts = new ObservableCollection<SerialPort> { };
            SerialPortsChangedEvents();
            ScanSerialPorts();
        }
        private void SerialPortsChangedEvents()
        {
            try
            {
                WqlEventQuery physicallyRemovedQuery = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3");
                serialPortsPhysicallyRemovedEvents = new ManagementEventWatcher(new ManagementScope("root\\CIMV2"), physicallyRemovedQuery);
                serialPortsPhysicallyRemovedEvents.EventArrived += SerialPortsPhysicallyRemovedEventHandler;
                serialPortsPhysicallyRemovedEvents.Start();

                WqlEventQuery query = new WqlEventQuery("SELECT * FROM __InstanceOperationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_USBControllerDevice'");
                serialPortsCreationDeletionEvents = new ManagementEventWatcher(query);
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
                   // MessageBox.Show("Physically Removed Events");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"SerialPortsPhysicallyRemovedEventHandler: {ex.Message}");
                }
            });
        }
        private void SerialPortsCreationDeletionEventHandler(object sender, EventArrivedEventArgs e)
        {
            // Olay türünü al
            string eventType = e.NewEvent.ClassPath.ClassName;
            // Bir önceki tarama sırasında bulunan portları saklamak için bir liste oluştur
            string[] previousPortNames = serialPorts.Select(port => port.PortName).ToArray();
            // Mevcut seri portları tara
            string[] currentPortNames = SerialPort.GetPortNames();

            // Seri port ekleme olayı ise
            if (eventType.Equals("__InstanceCreationEvent"))
            {
                foreach (string portName in currentPortNames)
                {
                    // Yeni bir seri port bulunduğunda
                    if (!previousPortNames.Contains(portName))
                    {
                        // Yeni eklenen seri portu işlemek için gerekli adımları gerçekleştir
                        SerialPort portToAdd = new SerialPort(portName);
                        serialPorts?.Add(portToAdd);
                        portToAdd.Disposed += (sender, e) => Port_Disposed(sender, e, portToAdd.PortName);
                        //MessageBox.Show($"AddEvent:{portToAdd.PortName}");
                        serialPortsEvent.Invoke();
                    }
                }
            }
            // Seri port kaldırma olayı ise
            else if (eventType.Equals("__InstanceDeletionEvent"))
            {
                // Bir önceki taramada var olan ancak şu anda taramada bulunmayan portları kontrol etmek için
                foreach (string previousPortName in previousPortNames)
                {
                    // Şu anda taramada bulunmayan bir seri port varsa, bu portun çıkartıldığını belirt
                    if (!currentPortNames.Contains(previousPortName))
                    {
                        // Çıkartılan portu serialPorts listesinden de çıkart
                        SerialPort? portToRemove = serialPorts.FirstOrDefault(p => p.PortName == previousPortName);
                        if (portToRemove != null)
                        {
                            serialPorts.Remove(portToRemove);
                            //MessageBox.Show($"RemoveEvent:{portToRemove.PortName}");
                            serialPortsEvent.Invoke();
                        }
                    }
                }
            }
        }

        private void Port_Disposed(object sender, EventArgs e, string portName)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Seri port nesnesi artık yok edildiğinde yapılacak işlemler...
                //MessageBox.Show($"Disposed: {portName}");
            });
        }
        public void ScanSerialPorts()
        {
            try
            {
                serialPorts?.Clear();
                string[] portNames = SerialPort.GetPortNames();
                foreach (var portName in portNames)
                {
                    SerialPort port = new SerialPort(portName);
                    serialPorts.Add(port);
                    port.Disposed += (sender, e) => Port_Disposed(sender, e, port.PortName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ScanSerialPorts: {ex.Message}");
            }
        }

        public void ConnectToSerialPort(string portName)
        {
            try
            {
                SerialPort? port = serialPorts.FirstOrDefault(p => p.PortName == portName);
                if (port == null)
                {
                    MessageBox.Show("Belirtilen seri port bulunamadı.");
                    return;
                }

                if (!port.IsOpen)
                {
                    port.BaudRate = 115200; // Baud rate ayarını yap
                    port.DataBits = 8; // Veri bit sayısını ayarla (varsayılan olarak 8)
                    port.Parity = Parity.None; // Çiftlik kontrol ayarını ayarla (varsayılan olarak Yok)
                    port.StopBits = StopBits.One; // Durma bitlerini ayarla (varsayılan olarak Bir)
                    port.Handshake = Handshake.None; // El sıkışma ayarını ayarla (varsayılan olarak Yok)

                    port.Open();
                    MessageBox.Show($"Open:{port}");
                }
                else
                {
                    MessageBox.Show("Seri port zaten açık.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bağlantı Başarısız: {ex.Message}");
            }
        }

        public void DisConnectToSerialPort(string portName)
        {
            try
            {
                SerialPort? port = serialPorts.FirstOrDefault(p => p.PortName == portName);
                if (port == null)
                {
                    MessageBox.Show("Belirtilen seri port bulunamadı.");
                    return;
                }

                if (port.IsOpen)
                {
                    port.Close();
                    MessageBox.Show($"Close:{port}");
                }
                else
                {
                    MessageBox.Show("Seri port zaten kapalı.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bağlantı Kesme Başarısız: {ex.Message}");
            }
        }
    }
}
