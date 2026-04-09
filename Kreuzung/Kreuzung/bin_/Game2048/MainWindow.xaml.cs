using System;
using System.Windows;
using System.Windows.Input;

namespace Game2048
{
    public partial class MainWindow : Window
    {
        private readonly Model2048 _model;

        public MainWindow()
        {
            InitializeComponent();
            _model = new Model2048();
            DataContext = _model;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    _model.Move(Direction.Left);
                    break;
                case Key.Right:
                    _model.Move(Direction.Right);
                    break;
                case Key.Up:
                    _model.Move(Direction.Up);
                    break;
                case Key.Down:
                    _model.Move(Direction.Down);
                    break;
            }
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            _model.Reset();
        }
    }
}
