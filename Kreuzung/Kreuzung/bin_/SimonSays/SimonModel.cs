using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimonSays
{
    public class SimonModel : INotifyPropertyChanged
    {
        public static readonly int ColorCount = 4;
        public static readonly int FlashMs = 500;

        public static readonly string[] ColorNames = { "Red", "Blue", "Green", "Yellow" };
        public static readonly string[] ColorCodes = { "#E74C3C", "#3498DB", "#2ECC71", "#F1C40F" };

        private readonly Random _random = new Random();

        private List<int> _sequence = new List<int>();
        public List<int> Sequence
        {
            get => _sequence;
            set { _sequence = value; OnPropertyChanged(); }
        }

        private int _level = 1;
        public int Level
        {
            get => _level;
            set { _level = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); }
        }

        private int _inputIndex;
        public int InputIndex
        {
            get => _inputIndex;
            set { _inputIndex = value; OnPropertyChanged(); }
        }

        private bool _gameOver;
        public bool GameOver
        {
            get => _gameOver;
            set { _gameOver = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); }
        }

        private bool _isShowingSequence;
        public bool IsShowingSequence
        {
            get => _isShowingSequence;
            set { _isShowingSequence = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); }
        }

        public string StatusText
        {
            get
            {
                if (GameOver)
                    return "Game Over! You reached Level " + Level;
                if (IsShowingSequence)
                    return "Watch the sequence...";
                return "Level " + Level + " - Your turn!";
            }
        }

        public bool RoundComplete => InputIndex >= Sequence.Count;

        public void StartNextRound()
        {
            int next = _random.Next(0, ColorCount);
            _sequence.Add(next);
            InputIndex = 0;
            OnPropertyChanged(nameof(Sequence));
        }

        public bool PlayerInput(int colorIndex)
        {
            if (GameOver || IsShowingSequence)
                return false;

            if (colorIndex == Sequence[InputIndex])
            {
                InputIndex++;
                if (RoundComplete)
                {
                    Level++;
                }
                return true;
            }
            else
            {
                GameOver = true;
                return false;
            }
        }

        public void Reset()
        {
            _sequence.Clear();
            Level = 1;
            InputIndex = 0;
            GameOver = false;
            IsShowingSequence = false;
            OnPropertyChanged(nameof(Sequence));
            OnPropertyChanged(nameof(StatusText));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
