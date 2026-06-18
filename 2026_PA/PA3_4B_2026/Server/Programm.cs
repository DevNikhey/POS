using DataModels;
using LinqToDB;
using Network;
using NinjaNye.SearchExtensions.Soundex;
using Server;
using System;
using System.Net;
using System.Net.Sockets;

internal class Programm
{
    internal Programm()
    {
        int port = 12345;
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Listening on port {port}. Waiting for clients...");

        while (true)
        {
            try
            {
                var tcp = listener.AcceptTcpClient();
                Console.WriteLine("Got a connection");

                // Pro Verbindung GENAU EIN Transfer-Objekt. Es bringt seinen
                // eigenen Empfangs-Thread mit und meldet jede Nachricht über
                // OnMessageReceived -> es gibt KEINE zweite Empfangsschleife und
                // der Transfer wird auch NICHT pro Nachricht neu angelegt.
                HandleClient(tcp);
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("Listener closed");
                break;
            }
        }
    }

    private void HandleClient(TcpClient client)
    {
        Transfer<MSG> transfer = new Transfer<MSG>(client);

        // Antworten gehen über DENSELBEN Transfer zurück.
        transfer.OnMessageReceived += (sender, netMsg) => HandleMessage(transfer, netMsg);
        transfer.OnDisconnected += (sender, e) => Console.WriteLine("Client disconnected");
    }

    private void HandleMessage(Transfer<MSG> transfer, MSG netMsg)
    {
        Console.WriteLine($"Message received: {netMsg.type}");

        switch (netMsg.type)
        {
            case MSG.MessageType.SEARCH:
                HandleSearch(transfer, netMsg);
                break;

            case MSG.MessageType.DETAIL:
                HandleDetail(transfer, netMsg);
                break;
        }
    }

    /// <summary>
    /// Suche: liefert die Liste der gefundenen Namen (Teilstring-Suche) und
    /// klangähnliche Alternativen (Soundex) zurück. Es werden HIER NOCH KEINE
    /// Details (Jahr/Anzahl) gesendet – die kommen erst bei der Detailabfrage.
    /// </summary>
    private void HandleSearch(Transfer<MSG> transfer, MSG netMsg)
    {
        var sex = netMsg.Sex;
        var name = netMsg.Search ?? "";
        Console.WriteLine($"Search: '{name}' / sex '{sex}'");

        BabynamesDB db = new BabynamesDB();

        // Teilstring-Suche -> Contains (NICHT ==)
        var matches = db.Babynames
            .Where(x => x.Sex.Equals(sex) && x.Name.Contains(name.ToLower()))
            .ToList();

        var alternatives = db.Babynames
            .Where(x => x.Sex.Equals(sex))
            .SoundexOf(x => x.Name).Matching(name)
            .Distinct()
            .ToList();

        MSG response = new MSG
        {
            type = MSG.MessageType.SEARCHRESULT,
            Names = matches.Where(x => x.Name != null).Select(x => x.Name).Distinct().ToList(),
            AlternativeNames = alternatives.Select(x => x.Name).Distinct().ToList()
            // Bewusst KEIN response.Details hier!
        };

        transfer.Send(response);
    }

    /// <summary>
    /// Detailabfrage für genau einen (zuvor ausgewählten) Namen: liefert alle
    /// Datensätze zu diesem Namen (z. B. über mehrere Jahre) in EINER Nachricht.
    /// </summary>
    private void HandleDetail(Transfer<MSG> transfer, MSG netMsg)
    {
        var sex = netMsg.Sex;
        var name = netMsg.Search;
        Console.WriteLine($"Detail: '{name}' / sex '{sex}'");

        if (name == null)
        {
            Console.WriteLine("Received name is null, ignoring detail request.");
            return;
        }

        BabynamesDB db = new BabynamesDB();

        // Detail eines konkret ausgewählten Namens -> exakter Vergleich (==).
        var details = db.Babynames
            .Where(x => x.Sex.Equals(sex) && x.Name == name)
            .ToList();

        MSG response = new MSG
        {
            type = MSG.MessageType.DETAILRESULT,
            Details = details   // EINE Nachricht mit der kompletten Liste
        };

        transfer.Send(response);
    }

    public static void Main(string[] args)
    {
        Programm programm = new();

        Console.ReadLine(); //Damit der Server nicht gleich wieder endet
    }
}
