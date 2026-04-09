using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Game2048
{
    public enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }

    public class Model2048 : INotifyPropertyChanged
    {
        public const int Size = 4;
        public const int WinTile = 2048;

        private readonly Random _random = new Random();
        private int _score;
        private bool _gameOver;
        private bool _won;

        public ObservableCollection<TileCell> Grid { get; }

        public int Score
        {
            get => _score;
            set { _score = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); }
        }

        public bool GameOver
        {
            get => _gameOver;
            set { _gameOver = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); }
        }

        public bool Won
        {
            get => _won;
            set { _won = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); }
        }

        public string StatusText
        {
            get
            {
                if (Won) return "You Win! Score: " + Score;
                if (GameOver) return "Game Over! Score: " + Score;
                return "Score: " + Score;
            }
        }

        public Model2048()
        {
            Grid = new ObservableCollection<TileCell>();
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    Grid.Add(new TileCell { Row = r, Col = c, Value = 0 });
                }
            }
            SpawnRandom();
            SpawnRandom();
        }

        private TileCell CellAt(int row, int col)
        {
            return Grid[row * Size + col];
        }

        public void SpawnRandom()
        {
            List<TileCell> empty = new List<TileCell>();
            foreach (TileCell cell in Grid)
            {
                if (cell.Value == 0) empty.Add(cell);
            }
            if (empty.Count == 0) return;

            TileCell chosen = empty[_random.Next(empty.Count)];
            chosen.Value = _random.NextDouble() < 0.9 ? 2 : 4;
        }

        public void Move(Direction direction)
        {
            if (GameOver || Won) return;

            bool changed = false;

            for (int i = 0; i < Size; i++)
            {
                List<int> lane = new List<int>();

                for (int j = 0; j < Size; j++)
                {
                    int r, c;
                    switch (direction)
                    {
                        case Direction.Left:  r = i; c = j; break;
                        case Direction.Right: r = i; c = Size - 1 - j; break;
                        case Direction.Up:    r = j; c = i; break;
                        case Direction.Down:  r = Size - 1 - j; c = i; break;
                        default: r = 0; c = 0; break;
                    }
                    lane.Add(CellAt(r, c).Value);
                }

                // Compact: remove zeros
                List<int> compacted = new List<int>();
                foreach (int v in lane)
                {
                    if (v != 0) compacted.Add(v);
                }

                // Merge adjacent equal values
                List<int> merged = new List<int>();
                int idx = 0;
                while (idx < compacted.Count)
                {
                    if (idx + 1 < compacted.Count && compacted[idx] == compacted[idx + 1])
                    {
                        int mergedValue = compacted[idx] * 2;
                        merged.Add(mergedValue);
                        Score += mergedValue;
                        if (mergedValue == WinTile) Won = true;
                        idx += 2;
                    }
                    else
                    {
                        merged.Add(compacted[idx]);
                        idx++;
                    }
                }

                // Pad with zeros
                while (merged.Count < Size)
                {
                    merged.Add(0);
                }

                // Write back
                for (int j = 0; j < Size; j++)
                {
                    int r, c;
                    switch (direction)
                    {
                        case Direction.Left:  r = i; c = j; break;
                        case Direction.Right: r = i; c = Size - 1 - j; break;
                        case Direction.Up:    r = j; c = i; break;
                        case Direction.Down:  r = Size - 1 - j; c = i; break;
                        default: r = 0; c = 0; break;
                    }
                    TileCell cell = CellAt(r, c);
                    if (cell.Value != merged[j])
                    {
                        cell.Value = merged[j];
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                SpawnRandom();
                CheckGameOver();
            }
        }

        private void CheckGameOver()
        {
            // Check for empty cells
            foreach (TileCell cell in Grid)
            {
                if (cell.Value == 0) return;
            }

            // Check for adjacent merges
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    int val = CellAt(r, c).Value;
                    if (c + 1 < Size && CellAt(r, c + 1).Value == val) return;
                    if (r + 1 < Size && CellAt(r + 1, c).Value == val) return;
                }
            }

            GameOver = true;
        }

        public void Reset()
        {
            Score = 0;
            GameOver = false;
            Won = false;
            foreach (TileCell cell in Grid)
            {
                cell.Value = 0;
            }
            SpawnRandom();
            SpawnRandom();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
