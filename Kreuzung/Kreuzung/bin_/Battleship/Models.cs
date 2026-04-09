using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Battleship
{
    public enum CellState
    {
        Water,
        Ship,
        Hit,
        Miss,
        Sunk
    }

    public class SeaCell : INotifyPropertyChanged
    {
        private CellState _state;

        public int Row { get; set; }
        public int Col { get; set; }

        public CellState State
        {
            get => _state;
            set
            {
                _state = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Display));
                OnPropertyChanged(nameof(Color));
            }
        }

        public string Display
        {
            get
            {
                switch (State)
                {
                    case CellState.Hit: return "X";
                    case CellState.Miss: return "\u2022";
                    case CellState.Sunk: return "X";
                    default: return "";
                }
            }
        }

        public SolidColorBrush Color
        {
            get
            {
                switch (State)
                {
                    case CellState.Water: return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3"));
                    case CellState.Ship: return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#607D8B"));
                    case CellState.Hit: return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336"));
                    case CellState.Miss: return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#90CAF9"));
                    case CellState.Sunk: return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D32F2F"));
                    default: return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3"));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class Board
    {
        public int Size { get; set; }
        public ObservableCollection<SeaCell> Cells { get; set; }

        public Board(int size)
        {
            Size = size;
            Cells = new ObservableCollection<SeaCell>();
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    Cells.Add(new SeaCell { Row = r, Col = c, State = CellState.Water });
                }
            }
        }

        public SeaCell At(int r, int c)
        {
            return Cells[r * Size + c];
        }
    }

    public class Ship
    {
        public int Length { get; set; }
        public string Name { get; set; } = "";
        public List<(int R, int C)> Positions { get; set; } = new List<(int R, int C)>();

        public bool IsSunk(Board board)
        {
            return Positions.All(p =>
            {
                CellState s = board.At(p.R, p.C).State;
                return s == CellState.Hit || s == CellState.Sunk;
            });
        }
    }

    public class BattleshipModel : INotifyPropertyChanged
    {
        private static readonly List<(int Length, string Name)> ShipDefs = new List<(int, string)>
        {
            (5, "Carrier"),
            (4, "Battleship"),
            (3, "Cruiser"),
            (3, "Submarine"),
            (2, "Destroyer")
        };

        private bool _isPlayerTurn;
        private bool _gameOver;
        private string _statusText = "";
        private readonly Random _rng = new Random();

        public int Size { get; } = 10;
        public Board PlayerBoard { get; set; }
        public Board EnemyBoard { get; set; }
        public List<Ship> PlayerShips { get; set; }
        public List<Ship> EnemyShips { get; set; }

        public bool IsPlayerTurn
        {
            get => _isPlayerTurn;
            set { _isPlayerTurn = value; OnPropertyChanged(); }
        }

        public bool GameOver
        {
            get => _gameOver;
            set { _gameOver = value; OnPropertyChanged(); }
        }

        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        public BattleshipModel()
        {
            PlayerBoard = new Board(Size);
            EnemyBoard = new Board(Size);
            PlayerShips = new List<Ship>();
            EnemyShips = new List<Ship>();
            NewGame();
        }

        public void NewGame()
        {
            PlayerBoard = new Board(Size);
            EnemyBoard = new Board(Size);
            PlayerShips = new List<Ship>();
            EnemyShips = new List<Ship>();

            PlaceShipsRandom(PlayerBoard, PlayerShips);
            PlaceShipsRandom(EnemyBoard, EnemyShips);

            // Show player ships on their board
            foreach (Ship ship in PlayerShips)
            {
                foreach ((int R, int C) pos in ship.Positions)
                {
                    PlayerBoard.At(pos.R, pos.C).State = CellState.Ship;
                }
            }

            IsPlayerTurn = true;
            GameOver = false;
            StatusText = "Your turn! Click on the enemy board to fire.";

            OnPropertyChanged(nameof(PlayerBoard));
            OnPropertyChanged(nameof(EnemyBoard));
        }

        private void PlaceShipsRandom(Board board, List<Ship> ships)
        {
            foreach ((int Length, string Name) def in ShipDefs)
            {
                Ship ship = new Ship { Length = def.Length, Name = def.Name };
                bool placed = false;

                while (!placed)
                {
                    int r = _rng.Next(Size);
                    int c = _rng.Next(Size);
                    bool horizontal = _rng.Next(2) == 0;

                    List<(int R, int C)> positions = new List<(int R, int C)>();
                    bool valid = true;

                    for (int i = 0; i < def.Length; i++)
                    {
                        int nr = horizontal ? r : r + i;
                        int nc = horizontal ? c + i : c;

                        if (nr >= Size || nc >= Size)
                        {
                            valid = false;
                            break;
                        }

                        // Check collision with already placed ships
                        bool collision = false;
                        foreach (Ship s in ships)
                        {
                            foreach ((int R, int C) p in s.Positions)
                            {
                                if (p.R == nr && p.C == nc)
                                {
                                    collision = true;
                                    break;
                                }
                            }
                            if (collision) break;
                        }

                        if (collision)
                        {
                            valid = false;
                            break;
                        }

                        positions.Add((nr, nc));
                    }

                    if (valid)
                    {
                        ship.Positions = positions;
                        ships.Add(ship);
                        placed = true;
                    }
                }
            }
        }

        public void PlayerShoot(int row, int col)
        {
            if (GameOver || !IsPlayerTurn) return;

            SeaCell cell = EnemyBoard.At(row, col);
            if (cell.State == CellState.Hit || cell.State == CellState.Miss || cell.State == CellState.Sunk)
                return;

            // Check if any enemy ship occupies this cell
            Ship? hitShip = null;
            foreach (Ship ship in EnemyShips)
            {
                foreach ((int R, int C) p in ship.Positions)
                {
                    if (p.R == row && p.C == col)
                    {
                        hitShip = ship;
                        break;
                    }
                }
                if (hitShip != null) break;
            }

            if (hitShip != null)
            {
                cell.State = CellState.Hit;
                if (hitShip.IsSunk(EnemyBoard))
                {
                    foreach ((int R, int C) p in hitShip.Positions)
                    {
                        EnemyBoard.At(p.R, p.C).State = CellState.Sunk;
                    }
                    StatusText = $"You sunk the enemy's {hitShip.Name}!";
                }
                else
                {
                    StatusText = "Hit!";
                }
            }
            else
            {
                cell.State = CellState.Miss;
                StatusText = "Miss!";
            }

            // Check win
            if (EnemyShips.All(s => s.IsSunk(EnemyBoard)))
            {
                GameOver = true;
                StatusText = "You win! All enemy ships sunk!";
                return;
            }

            // AI turn
            IsPlayerTurn = false;
            AiShoot();
            IsPlayerTurn = true;

            if (!GameOver)
            {
                StatusText += " Your turn!";
            }
        }

        private void AiShoot()
        {
            List<SeaCell> available = new List<SeaCell>();
            foreach (SeaCell cell in PlayerBoard.Cells)
            {
                if (cell.State == CellState.Water || cell.State == CellState.Ship)
                {
                    available.Add(cell);
                }
            }

            if (available.Count == 0) return;

            SeaCell target = available[_rng.Next(available.Count)];

            // Check if player ship occupies this cell
            Ship? hitShip = null;
            foreach (Ship ship in PlayerShips)
            {
                foreach ((int R, int C) p in ship.Positions)
                {
                    if (p.R == target.Row && p.C == target.Col)
                    {
                        hitShip = ship;
                        break;
                    }
                }
                if (hitShip != null) break;
            }

            if (hitShip != null)
            {
                target.State = CellState.Hit;
                if (hitShip.IsSunk(PlayerBoard))
                {
                    foreach ((int R, int C) p in hitShip.Positions)
                    {
                        PlayerBoard.At(p.R, p.C).State = CellState.Sunk;
                    }
                    StatusText = $"Enemy sunk your {hitShip.Name}!";
                }
                else
                {
                    StatusText = "Enemy hit your ship!";
                }
            }
            else
            {
                target.State = CellState.Miss;
                StatusText = "Enemy missed!";
            }

            // Check AI win
            if (PlayerShips.All(s => s.IsSunk(PlayerBoard)))
            {
                GameOver = true;
                StatusText = "You lose! All your ships were sunk!";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
