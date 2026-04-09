using System;
using System.Collections.Generic;

namespace PenteServer
{
    public class PenteLogic
    {
        public const int BoardSize = 19;
        public const int Center = 9;
        public const int CapturesForWin = 10;
        public const int StonesInRowForWin = 5;

        public int[,] Board { get; } = new int[BoardSize, BoardSize];
        public int WhiteCaptures { get; set; }
        public int BlackCaptures { get; set; }
        public int CurrentPlayer { get; set; } = 1; // 1=White, 2=Black
        public int MoveCount { get; set; }

        private static readonly int[][] Directions = new int[][]
        {
            new int[] { 0, 1 },   // horizontal
            new int[] { 1, 0 },   // vertical
            new int[] { 1, 1 },   // diagonal down-right
            new int[] { 1, -1 }   // diagonal down-left
        };

        public bool IsValidMove(int row, int col)
        {
            if (row < 0 || row >= BoardSize || col < 0 || col >= BoardSize)
                return false;

            if (Board[row, col] != 0)
                return false;

            // Tournament rule: first move must be center
            if (MoveCount == 0)
            {
                return row == Center && col == Center;
            }

            // Tournament rule: White's second move (MoveCount == 2) must be >= 3 from center
            if (MoveCount == 2 && CurrentPlayer == 1)
            {
                int dr = Math.Abs(row - Center);
                int dc = Math.Abs(col - Center);
                if (dr < 3 && dc < 3)
                    return false;
            }

            return true;
        }

        public bool TryPlaceStone(int row, int col, out List<(int, int)> captured, out bool won, out string? winReason)
        {
            captured = new List<(int, int)>();
            won = false;
            winReason = null;

            if (!IsValidMove(row, col))
                return false;

            Board[row, col] = CurrentPlayer;
            MoveCount++;

            // Check captures in all 4 directions (both orientations = 8 directions)
            int opponent = CurrentPlayer == 1 ? 2 : 1;
            foreach (int[] dir in Directions)
            {
                // Check positive direction: current, opponent, opponent, current
                CheckCapture(row, col, dir[0], dir[1], opponent, captured);
                // Check negative direction
                CheckCapture(row, col, -dir[0], -dir[1], opponent, captured);
            }

            // Apply captures
            foreach ((int cr, int cc) in captured)
            {
                Board[cr, cc] = 0;
            }

            if (CurrentPlayer == 1)
                WhiteCaptures += captured.Count;
            else
                BlackCaptures += captured.Count;

            // Check capture win
            if (CurrentPlayer == 1 && WhiteCaptures >= CapturesForWin)
            {
                won = true;
                winReason = "Captures";
                return true;
            }
            if (CurrentPlayer == 2 && BlackCaptures >= CapturesForWin)
            {
                won = true;
                winReason = "Captures";
                return true;
            }

            // Check five in a row
            if (CheckFiveInARow(row, col))
            {
                won = true;
                winReason = "FiveInARow";
                return true;
            }

            // Switch turn
            CurrentPlayer = opponent;
            return true;
        }

        private void CheckCapture(int row, int col, int dr, int dc, int opponent, List<(int, int)> captured)
        {
            int r1 = row + dr;
            int c1 = col + dc;
            int r2 = row + 2 * dr;
            int c2 = col + 2 * dc;
            int r3 = row + 3 * dr;
            int c3 = col + 3 * dc;

            if (!InBounds(r1, c1) || !InBounds(r2, c2) || !InBounds(r3, c3))
                return;

            if (Board[r1, c1] == opponent && Board[r2, c2] == opponent && Board[r3, c3] == CurrentPlayer)
            {
                captured.Add((r1, c1));
                captured.Add((r2, c2));
            }
        }

        private bool CheckFiveInARow(int row, int col)
        {
            int player = Board[row, col];
            foreach (int[] dir in Directions)
            {
                int count = 1;
                // Count in positive direction
                count += CountInDirection(row, col, dir[0], dir[1], player);
                // Count in negative direction
                count += CountInDirection(row, col, -dir[0], -dir[1], player);

                if (count >= StonesInRowForWin)
                    return true;
            }
            return false;
        }

        private int CountInDirection(int row, int col, int dr, int dc, int player)
        {
            int count = 0;
            int r = row + dr;
            int c = col + dc;
            while (InBounds(r, c) && Board[r, c] == player)
            {
                count++;
                r += dr;
                c += dc;
            }
            return count;
        }

        private static bool InBounds(int r, int c)
        {
            return r >= 0 && r < BoardSize && c >= 0 && c < BoardSize;
        }
    }
}
