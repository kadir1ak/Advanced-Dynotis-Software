using System;
using System.Windows;
using System.Windows.Controls;
using Advanced_Dynotis_Software.ViewModels.Managers;
using MahApps.Metro.IconPacks;

namespace Advanced_Dynotis_Software.Views.UserControls
{
    public partial class Record : UserControl
    {
        private RecordManager _recordManager;

        public Record()
        {
            InitializeComponent();
            _recordManager = new RecordManager();
            _recordManager.TimeUpdated += RecordManager_TimeUpdated;
        }

        private void RecordManager_TimeUpdated(object sender, TimeSpan e)
        {
            RecordTimeTextBlock.Text = String.Format("{0:00}:{1:00}:{2:00}",
                e.Hours, e.Minutes, e.Seconds);
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_recordManager.FileName))
            {
                MessageBox.Show("Please enter a file name before starting the recording.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _recordManager.ToggleRecording();
            RecordButtonIcon.Kind = _recordManager.IsRecording
                ? PackIconMaterialKind.StopCircleOutline
                : PackIconMaterialKind.PlayCircleOutline;
        }

        private void FileNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                _recordManager.FileName = textBox.Text;
            }
        }
    }
}
