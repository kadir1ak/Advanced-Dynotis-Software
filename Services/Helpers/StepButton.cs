using MahApps.Metro.IconPacks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media; // Bu using yönergesini ekleyin

namespace Advanced_Dynotis_Software.Services.Helpers
{
    public class StepButton : Button
    {
        public static readonly DependencyProperty EllipseColorProperty =
            DependencyProperty.Register("EllipseColor", typeof(Brush), typeof(StepButton), new PropertyMetadata(default(Brush)));

        public Brush EllipseColor
        {
            get { return (Brush)GetValue(EllipseColorProperty); }
            set { SetValue(EllipseColorProperty, value); }
        }

        public static readonly DependencyProperty IconKindProperty =
            DependencyProperty.Register("IconKind", typeof(PackIconFontAwesomeKind), typeof(StepButton), new PropertyMetadata(default(PackIconFontAwesomeKind)));

        public PackIconFontAwesomeKind IconKind
        {
            get { return (PackIconFontAwesomeKind)GetValue(IconKindProperty); }
            set { SetValue(IconKindProperty, value); }
        }

        public static readonly DependencyProperty IconColorProperty =
            DependencyProperty.Register("IconColor", typeof(Brush), typeof(StepButton), new PropertyMetadata(default(Brush)));

        public Brush IconColor
        {
            get { return (Brush)GetValue(IconColorProperty); }
            set { SetValue(IconColorProperty, value); }
        }
    }
}
