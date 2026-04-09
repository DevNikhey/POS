using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Chat;

public class ChatClient
{
    private TcpClient? _tcp;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    public string? CurrentUser { get; private set; }
    public string? CurrentRoom { get; private set; }
    public bool IsConnected => _tcp?.Connected ?? false;

    public event Action<Envelope>? MessageReceived;
    public event Action<string>? Disconnected;

    public async Task ConnectAsync(string host, int port)
    {
        _tcp = new TcpClient();
        await _tcp.ConnectAsync(host, port);
        _reader = new StreamReader(_tcp.GetStream());
        _writer = new StreamWriter(_tcp.GetStream()) { AutoFlush = true };
        _ = ReceiveLoop();
    }

    public void Disconnect()
    {
        _tcp?.Close();
        _tcp = null;
    }

    private async Task ReceiveLoop()
    {
        try
        {
            string? line;
            while ((line = await _reader!.ReadLineAsync()) != null)
            {
                var env = Envelope.FromJson(line);
                if (env != null) MessageReceived?.Invoke(env);
            }
        }
        catch { }
        Disconnected?.Invoke("Connection lost");
    }

    public async Task SendAsync(Envelope env)
    {
        if (_writer != null)
            await _writer.WriteLineAsync(env.ToJson());
    }

    public async Task<bool> RegisterAsync(string user, string pass)
    {
        await SendAsync(new Envelope { Type = "register", User = user, Pass = pass });
        return true;
    }

    public async Task<bool> LoginAsync(string user, string pass)
    {
        CurrentUser = user;
        await SendAsync(new Envelope { Type = "login", User = user, Pass = pass });
        return true;
    }

    public async Task JoinRoomAsync(string room)
    {
        CurrentRoom = room;
        await SendAsync(new Envelope { Type = "join", Room = room });
    }

    public async Task SendMessageAsync(string text)
    {
        await SendAsync(new Envelope { Type = "msg", Text = text });
    }

    public async Task WhisperAsync(string to, string text)
    {
        await SendAsync(new Envelope { Type = "whisper", To = to, Text = text });
    }
}
