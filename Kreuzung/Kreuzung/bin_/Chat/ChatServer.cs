using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Chat;

public class ChatServer
{
    private TcpListener? _listener;
    private readonly List<ClientHandle> _clients = new();
    private readonly object _lock = new();
    private SqliteConnection _db = null!;

    public event Action<string>? Log;
    public int Port { get; private set; }

    public async Task StartAsync(int port)
    {
        Port = port;
        InitDb();
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        Log?.Invoke($"Server started on port {port}");

        while (true)
        {
            try
            {
                var tcp = await _listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(tcp);
            }
            catch (ObjectDisposedException)
            {
                break;
            }
        }
    }

    public void Stop()
    {
        _listener?.Stop();
        lock (_lock)
        {
            foreach (var c in _clients) c.Tcp.Close();
            _clients.Clear();
        }
        _db?.Close();
        Log?.Invoke("Server stopped");
    }

    private void InitDb()
    {
        _db = new SqliteConnection("Data Source=chat.db");
        _db.Open();
        using var cmd = _db.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS users (id INTEGER PRIMARY KEY, name TEXT UNIQUE, pwhash TEXT);
            CREATE TABLE IF NOT EXISTS rooms (id INTEGER PRIMARY KEY, name TEXT UNIQUE);
            CREATE TABLE IF NOT EXISTS messages (id INTEGER PRIMARY KEY, user_id INT, room_id INT, sent TEXT, text TEXT);
            INSERT OR IGNORE INTO rooms(name) VALUES('general');";
        cmd.ExecuteNonQuery();
    }

    private static string Hash(string pw)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(pw)));

    private async Task HandleClientAsync(TcpClient tcp)
    {
        var handle = new ClientHandle { Tcp = tcp };
        lock (_lock) _clients.Add(handle);
        Log?.Invoke($"Client connected: {tcp.Client.RemoteEndPoint}");

        try
        {
            var reader = new StreamReader(tcp.GetStream());
            var writer = new StreamWriter(tcp.GetStream()) { AutoFlush = true };
            string? line;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                var env = Envelope.FromJson(line);
                if (env == null) continue;

                string? response = null;
                switch (env.Type)
                {
                    case "register":
                        response = HandleRegister(env);
                        break;
                    case "login":
                        response = HandleLogin(env, handle);
                        break;
                    case "join":
                        response = HandleJoin(env, handle);
                        break;
                    case "msg":
                        HandleMessage(env, handle);
                        break;
                    case "whisper":
                        HandleWhisper(env, handle);
                        break;
                }

                if (response != null)
                    await writer.WriteLineAsync(response);
            }
        }
        catch (Exception ex)
        {
            Log?.Invoke($"Client error: {ex.Message}");
        }
        finally
        {
            lock (_lock) _clients.Remove(handle);
            if (handle.User != null)
                Log?.Invoke($"{handle.User} disconnected");
            tcp.Close();
        }
    }

    private string HandleRegister(Envelope env)
    {
        lock (_db)
        {
            using var cmd = _db.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM users WHERE name=$n";
            cmd.Parameters.AddWithValue("$n", env.User);
            if ((long)cmd.ExecuteScalar()! > 0)
                return new Envelope { Type = "err", Text = "Username taken" }.ToJson();

            cmd.CommandText = "INSERT INTO users(name,pwhash) VALUES($n,$p)";
            cmd.Parameters.AddWithValue("$p", Hash(env.Pass!));
            cmd.ExecuteNonQuery();
        }
        Log?.Invoke($"[register] {env.User}");
        return new Envelope { Type = "ok", Text = "Registered successfully" }.ToJson();
    }

    private string HandleLogin(Envelope env, ClientHandle handle)
    {
        lock (_db)
        {
            using var cmd = _db.CreateCommand();
            cmd.CommandText = "SELECT pwhash FROM users WHERE name=$n";
            cmd.Parameters.AddWithValue("$n", env.User);
            var stored = cmd.ExecuteScalar() as string;
            if (stored == null || stored != Hash(env.Pass!))
                return new Envelope { Type = "err", Text = "Bad credentials" }.ToJson();
        }
        handle.User = env.User;
        Log?.Invoke($"[login] {env.User}");
        return new Envelope { Type = "ok", Text = $"Welcome {env.User}" }.ToJson();
    }

    private string HandleJoin(Envelope env, ClientHandle handle)
    {
        lock (_db)
        {
            using var cmd = _db.CreateCommand();
            cmd.CommandText = "INSERT OR IGNORE INTO rooms(name) VALUES($r)";
            cmd.Parameters.AddWithValue("$r", env.Room);
            cmd.ExecuteNonQuery();
        }
        handle.Room = env.Room;
        Log?.Invoke($"[join] {handle.User} -> #{env.Room}");
        Broadcast(new Envelope { Type = "broadcast", Room = env.Room, User = "System", Text = $"{handle.User} joined #{env.Room}" }, null);
        return new Envelope { Type = "ok", Text = $"Joined #{env.Room}" }.ToJson();
    }

    private void HandleMessage(Envelope env, ClientHandle sender)
    {
        if (sender.User == null || sender.Room == null) return;

        lock (_db)
        {
            using var cmd = _db.CreateCommand();
            cmd.CommandText = @"INSERT INTO messages(user_id,room_id,sent,text)
                VALUES((SELECT id FROM users WHERE name=$u),(SELECT id FROM rooms WHERE name=$r),$t,$m)";
            cmd.Parameters.AddWithValue("$u", sender.User);
            cmd.Parameters.AddWithValue("$r", sender.Room);
            cmd.Parameters.AddWithValue("$t", DateTime.UtcNow.ToString("O"));
            cmd.Parameters.AddWithValue("$m", env.Text);
            cmd.ExecuteNonQuery();
        }

        Log?.Invoke($"[{sender.Room}] {sender.User}: {env.Text}");
        Broadcast(new Envelope { Type = "broadcast", Room = sender.Room, User = sender.User, Text = env.Text }, null);
    }

    private void HandleWhisper(Envelope env, ClientHandle sender)
    {
        if (sender.User == null) return;
        ClientHandle? target;
        lock (_lock)
            target = _clients.FirstOrDefault(c => c.User == env.To);

        if (target == null) return;

        var msg = new Envelope { Type = "whisper", User = sender.User, Text = env.Text }.ToJson();
        try
        {
            var writer = new StreamWriter(target.Tcp.GetStream()) { AutoFlush = true };
            writer.WriteLine(msg);
        }
        catch { }

        Log?.Invoke($"[whisper] {sender.User} -> {env.To}: {env.Text}");
    }

    private void Broadcast(Envelope env, ClientHandle? except)
    {
        var json = env.ToJson();
        lock (_lock)
        {
            foreach (var c in _clients.Where(c => c != except && c.Room == env.Room))
            {
                try
                {
                    var writer = new StreamWriter(c.Tcp.GetStream()) { AutoFlush = true };
                    writer.WriteLine(json);
                }
                catch { }
            }
        }
    }
}

public class ClientHandle
{
    public TcpClient Tcp { get; set; } = null!;
    public string? User { get; set; }
    public string? Room { get; set; }
}
