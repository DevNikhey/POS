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
        // TODO (Aufgabe 1):
        //  - GENAU EIN Transfer<MSG> fuer diese Verbindung anlegen.
        //  - transfer.OnMessageReceived mit HandleMessage(transfer, msg) verbinden.
        //  - transfer.OnDisconnected behandeln: Transfer aus _clients entfernen
        //    (im lock!) und das aktualisierte Ergebnis broadcasten ist NICHT noetig,
        //    aber eine Konsolen-Ausgabe schon.
        //  - den Transfer (im lock) zu _clients hinzufuegen.
        throw new NotImplementedException("Aufgabe 1: Verbindung verwalten");
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
        // TODO (Aufgabe 2):
        //  - Konsolen-Ausgabe: wer ist beigetreten (msg.UserName).
        //  - dem NEUEN Client (nur diesem transfer) das aktuelle Ergebnis senden,
        //    damit er nicht leer startet. Nutze BuildResult().
        throw new NotImplementedException("Aufgabe 2: JOIN behandeln");
    }

    /// <summary>
    /// Aufgabe 3: Eine Stimme verarbeiten und das neue Ergebnis an ALLE senden.
    /// </summary>
    private void HandleVote(Transfer<MSG> transfer, MSG msg)
    {
        // TODO (Aufgabe 3):
        //  - Pruefen, ob msg.Option eine gueltige Option aus Options[] ist.
        //  - Doppelzaehlung verhindern: hat der Benutzer schon abgestimmt, alte
        //    Option -1 (in _counts), dann neue Option +1; sonst nur +1.
        //  - _lastVote[msg.UserName] aktualisieren.
        //  - Alles, was _counts/_lastVote anfasst, im lock(_sync).
        //  - danach Broadcast(BuildResult()) aufrufen.
        throw new NotImplementedException("Aufgabe 3: VOTE behandeln");
    }

    /// <summary>
    /// Baut aus _counts eine RESULT-Nachricht mit der kompletten Auszaehlung.
    /// </summary>
    private MSG BuildResult()
    {
        // TODO (Aufgabe 3): aus Options[] und _counts eine List<VoteCount> bauen
        // und in einer MSG mit Type = RESULT zurueckgeben.
        // (Lesezugriff auf _counts ebenfalls im lock absichern.)
        throw new NotImplementedException("Aufgabe 3: BuildResult");
    }

    /// <summary>
    /// Sendet die Nachricht an ALLE aktuell verbundenen Clients (Broadcast).
    /// </summary>
    private void Broadcast(MSG msg)
    {
        // TODO (Aufgabe 3):
        //  - ueber alle Transfers in _clients iterieren und Send(msg) aufrufen.
        //  - dabei _clients gegen gleichzeitige Aenderung schuetzen (lock oder Kopie).
        throw new NotImplementedException("Aufgabe 3: Broadcast");
    }

    public static void Main(string[] args)
    {
        new Program();
        Console.ReadLine(); // damit der Server nicht sofort beendet
    }
}
