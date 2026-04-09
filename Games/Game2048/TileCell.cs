using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Game2048
{
    public class TileCell : INotifyPropertyChanged
    {
        private static readonly Dictionary<int, string> ColorMap = new Dictionary<int, string>
        {
            { 0,    "#CDC1B4" },
            { 2,    "#EEE4DA" },
            { 4,    "#EDE0C8" },
            { 8,    "#F2B179" },
            { 16,   "#F59563" },
            { 32,   "#F67C5F" },
            { 64,   "#F65E3B" },
            { 128,  "#EDCF72" },
            { 256,  "#EDCC61" },
            { 512,  "#EDC850" },
            { 1024, "#EDC53F" },
            { 2048, "#EDC22E" }
        };

        private int _row;
        private int _col;
        private int _value;

        public int Row
        {
            get => _row;
            set { _row = value; OnPropertyChanged(); }
        }

        public int Col
        {
            get => _col;
            set { _col = value; OnPropertyChanged(); }
        }

        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Display));
                OnPropertyChanged(nameof(BgColor));
                OnPropertyChanged(nameof(FgColor));
            }
        }

        public string Display => _value == 0 ? "" : _value.ToString();

        public string BgColor
        {
            get
            {
                if (ColorMap.TryGetValue(_value, out string? color))
                    return color;
                return "#3C3A32";
            }
        }

        public string FgColor => _value >= 8 ? "#FFFFFF" : "#776E65";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
