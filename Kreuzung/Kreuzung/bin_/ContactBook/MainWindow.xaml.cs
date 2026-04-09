using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ContactBook
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Database.InitializeDb();
            LoadCategoryFilter();
            RefreshContacts();
        }

        private void LoadCategoryFilter()
        {
            CmbCategory.Items.Add("All");
            foreach (Category cat in Enum.GetValues(typeof(Category)))
            {
                CmbCategory.Items.Add(cat);
            }
            CmbCategory.SelectedIndex = 0;
        }

        private void RefreshContacts()
        {
            string searchText = TxtSearch.Text.Trim();
            object? selectedCategory = CmbCategory.SelectedItem;

            List<Contact> contacts;

            if (!string.IsNullOrEmpty(searchText))
            {
                contacts = Database.Search(searchText);

                // Also filter by category if one is selected
                if (selectedCategory is Category cat)
                {
                    contacts = contacts.FindAll(c => c.Category == cat);
                }
            }
            else if (selectedCategory is Category category)
            {
                contacts = Database.GetByCategory(category);
            }
            else
            {
                contacts = Database.GetAll();
            }

            LstContacts.ItemsSource = contacts;
            TxtStatus.Text = "Contacts: " + contacts.Count;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            ContactDialog dialog = new ContactDialog();
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                Database.Add(dialog.ContactResult);
                RefreshContacts();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            Contact? selected = LstContacts.SelectedItem as Contact;
            if (selected == null)
            {
                MessageBox.Show("Please select a contact to edit.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ContactDialog dialog = new ContactDialog(selected);
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                Database.Update(dialog.ContactResult);
                RefreshContacts();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            Contact? selected = LstContacts.SelectedItem as Contact;
            if (selected == null)
            {
                MessageBox.Show("Please select a contact to delete.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Delete contact \"" + selected.FullName + "\"?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Database.Delete(selected.Id);
                ClearDetails();
                RefreshContacts();
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshContacts();
        }

        private void CmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                RefreshContacts();
            }
        }

        private void LstContacts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Contact? selected = LstContacts.SelectedItem as Contact;
            if (selected != null)
            {
                ShowDetails(selected);
            }
            else
            {
                ClearDetails();
            }
        }

        private void ShowDetails(Contact contact)
        {
            TxtDetailFirstName.Text = contact.FirstName;
            TxtDetailLastName.Text = contact.LastName;
            TxtDetailEmail.Text = contact.Email;
            TxtDetailPhone.Text = contact.Phone;
            TxtDetailCategory.Text = contact.Category.ToString();
            TxtDetailAddress.Text = contact.Address;
            TxtDetailNotes.Text = contact.Notes;
        }

        private void ClearDetails()
        {
            TxtDetailFirstName.Text = string.Empty;
            TxtDetailLastName.Text = string.Empty;
            TxtDetailEmail.Text = string.Empty;
            TxtDetailPhone.Text = string.Empty;
            TxtDetailCategory.Text = string.Empty;
            TxtDetailAddress.Text = string.Empty;
            TxtDetailNotes.Text = string.Empty;
        }
    }
}
