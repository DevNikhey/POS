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
        }

        /// <summary>
        /// Aufgabe 4: Verbindung aufbauen und beitreten (JOIN).
        /// </summary>
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO (Aufgabe 4):
            //  - TcpClient anlegen und zu "localhost", Port 12345 verbinden.
            //  - GENAU EIN Transfer<MSG> anlegen (kein zweiter, keine eigene Schleife).
            //  - _transfer.OnMessageReceived += OnMessageReceived;
            //  - eine JOIN-Nachricht mit dem eingegebenen Benutzernamen senden.
            //  - statusText auf "Verbunden" setzen.
            // (Bonus Aufgabe 6: OnDisconnected behandeln, Abstimm-Buttons erst hier aktivieren.)
            throw new System.NotImplementedException("Aufgabe 4: Verbinden");
        }

        /// <summary>
        /// Aufgabe 4: Eine Stimme abgeben (VOTE). Die Option steht im Tag des Buttons.
        /// </summary>
        private void VoteButton_Click(object sender, RoutedEventArgs e)
        {
            // Die gewaehlte Option steht im Tag des geklickten Buttons.
            string option = (string)((Button)sender).Tag;

            // TODO (Aufgabe 4):
            //  - falls noch nicht verbunden (_transfer == null): nichts tun.
            //  - eine VOTE-Nachricht mit UserName + option an den Server senden.
            throw new System.NotImplementedException("Aufgabe 4: Abstimmen");
        }

        /// <summary>
        /// Wird vom Empfangs-Thread des Transfers aufgerufen (NICHT GUI-Thread!).
        /// </summary>
        private void OnMessageReceived(object? sender, MSG msg)
        {
            // TODO (Aufgabe 5):
            //  - WICHTIG: jeder UI-Zugriff muss ueber Dispatcher.Invoke(...) laufen.
            //  - bei msg.Type == RESULT die Auszaehlung (msg.Tally) als ItemsSource
            //    der resultListBox setzen.
            throw new System.NotImplementedException("Aufgabe 5: Ergebnis anzeigen (Dispatcher!)");
        }
    }
}
