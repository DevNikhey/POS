using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Connect4
{
    public abstract class Controller
    {
        public abstract Task<int> GetColumn(GameModel model);
        public virtual void Cancel() { }
    }

    public class HumanController : Controller
    {
        private TaskCompletionSource<int>? _tcs;

        public override Task<int> GetColumn(GameModel model)
        {
            _tcs = new TaskCompletionSource<int>();
            return _tcs.Task;
        }

        public void ColumnSelected(int col)
        {
            _tcs?.TrySetResult(col);
        }

        public override void Cancel()
        {
            _tcs?.TrySetCanceled();
        }
    }

    public class AiController : Controller
    {
        private readonly Random _random = new Random();

        public override async Task<int> GetColumn(GameModel model)
        {
            await Task.Delay(300); // Small delay so moves are visible

            int player = model.CurrentPlayer;
            int opponent = player == 1 ? 2 : 1;

            // 1. Try to win
            int col = FindWinningColumn(model, player);
            if (col >= 0) return col;

            // 2. Block opponent win
            col = FindWinningColumn(model, opponent);
            if (col >= 0) return col;

            // 3. Prefer center columns
            int[] preference = new int[] { 3, 2, 4, 1, 5, 0, 6 };
            foreach (int c in preference)
            {
                if (!model.IsColumnFull(c))
                {
                    // Avoid moves that let opponent win next turn
                    if (!GivesOpponentWin(model, c, player))
                        return c;
                }
            }

            // 4. Any valid column (even if it gives opponent a win - last resort)
            foreach (int c in preference)
            {
                if (!model.IsColumnFull(c))
                    return c;
            }

            return -1;
        }

        private int FindWinningColumn(GameModel model, int player)
        {
            for (int c = 0; c < GameModel.Cols; c++)
            {
                if (model.IsColumnFull(c)) continue;

                int row = GetLandingRow(model, c);
                if (row < 0) continue;

                // Simulate placing
                model.GetCell(row, c).Player = player;
                bool wins = model.CheckWin(row, c);
                model.GetCell(row, c).Player = 0;

                if (wins) return c;
            }
            return -1;
        }

        private bool GivesOpponentWin(GameModel model, int col, int currentPlayer)
        {
            int row = GetLandingRow(model, col);
            if (row < 0) return false;

            int opponent = currentPlayer == 1 ? 2 : 1;

            // Simulate our move
            model.GetCell(row, col).Player = currentPlayer;

            // Check if opponent can win on the cell above
            int aboveRow = row - 1;
            if (aboveRow >= 0)
            {
                model.GetCell(aboveRow, col).Player = opponent;
                bool opponentWins = model.CheckWin(aboveRow, col);
                model.GetCell(aboveRow, col).Player = 0;

                if (opponentWins)
                {
                    model.GetCell(row, col).Player = 0;
                    return true;
                }
            }

            model.GetCell(row, col).Player = 0;
            return false;
        }

        private int GetLandingRow(GameModel model, int col)
        {
            for (int r = GameModel.Rows - 1; r >= 0; r--)
            {
                if (model.GetCell(r, col).Player == 0)
                    return r;
            }
            return -1;
        }
    }
}
