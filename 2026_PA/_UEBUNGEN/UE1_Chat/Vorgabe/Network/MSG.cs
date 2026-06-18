namespace Network;

/// <summary>
/// Eine einzelne Auszählungs-Zeile: zu einer Option die aktuelle Stimmenzahl.
/// Bewusst eine eigene Klasse (statt Dictionary), weil ein Dictionary NICHT
/// XML-serialisierbar ist und Transfer&lt;T&gt; per XmlSerializer arbeitet.
/// </summary>
public class VoteCount
{
    public string? Option { get; set; }
    public int Count { get; set; }
}

/// <summary>
/// Die Netzwerk-Nachricht, die zwischen Client und Server hin- und hergeschickt
/// wird. EINE Klasse fuer ALLE Nachrichtenarten; welche Art es ist, steht in
/// <see cref="Type"/>. Je nach Art sind nur manche Felder befuellt.
///
/// VORGEGEBEN - du darfst Felder ergaenzen, aber nicht entfernen.
/// </summary>
public class MSG
{
    public enum MessageType
    {
        JOIN,    // Client -> Server: Benutzer tritt bei (UserName gesetzt)
        VOTE,    // Client -> Server: Stimme abgegeben (UserName + Option gesetzt)
        RESULT   // Server -> Client: aktuelle Auszaehlung (Tally gesetzt)
    }

    public MessageType Type { get; set; }

    // Bei JOIN und VOTE: der Benutzername des Clients.
    public string? UserName { get; set; }

    // Bei VOTE: die gewaehlte Option (z. B. "Sehr gut").
    public string? Option { get; set; }

    // Bei RESULT: die komplette Auszaehlung (Option + Anzahl) aller Optionen.
    public List<VoteCount>? Tally { get; set; }

    // Parameterloser Konstruktor wird vom XmlSerializer benoetigt.
    public MSG() { }
}
