using System;
using System.Windows;
using System.Windows.Controls;

namespace Chess
{
    public partial class MainWindow : Window
    {
        private ChessModel _model;
        private Square? _selected;

        public MainWindow()
        {
            InitializeComponent();
            _model = new ChessModel();
            DataContext = _model;
            StatusText.Text = _model.StatusText;
        }

        private void Square_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Square sq = (Square)btn.Tag;

            if (_selected == null)
            {
                // Select a piece belonging to current player
                if (sq.Piece != null && sq.Piece.Color == _model.CurrentPlayer)
                {
                    _selected = sq;
                    _model.HighlightMoves(sq.Row, sq.Col);
                }
            }
            else
            {
                // Clicking the same square deselects
                if (sq == _selected)
                {
                    _model.ClearHighlights();
                    _selected = null;
                    return;
                }

                // Clicking another own piece switches selection
                if (sq.Piece != null && sq.Piece.Color == _model.CurrentPlayer)
                {
                    _model.ClearHighlights();
                    _selected = sq;
                    _model.HighlightMoves(sq.Row, sq.Col);
                    return;
                }

                // Try to move
                bool moved = _model.TryMove(_selected.Row, _selected.Col, sq.Row, sq.Col);
                _model.ClearHighlights();
                _selected = null;

                if (moved)
                {
                    StatusText.Text = _model.StatusText;
                }
            }
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            _model.ClearHighlights();
            _selected = null;
            _model.InitBoard();
            StatusText.Text = _model.StatusText;
        }
    }
}
