using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Wordle
{
    public enum LetterState
    {
        Empty,
        Correct,
        WrongPlace,
        NotInWord
    }

    public class LetterCell : INotifyPropertyChanged
    {
        private char _letter;
        private LetterState _state;

        public event PropertyChangedEventHandler PropertyChanged;

        public char Letter
        {
            get => _letter;
            set
            {
                if (_letter != value)
                {
                    _letter = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Display));
                }
            }
        }

        public string Display
        {
            get => _letter == '\0' ? "" : _letter.ToString().ToUpper();
        }

        public LetterState State
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Color));
                }
            }
        }

        public string Color
        {
            get
            {
                switch (_state)
                {
                    case LetterState.Correct: return "#6AAA64";
                    case LetterState.WrongPlace: return "#C9B458";
                    case LetterState.NotInWord: return "#787C7E";
                    default: return "#3A3A3C";
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class WordleModel : INotifyPropertyChanged
    {
        public const int WordLength = 5;
        public const int MaxGuesses = 6;

        private static readonly string[] WordList = new string[]
        {
            "APFEL", "BIRNE", "TIGER", "STERN", "BLUME",
            "KNOPF", "TRAUM", "FLUSS", "STURM", "KREIS",
            "STEIN", "NACHT", "LICHT", "WOLKE", "PFERD",
            "PUNKT", "TISCH", "STUHL", "GABEL"
        };

        private static readonly Random _random = new Random();

        private int _currentRow;
        private int _currentCol;
        private string _targetWord;
        private bool _gameOver;
        private bool _won;
        private string _statusText;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<LetterCell> Grid { get; private set; }

        public int CurrentRow
        {
            get => _currentRow;
            private set
            {
                if (_currentRow != value)
                {
                    _currentRow = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TargetWord
        {
            get => _targetWord;
            private set
            {
                if (_targetWord != value)
                {
                    _targetWord = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool GameOver
        {
            get => _gameOver;
            private set
            {
                if (_gameOver != value)
                {
                    _gameOver = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Won
        {
            get => _won;
            private set
            {
                if (_won != value)
                {
                    _won = value;
                    OnPropertyChanged();
                }
            }
        }

        public string StatusText
        {
            get => _statusText;
            private set
            {
                if (_statusText != value)
                {
                    _statusText = value;
                    OnPropertyChanged();
                }
            }
        }

        public WordleModel()
        {
            Grid = new ObservableCollection<LetterCell>();
            NewGame();
        }

        public void NewGame()
        {
            Grid.Clear();
            for (int i = 0; i < MaxGuesses * WordLength; i++)
            {
                Grid.Add(new LetterCell());
            }
            CurrentRow = 0;
            _currentCol = 0;
            TargetWord = WordList[_random.Next(WordList.Length)];
            GameOver = false;
            Won = false;
            StatusText = "Rate das Wort!";
        }

        public void TypeLetter(char letter)
        {
            if (GameOver) return;
            if (_currentCol >= WordLength) return;

            char upper = char.ToUpper(letter);
            int index = CurrentRow * WordLength + _currentCol;
            Grid[index].Letter = upper;
            _currentCol++;
        }

        public void Backspace()
        {
            if (GameOver) return;
            if (_currentCol <= 0) return;

            _currentCol--;
            int index = CurrentRow * WordLength + _currentCol;
            Grid[index].Letter = '\0';
        }

        public void SubmitGuess()
        {
            if (GameOver) return;
            if (_currentCol < WordLength)
            {
                StatusText = "Nicht genug Buchstaben!";
                return;
            }

            // Build the guess string
            char[] guess = new char[WordLength];
            for (int i = 0; i < WordLength; i++)
            {
                guess[i] = Grid[CurrentRow * WordLength + i].Letter;
            }

            char[] target = TargetWord.ToCharArray();
            bool[] used = new bool[WordLength];
            LetterState[] states = new LetterState[WordLength];

            // First pass: mark correct (green)
            for (int i = 0; i < WordLength; i++)
            {
                if (guess[i] == target[i])
                {
                    states[i] = LetterState.Correct;
                    used[i] = true;
                }
            }

            // Second pass: mark wrong place (yellow) or not in word (gray)
            for (int i = 0; i < WordLength; i++)
            {
                if (states[i] == LetterState.Correct) continue;

                bool found = false;
                for (int j = 0; j < WordLength; j++)
                {
                    if (!used[j] && guess[i] == target[j])
                    {
                        states[i] = LetterState.WrongPlace;
                        used[j] = true;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    states[i] = LetterState.NotInWord;
                }
            }

            // Apply states to grid cells
            for (int i = 0; i < WordLength; i++)
            {
                Grid[CurrentRow * WordLength + i].State = states[i];
            }

            // Check for win
            bool allCorrect = true;
            for (int i = 0; i < WordLength; i++)
            {
                if (states[i] != LetterState.Correct)
                {
                    allCorrect = false;
                    break;
                }
            }

            if (allCorrect)
            {
                Won = true;
                GameOver = true;
                StatusText = "Gewonnen!";
                return;
            }

            CurrentRow++;
            _currentCol = 0;

            if (CurrentRow >= MaxGuesses)
            {
                GameOver = true;
                StatusText = "Verloren! Das Wort war: " + TargetWord;
            }
            else
            {
                StatusText = "Rate das Wort!";
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
