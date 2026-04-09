using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace ExpenseTracker
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Transaction> _transactions = new ObservableCollection<Transaction>();

        public MainWindow()
        {
            InitializeComponent();

            // Initialize database and seed sample data
            Database.InitializeDb();
            Database.SeedSampleData();

            // Setup category filter ComboBox
            CmbCategoryFilter.Items.Add("All");
            foreach (var cat in Enum.GetValues(typeof(Category)))
            {
                CmbCategoryFilter.Items.Add(cat);
            }
            CmbCategoryFilter.SelectedIndex = 0;

            // Set default date range (current month)
            DpFrom.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DpTo.SelectedDate = DateTime.Now;

            // Load data
            LoadTransactions();
        }

        private void LoadTransactions()
        {
            var list = Database.GetAll();
            _transactions.Clear();
            foreach (var t in list)
            {
                _transactions.Add(t);
            }
            LvTransactions.ItemsSource = _transactions;
            UpdateSummary();
            TxtStatus.Text = $"{_transactions.Count} transactions";
        }

        private void UpdateSummary()
        {
            decimal balance = Database.GetBalance();
            decimal income = Database.GetTotalIncome();
            decimal expenses = Database.GetTotalExpenses();

            TxtBalance.Text = $"\u20ac{balance:F2}";
            TxtBalance.Foreground = balance >= 0
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));

            TxtIncome.Text = $"\u20ac{income:F2}";
            TxtExpenses.Text = $"\u20ac{expenses:F2}";
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddTransactionDialog();
            dialog.Owner = this;
            if (dialog.ShowDialog() == true && dialog.Result != null)
            {
                Database.Add(dialog.Result);
                LoadTransactions();
            }
        }

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            List<Transaction> filtered;

            // Apply date filter
            if (DpFrom.SelectedDate.HasValue && DpTo.SelectedDate.HasValue)
            {
                filtered = Database.GetByDateRange(DpFrom.SelectedDate.Value, DpTo.SelectedDate.Value);
            }
            else
            {
                filtered = Database.GetAll();
            }

            // Apply category filter
            if (CmbCategoryFilter.SelectedIndex > 0)
            {
                var selectedCat = (Category)CmbCategoryFilter.SelectedItem;
                filtered = filtered.Where(t => t.Category == selectedCat).ToList();
            }

            _transactions.Clear();
            foreach (var t in filtered)
            {
                _transactions.Add(t);
            }

            // Update summary with filtered totals using LINQ aggregation
            decimal income = filtered.Where(t => t.IsIncome).Sum(t => t.Amount);
            decimal expenses = filtered.Where(t => !t.IsIncome).Sum(t => t.Amount);
            decimal balance = income - expenses;

            TxtBalance.Text = $"\u20ac{balance:F2}";
            TxtBalance.Foreground = balance >= 0
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));
            TxtIncome.Text = $"\u20ac{income:F2}";
            TxtExpenses.Text = $"\u20ac{expenses:F2}";

            TxtStatus.Text = $"{_transactions.Count} transactions (filtered)";
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            CmbCategoryFilter.SelectedIndex = 0;
            DpFrom.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DpTo.SelectedDate = DateTime.Now;
            LoadTransactions();
        }

        private void MenuDelete_Click(object sender, RoutedEventArgs e)
        {
            if (LvTransactions.SelectedItem is Transaction selected)
            {
                var result = MessageBox.Show(
                    $"Delete \"{selected.Description}\" ({selected.AmountText})?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Database.Delete(selected.Id);
                    LoadTransactions();
                }
            }
        }

        private void LvTransactions_ColumnHeaderClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader header && header.Column != null)
            {
                string? sortBy = null;
                var binding = header.Column.DisplayMemberBinding as Binding;
                if (binding != null)
                {
                    sortBy = binding.Path.Path;
                }
                else if (header.Column.Header?.ToString() == "Amount")
                {
                    sortBy = "Amount";
                }

                if (sortBy != null)
                {
                    var view = CollectionViewSource.GetDefaultView(LvTransactions.ItemsSource);
                    if (view.SortDescriptions.Count > 0 && view.SortDescriptions[0].PropertyName == sortBy)
                    {
                        var dir = view.SortDescriptions[0].Direction == ListSortDirection.Ascending
                            ? ListSortDirection.Descending
                            : ListSortDirection.Ascending;
                        view.SortDescriptions.Clear();
                        view.SortDescriptions.Add(new SortDescription(sortBy, dir));
                    }
                    else
                    {
                        view.SortDescriptions.Clear();
                        view.SortDescriptions.Add(new SortDescription(sortBy, ListSortDirection.Ascending));
                    }
                }
            }
        }
    }
}
