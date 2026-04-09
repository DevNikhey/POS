using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Connect4
{
    public class GameModel : INotifyPropertyChanged
    {
        public const int Rows = 6;
        public const int Cols = 7;

        private int _currentPlayer;
        private int _winner;

        private bool _gameOver;

        public ObservableCollection<Cell> Cells { get; } = new ObservableCollection<Cell>();

        public int CurrentPlayer
        {
            get => _currentPlayer;
            set { _currentPlayer = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); }
        }

        public int Winner
        {
            get => _winner;
            set { _winner = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); }
        }

        public bool GameOver
        {
            get => _gameOver;
            set { _gameOver = value; OnPropertyChanged(); }
        }

        public string StatusText
        {
            get
            {
                if (_winner == 1) return "Red wins!";
                if (_winner == 2) return "Yellow wins!";
                if (_gameOver) return "Draw!";
                return _currentPlayer == 1 ? "Red's turn" : "Yellow's turn";
            }
        }

        public GameModel()
        {
            InitBoard();
        }

        private void InitBoard()
        {
            Cells.Clear();
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    Cells.Add(new Cell(r, c));
                }
            }
        }

        public Cell GetCell(int row, int col)
        {
            return Cells[row * Cols + col];
        }

        public void Reset()
        {
            foreach (var cell in Cells)
            {
                cell.Player = 0;
            }
            CurrentPlayer = 1;
            Winner = 0;
            GameOver = false;
            OnPropertyChanged(nameof(StatusText));
        }

        /// <summary>
        /// Drops a disc in the given column. Returns the row it landed on, or -1 if column is full.
        /// </summary>
        public int Drop(int col)
        {
            if (col < 0 || col >= Cols || GameOver)
                return -1;

            for (int r = Rows - 1; r >= 0; r--)
            {
                var cell = GetCell(r, col);
                if (cell.Player == 0)
                {
                    cell.Player = CurrentPlayer;

                    if (CheckWin(r, col))
                    {
                        Winner = CurrentPlayer;
                        GameOver = true;
                    }
                    else if (IsBoardFull())
                    {
                        GameOver = true;
                        OnPropertyChanged(nameof(StatusText));
                    }
                    else
                    {
                        CurrentPlayer = CurrentPlayer == 1 ? 2 : 1;
                    }

                    return r;
                }
            }
            return -1;
        }

        public bool IsColumnFull(int col)
        {
            return GetCell(0, col).Player != 0;
        }

        private bool IsBoardFull()
        {
            for (int c = 0; c < Cols; c++)
            {
                if (GetCell(0, c).Player == 0) return false;
            }
            return true;
        }

        public bool CheckWin(int row, int col)
        {
            int player = GetCell(row, col).Player;
            if (player == 0) return false;

            // Directions: horizontal, vertical, diagonal-down-right, diagonal-down-left
            int[][] dRows = new int[][] { new[] { 0, 0 }, new[] { -1, 1 }, new[] { -1, 1 }, new[] { 1, -1 } };
            int[][] dCols = new int[][] { new[] { -1, 1 }, new[] { 0, 0 }, new[] { -1, 1 }, new[] { -1, 1 } };

            for (int d = 0; d < 4; d++)
            {
                int count = 1;
                for (int dir = 0; dir < 2; dir++)
                {
                    int dr = dRows[d][dir];
                    int dc = dCols[d][dir];
                    int r = row + dr;
                    int c = col + dc;
                    while (r >= 0 && r < Rows && c >= 0 && c < Cols && GetCell(r, c).Player == player)
                    {
                        count++;
                        r += dr;
                        c += dc;
                    }
                }
                if (count >= 4) return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the player at (row, col), or 0 if empty.
        /// </summary>
        public int GetPlayer(int row, int col)
        {
            return GetCell(row, col).Player;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
