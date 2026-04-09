using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Minesweeper
{
    public partial class MainWindow : Window
    {
        private MinesweeperModel _model;

        public MainWindow()
        {
            InitializeComponent();
            _model = new MinesweeperModel();
            DataContext = _model;
        }

        private void Cell_LeftClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is MineCell cell)
            {
                _model.Reveal(cell.Row, cell.Col);
            }
        }

        private void Cell_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is MineCell cell)
            {
                _model.ToggleFlag(cell.Row, cell.Col);
            }
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            _model.NewGame();
        }
    }
}
