using System;
using System.Windows;
using System.Windows.Controls;

namespace StudentGrades
{
    public partial class AddStudentDialog : Window
    {
        public string FirstNameResult { get; private set; } = string.Empty;
        public string LastNameResult { get; private set; } = string.Empty;
        public string ClassResult { get; private set; } = string.Empty;

        public AddStudentDialog()
        {
            InitializeComponent();
            CmbClass.SelectedIndex = 0;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtFirstName.Text) ||
                string.IsNullOrWhiteSpace(TxtLastName.Text) ||
                string.IsNullOrWhiteSpace(CmbClass.Text))
            {
                MessageBox.Show("Bitte alle Felder ausfuellen.", "Fehler",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            FirstNameResult = TxtFirstName.Text.Trim();
            LastNameResult = TxtLastName.Text.Trim();
            ClassResult = CmbClass.Text.Trim();

            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
