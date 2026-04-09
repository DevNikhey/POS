using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Chess
{
    public enum PieceType
    {
        None,
        Pawn,
        Rook,
        Knight,
        Bishop,
        Queen,
        King
    }

    public class Piece
    {
        public PieceType Type { get; set; }
        public int Color { get; set; } // 1=White, 2=Black

        public string Symbol
        {
            get
            {
                if (Color == 1)
                {
                    switch (Type)
                    {
                        case PieceType.King: return "\u2654";
                        case PieceType.Queen: return "\u2655";
                        case PieceType.Rook: return "\u2656";
                        case PieceType.Bishop: return "\u2657";
                        case PieceType.Knight: return "\u2658";
                        case PieceType.Pawn: return "\u2659";
                        default: return "";
                    }
                }
                else
                {
                    switch (Type)
                    {
                        case PieceType.King: return "\u265A";
                        case PieceType.Queen: return "\u265B";
                        case PieceType.Rook: return "\u265C";
                        case PieceType.Bishop: return "\u265D";
                        case PieceType.Knight: return "\u265E";
                        case PieceType.Pawn: return "\u265F";
                        default: return "";
                    }
                }
            }
        }
    }

    public class Square : INotifyPropertyChanged
    {
        public int Row { get; set; }
        public int Col { get; set; }

        public bool IsLight => (Row + Col) % 2 == 0;

        public string BgColor => IsLight ? "#F0D9B5" : "#B58863";

        private Piece? _piece;
        public Piece? Piece
        {
            get => _piece;
            set
            {
                _piece = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Symbol));
            }
        }

        public string Symbol => _piece?.Symbol ?? "";

        private bool _highlighted;
        public bool Highlighted
        {
            get => _highlighted;
            set
            {
                _highlighted = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class ChessModel : INotifyPropertyChanged
    {
        public ObservableCollection<Square> Squares { get; set; } = new ObservableCollection<Square>();

        private int _currentPlayer = 1;
        public int CurrentPlayer
        {
            get => _currentPlayer;
            set
            {
                _currentPlayer = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusText));
            }
        }

        public string StatusText => CurrentPlayer == 1 ? "White's Turn" : "Black's Turn";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ChessModel()
        {
            InitBoard();
        }

        public void InitBoard()
        {
            Squares.Clear();
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Squares.Add(new Square { Row = r, Col = c });
                }
            }

            // Black pieces (top)
            PlaceRow(0, 2);
            PlacePawns(1, 2);

            // White pieces (bottom)
            PlacePawns(6, 1);
            PlaceRow(7, 1);

            CurrentPlayer = 1;
        }

        private Square GetSquare(int row, int col)
        {
            return Squares[row * 8 + col];
        }

        public void PlaceRow(int row, int color)
        {
            PieceType[] order = {
                PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen,
                PieceType.King, PieceType.Bishop, PieceType.Knight, PieceType.Rook
            };
            for (int c = 0; c < 8; c++)
            {
                GetSquare(row, c).Piece = new Piece { Type = order[c], Color = color };
            }
        }

        public void PlacePawns(int row, int color)
        {
            for (int c = 0; c < 8; c++)
            {
                GetSquare(row, c).Piece = new Piece { Type = PieceType.Pawn, Color = color };
            }
        }

        public List<(int Row, int Col)> GetLegalMoves(int row, int col)
        {
            List<(int, int)> moves = new List<(int, int)>();
            Square sq = GetSquare(row, col);
            Piece? piece = sq.Piece;
            if (piece == null) return moves;

            int color = piece.Color;

            switch (piece.Type)
            {
                case PieceType.Pawn:
                    int dir = (color == 1) ? -1 : 1;
                    int startRow = (color == 1) ? 6 : 1;

                    // Forward one
                    if (InBounds(row + dir, col) && GetSquare(row + dir, col).Piece == null)
                    {
                        moves.Add((row + dir, col));

                        // Forward two from start
                        if (row == startRow && GetSquare(row + 2 * dir, col).Piece == null)
                        {
                            moves.Add((row + 2 * dir, col));
                        }
                    }

                    // Diagonal captures
                    foreach (int dc in new[] { -1, 1 })
                    {
                        int nr = row + dir;
                        int nc = col + dc;
                        if (InBounds(nr, nc))
                        {
                            Piece? target = GetSquare(nr, nc).Piece;
                            if (target != null && target.Color != color)
                            {
                                moves.Add((nr, nc));
                            }
                        }
                    }
                    break;

                case PieceType.Rook:
                    AddSliding(moves, row, col, color, new (int, int)[] { (-1, 0), (1, 0), (0, -1), (0, 1) });
                    break;

                case PieceType.Bishop:
                    AddSliding(moves, row, col, color, new (int, int)[] { (-1, -1), (-1, 1), (1, -1), (1, 1) });
                    break;

                case PieceType.Queen:
                    AddSliding(moves, row, col, color, new (int, int)[] {
                        (-1, 0), (1, 0), (0, -1), (0, 1),
                        (-1, -1), (-1, 1), (1, -1), (1, 1)
                    });
                    break;

                case PieceType.Knight:
                    int[][] knightMoves = new int[][] {
                        new[] { -2, -1 }, new[] { -2, 1 }, new[] { -1, -2 }, new[] { -1, 2 },
                        new[] { 1, -2 }, new[] { 1, 2 }, new[] { 2, -1 }, new[] { 2, 1 }
                    };
                    foreach (int[] km in knightMoves)
                    {
                        int nr = row + km[0];
                        int nc = col + km[1];
                        if (InBounds(nr, nc))
                        {
                            Piece? target = GetSquare(nr, nc).Piece;
                            if (target == null || target.Color != color)
                            {
                                moves.Add((nr, nc));
                            }
                        }
                    }
                    break;

                case PieceType.King:
                    for (int dr = -1; dr <= 1; dr++)
                    {
                        for (int dc = -1; dc <= 1; dc++)
                        {
                            if (dr == 0 && dc == 0) continue;
                            int nr = row + dr;
                            int nc = col + dc;
                            if (InBounds(nr, nc))
                            {
                                Piece? target = GetSquare(nr, nc).Piece;
                                if (target == null || target.Color != color)
                                {
                                    moves.Add((nr, nc));
                                }
                            }
                        }
                    }
                    break;
            }

            return moves;
        }

        private void AddSliding(List<(int, int)> moves, int row, int col, int color, (int dr, int dc)[] directions)
        {
            foreach (var (dr, dc) in directions)
            {
                int nr = row + dr;
                int nc = col + dc;
                while (InBounds(nr, nc))
                {
                    Piece? target = GetSquare(nr, nc).Piece;
                    if (target == null)
                    {
                        moves.Add((nr, nc));
                    }
                    else
                    {
                        if (target.Color != color)
                        {
                            moves.Add((nr, nc));
                        }
                        break;
                    }
                    nr += dr;
                    nc += dc;
                }
            }
        }

        private bool InBounds(int r, int c)
        {
            return r >= 0 && r < 8 && c >= 0 && c < 8;
        }

        public bool TryMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            List<(int, int)> legal = GetLegalMoves(fromRow, fromCol);
            foreach (var (r, c) in legal)
            {
                if (r == toRow && c == toCol)
                {
                    Square from = GetSquare(fromRow, fromCol);
                    Square to = GetSquare(toRow, toCol);
                    to.Piece = from.Piece;
                    from.Piece = null;
                    CurrentPlayer = (CurrentPlayer == 1) ? 2 : 1;
                    return true;
                }
            }
            return false;
        }

        public void HighlightMoves(int row, int col)
        {
            ClearHighlights();
            List<(int, int)> moves = GetLegalMoves(row, col);
            foreach (var (r, c) in moves)
            {
                GetSquare(r, c).Highlighted = true;
            }
        }

        public void ClearHighlights()
        {
            foreach (Square sq in Squares)
            {
                sq.Highlighted = false;
            }
        }
    }
}
