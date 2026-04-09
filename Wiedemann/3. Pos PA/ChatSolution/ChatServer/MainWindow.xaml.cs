using ChatShared.Models;
using ChatShared.Network;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Xml.Serialization;

namespace ChatServer
{
    public partial class MainWindow : Window
    {
        private const string DELIMITER = "<END>";

        private TcpListener _listener;
        private List<TcpClient> _clients = new();
        private Dictionary<TcpClient, string> _usernames = new();
        private Dictionary<string, List<TcpClient>> _rooms = new();

        private Dictionary<string, string> _profileImages = new();

        public MainWindow()
        {
            InitializeComponent();

            using var db = new ChatDbContext();
            db.Database.EnsureCreated();

            Log("Server gestartet");

            StartServer();
        }

        private async void StartServer()
        {
            _listener = new TcpListener(IPAddress.Any, 5000);
            _listener.Start();

            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                _clients.Add(client);

                Log("Client verbunden");

                _ = HandleClient(client);
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            try
            {
                var stream = client.GetStream();
                byte[] buffer = new byte[8192];
                string bufferString = "";

                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    bufferString += Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    while (bufferString.Contains(DELIMITER))
                    {
                        int index = bufferString.IndexOf(DELIMITER);
                        string message = bufferString.Substring(0, index);
                        bufferString = bufferString.Substring(index + DELIMITER.Length);

                        NetworkMessage netMsg;

                        try
                        {
                            netMsg = FromXml<NetworkMessage>(message);
                        }
                        catch (Exception ex)
                        {
                            Log("XML ERROR: " + ex.Message);
                            continue;
                        }

                        try
                        {
                            switch (netMsg.Type)
                            {
                                // ================= LOGIN =================
                                case "login":
                                    {
                                        var login = FromXml<LoginPayload>(netMsg.Payload);

                                        using var db = new ChatDbContext();

                                        var user = db.Users.FirstOrDefault(u =>
                                            u.Username == login.Username &&
                                            u.Password == login.Password);

                                        if (user == null)
                                        {
                                            await SendToClient(client, new NetworkMessage
                                            {
                                                Type = "error",
                                                Payload = "Login fehlgeschlagen"
                                            });
                                            break;
                                        }

                                        _usernames[client] = user.Username;

                                        Log("Login: " + user.Username);

                                        await SendToClient(client, new NetworkMessage
                                        {
                                            Type = "login_success",
                                            Payload = user.Username
                                        });

                                        var rooms = db.ChatRoom.Select(r => r.Name).ToList();

                                        foreach (var room in rooms)
                                        {
                                            await SendToClient(client, new NetworkMessage
                                            {
                                                Type = "room_created",
                                                Payload = room
                                            });
                                        }
                                    }
                                    break;

                                // ================= CREATE ROOM =================
                                case "create_room":
                                    {
                                        string room = netMsg.Payload;

                                        using var db = new ChatDbContext();

                                        if (!db.ChatRoom.Any(r => r.Name == room))
                                        {
                                            db.ChatRoom.Add(new ChatRoom { Name = room });
                                            db.SaveChanges();

                                            Log("Raum erstellt: " + room);
                                        }

                                        if (!_rooms.ContainsKey(room))
                                            _rooms[room] = new List<TcpClient>();

                                        foreach (var c in _clients)
                                        {
                                            await SendToClient(c, new NetworkMessage
                                            {
                                                Type = "room_created",
                                                Payload = room
                                            });
                                        }
                                    }
                                    break;

                                // ================= JOIN ROOM =================
                                case "join_room":
                                    {

                                        using var db = new ChatDbContext();
                                        string room = netMsg.Payload;

                                        if (!_rooms.ContainsKey(room))
                                            _rooms[room] = new List<TcpClient>();

                                        // 🔥 ALLE alten Räume verlassen
                                        foreach (var r in _rooms.Values)
                                        {
                                            r.Remove(client);
                                        }

                                        _rooms[room].Add(client);

                                        string username = _usernames.ContainsKey(client)
                                            ? _usernames[client]
                                            : "UNKNOWN";

                                        Log($"{username} joined {room}");

                                        await SendToClient(client, new NetworkMessage
                                        {
                                            Type = "join_success",
                                            Payload = room
                                        });

                                        var cutoff = DateTimeOffset.UtcNow.AddDays(-2).ToUnixTimeSeconds();

                                        var messages = db.Messages
                                            .Where(m => m.ChatRoom == room && m.Time >= cutoff)
                                            .OrderBy(m => m.Time)
                                            .Take(100)
                                            .ToList();

                                        foreach (var m in messages)
                                        {
                                            await SendToClient(client, new NetworkMessage
                                            {
                                                Type = "room_message",
                                                Payload = $"{room} | {m.Sender}: {m.Content}"
                                            });
                                        }
                                    }
                                    break;

                                // ================= MESSAGE =================
                                case "room_message":
                                    {
                                        var payload = FromXml<ChatPayload>(netMsg.Payload);

                                        if (!_rooms.ContainsKey(payload.Room))
                                            break;

                                        using var db = new ChatDbContext();

                                        db.Messages.Add(new Message
                                        {
                                            Sender = payload.Sender,
                                            Content = payload.Text,
                                            Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                                            ChatRoom = payload.Room
                                        });

                                        Log($"DEBUG → Sender: {payload.Sender}");
                                        Log($"DEBUG → Content: {payload.Text}");
                                        Log($"DEBUG → Room: {payload.Room}");
                                        Log($"DEBUG → Time: {DateTime.UtcNow}");

                                        db.SaveChanges();
                                      
                                        Log($"MSG {payload.Room}: {payload.Text}");

                                        foreach (var c in _rooms[payload.Room])
                                        {
                                            await SendToClient(c, new NetworkMessage
                                            {
                                                Type = "room_message",
                                                Payload = $"{payload.Room} | {payload.Sender}: {payload.Text}"
                                            });
                                        }
                                    }
                                    break;
                                case "register":
                                    {
                                        var register = FromXml<RegisterPayload>(netMsg.Payload);

                                        using var db = new ChatDbContext();

                                        if (db.Users.Any(u => u.Username == register.Username))
                                        {
                                            await SendToClient(client, new NetworkMessage
                                            {
                                                Type = "error",
                                                Payload = "User existiert bereits"
                                            });
                                            break;
                                        }

                                        db.Users.Add(new User
                                        {
                                            Username = register.Username,
                                            Password = register.Password
                                        });

                                        db.SaveChanges();

                                        await SendToClient(client, new NetworkMessage
                                        {
                                            Type = "register_success",
                                            Payload = "Registrierung erfolgreich"
                                        });

                                        Log("User registriert: " + register.Username);
                                    }
                                    break;

                                case "profile_image":
                                    {
                                        var data = FromXml<ProfileImagePayload>(netMsg.Payload);

                                        _profileImages[data.Username] = data.ImageBase64;

                                        // 🔥 an alle Clients senden
                                        foreach (var c in _clients)
                                        {
                                            await SendToClient(c, new NetworkMessage
                                            {
                                                Type = "profile_image",
                                                Payload = netMsg.Payload
                                            });
                                        }
                                    }
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log("HANDLE ERROR: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("CLIENT CRASH: " + ex.Message);
            }

            _clients.Remove(client);
            _usernames.Remove(client);

            Log("Client getrennt");
        }

        private async Task SendToClient(TcpClient client, NetworkMessage msg)
        {
            var stream = client.GetStream();
            string xml = ToXml(msg) + DELIMITER;
            byte[] data = Encoding.UTF8.GetBytes(xml);
            await stream.WriteAsync(data, 0, data.Length);
        }

        private void Log(string text)
        {
            Dispatcher.Invoke(() => LogListBox.Items.Add(text));
        }

        private string ToXml<T>(T obj)
        {
            var serializer = new XmlSerializer(typeof(T));
            using var sw = new StringWriter();
            serializer.Serialize(sw, obj);
            return sw.ToString();
        }

        private T FromXml<T>(string xml)
        {
            var serializer = new XmlSerializer(typeof(T));
            using var sr = new StringReader(xml);
            return (T)serializer.Deserialize(sr);
        }
    }
}