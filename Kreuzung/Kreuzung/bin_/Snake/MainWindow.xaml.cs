using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Snake
{
    public partial class MainWindow : Window
    {
        private SnakeModel _model = new();
        private DispatcherTimer _timer = new();

        public MainWindow()
        {
            InitializeComponent();

            _timer.Tick += Timer_Tick;
            _timer.Interval = TimeSpan.FromMilliseconds(_model.Speed);
            _timer.Start();

            GameCanvas.Focus();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _model.Tick();
            _timer.Interval = TimeSpan.FromMilliseconds(_model.Speed);
            Draw();
        }

        private void Draw()
        {
            GameCanvas.Children.Clear();

            double canvasWidth = GameCanvas.ActualWidth;
            double canvasHeight = GameCanvas.ActualHeight;

            if (canvasWidth <= 0 || canvasHeight <= 0) return;

            double cellW = canvasWidth / _model.GridSize;
            double cellH = canvasHeight / _model.GridSize;

            // Draw food
            var food = new Ellipse
            {
                Width = cellW,
                Height = cellH,
                Fill = Brushes.Red
            };
            Canvas.SetLeft(food, _model.Food.X * cellW);
            Canvas.SetTop(food, _model.Food.Y * cellH);
            GameCanvas.Children.Add(food);

            // Draw snake
            bool isHead = true;
            foreach (var segment in _model.Body)
            {
                var rect = new Rectangle
                {
                    Width = cellW,
                    Height = cellH,
                    Fill = isHead ? Brushes.Lime : Brushes.LimeGreen
                };
                Canvas.SetLeft(rect, segment.X * cellW);
                Canvas.SetTop(rect, segment.Y * cellH);
                GameCanvas.Children.Add(rect);
                isHead = false;
            }

            // Update score
            ScoreText.Text = _model.GameOver
                ? $"Game Over! Score: {_model.Score} - Press SPACE for New Game"
                : $"Score: {_model.Score}";
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    _model.ChangeDirection(Direction.Up);
                    break;
                case Key.Down:
                    _model.ChangeDirection(Direction.Down);
                    break;
                case Key.Left:
                    _model.ChangeDirection(Direction.Left);
                    break;
                case Key.Right:
                    _model.ChangeDirection(Direction.Right);
                    break;
                case Key.Space when _model.GameOver:
                    _model.Reset();
                    _timer.Interval = TimeSpan.FromMilliseconds(_model.Speed);
                    break;
            }
        }
    }
}
