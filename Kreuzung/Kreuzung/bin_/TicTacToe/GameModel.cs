using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TicTacToe;

public class GameModel : INotifyPropertyChanged
{
    public int Size { get; }
    public ObservableCollection<Cell> Cells { get; }

    private int _currentPlayer = 1;
    public int CurrentPlayer
    {
        get => _currentPlayer;
        private set { _currentPlayer = value; OnPropertyChanged(nameof(CurrentPlayer)); OnPropertyChanged(nameof(StatusText)); }
    }

    private int? _winner;
    public int? Winner
    {
        get => _winner;
        private set { _winner = value; OnPropertyChanged(nameof(Winner)); OnPropertyChanged(nameof(StatusText)); OnPropertyChanged(nameof(IsGameOver)); }
    }

    public bool IsGameOver => Winner != null;
    public int MoveCount { get; private set; }

    public string StatusText => Winner switch
    {
        null => $"Player {CurrentPlayer}'s turn ({(CurrentPlayer == 1 ? "X" : "O")})",
        -1 => "It's a draw!",
        _ => $"Player {Winner} ({(Winner == 1 ? "X" : "O")}) wins!"
    };

    public GameModel(int size = 3)
    {
        Size = size;
        Cells = new ObservableCollection<Cell>();
        for (int r = 0; r < size; r++)
            for (int c = 0; c < size; c++)
                Cells.Add(new Cell(r, c));
    }

    public Cell GetCell(int row, int col) => Cells[row * Size + col];

    public bool Place(int row, int col)
    {
        if (row < 0 || col < 0 || row >= Size || col >= Size) return false;
        var cell = GetCell(row, col);
        if (cell.Player != 0 || Winner != null) return false;

        cell.Player = CurrentPlayer;
        MoveCount++;

        if (CheckWin(row, col))
            Winner = CurrentPlayer;
        else if (MoveCount == Size * Size)
            Winner = -1;
        else
            CurrentPlayer = 3 - CurrentPlayer;

        return true;
    }

    private bool CheckWin(int r, int c)
    {
        int p = GetCell(r, c).Player;
        if (p == 0) return false;

        var dirs = new[] { (0, 1), (1, 0), (1, 1), (1, -1) };
        foreach (var (dr, dc) in dirs)
        {
            int count = 1;
            for (int s = 1; s < 3; s++)
            {
                int nr = r + s * dr, nc = c + s * dc;
                if (nr < 0 || nc < 0 || nr >= Size || nc >= Size) break;
                if (GetCell(nr, nc).Player != p) break;
                count++;
            }
            for (int s = 1; s < 3; s++)
            {
                int nr = r - s * dr, nc = c - s * dc;
                if (nr < 0 || nc < 0 || nr >= Size || nc >= Size) break;
                if (GetCell(nr, nc).Player != p) break;
                count++;
            }
            if (count >= 3) return true;
        }
        return false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
