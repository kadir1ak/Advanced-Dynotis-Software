using Advanced_Dynotis_Software.ViewModels.Pages;
using System.Windows.Controls;
using System.Windows.Input;

namespace Advanced_Dynotis_Software.Views.Pages
{
    public partial class AutomateTestPage : UserControl
    {
        public AutomateTestPage()
        {
            InitializeComponent();
            DataContext = new AutomateTestViewModel();
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (DataContext is AutomateTestViewModel viewModel)
            {
                // Hücre düzenlemesi tamamlandığında verileri sıralayıp güncelle
                viewModel.SortAndRefreshSequenceItems();
                viewModel.UpdateChartCommand.Execute(null);
            }
        }
        private void DataGridScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                scv.ScrollToHorizontalOffset(scv.HorizontalOffset - e.Delta);
            }
            else
            {
                scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            }
            e.Handled = true;
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
    }
}
