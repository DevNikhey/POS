using System;
using System.Windows;

namespace ContactBook
{
    public partial class ContactDialog : Window
    {
        public Contact ContactResult { get; private set; }

        public ContactDialog()
        {
            InitializeComponent();
            Title = "Add Contact";
            ContactResult = new Contact();
            LoadCategories();
            CmbCategory.SelectedIndex = 0;
        }

        public ContactDialog(Contact existing)
        {
            InitializeComponent();
            Title = "Edit Contact";
            ContactResult = new Contact
            {
                Id = existing.Id,
                FirstName = existing.FirstName,
                LastName = existing.LastName,
                Email = existing.Email,
                Phone = existing.Phone,
                Category = existing.Category,
                Address = existing.Address,
                Notes = existing.Notes
            };

            LoadCategories();

            TxtFirstName.Text = existing.FirstName;
            TxtLastName.Text = existing.LastName;
            TxtEmail.Text = existing.Email;
            TxtPhone.Text = existing.Phone;
            CmbCategory.SelectedItem = existing.Category;
            TxtAddress.Text = existing.Address;
            TxtNotes.Text = existing.Notes;
        }

        private void LoadCategories()
        {
            foreach (Category cat in Enum.GetValues(typeof(Category)))
            {
                CmbCategory.Items.Add(cat);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string firstName = TxtFirstName.Text.Trim();
            string lastName = TxtLastName.Text.Trim();

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                MessageBox.Show("First name and last name are required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ContactResult.FirstName = firstName;
            ContactResult.LastName = lastName;
            ContactResult.Email = TxtEmail.Text.Trim();
            ContactResult.Phone = TxtPhone.Text.Trim();
            ContactResult.Category = (Category)CmbCategory.SelectedItem!;
            ContactResult.Address = TxtAddress.Text.Trim();
            ContactResult.Notes = TxtNotes.Text.Trim();

            DialogResult = true;
        }
    }
}
