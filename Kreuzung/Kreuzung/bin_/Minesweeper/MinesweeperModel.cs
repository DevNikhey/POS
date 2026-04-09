using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Minesweeper
{
    public class MinesweeperModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public const int Rows = 10;
        public const int Cols = 10;
        public const int MineCount = 15;

        public ObservableCollection<MineCell> Cells { get; set; } = new ObservableCollection<MineCell>();

        private bool _gameOver;
        public bool GameOver
        {
            get => _gameOver;
            set { _gameOver = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); }
        }

        private bool _won;
        public bool Won
        {
            get => _won;
            set { _won = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); }
        }

        private int _flagsPlaced;
        public int FlagsPlaced
        {
            get => _flagsPlaced;
            set { _flagsPlaced = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); }
        }

        public string StatusText
        {
            get
            {
                if (Won) return "You Win!";
                if (GameOver) return "Game Over!";
                return "Flags: " + FlagsPlaced + " / " + MineCount;
            }
        }

        public MinesweeperModel()
        {
            NewGame();
        }

        public void NewGame()
        {
            Cells.Clear();
            GameOver = false;
            Won = false;
            FlagsPlaced = 0;

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    Cells.Add(new MineCell { Row = r, Col = c });
                }
            }

            PlaceMines();
            CalcAdjacent();
        }

        private void PlaceMines()
        {
            Random rng = new Random();
            int placed = 0;
            while (placed < MineCount)
            {
                int idx = rng.Next(Cells.Count);
                if (!Cells[idx].IsMine)
                {
                    Cells[idx].IsMine = true;
                    placed++;
                }
            }
        }

        private void CalcAdjacent()
        {
            foreach (MineCell cell in Cells)
            {
                if (cell.IsMine) continue;
                int count = 0;
                foreach (MineCell neighbor in Neighbors(cell.Row, cell.Col))
                {
                    if (neighbor.IsMine) count++;
                }
                cell.AdjacentMines = count;
            }
        }

        public void Reveal(int row, int col)
        {
            if (GameOver || Won) return;
            MineCell cell = Cells[row * Cols + col];
            if (cell.Revealed || cell.Flagged) return;

            if (cell.IsMine)
            {
                foreach (MineCell c in Cells)
                {
                    if (c.IsMine) c.Revealed = true;
                }
                GameOver = true;
                return;
            }

            FloodReveal(row, col);
            CheckWin();
        }

        private void FloodReveal(int row, int col)
        {
            if (row < 0 || row >= Rows || col < 0 || col >= Cols) return;
            MineCell cell = Cells[row * Cols + col];
            if (cell.Revealed || cell.IsMine || cell.Flagged) return;

            cell.Revealed = true;

            if (cell.AdjacentMines == 0)
            {
                foreach (MineCell neighbor in Neighbors(row, col))
                {
                    FloodReveal(neighbor.Row, neighbor.Col);
                }
            }
        }

        public void ToggleFlag(int row, int col)
        {
            if (GameOver || Won) return;
            MineCell cell = Cells[row * Cols + col];
            if (cell.Revealed) return;

            cell.Flagged = !cell.Flagged;
            FlagsPlaced += cell.Flagged ? 1 : -1;
            CheckWin();
        }

        private void CheckWin()
        {
            int unrevealed = 0;
            foreach (MineCell cell in Cells)
            {
                if (!cell.Revealed) unrevealed++;
            }
            if (unrevealed == MineCount)
            {
                Won = true;
            }
        }

        public IEnumerable<MineCell> Neighbors(int r, int c)
        {
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    int nr = r + dr;
                    int nc = c + dc;
                    if (nr >= 0 && nr < Rows && nc >= 0 && nc < Cols)
                    {
                        yield return Cells[nr * Cols + nc];
                    }
                }
            }
        }
    }
}
