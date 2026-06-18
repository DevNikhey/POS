using Network;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;

namespace UE1_Chat
{
    /// <summary>
    /// Client der Live-Umfrage.
    ///
    /// Aufgabe: mit dem Server verbinden (GENAU EIN Transfer&lt;MSG&gt;), beitreten,
    /// abstimmen und das per Broadcast empfangene Ergebnis anzeigen.
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient? _client;
        private Transfer<MSG>? _transfer;

        public MainWindow()
        {
            InitializeComponent();

            // Bonus (Aufgabe 6): Abstimmen erst nach dem Verbinden erlauben.
            voteGoodButton.IsEnabled = false;
            voteOkButton.IsEnabled = false;
            voteBadButton.IsEnabled = false;
        }

        /// <summary>
        /// Aufgabe 4: Verbindung aufbauen und beitreten (JOIN).
        /// </summary>
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            // Schon verbunden? Dann nichts tun (kein zweiter Transfer!).
            if (_transfer != null)
                return;

            try
            {
                // TCP-Verbindung zum Server aufbauen.
                _client = new TcpClient();
                _client.Connect("localhost", 12345);

                // GENAU EIN Transfer<MSG> fuer diese Verbindung. Der Transfer
                // empfaengt selbst im Hintergrund-Thread -> keine eigene Schleife.
                _transfer = new Transfer<MSG>(_client);
                _transfer.OnMessageReceived += OnMessageReceived;

                // Bonus (Aufgabe 6): Verbindungsabbruch behandeln.
                _transfer.OnDisconnected += OnDisconnected;

                // JOIN-Nachricht mit dem eingegebenen Benutzernamen senden.
                _transfer.Send(new MSG
                {
                    Type = MSG.MessageType.JOIN,
                    UserName = userNameBox.Text
                });

                statusText.Text = "Verbunden";

                // Jetzt darf abgestimmt werden.
                voteGoodButton.IsEnabled = true;
                voteOkButton.IsEnabled = true;
                voteBadButton.IsEnabled = true;
                connectButton.IsEnabled = false;
            }
            catch (System.Exception ex)
            {
                statusText.Text = "Verbindung fehlgeschlagen: " + ex.Message;
                _transfer = null;
                _client = null;
            }
        }

        /// <summary>
        /// Aufgabe 4: Eine Stimme abgeben (VOTE). Die Option steht im Tag des Buttons.
        /// </summary>
        private void VoteButton_Click(object sender, RoutedEventArgs e)
        {
            // Die gewaehlte Option steht im Tag des geklickten Buttons.
            string option = (string)((Button)sender).Tag;

            // Noch nicht verbunden? Dann nichts tun.
            if (_transfer == null)
                return;

            // VOTE-Nachricht mit UserName + gewaehlter Option an den Server senden.
            _transfer.Send(new MSG
            {
                Type = MSG.MessageType.VOTE,
                UserName = userNameBox.Text,
                Option = option
            });
        }

        /// <summary>
        /// Wird vom Empfangs-Thread des Transfers aufgerufen (NICHT GUI-Thread!).
        /// </summary>
        private void OnMessageReceived(object? sender, MSG msg)
        {
            // WICHTIG: jeder UI-Zugriff muss ueber den GUI-Thread laufen.
            Dispatcher.Invoke(() =>
            {
                if (msg.Type == MSG.MessageType.RESULT)
                {
                    resultListBox.ItemsSource = msg.Tally;
                }
            });
        }

        /// <summary>
        /// Bonus (Aufgabe 6): Die Verbindung wurde getrennt.
        /// Laeuft im Empfangs-Thread -> UI-Zugriff via Dispatcher.
        /// </summary>
        private void OnDisconnected(object? sender, System.EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                statusText.Text = "Verbindung getrennt";

                voteGoodButton.IsEnabled = false;
                voteOkButton.IsEnabled = false;
                voteBadButton.IsEnabled = false;
                connectButton.IsEnabled = true;

                _transfer = null;
                _client = null;
            });
        }
    }
}
