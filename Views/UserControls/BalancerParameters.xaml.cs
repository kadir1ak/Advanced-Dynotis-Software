using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace Advanced_Dynotis_Software.Views.UserControls
{
    /// <summary>
    /// BalancerParameters.xaml etkileşim mantığı
    /// </summary>
    public partial class BalancerParameters : UserControl
    {
        public BalancerParameters()
        {
            InitializeComponent();
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
