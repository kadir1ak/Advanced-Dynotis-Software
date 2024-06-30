using System;
using System.Diagnostics;
using System.Windows.Threading;
using Advanced_Dynotis_Software.Models.Record;

namespace Advanced_Dynotis_Software.ViewModels.Managers
{
    public class RecordManager
    {
        private Record _record;
        private DispatcherTimer _timer;
        private Stopwatch _stopWatch;

        public event EventHandler<TimeSpan> TimeUpdated;

        public RecordManager()
        {
            _record = new Record();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100); // Daha hassas bir zamanlayıcı
            _timer.Tick += Timer_Tick;

            _stopWatch = new Stopwatch();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_stopWatch.IsRunning)
            {
                _record.Duration = _stopWatch.Elapsed;
                TimeUpdated?.Invoke(this, _record.Duration);
            }
        }

        public void ToggleRecording()
        {
            if (_record.IsRecording)
            {
                StopRecording();
            }
            else
            {
                StartRecording();
            }
        }

        private void StartRecording()
        {
            _record.IsRecording = true;
            _stopWatch.Start();
            _timer.Start();
        }

        private void StopRecording()
        {
            _record.IsRecording = false;
            _stopWatch.Stop();
            _timer.Stop();
        }

        public bool IsRecording => _record.IsRecording;
        public TimeSpan Duration => _record.Duration;
        public string FileName
        {
            get => _record.FileName;
            set => _record.FileName = value;
        }
    }
}
