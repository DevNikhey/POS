using Gomoku.shared.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Gomoku.Controller
{
    public class LocalGameController : IGameController
    {
        private Gameboard _board;
        private Player _currentPlayer = Player.Black;

        public LocalGameController(Gameboard board)
        {
            _board = board;
        }

        public void MakeMove(Cell cell)
        {
            if (cell.OccupiedBy != Player.None)
                return;

            cell.OccupiedBy = _currentPlayer;

            if (CheckWin(cell))
            {
                MessageBox.Show($"{_currentPlayer} hat gewonnen!");
                return;
            }

            _currentPlayer = _currentPlayer == Player.Black
                ? Player.White
                : Player.Black;
        }

        // ===== GEWINNLOGIK =====

        private bool CheckWin(Cell startCell)
        {
            var player = startCell.OccupiedBy;

            return CheckDirection(startCell, player, 1, 0) ||
                   CheckDirection(startCell, player, 0, 1) ||
                   CheckDirection(startCell, player, 1, 1) ||
                   CheckDirection(startCell, player, 1, -1);
        }

        private bool CheckDirection(Cell start, Player player, int dx, int dy)
        {
            int count = 1;

            count += Count(start, player, dx, dy);
            count += Count(start, player, -dx, -dy);

            return count >= 5;
        }

        private int Count(Cell start, Player player, int dx, int dy)
        {
            int count = 0;

            int x = start.X + dx;
            int y = start.Y + dy;

            while (true)
            {
                var cell = _board.Cells.FirstOrDefault(c => c.X == x && c.Y == y);

                if (cell == null || cell.OccupiedBy != player)
                    break;

                count++;
                x += dx;
                y += dy;
            }

            return count;
        }
    }
}
