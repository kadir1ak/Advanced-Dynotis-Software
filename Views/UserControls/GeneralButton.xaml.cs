using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace Advanced_Dynotis_Software.Views.UserControls
{
    public partial class GeneralButton : UserControl
    {
        public GeneralButton()
        {
            InitializeComponent();
        }

        public PackIconFontAwesomeKind Icon
        {
            get => (PackIconFontAwesomeKind)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(PackIconFontAwesomeKind), typeof(GeneralButton));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(GeneralButton));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(GeneralButton));

        public event RoutedEventHandler Click;

        private void Button_Click(object sender, RoutedEventArgs e) => Click?.Invoke(this, e);

        public void UpdateTheme()
        {
            var newForeground = (SolidColorBrush)Application.Current.Resources["Text"];
            var button = (Button)Content;
            button.Foreground = newForeground;
        }
    }
}
