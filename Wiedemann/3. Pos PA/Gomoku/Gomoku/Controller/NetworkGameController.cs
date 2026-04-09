using Gomoku.Network;
using Gomoku.shared.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Gomoku.Controller
{
    public class NetworkGameController: IGameController
    {
        private Gameboard _board;
        private NetworkClient _client;

        private Player _myPlayer;
        private Player _currentPlayer = Player.Black;

        public NetworkGameController(Gameboard board, NetworkClient client, Player myPlayer)
        {
            _board = board;
            _client = client;
            _myPlayer = myPlayer;

            _client.OnMessageReceived += OnNetworkMove;
        }

        public void MakeMove(Cell cell)
        {
            if (_currentPlayer != _myPlayer)
                return;
            if (cell.OccupiedBy != Player.None)
                return;

            var msg = new NetworkMessage
            {
                X = cell.X,
                Y = cell.Y,
                Player = _myPlayer
            };

            _client.Send(msg);
        }

        private void OnNetworkMove(NetworkMessage msg)
        {
            var cell = _board.Cells.FirstOrDefault(c => c.X == msg.X && c.Y == msg.Y);

            if (cell == null || cell.OccupiedBy != Player.None)
                return;

            cell.OccupiedBy = msg.Player;

            if (CheckWin(cell, msg.Player))
            {
                MessageBox.Show($"{msg.Player} hat gewonnen!");
                return;
            }

            // Spieler wechseln
            _currentPlayer = _currentPlayer == Player.Black
                ? Player.White
                : Player.Black;
        }

        private bool CheckWin(Cell startCell, Player player)
        {
            return CheckDirection(startCell, player, 1, 0) ||   // horizontal
                   CheckDirection(startCell, player, 0, 1) ||   // vertikal
                   CheckDirection(startCell, player, 1, 1) ||   // diagonal ↘
                   CheckDirection(startCell, player, 1, -1);    // diagonal ↙
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
                var cell = _board.GetCell(x, y);

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
