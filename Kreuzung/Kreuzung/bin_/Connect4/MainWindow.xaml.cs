using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Connect4
{
    public partial class MainWindow : Window
    {
        private GameModel _model = new GameModel();
        private Controller? _controller1;
        private Controller? _controller2;
        private CancellationTokenSource? _cts;
        private HumanController? _activeHuman;

        public MainWindow()
        {
            InitializeComponent();
            BoardControl.ItemsSource = _model.Cells;
            _model.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(GameModel.StatusText))
                    StatusText.Text = _model.StatusText;
            };
            StartNewGame();
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }

        private void StartNewGame()
        {
            // Cancel any running game loop
            _controller1?.Cancel();
            _controller2?.Cancel();
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            _model.Reset();
            StatusText.Text = _model.StatusText;

            int mode = ModeCombo.SelectedIndex;
            switch (mode)
            {
                case 0: // Human vs AI
                    _controller1 = new HumanController();
                    _controller2 = new AiController();
                    break;
                case 1: // Human vs Human
                    _controller1 = new HumanController();
                    _controller2 = new HumanController();
                    break;
                case 2: // AI vs AI
                    _controller1 = new AiController();
                    _controller2 = new AiController();
                    break;
                default:
                    _controller1 = new HumanController();
                    _controller2 = new AiController();
                    break;
            }

            var token = _cts.Token;
            _ = GameLoop(token);
        }

        private async Task GameLoop(CancellationToken token)
        {
            while (!_model.GameOver)
            {
                if (token.IsCancellationRequested) return;

                var controller = _model.CurrentPlayer == 1 ? _controller1 : _controller2;
                if (controller == null) return;

                _activeHuman = controller as HumanController;

                try
                {
                    int col = await controller.GetColumn(_model);

                    if (token.IsCancellationRequested) return;

                    if (col >= 0 && col < GameModel.Cols)
                    {
                        int result = _model.Drop(col);
                        if (result < 0 && _activeHuman != null)
                        {
                            // Column was full, let human try again
                            continue;
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    return;
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }

            _activeHuman = null;
        }

        private void DropButton_Click(object sender, RoutedEventArgs e)
        {
            if (_activeHuman != null && sender is Button btn && btn.Tag is string tagStr)
            {
                if (int.TryParse(tagStr, out int col))
                {
                    _activeHuman.ColumnSelected(col);
                }
            }
        }

        private void Cell_Click(object sender, MouseButtonEventArgs e)
        {
            if (_activeHuman != null && sender is FrameworkElement fe && fe.DataContext is Cell cell)
            {
                _activeHuman.ColumnSelected(cell.Col);
            }
        }
    }
}
