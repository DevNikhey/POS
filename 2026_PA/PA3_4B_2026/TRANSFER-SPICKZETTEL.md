# Transfer&lt;T&gt; – Spickzettel (Client/Server)

Kurzreferenz: die `Transfer<T>`-Klasse, wie eine Nachrichtenklasse (`MSG`) aussehen muss,
und wie man auf **Server-** und **Client-Seite** eine Verbindung aufbaut, empfängt und sendet.

---

## Die `Transfer<T>`-Klasse

```csharp
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;

namespace Network;

/// <summary>
/// Bidirektionale TCP-Hülle: sendet/empfängt Objekte vom Typ T als XML.
/// Protokoll = 4-Byte-Länge (int) vor den UTF-8-XML-Daten. Pro Verbindung GENAU EIN
/// Transfer; empfängt selbst in einem Hintergrund-Thread und meldet jede Nachricht
/// über <see cref="OnMessageReceived"/>.
/// </summary>
public class Transfer<T>
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly XmlSerializer _serializer = new XmlSerializer(typeof(T));

    /// <summary>Wird pro vollständig empfangener Nachricht ausgelöst – im Empfangs-Thread!</summary>
    public EventHandler<T>? OnMessageReceived;

    /// <summary>Wird ausgelöst, wenn die Gegenstelle die Verbindung schließt.</summary>
    public EventHandler? OnDisconnected;

    /// <summary>Übernimmt die offene Verbindung und startet den Empfangs-Thread.</summary>
    public Transfer(TcpClient client)
    {
        _client = client;
        _stream = _client.GetStream();
        ThreadPool.QueueUserWorkItem(_ => Receive());
    }

    /// <summary>Serialisiert <paramref name="data"/> nach XML und sendet Länge (4 Byte) + Daten.</summary>
    public void Send(T data)
    {
        StringWriter stringWriter = new StringWriter();
        _serializer.Serialize(stringWriter, data);
        byte[] payload = Encoding.UTF8.GetBytes(stringWriter.ToString());
        byte[] lengthPrefix = BitConverter.GetBytes(payload.Length);
        lock (_stream)
        {
            _stream.Write(lengthPrefix, 0, lengthPrefix.Length);
            _stream.Write(payload, 0, payload.Length);
            _stream.Flush();
        }
    }

    private void Receive()
    {
        try
        {
            while (true)
            {
                byte[] lengthBuffer = new byte[4];
                ReadExactly(lengthBuffer, 4);
                int length = BitConverter.ToInt32(lengthBuffer, 0);

                byte[] dataBuffer = new byte[length];
                ReadExactly(dataBuffer, length);

                string xml = Encoding.UTF8.GetString(dataBuffer);
                using StringReader stringReader = new StringReader(xml);
                T dataObject = (T)_serializer.Deserialize(stringReader)!;

                OnMessageReceived?.Invoke(this, dataObject);
            }
        }
        catch (Exception)
        {
            OnDisconnected?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Liest genau <paramref name="count"/> Bytes (TCP liefert evtl. in Stücken).</summary>
    private void ReadExactly(byte[] buffer, int count)
    {
        int totalRead = 0;
        while (totalRead < count)
        {
            int read = _stream.Read(buffer, totalRead, count - totalRead);
            if (read == 0) throw new IOException("Verbindung geschlossen.");
            totalRead += read;
        }
    }
}
```

---

## Wie eine Nachrichtenklasse (`MSG`) aussehen muss

`Transfer<T>` serialisiert mit dem **`XmlSerializer`**. Damit das funktioniert, muss die
Nachrichtenklasse folgende Regeln einhalten:

- **`public`** Klasse mit **parameterlosem Konstruktor** (der implizite reicht, solange du keinen
  anderen Konstruktor schreibst).
- Daten als **öffentliche Properties mit `get` UND `set`** (oder public Felder). Nur solche werden serialisiert.
- Nur **XML-serialisierbare Typen**: `int`, `double`, `bool`, `string`, `enum`, `List<…>` solcher Typen,
  und andere solche Klassen. **KEIN `Dictionary`**, keine Interfaces, keine Tupel.
- Üblich: ein **`enum` + ein `Type`-Feld**, damit der Empfänger weiß, *welche Art* Nachricht es ist
  (je nach Art sind nur manche Felder befüllt).
- **Beide Seiten** brauchen dieselbe Nachricht: am besten **eine** `MSG`-Klasse im gemeinsamen
  `Network`-Projekt. (Geht auch dupliziert pro Projekt – dann müssen Klassenname **und** alle
  Property-Namen exakt gleich sein, weil das XML-Wurzelelement = Klassenname ist.)

```csharp
namespace Network;   // am besten im gemeinsamen Network-Projekt

public class MSG
{
    // Welche Art Nachricht? (eigene Werte je nach Aufgabe)
    public enum MessageType { REQUEST, RESPONSE }
    public MessageType Type { get; set; }

    // Einzelwerte – nur die füllen, die zur jeweiligen Art passen:
    public string? Search { get; set; }
    public string? User { get; set; }

    // Listen sind ok:
    public List<string>? Names { get; set; }

    // KEIN Dictionary! Für Schlüssel/Wert eine eigene kleine Klasse + List<…>:
    // public List<Eintrag>? Tally { get; set; }

    public MSG() { }   // parameterloser Konstruktor für den XmlSerializer
}
```

> `T` in `Transfer<T>` ist diese Klasse, also überall **`Transfer<MSG>`**.

---

## Server-Seite

```csharp
// 1) VERBINDUNG AUFBAUEN: lauschen + pro Client GENAU EINEN Transfer anlegen
TcpListener listener = new TcpListener(IPAddress.Any, 12345);
listener.Start();
while (true)
{
    TcpClient tcp = listener.AcceptTcpClient();        // blockiert, bis ein Client kommt
    Transfer<MSG> transfer = new Transfer<MSG>(tcp);   // EIN Transfer pro Verbindung

    // 2) NACHRICHT EMPFANGEN
    transfer.OnMessageReceived += (sender, msg) =>
    {
        // ... msg auswerten (z. B. msg.Type) ...
        // 3) NACHRICHT SENDEN (Antwort über DENSELBEN Transfer)
        transfer.Send(antwort);
    };
}
```

Für **mehrere Clients gleichzeitig** alle `Transfer<MSG>` in einer Liste sammeln (Zugriff aus
mehreren Empfangs-Threads → mit `lock` absichern) und beim Senden über die Liste iterieren (Broadcast).

---

## Client-Seite (WPF)

```csharp
// 1) VERBINDUNG AUFBAUEN
TcpClient client = new TcpClient();
client.Connect("localhost", 12345);
Transfer<MSG> transfer = new Transfer<MSG>(client);

// 2) NACHRICHT EMPFANGEN – läuft im Empfangs-Thread, GUI daher NUR über Dispatcher!
transfer.OnMessageReceived += (sender, msg) =>
{
    Dispatcher.Invoke(() =>
    {
        // ... GUI mit msg aktualisieren ...
    });
};

// 3) NACHRICHT SENDEN (z. B. im Button-Click)
transfer.Send(new MSG { Type = MSG.MessageType.REQUEST, Search = suche.Text });
```

---

## Merksätze

- Je Verbindung **ein** `Transfer` – **nie** pro Nachricht ein neues anlegen.
- Empfang **nur** über `OnMessageReceived` – **keine** zweite/eigene Leseschleife.
- Im Client jeden GUI-Zugriff im Handler in **`Dispatcher.Invoke(...)`** kapseln.
- `MSG`: public, parameterloser Konstruktor, public Properties, **kein `Dictionary`**.
