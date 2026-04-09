using System;
using System.Windows;
using System.Windows.Controls;

namespace StudentGrades
{
    public partial class AddGradeDialog : Window
    {
        public string SubjectResult { get; private set; } = string.Empty;
        public double ScoreResult { get; private set; }
        public DateTime DateResult { get; private set; } = DateTime.Today;

        public AddGradeDialog(string studentName)
        {
            InitializeComponent();

            TxtStudentName.Text = $"Note fuer: {studentName}";
            CmbSubject.SelectedIndex = 0;
            CmbScore.SelectedIndex = 0;
            DpDate.SelectedDate = DateTime.Today;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (CmbSubject.SelectedItem == null || CmbScore.SelectedItem == null || DpDate.SelectedDate == null)
            {
                MessageBox.Show("Bitte alle Felder ausfuellen.", "Fehler",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SubjectResult = ((ComboBoxItem)CmbSubject.SelectedItem).Content.ToString()!;
            // Extract score number from "1 - Sehr gut" format
            ScoreResult = CmbScore.SelectedIndex + 1;
            DateResult = DpDate.SelectedDate.Value;

            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
