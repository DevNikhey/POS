using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace WeatherViewer
{
    /// <summary>
    /// Main window: loads weather data, applies filters, displays statistics.
    /// Demonstrates LINQ filtering and aggregation.
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<WeatherRecord> _allRecords = new List<WeatherRecord>();
        private bool _isInitializing = true;

        public MainWindow()
        {
            InitializeComponent();
            LoadEmbeddedData();
            SetupConditionFilter();
            _isInitializing = false;
            ApplyFilters();
        }

        /// <summary>
        /// Parses the embedded CSV data on startup.
        /// </summary>
        private void LoadEmbeddedData()
        {
            _allRecords = CsvData.Parse();

            if (_allRecords.Count > 0)
            {
                dpFrom.SelectedDate = _allRecords.Min(r => r.Date);
                dpTo.SelectedDate = _allRecords.Max(r => r.Date);
            }
        }

        /// <summary>
        /// Populates the condition ComboBox with "All" plus each unique condition.
        /// </summary>
        private void SetupConditionFilter()
        {
            List<string> conditions = new List<string> { "All" };
            conditions.AddRange(
                _allRecords
                    .Select(r => r.Condition)
                    .Distinct()
                    .OrderBy(c => c)
            );
            cbCondition.ItemsSource = conditions;
            cbCondition.SelectedIndex = 0;
        }

        /// <summary>
        /// Applies date range and condition filters using LINQ, then updates the grid and statistics.
        /// </summary>
        private void ApplyFilters()
        {
            IEnumerable<WeatherRecord> filtered = _allRecords;

            // Filter by date range
            if (dpFrom.SelectedDate.HasValue)
            {
                DateTime from = dpFrom.SelectedDate.Value.Date;
                filtered = filtered.Where(r => r.Date >= from);
            }
            if (dpTo.SelectedDate.HasValue)
            {
                DateTime to = dpTo.SelectedDate.Value.Date;
                filtered = filtered.Where(r => r.Date <= to);
            }

            // Filter by condition
            string? selectedCondition = cbCondition.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedCondition) && selectedCondition != "All")
            {
                filtered = filtered.Where(r => r.Condition == selectedCondition);
            }

            List<WeatherRecord> result = filtered.ToList();

            // Update DataGrid
            dgWeather.ItemsSource = result;

            // Update statistics
            UpdateStatistics(result);

            // Update status bar
            txtRecordCount.Text = $"Records: {result.Count}";
            if (result.Count > 0)
            {
                DateTime minDate = result.Min(r => r.Date);
                DateTime maxDate = result.Max(r => r.Date);
                txtDateRange.Text = $"Date range: {minDate:yyyy-MM-dd} to {maxDate:yyyy-MM-dd}";
            }
            else
            {
                txtDateRange.Text = "Date range: --";
            }
        }

        /// <summary>
        /// Calculates and displays aggregate statistics using LINQ.
        /// </summary>
        private void UpdateStatistics(List<WeatherRecord> records)
        {
            if (records.Count == 0)
            {
                txtAvgTemp.Text = "--";
                txtMinTemp.Text = "--";
                txtMaxTemp.Text = "--";
                txtAvgHumidity.Text = "--";
                txtAvgWind.Text = "--";
                return;
            }

            double avgTemp = records.Average(r => r.Temperature);
            double minTemp = records.Min(r => r.Temperature);
            double maxTemp = records.Max(r => r.Temperature);
            double avgHumidity = records.Average(r => r.Humidity);
            double avgWind = records.Average(r => r.WindSpeed);

            txtAvgTemp.Text = avgTemp.ToString("F1");
            txtMinTemp.Text = minTemp.ToString("F1");
            txtMaxTemp.Text = maxTemp.ToString("F1");
            txtAvgHumidity.Text = avgHumidity.ToString("F0");
            txtAvgWind.Text = avgWind.ToString("F1");
        }

        /// <summary>
        /// Called when any filter control changes. Reapplies all filters.
        /// </summary>
        private void Filter_Changed(object sender, EventArgs e)
        {
            if (_isInitializing) return;
            ApplyFilters();
        }

        /// <summary>
        /// Opens an external CSV file via OpenFileDialog and loads it.
        /// </summary>
        private void BtnLoadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                Title = "Open Weather CSV File"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    _allRecords = CsvData.LoadFromFile(dlg.FileName);

                    if (_allRecords.Count > 0)
                    {
                        _isInitializing = true;
                        dpFrom.SelectedDate = _allRecords.Min(r => r.Date);
                        dpTo.SelectedDate = _allRecords.Max(r => r.Date);
                        SetupConditionFilter();
                        _isInitializing = false;
                    }

                    ApplyFilters();
                    Title = $"Weather Data Viewer - {System.IO.Path.GetFileName(dlg.FileName)}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading file:\n{ex.Message}",
                        "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
