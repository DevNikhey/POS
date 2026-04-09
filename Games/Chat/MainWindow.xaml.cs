using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Chat;

public partial class MainWindow : Window
{
    private ChatServer? _server;
    private readonly ChatClient _client = new();
    private readonly ObservableCollection<string> _serverLog = new();
    private readonly ObservableCollection<string> _messages = new();

    public MainWindow()
    {
        InitializeComponent();
        lstServerLog.ItemsSource = _serverLog;
        lstMessages.ItemsSource = _messages;

        _client.MessageReceived += env =>
        {
            Dispatcher.Invoke(() =>
            {
                switch (env.Type)
                {
                    case "broadcast":
                        _messages.Add($"[{DateTime.Now:HH:mm:ss}] {env.User}: {env.Text}");
                        break;
                    case "whisper":
                        _messages.Add($"[{DateTime.Now:HH:mm:ss}] *whisper from {env.User}*: {env.Text}");
                        break;
                    case "ok":
                        _messages.Add($"[system] {env.Text}");
                        break;
                    case "err":
                        _messages.Add($"[error] {env.Text}");
                        break;
                }
                lstMessages.ScrollIntoView(lstMessages.Items[lstMessages.Items.Count - 1]);
            });
        };

        _client.Disconnected += msg =>
        {
            Dispatcher.Invoke(() =>
            {
                txtClientStatus.Text = "Disconnected";
                txtClientStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0x88, 0x88));
                _messages.Add($"[system] {msg}");
            });
        };
    }

    private async void StartServer_Click(object sender, RoutedEventArgs e)
    {
        if (_server != null) return;
        int port = int.Parse(txtPort.Text);
        _server = new ChatServer();
        _server.Log += msg => Dispatcher.Invoke(() =>
        {
            _serverLog.Add($"[{DateTime.Now:HH:mm:ss}] {msg}");
            lstServerLog.ScrollIntoView(lstServerLog.Items[lstServerLog.Items.Count - 1]);
        });
        btnStartServer.IsEnabled = false;
        btnStopServer.IsEnabled = true;
        _ = _server.StartAsync(port);
    }

    private void StopServer_Click(object sender, RoutedEventArgs e)
    {
        _server?.Stop();
        _server = null;
        btnStartServer.IsEnabled = true;
        btnStopServer.IsEnabled = false;
    }

    private async void Connect_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _client.ConnectAsync(txtHost.Text, int.Parse(txtClientPort.Text));
            txtClientStatus.Text = "Connected";
            txtClientStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x88, 0xFF, 0x88));
            _messages.Add("[system] Connected to server");
        }
        catch (Exception ex)
        {
            _messages.Add($"[error] {ex.Message}");
        }
    }

    private async void Register_Click(object sender, RoutedEventArgs e)
    {
        if (!_client.IsConnected) { _messages.Add("[error] Not connected"); return; }
        await _client.RegisterAsync(txtUser.Text, txtPass.Password);
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        if (!_client.IsConnected) { _messages.Add("[error] Not connected"); return; }
        await _client.LoginAsync(txtUser.Text, txtPass.Password);
    }

    private async void Join_Click(object sender, RoutedEventArgs e)
    {
        if (!_client.IsConnected) { _messages.Add("[error] Not connected"); return; }
        await _client.JoinRoomAsync(txtRoom.Text);
    }

    private async void Send_Click(object sender, RoutedEventArgs e) => await SendMessage();
    private async void Message_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) await SendMessage();
    }

    private async Task SendMessage()
    {
        if (!_client.IsConnected || string.IsNullOrWhiteSpace(txtMessage.Text)) return;
        string text = txtMessage.Text;
        txtMessage.Clear();

        if (text.StartsWith("/w "))
        {
            var parts = text.Split(' ', 3);
            if (parts.Length == 3)
                await _client.WhisperAsync(parts[1], parts[2]);
        }
        else
        {
            await _client.SendMessageAsync(text);
        }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        _client.Disconnect();
        _server?.Stop();
    }
}
