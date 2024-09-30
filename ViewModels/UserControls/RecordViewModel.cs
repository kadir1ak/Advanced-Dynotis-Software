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
            IsRecording = interfaceVariables.IsRecording;
            Duration = interfaceVariables.Duration;
            FileName = interfaceVariables.FileName;
            RecordCommand = new RelayCommand(param => ToggleRecording());
        }

        private void ToggleRecording()
        {
            MessageBox.Show("Recording stopped.");

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

        }

        private void StopRecording()
        {

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
