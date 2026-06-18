namespace Network;

// Beispiel-Nachricht fuer Client/Server. Auto-Properties + parameterloser ctor => XML-serialisierbar.
// Felder nach Bedarf anpassen/ergaenzen.
public class MSG
{
    public string? User { get; set; }
    public string? Text { get; set; }

    public override string ToString() => $"{User}: {Text}";
}
