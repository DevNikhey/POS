using ChatShared.Models;
using ChatShared.Network;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace ChatClient
{
    public partial class MainWindow : Window
    {
        private const string DELIMITER = "<END>";

        private TcpClient _client;
        private NetworkStream _stream;
        private string _username;
        private string _currentRoom = null;
        private Dictionary<string, BitmapImage> _userImages = new();
        private BitmapImage ProfileImage;

        private readonly object _sendLock = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            _client = new TcpClient();
            await _client.ConnectAsync("127.0.0.1", 5000);

            _stream = _client.GetStream();
            StatusText.Text = "Verbunden!";

            _ = ReceiveLoop();
        }

        private async Task ReceiveLoop()
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
                        if (msg.Type == "room_message")
                        {
                            var parts = msg.Payload.Split('|');
                            if (parts.Length < 2) return;

                            string room = parts[0].Trim();
                            string text = parts[1].Trim();
                            string sender = text.Split(':')[0];

                            foreach (TabItem tab in ChatTabs.Items)
                            {
                                if (tab.Header.ToString() == room)
                                {
                                    var list = tab.Content as ListBox;
                                    var panel = new StackPanel { Orientation = Orientation.Horizontal };

                                    var img = new Image
                                    {
                                        Width = 50,
                                        Height = 50,
                                        Margin = new Thickness(5)
                                    };

                                    if (_userImages.ContainsKey(sender))
                                    {
                                        img.Source = _userImages[sender];
                                    }

                                    var txt = new TextBlock
                                    {
                                        Text = text,
                                        VerticalAlignment = VerticalAlignment.Center
                                    };

                                    panel.Children.Add(img);
                                    panel.Children.Add(txt);

                                    list.Items.Add(panel);
                                }
                            }
                        }
                        else if (msg.Type == "login_success")
                        {
                            LoginPanel.Visibility = Visibility.Collapsed;
                            ChatPanel.Visibility = Visibility.Visible;
                            StatusText.Text = "Eingeloggt als: " + msg.Payload;
                        }
                        else if (msg.Type == "room_created")
                        {
                            foreach (TabItem t in ChatTabs.Items)
                                if (t.Header.ToString() == msg.Payload)
                                    return;

                            ChatTabs.Items.Add(new TabItem
                            {
                                Header = msg.Payload,
                                Content = new ListBox()
                            });
                        }
                        else if (msg.Type == "join_success")
                        {
                            _currentRoom = msg.Payload;
                            StatusText.Text = "Beigetreten: " + msg.Payload;
                        }
                        else if (msg.Type == "profile_image")
                        {
                            var data = FromXml<ProfileImagePayload>(msg.Payload);

                            _userImages[data.Username] = Base64ToImage(data.ImageBase64);
                        }
                        else
                        {
                            LoginStatusText.Text = msg.Payload;
                        }
                    });
                }
            }
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            _username = UsernameBox.Text;

            await Send(new NetworkMessage
            {
                Type = "login",
                Payload = ToXml(new LoginPayload
                {
                    Username = UsernameBox.Text,
                    Password = PasswordBox.Password
                })
            });
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            await Send(new NetworkMessage
            {
                Type = "register",
                Payload = ToXml(new RegisterPayload
                {
                    Username = UsernameBox.Text,
                    Password = PasswordBox.Password
                })
            });
        }

        private async void CreateRoom_Click(object sender, RoutedEventArgs e)
        {
            await Send(new NetworkMessage
            {
                Type = "create_room",
                Payload = RoomNameBox.Text
            });
        }

        private async void JoinSelectedRoom_Click(object sender, RoutedEventArgs e)
        {
            if (ChatTabs.SelectedItem is not TabItem tab)
                return;

            string room = tab.Header.ToString();

            await Send(new NetworkMessage
            {
                Type = "join_room",
                Payload = room
            });
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            if (ChatTabs.SelectedItem is not TabItem tab)
                return;

            string room = tab.Header.ToString();

            if (_currentRoom != room)
            {
                StatusText.Text = "Du musst zuerst joinen!";
                return;
            }

            await Send(new NetworkMessage
            {
                Type = "room_message",
                Payload = ToXml(new ChatPayload
                {
                    Sender = _username,
                    Text = MessageBox.Text,
                    Room = room
                })
            });

            MessageBox.Clear();
        }

        private async Task Send(NetworkMessage msg)
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
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Images (*.png;*.jpg)|*.png;*.jpg";

            if (dialog.ShowDialog() == true)
            {
                var image = new BitmapImage(new Uri(dialog.FileName));

                ProfileImage = ResizeImage(image, 50, 50);

                SendProfileImage();
            }
        }

        private BitmapImage ResizeImage(BitmapImage image, int width, int height)
        {
            var group = new DrawingGroup();
            group.Children.Add(new ImageDrawing(image, new Rect(0, 0, width, height)));

            var drawingImage = new DrawingImage(group);

            var renderTarget = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);

            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                context.DrawImage(drawingImage, new Rect(0, 0, width, height));
            }

            renderTarget.Render(visual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTarget));

            using var ms = new MemoryStream();
            encoder.Save(ms);

            var result = new BitmapImage();
            result.BeginInit();
            result.StreamSource = new MemoryStream(ms.ToArray());
            result.CacheOption = BitmapCacheOption.OnLoad;
            result.EndInit();

            return result;
        }

        private string ImageToBase64(BitmapImage image)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using var ms = new MemoryStream();
            encoder.Save(ms);
            return Convert.ToBase64String(ms.ToArray());
        }

        private BitmapImage Base64ToImage(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);

            var image = new BitmapImage();
            using var ms = new MemoryStream(bytes);

            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = ms;
            image.EndInit();

            return image;
        }

        private async void SendProfileImage()
        {
            await Send(new NetworkMessage
            {
                Type = "profile_image",
                Payload = ToXml(new ProfileImagePayload
                {
                    Username = _username,
                    ImageBase64 = ImageToBase64(ProfileImage)
                })
            });
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