using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Memory
{
    public class Card : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private int _pairId;
        private string _symbol = "";
        private bool _faceUp;
        private bool _matched;

        public int PairId
        {
            get => _pairId;
            set { _pairId = value; OnPropertyChanged(); }
        }

        public string Symbol
        {
            get => _symbol;
            set { _symbol = value; OnPropertyChanged(); OnPropertyChanged(nameof(Display)); }
        }

        public bool FaceUp
        {
            get => _faceUp;
            set { _faceUp = value; OnPropertyChanged(); OnPropertyChanged(nameof(Display)); }
        }

        public bool Matched
        {
            get => _matched;
            set { _matched = value; OnPropertyChanged(); OnPropertyChanged(nameof(Display)); }
        }

        public string Display => (FaceUp || Matched) ? Symbol : "?";

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
