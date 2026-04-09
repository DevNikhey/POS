using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku.shared.models
{
    public enum Player
    {
        None,
        Black,
        White
    }

    public class Cell: INotifyPropertyChanged
    {
        public int X { get; set; }
        public int Y { get; set; }
        private Player _occupiedBy = Player.None;
        public Player OccupiedBy
        {
            get => _occupiedBy;
            set
            {
                if (_occupiedBy != value)
                {
                    _occupiedBy = value;
                    OnPropertyChanged(nameof(OccupiedBy));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
