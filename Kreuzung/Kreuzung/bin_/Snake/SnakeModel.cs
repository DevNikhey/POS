using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Snake
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public class SnakeModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public int GridSize { get; } = 20;

        public LinkedList<(int X, int Y)> Body { get; private set; } = new();

        private (int X, int Y) _food;
        public (int X, int Y) Food
        {
            get => _food;
            private set { _food = value; OnPropertyChanged(); }
        }

        private Direction _dir;
        public Direction Dir
        {
            get => _dir;
            private set { _dir = value; OnPropertyChanged(); }
        }

        private bool _gameOver;
        public bool GameOver
        {
            get => _gameOver;
            private set { _gameOver = value; OnPropertyChanged(); }
        }

        private int _score;
        public int Score
        {
            get => _score;
            private set { _score = value; OnPropertyChanged(); }
        }

        private int _speed;
        public int Speed
        {
            get => _speed;
            private set { _speed = value; OnPropertyChanged(); }
        }

        private readonly Random _rng = new();

        public SnakeModel()
        {
            Reset();
        }

        public void Reset()
        {
            Body = new LinkedList<(int X, int Y)>();
            int midX = GridSize / 2;
            int midY = GridSize / 2;
            Body.AddFirst((midX, midY));
            Body.AddLast((midX - 1, midY));
            Body.AddLast((midX - 2, midY));

            Dir = Direction.Right;
            GameOver = false;
            Score = 0;
            Speed = 150;
            SpawnFood();
        }

        public void ChangeDirection(Direction d)
        {
            if (GameOver) return;

            if (Dir == Direction.Up && d == Direction.Down) return;
            if (Dir == Direction.Down && d == Direction.Up) return;
            if (Dir == Direction.Left && d == Direction.Right) return;
            if (Dir == Direction.Right && d == Direction.Left) return;

            Dir = d;
        }

        public void Tick()
        {
            if (GameOver) return;

            var head = Body.First!.Value;
            (int X, int Y) newHead = Dir switch
            {
                Direction.Up => (head.X, head.Y - 1),
                Direction.Down => (head.X, head.Y + 1),
                Direction.Left => (head.X - 1, head.Y),
                Direction.Right => (head.X + 1, head.Y),
                _ => head
            };

            // Wall collision
            if (newHead.X < 0 || newHead.X >= GridSize || newHead.Y < 0 || newHead.Y >= GridSize)
            {
                GameOver = true;
                return;
            }

            // Self collision
            if (Body.Contains(newHead))
            {
                GameOver = true;
                return;
            }

            Body.AddFirst(newHead);

            if (newHead == Food)
            {
                Score++;
                Speed = Math.Max(50, Speed - 5);
                SpawnFood();
            }
            else
            {
                Body.RemoveLast();
            }
        }

        private void SpawnFood()
        {
            var occupied = new HashSet<(int, int)>(Body);
            var free = new List<(int, int)>();

            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    if (!occupied.Contains((x, y)))
                        free.Add((x, y));
                }
            }

            if (free.Count > 0)
            {
                Food = free[_rng.Next(free.Count)];
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
