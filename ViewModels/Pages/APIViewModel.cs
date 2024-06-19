using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Advanced_Dynotis_Software.ViewModels.Pages
{
    public class APIViewModel : INotifyPropertyChanged
    {
        // Add properties and methods as needed

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
