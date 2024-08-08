using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.Services.Helpers;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class RecordViewModel : INotifyPropertyChanged
    {
        private string _testMode;
        private bool _isRecording;
        private TimeSpan _duration;
        private string _fileName;
        private InterfaceVariables _interfaceVariables;
        private StreamWriter _streamWriter;
        private Task _recordingTask;
        private bool _stopRequested;

        public string TestMode
        {
            get => _testMode;
            set
            {
                if (SetProperty(ref _testMode, value))
                {
                    _interfaceVariables.TestMode = value;
                    OnPropertyChanged(nameof(_interfaceVariables));
                }
            }
        }

        public bool IsRecording
        {
            get => _isRecording;
            set
            {
                if (SetProperty(ref _isRecording, value))
                {
                    _interfaceVariables.IsRecording = value;
                    OnPropertyChanged(nameof(_interfaceVariables));
                }
            }
        }

        public TimeSpan Duration
        {
            get => _duration;
            set
            {
                if (SetProperty(ref _duration, value))
                {
                    _interfaceVariables.Duration = value;
                    OnPropertyChanged(nameof(_interfaceVariables));
                }
            }
        }

        public string FileName
        {
            get => _fileName;
            set
            {
                if (SetProperty(ref _fileName, value))
                {
                    _interfaceVariables.FileName = value;
                    OnPropertyChanged(nameof(_interfaceVariables));
                }
            }
        }

        public ICommand RecordCommand { get; }

        public RecordViewModel(InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
            TestMode = interfaceVariables.TestMode;
            IsRecording = interfaceVariables.IsRecording;
            Duration = interfaceVariables.Duration;
            FileName = interfaceVariables.FileName;
            RecordCommand = new RelayCommand(param => ToggleRecording());
        }

        private void ToggleRecording()
        {
            if (IsRecording)
            {
                StopRecording();
                MessageBox.Show("Recording stopped.");
            }
            else
            {
                StartRecording();
                MessageBox.Show("Recording started.");
            }
        }

        private void StartRecording()
        {
            if (string.IsNullOrWhiteSpace(FileName))
            {
                MessageBox.Show("Lütfen geçerli bir dosya adı girin.");
                return;
            }

            IsRecording = true;
            _stopRequested = false;

            string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Records");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string filePath = Path.Combine(directoryPath, $"{FileName}.csv");
            _streamWriter = new StreamWriter(filePath, true);
            _streamWriter.WriteLine("Time,Current,Voltage,Thrust,Torque,MotorSpeed,VibrationX,VibrationY,VibrationZ,AmbientTemp,MotorTemp,Pressure,WindSpeed,Power,Vibration,WindDirection,AirDensity");

            _recordingTask = Task.Run(() => RecordData());
        }

        private void StopRecording()
        {
            _stopRequested = true;
            _recordingTask.Wait();
            _streamWriter.Close();
            IsRecording = false;
            // CSV formatına çevirme işlemi burada yapılabilir
            ConvertToCsv();
        }

        private void ConvertToCsv()
        {
            // Gerekirse burada dosyayı CSV formatına çevirme işlemleri yapılabilir
            // Mevcut dosya zaten CSV formatında oluşturulduğu için ek bir işleme gerek olmayabilir
        }

        private async Task RecordData()
        {
            while (!_stopRequested)
            {
                string dataLine = $"{_interfaceVariables.Time},{_interfaceVariables.Current},{_interfaceVariables.Voltage},{_interfaceVariables.Thrust.Value},{_interfaceVariables.Torque.Value},{_interfaceVariables.MotorSpeed.Value},{_interfaceVariables.Vibration.VibrationX},{_interfaceVariables.Vibration.VibrationY},{_interfaceVariables.Vibration.VibrationZ},{_interfaceVariables.AmbientTemp.Value},{_interfaceVariables.MotorTemp.Value},{_interfaceVariables.Pressure.Value},{_interfaceVariables.WindSpeed.Value},{_interfaceVariables.Theoric.Power},{_interfaceVariables.Vibration},{_interfaceVariables.WindDirection},{_interfaceVariables.Theoric.AirDensity}";
                await _streamWriter.WriteLineAsync(dataLine);
                await Task.Delay(1); // 1000 Hz frekansla kayıt için 1ms bekle
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
