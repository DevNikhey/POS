using simplChatShared.Models;
using simplChatShared.Network;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace simplChatServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string DELIMITER = "<END>";

        private TcpListener _listener;
        private List<TcpClient> _clients = new();

        public MainWindow()
        {
            InitializeComponent();

            using var db = new AppDbContext();
            db.Database.EnsureCreated();
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

        //Handle Client mit Delimeter
        /*
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
                                case "message":
                                    var payload = FromXml<Message>(netMsg.Payload);

                                    foreach (var c in _clients)
                                    {
                                        await SendToClient(c, new NetworkMessage
                                        {
                                            Type = "message",
                                            Payload = $"{payload.Content}"
                                        });
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

            Log("Client getrennt");
        }
        */
        private async Task HandleClient(TcpClient client)
        {
            var stream = client.GetStream();
        
            while (true)
            {
                // 🔥 Länge lesen
                byte[] lengthBuffer = new byte[4];
                await stream.ReadAsync(lengthBuffer, 0, 4);
                int length = BitConverter.ToInt32(lengthBuffer, 0);

                // 🔥 Daten lesen
                byte[] dataBuffer = new byte[length];
                int totalRead = 0;

                while (totalRead < length)
                {
                    int read = await stream.ReadAsync(dataBuffer, totalRead, length - totalRead);
                    totalRead += read;
                }

                string xml = Encoding.UTF8.GetString(dataBuffer);

                NetworkMessage netMsg;

                try
                {
                    netMsg = FromXml<NetworkMessage>(xml);
                }
                catch
                {
                    continue;
                }

                switch (netMsg.Type)
                {
                    case "message":
                        {
                            var payload = FromXml<Message>(netMsg.Payload);

                            foreach (var c in _clients)
                            {
                                await SendToClient(c, new NetworkMessage
                                {
                                    Type = "message",
                                    Payload = payload.Content
                                });
                            }

                            using var db = new AppDbContext();

                            var neu = new User
                            {
                                Username = payload.Content,
                                Password = "1234",
                                Color = "Beispiel",
                                ProfileImageBase64 = "Beispiel",
                            };
                            db.Users.Add(neu);
                            db.SaveChanges();
                        }
                        break;
                }
            }
        }

        //Senden mit Delimeter
        /*
        private async Task SendToClient(TcpClient client, NetworkMessage msg)
        {
            var stream = client.GetStream();
            string xml = ToXml(msg) + DELIMITER;
            byte[] data = Encoding.UTF8.GetBytes(xml);
            await stream.WriteAsync(data, 0, data.Length);
        }*/

        //Senden ohne Delimeter
        private async Task SendToClient(TcpClient client, NetworkMessage msg)
        {
            var stream = client.GetStream();

            string xml = ToXml(msg);
            byte[] data = Encoding.UTF8.GetBytes(xml);

            byte[] length = BitConverter.GetBytes(data.Length);

            await stream.WriteAsync(length, 0, length.Length);
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