using System.ComponentModel;

namespace Gomoku;

public class Cell : INotifyPropertyChanged
{
    public int Row { get; }
    public int Col { get; }

    private int _player;
    public int Player
    {
        get => _player;
        set
        {
            if (_player != value)
            {
                _player = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Player)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Symbol)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Background)));
            }
        }
    }

    public string Symbol => Player switch { 1 => "X", 2 => "O", _ => "" };

    public string Background => Player switch
    {
        1 => "#4A90D9",
        2 => "#D94A4A",
        _ => "#F0F0F0"
    };

    public Cell(int row, int col)
    {
        Row = row;
        Col = col;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
