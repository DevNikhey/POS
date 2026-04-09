using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Minesweeper
{
    public class MineCell : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public int Row { get; set; }
        public int Col { get; set; }

        private bool _isMine;
        public bool IsMine
        {
            get => _isMine;
            set { _isMine = value; OnPropertyChanged(); OnPropertyChanged(nameof(Display)); OnPropertyChanged(nameof(BgColor)); OnPropertyChanged(nameof(ForeColor)); }
        }

        private int _adjacentMines;
        public int AdjacentMines
        {
            get => _adjacentMines;
            set { _adjacentMines = value; OnPropertyChanged(); OnPropertyChanged(nameof(Display)); OnPropertyChanged(nameof(ForeColor)); }
        }

        private bool _revealed;
        public bool Revealed
        {
            get => _revealed;
            set { _revealed = value; OnPropertyChanged(); OnPropertyChanged(nameof(Display)); OnPropertyChanged(nameof(BgColor)); OnPropertyChanged(nameof(ForeColor)); }
        }

        private bool _flagged;
        public bool Flagged
        {
            get => _flagged;
            set { _flagged = value; OnPropertyChanged(); OnPropertyChanged(nameof(Display)); OnPropertyChanged(nameof(BgColor)); OnPropertyChanged(nameof(ForeColor)); }
        }

        public string Display
        {
            get
            {
                if (Flagged && !Revealed) return "\u2691";
                if (!Revealed) return "";
                if (IsMine) return "\u2739";
                if (AdjacentMines > 0) return AdjacentMines.ToString();
                return "";
            }
        }

        public string BgColor
        {
            get
            {
                if (Revealed && IsMine) return "#FF4444";
                if (Revealed) return "#D7D7D7";
                return "#4A752C";
            }
        }

        public string ForeColor
        {
            get
            {
                if (!Revealed || IsMine) return "Black";
                switch (AdjacentMines)
                {
                    case 1: return "Blue";
                    case 2: return "Green";
                    case 3: return "Red";
                    case 4: return "DarkBlue";
                    case 5: return "DarkRed";
                    default: return "Black";
                }
            }
        }
    }
}
