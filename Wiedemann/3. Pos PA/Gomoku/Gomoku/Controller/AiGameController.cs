using Gomoku.shared.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Gomoku.Controller
{
    public class AiGameController : IGameController
    {
        private Gameboard _board;

        private Player _humanPlayer = Player.Black;
        private Player _aiPlayer = Player.White;

        private bool _gameOver = false;

        public AiGameController(Gameboard board)
        {
            _board = board;
        }

        public void MakeMove(Cell cell)
        {
            if (_gameOver)
                return;

            // Mensch Zug
            if (cell.OccupiedBy != Player.None)
                return;

            cell.OccupiedBy = _humanPlayer;

            if (CheckWin(cell))
            {
                MessageBox.Show("Du hast gewonnen!");
                _gameOver = true;
                return;
            }

            // 🔥 AI Zug
            MakeAiMove();
        }

        private void MakeAiMove()
        {
            var move = GetBestMove();

            if (move == null)
                return;

            move.OccupiedBy = _aiPlayer;

            if (CheckWin(move))
            {
                MessageBox.Show("Computer hat gewonnen!");
                _gameOver = true;
            }
        }
        private Cell GetBestMove()
        {
            var freeCells = _board.Cells
                .Where(c => c.OccupiedBy == Player.None)
                .ToList();

            if (!freeCells.Any())
                return null;

            // 🔥 einfache Strategie:
            // 1. Wenn AI gewinnen kann → mach das
            foreach (var cell in freeCells)
            {
                cell.OccupiedBy = _aiPlayer;
                if (CheckWin(cell))
                {
                    cell.OccupiedBy = Player.None;
                    return cell;
                }
                cell.OccupiedBy = Player.None;
            }

            // 2. Blockiere Spieler
            foreach (var cell in freeCells)
            {
                cell.OccupiedBy = _humanPlayer;
                if (CheckWin(cell))
                {
                    cell.OccupiedBy = Player.None;
                    return cell;
                }
                cell.OccupiedBy = Player.None;
            }

            // 3. sonst random
            return freeCells[new Random().Next(freeCells.Count)];
        }
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
