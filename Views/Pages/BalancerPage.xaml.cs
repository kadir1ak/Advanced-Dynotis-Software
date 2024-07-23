using Advanced_Dynotis_Software.ViewModels.Pages;
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

namespace Advanced_Dynotis_Software.Views.Pages
{
    /// <summary>
    /// BalancerPage.xaml etkileşim mantığı
    /// </summary>
    public partial class BalancerPage : UserControl
    {
        public BalancerPage()
        {
            InitializeComponent();
        }
        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.,]+"); // Yalnızca sayı, nokta ve virgül izin ver
            e.Handled = regex.IsMatch(e.Text);
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (DataContext is AutomateTestViewModel viewModel)
            {
                viewModel.SortAndRefreshSequenceItems();
                viewModel.UpdateChartCommand.Execute(null);
            }
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

                if (DataContext is AutomateTestViewModel viewModel)
                {
                    viewModel.SortAndRefreshSequenceItems();
                    viewModel.UpdateChartCommand.Execute(null);
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
    }
}
