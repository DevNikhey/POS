using System;
using System.Windows;
using System.Windows.Input;

namespace Battleship
{
    public partial class MainWindow : Window
    {
        private BattleshipModel _model;

        public MainWindow()
        {
            InitializeComponent();
            _model = new BattleshipModel();
            BindBoards();
        }

        private void BindBoards()
        {
            PlayerBoardControl.ItemsSource = _model.PlayerBoard.Cells;
            EnemyBoardControl.ItemsSource = _model.EnemyBoard.Cells;
            StatusLabel.Text = _model.StatusText;
        }

        private void EnemyCell_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is SeaCell cell)
            {
                _model.PlayerShoot(cell.Row, cell.Col);
                StatusLabel.Text = _model.StatusText;
            }
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            _model = new BattleshipModel();
            BindBoards();
        }
    }
}
