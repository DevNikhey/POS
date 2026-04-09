using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Microsoft.Win32;

namespace EmployeeDirectory
{
    public partial class MainWindow : Window
    {
        private List<Employee> _allEmployees = new List<Employee>();

        public MainWindow()
        {
            InitializeComponent();
            LoadEmbeddedData();
        }

        private void LoadEmbeddedData()
        {
            _allEmployees = CsvData.Parse();
            InitializeDepartmentFilter();
            ApplyFilters();
            UpdateStatistics();
            StatusText.Text = $"{_allEmployees.Count} employees loaded from embedded data.";
        }

        private void InitializeDepartmentFilter()
        {
            DepartmentFilter.Items.Clear();
            DepartmentFilter.Items.Add("All");

            List<string> departments = _allEmployees
                .Select(e => e.Department)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            foreach (string dept in departments)
            {
                DepartmentFilter.Items.Add(dept);
            }

            DepartmentFilter.SelectedIndex = 0;
        }

        private void ApplyFilters()
        {
            string searchText = SearchBox.Text?.Trim().ToLowerInvariant() ?? string.Empty;
            string selectedDept = DepartmentFilter.SelectedItem as string ?? "All";

            List<Employee> filtered = _allEmployees.Where(e =>
            {
                // Department filter
                if (selectedDept != "All" && e.Department != selectedDept)
                    return false;

                // Search filter
                if (!string.IsNullOrEmpty(searchText))
                {
                    return e.FullName.ToLowerInvariant().Contains(searchText)
                        || e.Email.ToLowerInvariant().Contains(searchText)
                        || e.Department.ToLowerInvariant().Contains(searchText)
                        || e.Position.ToLowerInvariant().Contains(searchText);
                }

                return true;
            }).ToList();

            BuildTreeView(filtered);
            UpdateStatistics(filtered);
        }

        private void BuildTreeView(List<Employee> employees)
        {
            EmployeeTree.Items.Clear();

            var groups = employees
                .GroupBy(e => e.Department)
                .OrderBy(g => g.Key);

            foreach (var group in groups)
            {
                TreeViewItem deptNode = new TreeViewItem
                {
                    Header = $"{group.Key} ({group.Count()})",
                    IsExpanded = true,
                    FontWeight = FontWeights.SemiBold
                };

                foreach (Employee emp in group.OrderBy(e => e.LastName).ThenBy(e => e.FirstName))
                {
                    TreeViewItem empNode = new TreeViewItem
                    {
                        Header = $"{emp.FullName} - {emp.Position}",
                        Tag = emp,
                        FontWeight = FontWeights.Normal
                    };
                    deptNode.Items.Add(empNode);
                }

                EmployeeTree.Items.Add(deptNode);
            }
        }

        private void UpdateStatistics(List<Employee>? filtered = null)
        {
            List<Employee> source = filtered ?? _allEmployees;

            if (source.Count == 0)
            {
                TotalEmployeesText.Text = "0";
                AvgSalaryText.Text = "\u20ac0";
                LargestDeptText.Text = "-";
                LongestTenureText.Text = "-";
                return;
            }

            TotalEmployeesText.Text = source.Count.ToString();

            decimal avgSalary = source.Average(e => e.Salary);
            AvgSalaryText.Text = $"\u20ac{avgSalary:N0}";

            var largestDept = source
                .GroupBy(e => e.Department)
                .OrderByDescending(g => g.Count())
                .First();
            LargestDeptText.Text = $"{largestDept.Key} ({largestDept.Count()})";

            Employee longestEmployee = source.OrderBy(e => e.HireDate).First();
            LongestTenureText.Text = $"{longestEmployee.FullName} ({longestEmployee.Tenure} years)";
        }

        private void ShowEmployeeDetail(Employee emp)
        {
            DetailPanel.Visibility = Visibility.Visible;
            DetailName.Text = emp.FullName;
            DetailPosition.Text = emp.Position;
            DetailId.Text = emp.Id.ToString();
            DetailEmail.Text = emp.Email;
            DetailDepartment.Text = emp.Department;
            DetailPositionFull.Text = emp.Position;
            DetailSalary.Text = emp.SalaryText;
            DetailHireDate.Text = emp.HireDate.ToString("dd.MM.yyyy");
            DetailTenure.Text = $"{emp.Tenure} years";
        }

        // --- Event Handlers ---

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void DepartmentFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DepartmentFilter.SelectedItem != null)
            {
                ApplyFilters();
            }
        }

        private void EmployeeTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem item && item.Tag is Employee emp)
            {
                ShowEmployeeDetail(emp);
            }
        }

        private void LoadCsv_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                Title = "Load Employee CSV"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    _allEmployees = CsvData.LoadFromFile(dlg.FileName);
                    InitializeDepartmentFilter();
                    ApplyFilters();
                    UpdateStatistics();
                    StatusText.Text = $"{_allEmployees.Count} employees loaded from {System.IO.Path.GetFileName(dlg.FileName)}.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading CSV:\n{ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = "Error loading CSV file.";
                }
            }
        }

        private void ExportXml_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml",
                Title = "Export Employees as XML",
                FileName = "employees.xml"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    XDocument doc = new XDocument(
                        new XDeclaration("1.0", "utf-8", "yes"),
                        new XElement("Employees",
                            _allEmployees.Select(emp =>
                                new XElement("Employee",
                                    new XAttribute("Id", emp.Id),
                                    new XElement("FirstName", emp.FirstName),
                                    new XElement("LastName", emp.LastName),
                                    new XElement("Email", emp.Email),
                                    new XElement("Department", emp.Department),
                                    new XElement("Position", emp.Position),
                                    new XElement("Salary", emp.Salary),
                                    new XElement("HireDate", emp.HireDate.ToString("yyyy-MM-dd"))
                                )
                            )
                        )
                    );

                    doc.Save(dlg.FileName);
                    StatusText.Text = $"Exported {_allEmployees.Count} employees to {System.IO.Path.GetFileName(dlg.FileName)}.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting XML:\n{ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = "Error exporting XML.";
                }
            }
        }
    }
}
