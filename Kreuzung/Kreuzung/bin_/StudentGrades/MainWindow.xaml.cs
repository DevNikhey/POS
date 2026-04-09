using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StudentGrades
{
    public partial class MainWindow : Window
    {
        private List<StudentWithAvg> _allStudents = new List<StudentWithAvg>();

        public MainWindow()
        {
            InitializeComponent();

            Database.InitializeDb();
            Database.SeedSampleData();

            LoadStudents();
        }

        private void LoadStudents()
        {
            _allStudents = Database.GetStudentsWithAvg();

            // Update class filter
            var currentFilter = CmbClassFilter.SelectedItem as string;
            var classes = _allStudents.Select(s => s.Class).Distinct().OrderBy(c => c).ToList();
            classes.Insert(0, "Alle");
            CmbClassFilter.ItemsSource = classes;

            if (currentFilter != null && classes.Contains(currentFilter))
                CmbClassFilter.SelectedItem = currentFilter;
            else
                CmbClassFilter.SelectedIndex = 0;

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var filter = CmbClassFilter.SelectedItem as string;
            List<StudentWithAvg> filtered;

            if (string.IsNullOrEmpty(filter) || filter == "Alle")
                filtered = _allStudents;
            else
                filtered = _allStudents.Where(s => s.Class == filter).ToList();

            DgStudents.ItemsSource = filtered;

            // Update status bar
            TxtStudentCount.Text = $"Schueler: {filtered.Count}";

            if (filtered.Count > 0 && filtered.Any(s => s.GradeCount > 0))
            {
                var avg = filtered.Where(s => s.GradeCount > 0).Average(s => s.Average);
                TxtClassAverage.Text = $"Klassenschnitt: {avg:N2}";
            }
            else
            {
                TxtClassAverage.Text = "Klassenschnitt: -";
            }
        }

        private void CmbClassFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void DgStudents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgStudents.SelectedItem is StudentWithAvg student)
            {
                var grades = Database.GetGradesForStudent(student.Id);
                DgGrades.ItemsSource = grades;
                TxtSelectedStudent.Text = $"Noten: {student.FullName}";
            }
            else
            {
                DgGrades.ItemsSource = null;
                TxtSelectedStudent.Text = "Noten";
            }
        }

        private void BtnAddStudent_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddStudentDialog();
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                var student = new Student
                {
                    FirstName = dialog.FirstNameResult,
                    LastName = dialog.LastNameResult,
                    Class = dialog.ClassResult
                };

                Database.AddStudent(student);
                LoadStudents();
            }
        }

        private void BtnDeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            if (DgStudents.SelectedItem is StudentWithAvg student)
            {
                var result = MessageBox.Show(
                    $"Schueler '{student.FullName}' und alle Noten loeschen?",
                    "Bestaetigung", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    Database.DeleteStudent(student.Id);
                    LoadStudents();
                }
            }
            else
            {
                MessageBox.Show("Bitte zuerst einen Schueler auswaehlen.", "Hinweis",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnAddGrade_Click(object sender, RoutedEventArgs e)
        {
            if (DgStudents.SelectedItem is StudentWithAvg student)
            {
                var dialog = new AddGradeDialog(student.FullName);
                dialog.Owner = this;

                if (dialog.ShowDialog() == true)
                {
                    var grade = new Grade
                    {
                        StudentId = student.Id,
                        Subject = dialog.SubjectResult,
                        Score = dialog.ScoreResult,
                        Date = dialog.DateResult
                    };

                    Database.AddGrade(grade);
                    LoadStudents();

                    // Re-select the student
                    var updated = (DgStudents.ItemsSource as List<StudentWithAvg>)?
                        .FirstOrDefault(s => s.Id == student.Id);
                    if (updated != null)
                    {
                        DgStudents.SelectedItem = updated;
                    }
                }
            }
            else
            {
                MessageBox.Show("Bitte zuerst einen Schueler auswaehlen.", "Hinweis",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDeleteGrade_Click(object sender, RoutedEventArgs e)
        {
            if (DgGrades.SelectedItem is Grade grade)
            {
                var result = MessageBox.Show(
                    $"Note '{grade.Subject}: {grade.Score}' loeschen?",
                    "Bestaetigung", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    var selectedStudent = DgStudents.SelectedItem as StudentWithAvg;
                    Database.DeleteGrade(grade.Id);
                    LoadStudents();

                    // Re-select the student
                    if (selectedStudent != null)
                    {
                        var updated = (DgStudents.ItemsSource as List<StudentWithAvg>)?
                            .FirstOrDefault(s => s.Id == selectedStudent.Id);
                        if (updated != null)
                        {
                            DgStudents.SelectedItem = updated;
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Bitte zuerst eine Note auswaehlen.", "Hinweis",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
