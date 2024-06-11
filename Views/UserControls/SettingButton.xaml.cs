using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.IconPacks;

namespace Advanced_Dynotis_Software.Views.UserControls
{
    public partial class SettingButton : UserControl
    {
        public SettingButton()
        {
            InitializeComponent();
        }

        public PackIconMaterialKind Icon
        {
            get => (PackIconMaterialKind)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(PackIconMaterialKind), typeof(SettingButton));

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(SettingButton));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(SettingButton));

        public event RoutedEventHandler Click;
        public event MouseEventHandler MouseEnterEvent;
        public event MouseEventHandler MouseLeaveEvent;

        private void Button_Click(object sender, RoutedEventArgs e) => Click?.Invoke(this, e);

        private void Button_MouseEnter(object sender, MouseEventArgs e) => MouseEnterEvent?.Invoke(this, e);

        private void Button_MouseLeave(object sender, MouseEventArgs e) => MouseLeaveEvent?.Invoke(this, e);
    }
}
