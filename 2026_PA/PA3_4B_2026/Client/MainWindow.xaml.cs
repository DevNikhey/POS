using Network;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;

namespace Client
{
    public partial class MainWindow : Window
    {
        private readonly TcpClient _client;
        private readonly Transfer<MSG> _transfer;

        // Verhindert, dass das programmatische Befüllen der ComboBox (Setzen der
        // ItemsSource) eine Detailabfrage auslöst. Nur eine echte Benutzerauswahl
        // soll eine DETAIL-Nachricht senden.
        private bool _suppressSelection;

        public MainWindow()
        {
            InitializeComponent();

            // Verbindung zum Server aufbauen ...
            _client = new TcpClient();
            _client.Connect("localhost", 12345);

            // ... und GENAU EINEN Transfer dafür anlegen. Der Transfer empfängt
            // selbst im Hintergrund, deshalb gibt es KEINE eigene ReceiveLoop.
            _transfer = new Transfer<MSG>(_client);
            _transfer.OnMessageReceived += OnMessageReceived;
        }

        /// <summary>
        /// Wird vom Empfangs-Thread des Transfers aufgerufen. UI-Zugriffe müssen
        /// daher über den Dispatcher auf den GUI-Thread "umgeleitet" werden.
        /// </summary>
        private void OnMessageReceived(object? sender, MSG msg)
        {
            Dispatcher.Invoke(() =>
            {
                if (msg.type == MSG.MessageType.SEARCHRESULT)
                {
                    // Während des Befüllens kein Detail-Senden zulassen.
                    _suppressSelection = true;

                    // Listen als ItemsSource setzen (NICHT die ganze Liste als ein
                    // einzelnes Item per Items.Add hinzufügen).
                    resultBox.ItemsSource = msg.Names;
                    resultBox.SelectedIndex = -1;
                    alternativeBox.ItemsSource = msg.AlternativeNames;
                    alternativeBox.SelectedIndex = -1;

                    // Alte Details verwerfen, bis ein Name ausgewählt wird.
                    detailsListBox.ItemsSource = null;

                    _suppressSelection = false;
                }
                else if (msg.type == MSG.MessageType.DETAILRESULT)
                {
                    // Details über ItemsSource an die ListBox binden; die
                    // Darstellung übernimmt das DataTemplate aus dem XAML.
                    detailsListBox.ItemsSource = msg.Details;
                }
            });
        }

        /// <summary>
        /// Such-Button: schickt eine SEARCH-Nachricht an den Server.
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MSG msg = new MSG
            {
                type = MSG.MessageType.SEARCH,
                Search = suche.Text,
                Sex = geschlecht.Text
            };

            _transfer.Send(msg);
        }

        /// <summary>
        /// Wird ausgelöst, wenn der Benutzer einen gefundenen Namen auswählt:
        /// schickt eine DETAIL-Nachricht für genau diesen Namen an den Server.
        /// </summary>
        private void resultBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Programmatische Änderungen (neue Suchergebnisse) ignorieren.
            if (_suppressSelection)
                return;

            if (resultBox.SelectedItem is string name)
            {
                MSG msg = new MSG
                {
                    type = MSG.MessageType.DETAIL,
                    Search = name,
                    Sex = geschlecht.Text
                };

                _transfer.Send(msg);
            }
        }
    }
}
