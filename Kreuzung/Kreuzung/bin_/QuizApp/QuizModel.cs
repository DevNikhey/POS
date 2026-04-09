using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace QuizApp
{
    /// <summary>
    /// State-machine model for the quiz.
    /// Demonstrates: INotifyPropertyChanged, LINQ shuffle, computed properties.
    /// States: NotStarted -> InProgress -> Finished
    /// </summary>
    public class QuizModel : INotifyPropertyChanged
    {
        // ── INotifyPropertyChanged ──────────────────────────────────

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // ── Backing fields ──────────────────────────────────────────
        private List<QuizQuestion> _questions = new List<QuizQuestion>();
        private int _currentIndex;
        private int _score;
        private bool _isFinished;
        private char _lastAnswer;
        private bool _lastWasCorrect;

        // ── Properties ──────────────────────────────────────────────

        /// <summary>All questions for the current session (shuffled).</summary>
        public List<QuizQuestion> Questions
        {
            get => _questions;
            private set { _questions = value; OnPropertyChanged(); }
        }

        public int CurrentIndex
        {
            get => _currentIndex;
            private set
            {
                _currentIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentQuestion));
                OnPropertyChanged(nameof(QuestionText));
                OnPropertyChanged(nameof(OptionAText));
                OnPropertyChanged(nameof(OptionBText));
                OnPropertyChanged(nameof(OptionCText));
                OnPropertyChanged(nameof(OptionDText));
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(ProgressValue));
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        public int Score
        {
            get => _score;
            private set { _score = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); }
        }

        public bool IsFinished
        {
            get => _isFinished;
            private set { _isFinished = value; OnPropertyChanged(); }
        }

        public bool LastWasCorrect => _lastWasCorrect;
        public char LastAnswer => _lastAnswer;

        // ── Computed properties for data binding ────────────────────

        public QuizQuestion? CurrentQuestion =>
            (_currentIndex >= 0 && _currentIndex < _questions.Count)
                ? _questions[_currentIndex]
                : null;

        public string QuestionText => CurrentQuestion?.Question ?? string.Empty;
        public string OptionAText => CurrentQuestion != null ? "A:  " + CurrentQuestion.OptionA : string.Empty;
        public string OptionBText => CurrentQuestion != null ? "B:  " + CurrentQuestion.OptionB : string.Empty;
        public string OptionCText => CurrentQuestion != null ? "C:  " + CurrentQuestion.OptionC : string.Empty;
        public string OptionDText => CurrentQuestion != null ? "D:  " + CurrentQuestion.OptionD : string.Empty;

        public string StatusText =>
            IsFinished
                ? "Quiz Complete!"
                : $"Question {CurrentIndex + 1}/{Questions.Count}  |  Score: {Score}";

        public double ProgressValue =>
            Questions.Count > 0 ? (double)CurrentIndex / Questions.Count * 100.0 : 0;

        public string ProgressText =>
            Questions.Count > 0
                ? $"{CurrentIndex}/{Questions.Count}"
                : "0/0";

        public int TotalQuestions => Questions.Count;

        public double Percentage =>
            Questions.Count > 0 ? Math.Round((double)Score / Questions.Count * 100.0, 1) : 0;

        public string ResultText =>
            $"You scored {Score} out of {Questions.Count} ({Percentage}%)";

        // ── Methods ─────────────────────────────────────────────────

        /// <summary>
        /// Initialise (or restart) the quiz with a filtered, shuffled list.
        /// Demonstrates: LINQ Where, OrderBy with Random (shuffle), ToList.
        /// </summary>
        public void Start(List<QuizQuestion> allQuestions, string? categoryFilter, string? difficultyFilter)
        {
            Random rng = new Random();

            IEnumerable<QuizQuestion> filtered = allQuestions;

            if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "All")
            {
                filtered = filtered.Where(q => q.Category == categoryFilter);
            }

            if (!string.IsNullOrEmpty(difficultyFilter) && difficultyFilter != "All")
            {
                filtered = filtered.Where(q => q.Difficulty == difficultyFilter);
            }

            // Shuffle using LINQ OrderBy with random key
            Questions = filtered.OrderBy(_ => rng.Next()).ToList();
            CurrentIndex = 0;
            Score = 0;
            IsFinished = false;
        }

        /// <summary>
        /// Process an answer choice. Returns true if correct.
        /// Advances to the next question or finishes the quiz.
        /// </summary>
        public bool Answer(char choice)
        {
            if (IsFinished || CurrentQuestion == null)
                return false;

            choice = char.ToUpper(choice);
            _lastAnswer = choice;
            _lastWasCorrect = (choice == CurrentQuestion.CorrectAnswer);

            if (_lastWasCorrect)
            {
                Score++;
            }

            return _lastWasCorrect;
        }

        /// <summary>
        /// Move to the next question after feedback has been shown.
        /// </summary>
        public void Advance()
        {
            if (CurrentIndex + 1 >= Questions.Count)
            {
                IsFinished = true;
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(ResultText));
                OnPropertyChanged(nameof(Percentage));
            }
            else
            {
                CurrentIndex++;
            }
        }
    }
}
