using System;
using System.Windows;
using System.Windows.Controls;

namespace TodoApp
{
    // Code-behind for dialogen som anvands bade for att lagga till och redigera uppgifter.
    // Visar hur man skapar en modal dialog i WPF med DialogResult.
    public partial class AddEditDialog : Window
    {
        // Den uppgift som skapas eller redigeras
        public TodoItem TodoItem { get; private set; }

        // Konstruktor for att LAGGA TILL en ny uppgift
        public AddEditDialog()
        {
            InitializeComponent();
            TodoItem = new TodoItem();
            Title = "Add Task"; // Satt fonstertiteln
        }

        // Konstruktor for att REDIGERA en befintlig uppgift
        public AddEditDialog(TodoItem existingItem)
        {
            InitializeComponent();

            // Kopiera befintlig data till en ny instans (sa vi inte andrar originalet direkt)
            TodoItem = new TodoItem
            {
                Id = existingItem.Id,
                Title = existingItem.Title,
                Description = existingItem.Description,
                Priority = existingItem.Priority,
                DueDate = existingItem.DueDate,
                IsCompleted = existingItem.IsCompleted,
                CreatedAt = existingItem.CreatedAt
            };

            Title = "Edit Task"; // Satt fonstertiteln

            // Fyll i formularet med befintliga varden
            TitleTextBox.Text = TodoItem.Title;
            DescriptionTextBox.Text = TodoItem.Description;
            DueDatePicker.SelectedDate = TodoItem.DueDate;

            // Valj ratt prioritet i ComboBox:en
            switch (TodoItem.Priority)
            {
                case Priority.Low:
                    PriorityComboBox.SelectedIndex = 0;
                    break;
                case Priority.Medium:
                    PriorityComboBox.SelectedIndex = 1;
                    break;
                case Priority.High:
                    PriorityComboBox.SelectedIndex = 2;
                    break;
            }
        }

        // Save-knappen klickad
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validering: titel ar obligatorisk
            string title = TitleTextBox.Text.Trim();
            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Title is required!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TitleTextBox.Focus();
                return;
            }

            // Overfor varden fran formularet till TodoItem-objektet
            TodoItem.Title = title;
            TodoItem.Description = DescriptionTextBox.Text.Trim();
            TodoItem.DueDate = DueDatePicker.SelectedDate;

            // Las av vald prioritet fran ComboBox
            if (PriorityComboBox.SelectedItem is ComboBoxItem selectedPriority)
            {
                string priorityText = selectedPriority.Content?.ToString() ?? "Medium";
                switch (priorityText)
                {
                    case "Low":
                        TodoItem.Priority = Priority.Low;
                        break;
                    case "Medium":
                        TodoItem.Priority = Priority.Medium;
                        break;
                    case "High":
                        TodoItem.Priority = Priority.High;
                        break;
                }
            }

            // Satt DialogResult till true (signalerar att anvandaren klickade Save)
            DialogResult = true;
            Close();
        }

        // Cancel-knappen klickad
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // DialogResult forblir false/null (signalerar avbryt)
            DialogResult = false;
            Close();
        }
    }
}
