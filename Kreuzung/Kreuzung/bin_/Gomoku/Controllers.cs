using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gomoku;

public abstract class Controller
{
    public abstract Task<(int Row, int Col)> GetMove(GameModel model, int player);
}

public class HumanController : Controller
{
    private TaskCompletionSource<(int Row, int Col)>? _tcs;

    public override Task<(int Row, int Col)> GetMove(GameModel model, int player)
    {
        _tcs = new TaskCompletionSource<(int, int)>();
        return _tcs.Task;
    }

    public void OnCellClicked(int row, int col)
    {
        _tcs?.TrySetResult((row, col));
    }
}

public class GreedyAiController : Controller
{
    public override Task<(int Row, int Col)> GetMove(GameModel model, int player)
    {
        int opp = 3 - player;
        int bestScore = -1;
        (int Row, int Col) best = (0, 0);

        for (int r = 0; r < model.Size; r++)
        {
            for (int c = 0; c < model.Size; c++)
            {
                if (model.GetCell(r, c).Player != 0) continue;

                int attack = LineLen(model, r, c, player);
                int defend = LineLen(model, r, c, opp);
                int score = attack * attack * 2 + defend * defend;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = (r, c);
                }
            }
        }

        return Task.FromResult(best);
    }

    private int LineLen(GameModel model, int r, int c, int p)
    {
        int best = 0;
        var dirs = new[] { (0, 1), (1, 0), (1, 1), (1, -1) };
        foreach (var (dr, dc) in dirs)
        {
            int count = 1;
            for (int s = 1; s < 5; s++)
            {
                int nr = r + s * dr, nc = c + s * dc;
                if (nr < 0 || nc < 0 || nr >= model.Size || nc >= model.Size || model.GetCell(nr, nc).Player != p) break;
                count++;
            }
            for (int s = 1; s < 5; s++)
            {
                int nr = r - s * dr, nc = c - s * dc;
                if (nr < 0 || nc < 0 || nr >= model.Size || nc >= model.Size || model.GetCell(nr, nc).Player != p) break;
                count++;
            }
            best = Math.Max(best, count);
        }
        return best;
    }
}

public class RandomController : Controller
{
    private readonly Random _rng = new();

    public override Task<(int Row, int Col)> GetMove(GameModel model, int player)
    {
        var empty = new List<(int, int)>();
        for (int r = 0; r < model.Size; r++)
            for (int c = 0; c < model.Size; c++)
                if (model.GetCell(r, c).Player == 0)
                    empty.Add((r, c));

        return Task.FromResult(empty[_rng.Next(empty.Count)]);
    }
}
