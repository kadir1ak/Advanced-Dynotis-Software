using Advanced_Dynotis_Software.ViewModels.Pages;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Advanced_Dynotis_Software.Views.UserControls
{
    /// <summary>
    /// BalancedPropellers.xaml etkileşim mantığı
    /// </summary>
    public partial class BalancedPropellers : UserControl
    {
        public BalancedPropellers()
        {
            InitializeComponent();
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {

        }

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var grid = (DataGrid)sender;

                if (grid.CurrentColumn is DataGridTextColumn)
                {
                    var cellContent = grid.CurrentColumn.GetCellContent(grid.CurrentItem);

                    if (cellContent is TextBox textBox)
                    {
                        var bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
                        bindingExpression.UpdateSource();
                    }
                }

                e.Handled = true;

                if (DataContext is BalancedPropellersViewModel viewModel)
                {
                   // viewModel.SortAndRefreshItems();
                   // viewModel.UpdateChartCommand.Execute(null);
                }
            }
        }
        private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                ScrollViewer scrollViewer = (ScrollViewer)sender;
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
                e.Handled = true;
            }
        }
        private void NumericTextBoxDouble_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.,]+"); // Allow only numbers, dots, and commas
            e.Handled = regex.IsMatch(e.Text);
        }

        private void NumericTextBoxInt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+"); // Allow only numbers
            e.Handled = regex.IsMatch(e.Text);
        }

        private void NumericTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text.Length > 9)
            {
                textBox.Text = textBox.Text.Substring(0, 9);
                textBox.CaretIndex = textBox.Text.Length; // Move caret to end
            }
        }

        private void NumericTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true; // Prevent space key input
            }
        }
    }
}
