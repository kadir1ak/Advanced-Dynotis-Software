using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using MahApps.Metro.IconPacks;

namespace Advanced_Dynotis_Software.Views.UserControls
{
    public partial class MenuButton : UserControl
    {
        public MenuButton()
        {
            InitializeComponent();
        }

        public PackIconMaterialKind Icon
        {
            get { return (PackIconMaterialKind)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(PackIconMaterialKind), typeof(MenuButton));

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(MenuButton));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MenuButton));

        public event RoutedEventHandler Click;
        public event MouseEventHandler MouseEnterEvent;
        public event MouseEventHandler MouseLeaveEvent;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            MouseEnterEvent?.Invoke(this, e);
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseLeaveEvent?.Invoke(this, e);
        }
    }
}