using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Connect4
{
    public class Cell : INotifyPropertyChanged
    {
        private int _player;

        public int Row { get; }
        public int Col { get; }

        public int Player
        {
            get => _player;
            set
            {
                if (_player != value)
                {
                    _player = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Color));
                }
            }
        }

        public string Color
        {
            get
            {
                switch (_player)
                {
                    case 1: return "Red";
                    case 2: return "Yellow";
                    default: return "LightGray";
                }
            }
        }

        public Cell(int row, int col)
        {
            Row = row;
            Col = col;
            _player = 0;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
