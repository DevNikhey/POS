using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;

namespace QuizApp
{
    /// <summary>
    /// Code-behind for MainWindow.
    /// Demonstrates: event handlers, DispatcherTimer for delayed feedback,
    /// OpenFileDialog, screen toggling via Visibility.
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<QuizQuestion> _allQuestions;
        private readonly QuizModel _model;
        private readonly DispatcherTimer _feedbackTimer;

        public MainWindow()
        {
            InitializeComponent();

            // Parse embedded CSV on startup
            _allQuestions = CsvData.Parse();
            _model = new QuizModel();

            // Timer for brief color feedback (500 ms)
            _feedbackTimer = new DispatcherTimer();
            _feedbackTimer.Interval = TimeSpan.FromMilliseconds(500);
            _feedbackTimer.Tick += FeedbackTimer_Tick;

            PopulateFilters();
        }

        // ── Filter setup ────────────────────────────────────────────

        private void PopulateFilters()
        {
            // Categories from data using LINQ Distinct
            List<string> categories = _allQuestions
                .Select(q => q.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
            categories.Insert(0, "All");

            CategoryCombo.ItemsSource = categories;
            CategoryCombo.SelectedIndex = 0;

            DifficultyCombo.ItemsSource = new List<string> { "All", "Easy", "Medium", "Hard" };
            DifficultyCombo.SelectedIndex = 0;
        }

        // ── Screen toggling ─────────────────────────────────────────

        private void ShowScreen(string screen)
        {
            StartScreen.Visibility = screen == "start" ? Visibility.Visible : Visibility.Collapsed;
            QuizScreen.Visibility = screen == "quiz" ? Visibility.Visible : Visibility.Collapsed;
            ResultScreen.Visibility = screen == "result" ? Visibility.Visible : Visibility.Collapsed;
        }

        // ── Start Quiz ──────────────────────────────────────────────

        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            string? category = CategoryCombo.SelectedItem as string;
            string? difficulty = DifficultyCombo.SelectedItem as string;

            _model.Start(_allQuestions, category, difficulty);

            if (_model.Questions.Count == 0)
            {
                MessageBox.Show("No questions match the selected filters.",
                    "No Questions", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ShowScreen("quiz");
            UpdateQuizUI();
        }

        // ── Answer handling ─────────────────────────────────────────

        private void Answer_Click(object sender, RoutedEventArgs e)
        {
            if (_feedbackTimer.IsEnabled)
                return; // ignore clicks during feedback

            Button btn = (Button)sender;
            char choice = ((string)btn.Tag)[0];

            bool correct = _model.Answer(choice);

            // Show colored feedback
            ShowFeedback(correct, choice);

            // Disable buttons during feedback
            SetAnswerButtonsEnabled(false);

            // Start timer to advance
            _feedbackTimer.Start();
        }

        private void ShowFeedback(bool correct, char chosen)
        {
            // Highlight chosen button
            Button chosenBtn = GetButton(chosen);

            if (correct)
            {
                chosenBtn.Background = new SolidColorBrush(Color.FromRgb(0xA6, 0xE3, 0xA1)); // green
                chosenBtn.Foreground = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x2E));
                FeedbackLabel.Text = "Correct!";
                FeedbackLabel.Foreground = new SolidColorBrush(Color.FromRgb(0xA6, 0xE3, 0xA1));
            }
            else
            {
                chosenBtn.Background = new SolidColorBrush(Color.FromRgb(0xF3, 0x8B, 0xA8)); // red
                chosenBtn.Foreground = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x2E));

                // Also highlight the correct answer in green
                if (_model.CurrentQuestion != null)
                {
                    Button correctBtn = GetButton(_model.CurrentQuestion.CorrectAnswer);
                    correctBtn.Background = new SolidColorBrush(Color.FromRgb(0xA6, 0xE3, 0xA1));
                    correctBtn.Foreground = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x2E));
                }

                FeedbackLabel.Text = "Wrong!";
                FeedbackLabel.Foreground = new SolidColorBrush(Color.FromRgb(0xF3, 0x8B, 0xA8));
            }

            FeedbackLabel.Visibility = Visibility.Visible;
        }

        private void FeedbackTimer_Tick(object? sender, EventArgs e)
        {
            _feedbackTimer.Stop();
            FeedbackLabel.Visibility = Visibility.Collapsed;

            // Advance to next question
            _model.Advance();

            if (_model.IsFinished)
            {
                ShowResults();
            }
            else
            {
                UpdateQuizUI();
            }

            SetAnswerButtonsEnabled(true);
        }

        // ── UI updates ──────────────────────────────────────────────

        private void UpdateQuizUI()
        {
            StatusLabel.Text = _model.StatusText;
            ProgressBar.Value = _model.ProgressValue;
            ProgressLabel.Text = _model.ProgressText;
            QuestionLabel.Text = _model.QuestionText;

            BtnA.Content = _model.OptionAText;
            BtnB.Content = _model.OptionBText;
            BtnC.Content = _model.OptionCText;
            BtnD.Content = _model.OptionDText;

            // Reset button colors
            SolidColorBrush defaultBg = new SolidColorBrush(Color.FromRgb(0x31, 0x32, 0x44));
            SolidColorBrush defaultFg = new SolidColorBrush(Color.FromRgb(0xCD, 0xD6, 0xF4));
            foreach (Button btn in new[] { BtnA, BtnB, BtnC, BtnD })
            {
                btn.Background = defaultBg;
                btn.Foreground = defaultFg;
            }
        }

        private void ShowResults()
        {
            ShowScreen("result");

            ScoreLabel.Text = $"{_model.Score} / {_model.TotalQuestions}";

            double pct = _model.Percentage;
            PercentLabel.Text = $"{pct}%";

            // Color based on score
            if (pct >= 80)
                PercentLabel.Foreground = new SolidColorBrush(Color.FromRgb(0xA6, 0xE3, 0xA1)); // green
            else if (pct >= 50)
                PercentLabel.Foreground = new SolidColorBrush(Color.FromRgb(0xF9, 0xE2, 0xAF)); // yellow
            else
                PercentLabel.Foreground = new SolidColorBrush(Color.FromRgb(0xF3, 0x8B, 0xA8)); // red
        }

        // ── Try Again ───────────────────────────────────────────────

        private void TryAgain_Click(object sender, RoutedEventArgs e)
        {
            ShowScreen("start");
        }

        // ── Load External CSV ───────────────────────────────────────

        private void LoadCsv_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            dialog.Title = "Load Quiz Questions CSV";

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string csvText = File.ReadAllText(dialog.FileName);
                    List<QuizQuestion> loaded = CsvData.Parse(csvText);

                    if (loaded.Count == 0)
                    {
                        MessageBox.Show("No valid questions found in the file.\n\n" +
                            "Expected format (semicolon-separated):\n" +
                            "question;optionA;optionB;optionC;optionD;correct;category;difficulty",
                            "Parse Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _allQuestions = loaded;
                    PopulateFilters();

                    MessageBox.Show($"Loaded {loaded.Count} questions successfully!",
                        "CSV Loaded", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading file:\n{ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ── Helpers ─────────────────────────────────────────────────

        private Button GetButton(char letter)
        {
            switch (char.ToUpper(letter))
            {
                case 'A': return BtnA;
                case 'B': return BtnB;
                case 'C': return BtnC;
                case 'D': return BtnD;
                default: return BtnA;
            }
        }

        private void SetAnswerButtonsEnabled(bool enabled)
        {
            BtnA.IsEnabled = enabled;
            BtnB.IsEnabled = enabled;
            BtnC.IsEnabled = enabled;
            BtnD.IsEnabled = enabled;
        }
    }
}
