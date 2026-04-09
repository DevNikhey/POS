using simplChatShared.Models;
using simplChatShared.Network;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
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
using static System.Net.Mime.MediaTypeNames;

namespace simplChatClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient _client;
        private NetworkStream _stream;
        //private const string DELIMITER = "<END>";
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await ConnectToServer();
        }

        private async Task ConnectToServer()
        {
            _client = new TcpClient();
            await _client.ConnectAsync("127.0.0.1", 5000);

            _stream = _client.GetStream();
            StatusText.Text = "Verbunden!";

            _ = ReceiveLoop();
        }

        /*private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            _client = new TcpClient();
            await _client.ConnectAsync("127.0.0.1", 5000);

            _stream = _client.GetStream();
            StatusText.Text = "Verbunden!";

            _ = ReceiveLoop();
        }*/

        //Mit Delimeter
        /*private async Task ReceiveLoop()
        {
            byte[] buffer = new byte[8192];
            string bufferString = "";

            while (true)
            {
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                bufferString += Encoding.UTF8.GetString(buffer, 0, bytesRead);

                while (bufferString.Contains(DELIMITER))
                {
                    int index = bufferString.IndexOf(DELIMITER);
                    string message = bufferString.Substring(0, index);
                    bufferString = bufferString.Substring(index + DELIMITER.Length);

                    NetworkMessage msg;

                    try
                    {
                        msg = FromXml<NetworkMessage>(message);
                    }
                    catch
                    {
                        continue;
                    }

                    _ = Dispatcher.BeginInvoke(() =>
                    {
                        if(msg.Type == "message")
                        {
                            var txt = new TextBlock
                            {
                                Text = msg.Payload,
                                VerticalAlignment = VerticalAlignment.Center
                            };

                            Show.Text = txt.Text;
                        }
                        
                    });
                }
            }
        }*/

        //Ohne Delimeter
        private async Task ReceiveLoop()
        {
            try
            {
                while (true)
                {
                    // 🔥 1. Länge lesen (4 Bytes)
                    byte[] lengthBuffer = new byte[4];
                    await _stream.ReadAsync(lengthBuffer, 0, 4);
                    int length = BitConverter.ToInt32(lengthBuffer, 0);

                    // 🔥 2. Daten lesen
                    byte[] dataBuffer = new byte[length];
                    int totalRead = 0;

                    while (totalRead < length)
                    {
                        int read = await _stream.ReadAsync(dataBuffer, totalRead, length - totalRead);
                        totalRead += read;
                    }

                    //Hier beides gändert
                    string json = Encoding.UTF8.GetString(dataBuffer);
                    NetworkMessage msg = FromJson<NetworkMessage>(json);

                    Dispatcher.Invoke(() =>
                    {
                        if (msg.Type == "message")
                        {
                            Show.Text = msg.Payload;
                        }
                    });
                }
            }
            catch
            {
                StatusText.Text = "Verbindung verloren";
            }
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            await Send(new NetworkMessage
            {
                Type = "message",
                //Hier zu ToJson geändert
                Payload = ToJson(new Message
                {
                    Content = Nachrichtenbox.Text
                })
            });

            Nachrichtenbox.Clear();
        }

        //Senden mit Delimeter
        /*private async Task Send(NetworkMessage msg)
        {
            if (_stream == null) return;

            try
            {
                string xml = ToXml(msg) + DELIMITER;
                byte[] data = Encoding.UTF8.GetBytes(xml);

                await _stream.WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    StatusText.Text = "Send Fehler: " + ex.Message;
                });
            }
        }*/

        //Senden ohne Delimeter
        private async Task Send(NetworkMessage msg)
        {
            //Hier beides geändert
            string json = ToJson(msg);
            byte[] data = Encoding.UTF8.GetBytes(json);

            byte[] length = BitConverter.GetBytes(data.Length);

            await _stream.WriteAsync(length, 0, length.Length); // Länge
            await _stream.WriteAsync(data, 0, data.Length);     // Daten
        }

        private string ToJson<T>(T obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        private T FromJson<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}