using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TodoApp
{
    // Code-behind for huvudfonstret.
    // Hanterar all logik: ladda data, filtrera, soka, CRUD-operationer.
    public partial class MainWindow : Window
    {
        // ObservableCollection meddelar UI automatiskt nar objekt laggs till/tas bort
        private ObservableCollection<TodoItem> _allItems = new ObservableCollection<TodoItem>();

        public MainWindow()
        {
            InitializeComponent();

            // Initiera databasen (skapa tabell om den saknas)
            Database.InitializeDb();

            // Ladda alla uppgifter fran databasen
            LoadItems();
        }

        // Laddar alla uppgifter fran databasen och uppdaterar listan
        private void LoadItems()
        {
            List<TodoItem> items = Database.GetAll();
            _allItems = new ObservableCollection<TodoItem>(items);
            ApplyFilter(); // Tillamppa eventuellt aktivt filter
            UpdateStatusBar();
        }

        // Tillamppar filter och sokning pa listan
        private void ApplyFilter()
        {
            // Hamta valt filter fran ComboBox
            string filter = "All";
            if (FilterComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                filter = selectedItem.Content?.ToString() ?? "All";
            }

            // Hamta soktext
            string search = SearchTextBox?.Text?.Trim() ?? "";

            // Borja med alla objekt
            IEnumerable<TodoItem> filtered = _allItems;

            // Filtrera pa status
            if (filter == "Active")
            {
                filtered = filtered.Where(t => !t.IsCompleted);
            }
            else if (filter == "Completed")
            {
                filtered = filtered.Where(t => t.IsCompleted);
            }

            // Filtrera pa soktext (soker i titel och beskrivning)
            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered.Where(t =>
                    t.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    t.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            // Satt resultatet som ListView:s datakalla
            TodoListView.ItemsSource = filtered.ToList();
        }

        // Uppdaterar statusfaltet med antal uppgifter
        private void UpdateStatusBar()
        {
            int total = _allItems.Count;
            int completed = _allItems.Count(t => t.IsCompleted);
            StatusText.Text = $"Total: {total}  |  Completed: {completed}  |  Active: {total - completed}";
        }

        // ===== EVENT HANDLERS =====

        // Knapp: Lagg till ny uppgift
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Oppna dialogen i "lagg till"-lage
            AddEditDialog dialog = new AddEditDialog();
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                // Anvandaren klickade Save - lagg till i databasen
                TodoItem newItem = dialog.TodoItem;
                newItem.CreatedAt = DateTime.Now;
                Database.Add(newItem);
                LoadItems(); // Ladda om listan
            }
        }

        // Kontextmeny: Redigera vald uppgift
        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (TodoListView.SelectedItem is TodoItem selected)
            {
                // Oppna dialogen i "redigera"-lage med befintlig data
                AddEditDialog dialog = new AddEditDialog(selected);
                dialog.Owner = this;

                if (dialog.ShowDialog() == true)
                {
                    // Anvandaren klickade Save - uppdatera i databasen
                    Database.Update(dialog.TodoItem);
                    LoadItems();
                }
            }
        }

        // Kontextmeny: Ta bort vald uppgift
        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (TodoListView.SelectedItem is TodoItem selected)
            {
                // Visa bekraftelsedialog innan borttagning
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to delete \"{selected.Title}\"?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Database.Delete(selected.Id);
                    LoadItems();
                }
            }
        }

        // Checkbox: Toggla avklarad-status
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is TodoItem item)
            {
                Database.ToggleComplete(item.Id);
                LoadItems();
            }
        }

        // Filter-ComboBox: filtret andrades
        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        // Sokfalt: texten andrades
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        // Kolumnrubriker: klickbar (for framtida sortering)
        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            // Kan utvidgas med sorteringslogik
        }
    }
}
