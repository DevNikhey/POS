using System;
using System.Globalization;
using System.Windows;

namespace ExpenseTracker
{
    public partial class AddTransactionDialog : Window
    {
        public Transaction? Result { get; private set; }

        public AddTransactionDialog()
        {
            InitializeComponent();

            // Populate category ComboBox
            foreach (var cat in Enum.GetValues(typeof(Category)))
            {
                CmbCategory.Items.Add(cat);
            }
            CmbCategory.SelectedIndex = 0;

            // Default date to today
            DpDate.SelectedDate = DateTime.Now;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validate description
            if (string.IsNullOrWhiteSpace(TxtDescription.Text))
            {
                TxtError.Text = "Please enter a description.";
                TxtDescription.Focus();
                return;
            }

            // Validate amount
            if (!decimal.TryParse(TxtAmount.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount) || amount <= 0)
            {
                TxtError.Text = "Please enter a valid positive amount.";
                TxtAmount.Focus();
                return;
            }

            // Validate date
            if (!DpDate.SelectedDate.HasValue)
            {
                TxtError.Text = "Please select a date.";
                return;
            }

            Result = new Transaction
            {
                Description = TxtDescription.Text.Trim(),
                Amount = amount,
                Category = (Category)CmbCategory.SelectedItem,
                IsIncome = RbIncome.IsChecked == true,
                Date = DpDate.SelectedDate.Value
            };

            DialogResult = true;
        }
    }
}
