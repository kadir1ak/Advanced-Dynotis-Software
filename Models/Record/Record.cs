using System;

namespace Advanced_Dynotis_Software.Models.Record
{
    public class Record
    {
        public bool IsRecording { get; set; }
        public TimeSpan Duration { get; set; }
        public string FileName { get; set; }

        public Record()
        {
            IsRecording = false;
            Duration = TimeSpan.Zero;
            FileName = string.Empty;
        }
    }
}
