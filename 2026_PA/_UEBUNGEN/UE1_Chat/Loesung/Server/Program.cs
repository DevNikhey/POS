using Network;
using System.Net;
using System.Net.Sockets;

namespace Server;

/// <summary>
/// Live-Umfrage-Server.
///
/// Aufgabe: mehrere Clients gleichzeitig annehmen (pro Verbindung GENAU EIN
/// Transfer&lt;MSG&gt;), Stimmen zaehlen und das Ergebnis an ALLE verbundenen
/// Clients verteilen (Broadcast).
/// </summary>
internal class Program
{
    // Die festen Antwort-Optionen der Umfrage.
    private static readonly string[] Options = { "Sehr gut", "Ok", "Schlecht" };

    // Alle aktiven Verbindungen. Achtung: wird aus mehreren Empfangs-Threads
    // benutzt -> Zugriff mit lock(_sync) absichern!
    private readonly List<Transfer<MSG>> _clients = new();

    // Aktuelle Stimmenzahl pro Option (Index passt zu Options[]).
    private readonly int[] _counts = new int[Options.Length];

    // Merkt sich pro Benutzername die zuletzt gewaehlte Option (fuer Stimm-Aenderung).
    private readonly Dictionary<string, string> _lastVote = new();

    // Gemeinsames Sperrobjekt fuer _clients, _counts und _lastVote.
    private readonly object _sync = new();

    internal Program()
    {
        int port = 12345;
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Live-Umfrage-Server laeuft auf Port {port}. Warte auf Clients...");

        while (true)
        {
            try
            {
                TcpClient tcp = listener.AcceptTcpClient();
                Console.WriteLine("Neue Verbindung angenommen.");
                HandleClient(tcp);
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("Listener geschlossen.");
                break;
            }
        }
    }

    /// <summary>
    /// Wird pro angenommener TCP-Verbindung EINMAL aufgerufen.
    /// </summary>
    private void HandleClient(TcpClient client)
    {
        // Aufgabe 1: GENAU EIN Transfer<MSG> fuer diese Verbindung.
        // Der Transfer hat einen eigenen Empfangs-Thread -> keine eigene Schleife.
        Transfer<MSG> transfer = new Transfer<MSG>(client);

        // Eingehende Nachrichten an die zentrale Verarbeitung weiterreichen.
        transfer.OnMessageReceived += (sender, msg) => HandleMessage(transfer, msg);

        // Bei Verbindungsabbruch den Transfer aus der gemeinsamen Liste entfernen.
        transfer.OnDisconnected += (sender, e) =>
        {
            lock (_sync)
            {
                _clients.Remove(transfer);
            }
            Console.WriteLine($"Verbindung getrennt. Aktive Clients: {_clients.Count}");
        };

        // Transfer in die gemeinsame Liste aufnehmen (Zugriff aus mehreren Threads!).
        lock (_sync)
        {
            _clients.Add(transfer);
        }
    }

    /// <summary>
    /// Verteilt die eingehenden Nachrichten je nach Typ.
    /// </summary>
    private void HandleMessage(Transfer<MSG> transfer, MSG msg)
    {
        switch (msg.Type)
        {
            case MSG.MessageType.JOIN:
                HandleJoin(transfer, msg);
                break;

            case MSG.MessageType.VOTE:
                HandleVote(transfer, msg);
                break;
        }
    }

    /// <summary>
    /// Aufgabe 2: Ein Benutzer tritt bei. Schicke ihm sofort das aktuelle Ergebnis.
    /// </summary>
    private void HandleJoin(Transfer<MSG> transfer, MSG msg)
    {
        Console.WriteLine($"Beigetreten: {msg.UserName}");

        // Dem NEUEN Client sofort den aktuellen Stand schicken, damit er nicht
        // bei leerer Anzeige startet.
        transfer.Send(BuildResult());
    }

    /// <summary>
    /// Aufgabe 3: Eine Stimme verarbeiten und das neue Ergebnis an ALLE senden.
    /// </summary>
    private void HandleVote(Transfer<MSG> transfer, MSG msg)
    {
        // Nur gueltige Optionen verarbeiten.
        int newIndex = Array.IndexOf(Options, msg.Option);
        if (newIndex < 0)
            return;

        string userName = msg.UserName ?? "";

        lock (_sync)
        {
            // Doppelzaehlung verhindern: hat der Benutzer schon abgestimmt,
            // dessen alte Option zuerst wieder abziehen.
            if (_lastVote.TryGetValue(userName, out string? oldOption))
            {
                int oldIndex = Array.IndexOf(Options, oldOption);
                if (oldIndex >= 0)
                    _counts[oldIndex]--;
            }

            // Neue Stimme zaehlen und merken.
            _counts[newIndex]++;
            _lastVote[userName] = msg.Option!;
        }

        // Aktualisiertes Ergebnis an ALLE verbundenen Clients verteilen.
        Broadcast(BuildResult());
    }

    /// <summary>
    /// Baut aus _counts eine RESULT-Nachricht mit der kompletten Auszaehlung.
    /// </summary>
    private MSG BuildResult()
    {
        List<VoteCount> tally = new List<VoteCount>();

        // Lesezugriff auf _counts ebenfalls absichern.
        lock (_sync)
        {
            for (int i = 0; i < Options.Length; i++)
            {
                tally.Add(new VoteCount { Option = Options[i], Count = _counts[i] });
            }
        }

        return new MSG
        {
            Type = MSG.MessageType.RESULT,
            Tally = tally
        };
    }

    /// <summary>
    /// Sendet die Nachricht an ALLE aktuell verbundenen Clients (Broadcast).
    /// </summary>
    private void Broadcast(MSG msg)
    {
        // Kopie unter lock ziehen, damit nicht ueber die Originalliste iteriert
        // wird, waehrend sie evtl. (durch Connect/Disconnect) veraendert wird.
        List<Transfer<MSG>> snapshot;
        lock (_sync)
        {
            snapshot = new List<Transfer<MSG>>(_clients);
        }

        foreach (Transfer<MSG> transfer in snapshot)
        {
            transfer.Send(msg);
        }
    }

    public static void Main(string[] args)
    {
        new Program();
        Console.ReadLine(); // damit der Server nicht sofort beendet
    }
}
