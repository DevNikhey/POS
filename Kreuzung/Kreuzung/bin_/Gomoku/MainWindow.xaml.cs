using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Gomoku;

public partial class MainWindow : Window
{
    private const int BoardSize = 9;
    private GameModel _game = null!;
    private Controller _player1 = null!;
    private Controller _player2 = null!;
    private bool _gameRunning;

    public MainWindow()
    {
        InitializeComponent();
        StartNewGame();
    }

    private void NewGame_Click(object sender, RoutedEventArgs e) => StartNewGame();
    private void Mode_Changed(object sender, SelectionChangedEventArgs e) => StartNewGame();

    private void StartNewGame()
    {
        _gameRunning = false;
        _game = new GameModel(BoardSize);
        boardControl.ItemsSource = _game.Cells;

        int mode = cmbMode?.SelectedIndex ?? 0;
        switch (mode)
        {
            case 0: // Human vs AI
                _player1 = new HumanController();
                _player2 = new GreedyAiController();
                break;
            case 1: // Human vs Human
                _player1 = new HumanController();
                _player2 = new HumanController();
                break;
            case 2: // AI vs AI
                _player1 = new GreedyAiController();
                _player2 = new RandomController();
                break;
        }

        UpdateStatus();
        RunGameLoop();
    }

    private async void RunGameLoop()
    {
        _gameRunning = true;
        var controllers = new Dictionary<int, Controller> { [1] = _player1, [2] = _player2 };

        while (_game.Winner == null && _gameRunning)
        {
            var currentController = controllers[_game.CurrentPlayer];
            var (row, col) = await currentController.GetMove(_game, _game.CurrentPlayer);

            if (!_gameRunning) return;

            _game.Place(row, col);
            UpdateStatus();

            // Small delay for AI moves so the user can see them
            if (currentController is not HumanController && _gameRunning)
                await Task.Delay(50);
        }
    }

    private void Cell_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Cell cell)
        {
            var controllers = new Controller[] { _player1, _player2 };
            foreach (var ctrl in controllers)
            {
                if (ctrl is HumanController human)
                    human.OnCellClicked(cell.Row, cell.Col);
            }
        }
    }

    private void UpdateStatus()
    {
        txtStatus.Text = _game.StatusText;
    }
}
