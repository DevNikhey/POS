using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using PenteNetwork;

namespace PenteClient
{
    public partial class MainWindow : Window
    {
        private const int BoardSize = 19;
        private const double CellSize = 30.0;
        private const double BoardMargin = 15.0;
        private const double StoneRadius = 12.0;

        private TcpClient? _client;
        private NetworkStream? _stream;
        private int _myColor;
        private string _myName = "";
        private string _opponentName = "";
        private int _currentPlayer = 1;
        private int _moveCount;
        private int _whiteCaptures;
        private int _blackCaptures;
        private int[,] _board = new int[BoardSize, BoardSize];
        private Ellipse?[,] _stones = new Ellipse?[BoardSize, BoardSize];
        private Ellipse? _lastMoveMarker;
        private bool _gameOver;

        public MainWindow()
        {
            InitializeComponent();
            DrawBoard();
        }

        private void DrawBoard()
        {
            BoardCanvas.Children.Clear();

            // Draw grid lines
            for (int i = 0; i < BoardSize; i++)
            {
                double pos = BoardMargin +i * CellSize;

                // Vertical line
                Line vLine = new Line
                {
                    X1 = pos, Y1 = BoardMargin,
                    X2 = pos, Y2 = BoardMargin +(BoardSize - 1) * CellSize,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 0.8
                };
                BoardCanvas.Children.Add(vLine);

                // Horizontal line
                Line hLine = new Line
                {
                    X1 = BoardMargin, Y1 = pos,
                    X2 = BoardMargin +(BoardSize - 1) * CellSize, Y2 = pos,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 0.8
                };
                BoardCanvas.Children.Add(hLine);
            }

            // Draw star points (standard Go/Pente star points)
            int[] starPoints = new int[] { 3, 9, 15 };
            foreach (int r in starPoints)
            {
                foreach (int c in starPoints)
                {
                    Ellipse dot = new Ellipse
                    {
                        Width = 6, Height = 6,
                        Fill = Brushes.Black
                    };
                    Canvas.SetLeft(dot, BoardMargin +c * CellSize - 3);
                    Canvas.SetTop(dot, BoardMargin +r * CellSize - 3);
                    BoardCanvas.Children.Add(dot);
                }
            }
        }

        private void PlaceStoneOnBoard(int row, int col, int player)
        {
            _board[row, col] = player;

            Brush fill = player == 1 ? Brushes.White : Brushes.Black;
            Brush stroke = Brushes.Black;

            Ellipse stone = new Ellipse
            {
                Width = StoneRadius * 2,
                Height = StoneRadius * 2,
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = 1.5
            };

            Canvas.SetLeft(stone, BoardMargin +col * CellSize - StoneRadius);
            Canvas.SetTop(stone, BoardMargin +row * CellSize - StoneRadius);
            BoardCanvas.Children.Add(stone);
            _stones[row, col] = stone;

            // Update last move marker
            if (_lastMoveMarker != null)
                BoardCanvas.Children.Remove(_lastMoveMarker);

            _lastMoveMarker = new Ellipse
            {
                Width = 8, Height = 8,
                Fill = player == 1 ? Brushes.Black : Brushes.White,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(_lastMoveMarker, BoardMargin +col * CellSize - 4);
            Canvas.SetTop(_lastMoveMarker, BoardMargin +row * CellSize - 4);
            BoardCanvas.Children.Add(_lastMoveMarker);
        }

        private void RemoveStoneFromBoard(int row, int col)
        {
            _board[row, col] = 0;
            Ellipse? stone = _stones[row, col];
            if (stone != null)
            {
                BoardCanvas.Children.Remove(stone);
                _stones[row, col] = null;
            }
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameBox.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                StatusLabel.Text = "Bitte einen Namen eingeben.";
                return;
            }

            string server = ServerBox.Text.Trim();
            if (!int.TryParse(PortBox.Text.Trim(), out int port))
            {
                StatusLabel.Text = "Ungueltiger Port.";
                return;
            }

            _myName = name;
            ConnectButton.IsEnabled = false;
            StatusLabel.Text = "Verbinde...";

            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(server, port);
                _stream = _client.GetStream();

                // Send Hello
                await NetHelper.SendMessage(_stream, NetMessage.Create(MessageType.Hello,
                    new HelloData { PlayerName = name }));

                StatusLabel.Text = "Warte auf Gegner...";

                // Start reading messages
                _ = Task.Run(() => ReadMessagesLoop());
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Verbindung fehlgeschlagen: {ex.Message}";
                ConnectButton.IsEnabled = true;
            }
        }

        private async Task ReadMessagesLoop()
        {
            try
            {
                while (_stream != null)
                {
                    NetMessage? msg = await NetHelper.ReadMessage(_stream);
                    if (msg == null)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            StatusLabel.Text = "Verbindung zum Server verloren.";
                            if (!_gameOver)
                                MessageBox.Show("Verbindung zum Server verloren.", "Fehler",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                        });
                        break;
                    }

                    Dispatcher.Invoke(() => HandleMessage(msg));
                }
            }
            catch (Exception)
            {
                Dispatcher.Invoke(() =>
                {
                    if (!_gameOver)
                        StatusLabel.Text = "Verbindung verloren.";
                });
            }
        }

        private void HandleMessage(NetMessage msg)
        {
            switch (msg.Type)
            {
                case MessageType.GameStart:
                    HandleGameStart(msg);
                    break;
                case MessageType.StonePlaced:
                    HandleStonePlaced(msg);
                    break;
                case MessageType.Capture:
                    HandleCapture(msg);
                    break;
                case MessageType.GameOver:
                    HandleGameOver(msg);
                    break;
                case MessageType.Chat:
                    HandleChat(msg);
                    break;
                case MessageType.Error:
                    HandleError(msg);
                    break;
                case MessageType.ScoreboardResponse:
                    HandleScoreboard(msg);
                    break;
            }
        }

        private void HandleGameStart(NetMessage msg)
        {
            GameStartData? data = msg.GetData<GameStartData>();
            if (data == null) return;

            _myColor = data.YourColor;
            _opponentName = data.OpponentName;
            _currentPlayer = 1;
            _moveCount = 0;
            _whiteCaptures = 0;
            _blackCaptures = 0;
            _board = new int[BoardSize, BoardSize];
            _stones = new Ellipse?[BoardSize, BoardSize];
            _gameOver = false;

            ConnectionPanel.Visibility = Visibility.Collapsed;
            GamePanel.Visibility = Visibility.Visible;

            MyColorIndicator.Fill = _myColor == 1 ? Brushes.White : Brushes.Black;

            string whiteName = _myColor == 1 ? _myName : _opponentName;
            string blackName = _myColor == 2 ? _myName : _opponentName;

            Player1Label.Text = $"Weiss: {whiteName}";
            Player2Label.Text = $"Schwarz: {blackName}";

            DrawBoard();
            UpdateTurnLabel();
            UpdateCaptureLabels();
            MoveCountLabel.Text = "Zug: 0";
        }

        private void HandleStonePlaced(NetMessage msg)
        {
            StonePlacedData? data = msg.GetData<StonePlacedData>();
            if (data == null) return;

            PlaceStoneOnBoard(data.Row, data.Col, data.Player);
            _currentPlayer = data.Player == 1 ? 2 : 1;
            _moveCount++;

            MoveCountLabel.Text = $"Zug: {_moveCount}";
            UpdateTurnLabel();
        }

        private void HandleCapture(NetMessage msg)
        {
            CaptureData? data = msg.GetData<CaptureData>();
            if (data == null) return;

            RemoveStoneFromBoard(data.Row1, data.Col1);
            RemoveStoneFromBoard(data.Row2, data.Col2);

            if (data.CapturedBy == 1)
                _whiteCaptures += 2;
            else
                _blackCaptures += 2;

            UpdateCaptureLabels();
        }

        private void HandleGameOver(NetMessage msg)
        {
            GameOverData? data = msg.GetData<GameOverData>();
            if (data == null) return;

            _gameOver = true;

            string winnerColor = data.Winner == 1 ? "Weiss" : "Schwarz";
            string winnerName = data.Winner == _myColor ? _myName : _opponentName;
            string reason = data.Reason == "FiveInARow" ? "5 in einer Reihe" : "10 Gefangene";
            string youWon = data.Winner == _myColor ? "Du hast gewonnen!" : "Du hast verloren.";

            string message = $"{youWon}\n\n{winnerColor} ({winnerName}) gewinnt durch {reason}.";

            MessageBoxResult result = MessageBox.Show(
                message + "\n\nNeues Spiel starten?",
                "Spielende",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                // Reset to connection screen
                GamePanel.Visibility = Visibility.Collapsed;
                ConnectionPanel.Visibility = Visibility.Visible;
                ConnectButton.IsEnabled = true;
                StatusLabel.Text = "";

                try { _client?.Close(); } catch { }
                _client = null;
                _stream = null;
            }
        }

        private void HandleChat(NetMessage msg)
        {
            ChatData? data = msg.GetData<ChatData>();
            if (data == null) return;

            ChatList.Items.Add($"{data.PlayerName}: {data.Message}");
            ChatList.ScrollIntoView(ChatList.Items[ChatList.Items.Count - 1]);
        }

        private void HandleError(NetMessage msg)
        {
            ErrorData? data = msg.GetData<ErrorData>();
            if (data == null) return;

            ChatList.Items.Add($"[Fehler] {data.Message}");
        }

        private void HandleScoreboard(NetMessage msg)
        {
            ScoreboardData? data = msg.GetData<ScoreboardData>();
            if (data == null) return;

            Window scoreWindow = new Window
            {
                Title = "Rangliste",
                Width = 500,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            DataGrid grid = new DataGrid
            {
                AutoGenerateColumns = false,
                IsReadOnly = true,
                ItemsSource = data.Scores,
                Margin = new Thickness(10)
            };

            grid.Columns.Add(new DataGridTextColumn
            {
                Header = "Spieler",
                Binding = new System.Windows.Data.Binding("PlayerName"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            });
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = "Siege",
                Binding = new System.Windows.Data.Binding("Wins"),
                Width = new DataGridLength(80)
            });
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = "Niederlagen",
                Binding = new System.Windows.Data.Binding("Losses"),
                Width = new DataGridLength(100)
            });
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = "Gefangene",
                Binding = new System.Windows.Data.Binding("TotalCaptures"),
                Width = new DataGridLength(100)
            });

            scoreWindow.Content = grid;
            scoreWindow.ShowDialog();
        }

        private void UpdateTurnLabel()
        {
            if (_currentPlayer == _myColor)
                TurnLabel.Text = "Dein Zug";
            else
                TurnLabel.Text = "Gegner am Zug";
        }

        private void UpdateCaptureLabels()
        {
            WhiteCaptureLabel.Text = $"Weiss: {_whiteCaptures} Gefangene";
            BlackCaptureLabel.Text = $"Schwarz: {_blackCaptures} Gefangene";
        }

        private async void BoardCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_gameOver || _stream == null || _currentPlayer != _myColor)
                return;

            Point pos = e.GetPosition(BoardCanvas);

            // Find nearest intersection
            int col = (int)Math.Round((pos.X - BoardMargin) / CellSize);
            int row = (int)Math.Round((pos.Y - BoardMargin) / CellSize);

            if (row < 0 || row >= BoardSize || col < 0 || col >= BoardSize)
                return;

            if (_board[row, col] != 0)
                return;

            try
            {
                await NetHelper.SendMessage(_stream, NetMessage.Create(MessageType.PlaceStone,
                    new PlaceStoneData { Row = row, Col = col }));
            }
            catch (Exception ex)
            {
                ChatList.Items.Add($"[Fehler] Senden fehlgeschlagen: {ex.Message}");
            }
        }

        private async void ChatSendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendChat();
        }

        private async void ChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                await SendChat();
        }

        private async Task SendChat()
        {
            string text = ChatInput.Text.Trim();
            if (string.IsNullOrEmpty(text) || _stream == null)
                return;

            try
            {
                await NetHelper.SendMessage(_stream, NetMessage.Create(MessageType.Chat,
                    new ChatData { PlayerName = _myName, Message = text }));
                ChatInput.Text = "";
            }
            catch (Exception ex)
            {
                ChatList.Items.Add($"[Fehler] Chat senden fehlgeschlagen: {ex.Message}");
            }
        }

        private async void ScoreboardButton_Click(object sender, RoutedEventArgs e)
        {
            if (_stream == null) return;

            try
            {
                await NetHelper.SendMessage(_stream, NetMessage.Create(MessageType.ScoreboardRequest));
            }
            catch (Exception ex)
            {
                ChatList.Items.Add($"[Fehler] Rangliste anfordern fehlgeschlagen: {ex.Message}");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try { _client?.Close(); } catch { }
        }
    }
}
