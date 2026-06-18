# PA3 – Client/Server „Babynames" – Erklärung & Ablauf

Diese Datei erklärt **vollständig**, wie die Anwendung aufgebaut ist, wie die
Client‑Server‑Verbindung funktioniert, wie Nachrichten übertragen werden und was
beim Suchen Schritt für Schritt passiert. Am Ende steht, **welche Fehler aus der
Bewertung korrigiert** wurden und **wie man das Programm startet**.

---

## 1. Überblick – die drei Projekte

Die Solution besteht aus drei Projekten:

| Projekt     | Typ                     | Aufgabe |
|-------------|-------------------------|---------|
| **Network** | Klassenbibliothek       | Enthält die generische Klasse `Transfer<T>` – die komplette Netzwerk‑Logik (Senden/Empfangen über TCP). |
| **Server**  | Konsolen­anwendung      | Lauscht auf Port **12345**, beantwortet Such‑ und Detailanfragen, greift über **linq2db** auf die SQLite‑Datenbank `Babynames.db` zu. |
| **Client**  | WPF‑Anwendung (Windows) | Grafische Oberfläche: Suchbegriff + Geschlecht eingeben, Ergebnisse und Details anzeigen. |

```
            ┌────────────────────────────┐                 ┌────────────────────────────┐
            │           CLIENT            │                 │           SERVER           │
            │         (WPF, GUI)          │                 │       (Konsole, DB)        │
            │                             │   TCP :12345    │                            │
            │   Transfer<MSG> ◄───────────┼─────────────────┼──────────► Transfer<MSG>   │
            │        │  ▲                  │   XML + Länge   │                  ▲  │       │
            │  Send  │  │ OnMessageReceived│                 │ OnMessageReceived│  │ Send  │
            │        ▼  │                  │                 │                  │  ▼       │
            │     GUI-Logik               │                 │   SEARCH / DETAIL Handler  │
            └────────────────────────────┘                 │            │               │
                                                            │            ▼               │
                                                            │      Babynames.db (SQLite) │
                                                            └────────────────────────────┘
```

Beide Seiten benutzen **dieselbe** Netzwerk‑Klasse `Transfer<MSG>` und dieselbe
Nachrichtenstruktur `MSG`. Dadurch „verstehen" sich Client und Server.

---

## 2. Die Nachricht: die Klasse `MSG`

Alles, was zwischen Client und Server geschickt wird, ist ein `MSG`‑Objekt
(siehe [Client/MSG.cs](PA3_4B_2026/Client/MSG.cs) bzw.
[Server/MSG.cs](PA3_4B_2026/Server/MSG.cs)):

```csharp
public class MSG
{
    public enum MessageType { SEARCH, SEARCHRESULT, DETAIL, DETAILRESULT }

    public String? Search { get; set; }                  // Suchbegriff bzw. ausgewählter Name
    public String? Sex { get; set; }                     // "F" oder "M"
    public List<String>? Names { get; set; }             // gefundene Namen (Ergebnis der Suche)
    public List<DataModels.Babyname>? Details { get; set; } // Detail-Datensätze (nur bei Detailabfrage!)
    public List<String>? AlternativeNames { get; set; }  // klangähnliche Namen (Soundex)

    public MessageType type { get; set; }                // Art der Nachricht
}
```

Über das Feld **`type`** weiß der Empfänger, **was** für eine Nachricht er bekommen
hat und wie er sie behandeln muss:

* `SEARCH` → Client fragt: „Suche Namen, die … enthalten."
* `SEARCHRESULT` → Server antwortet mit der **Liste der Namen** + **Alternativen**.
* `DETAIL` → Client fragt: „Gib mir alle Details zu **diesem einen** Namen."
* `DETAILRESULT` → Server antwortet mit den **Detail‑Datensätzen** (Jahr, Anzahl …).

> **Hinweis:** Es gibt absichtlich je eine `MSG`‑Klasse im Client‑ und im
> Server‑Projekt. Beide haben **exakt dieselben Felder und denselben Namen**,
> deshalb erzeugen sie **identisches XML** und sind über das Netz austauschbar
> (das XML‑Wurzelelement heißt in beiden Fällen `<MSG>`).

---

## 3. Verbindungsaufbau (TCP)

### Server: auf Verbindungen warten

[Server/Programm.cs](PA3_4B_2026/Server/Programm.cs)

```csharp
TcpListener listener = new TcpListener(IPAddress.Any, 12345);
listener.Start();

while (true)
{
    TcpClient tcp = listener.AcceptTcpClient();   // blockiert, bis sich ein Client verbindet
    HandleClient(tcp);                            // pro Verbindung EIN Transfer
}
```

* `TcpListener` lauscht auf Port **12345** auf allen Netzwerk­adressen (`IPAddress.Any`).
* `AcceptTcpClient()` **wartet (blockiert)**, bis sich ein Client verbindet, und
  liefert dann einen `TcpClient` für **genau diese eine Verbindung**.
* Für diese Verbindung wird **genau ein** `Transfer<MSG>` angelegt.

### Client: zum Server verbinden

[Client/MainWindow.xaml.cs](PA3_4B_2026/Client/MainWindow.xaml.cs)

```csharp
_client = new TcpClient();
_client.Connect("localhost", 12345);     // Verbindung zum Server aufbauen
_transfer = new Transfer<MSG>(_client);  // EIN Transfer für die ganze Sitzung
_transfer.OnMessageReceived += OnMessageReceived;
```

* `Connect("localhost", 12345)` stellt die TCP‑Verbindung zum Server her
  (`localhost` = derselbe Rechner; für einen anderen Rechner dessen IP eintragen).
* Danach wird **ein** `Transfer<MSG>` erzeugt, das ab jetzt die gesamte
  Kommunikation übernimmt.

---

## 4. Das Herzstück: `Transfer<T>` und das Übertragungs­protokoll

TCP ist ein **Byte‑Strom ohne Nachrichtengrenzen**. Das heißt: Wenn der Server
zwei Nachrichten schickt, kommen sie beim Client evtl. „zusammengeklebt" oder in
mehreren Stücken an. Der Empfänger muss also selbst wissen, **wo eine Nachricht
aufhört**. Dafür gibt es das **Längen‑Präfix‑Protokoll**.

### 4.1 Das Protokoll – „Länge zuerst, dann XML"

Vor jeder Nachricht werden **4 Bytes** gesendet, die die **Länge** der folgenden
XML‑Daten angeben:

```
 ┌─────────────┬───────────────────────────────────────────────┐
 │  4 Bytes    │   N Bytes                                       │
 │  Länge = N  │   XML-Nutzdaten (UTF-8)                         │
 │ (int32 LE)  │   z.B. <MSG><type>SEARCH</type>...</MSG>        │
 └─────────────┴───────────────────────────────────────────────┘
```

Der Empfänger liest **immer zuerst die 4 Längenbytes**, weiß dann **exakt**, wie
viele Bytes zur Nachricht gehören, liest genau diese Anzahl und hat damit **eine
vollständige Nachricht**.

### 4.2 Senden – `Transfer.Send()`

[Network/Transfer.cs](PA3_4B_2026/Network/Transfer.cs)

```csharp
public void Send(T data)
{
    // 1) Objekt -> XML-String
    StringWriter stringWriter = new StringWriter();
    _serializer.Serialize(stringWriter, data);

    // 2) XML-String -> Bytes
    byte[] payload = Encoding.UTF8.GetBytes(stringWriter.ToString());

    // 3) Länge als 4-Byte-Präfix
    byte[] lengthPrefix = BitConverter.GetBytes(payload.Length);

    lock (_stream)                 // atomar: Länge + Daten gehören zusammen
    {
        _stream.Write(lengthPrefix, 0, lengthPrefix.Length);  // zuerst die Länge
        _stream.Write(payload, 0, payload.Length);            // dann das XML
        _stream.Flush();
    }
}
```

Ablauf: **Objekt → XML‑Text → Bytes → [Länge][Daten] in den Stream**.
Das `lock` sorgt dafür, dass sich bei mehreren parallelen `Send`‑Aufrufen nicht
Länge und Daten verschiedener Nachrichten vermischen.

### 4.3 Empfangen – `Transfer.Receive()` (Hintergrund‑Thread)

```csharp
private void Receive()
{
    try
    {
        while (true)
        {
            // 1) 4-Byte-Länge lesen
            byte[] lengthBuffer = new byte[4];
            ReadExactly(lengthBuffer, 4);
            int length = BitConverter.ToInt32(lengthBuffer, 0);

            // 2) genau 'length' Bytes lesen
            byte[] dataBuffer = new byte[length];
            ReadExactly(dataBuffer, length);

            // 3) Bytes -> XML -> Objekt
            string xml = Encoding.UTF8.GetString(dataBuffer);
            using StringReader stringReader = new StringReader(xml);
            T dataObject = (T)_serializer.Deserialize(stringReader)!;

            OnMessageReceived?.Invoke(this, dataObject);   // Nachricht melden
        }
    }
    catch (Exception)
    {
        OnDisconnected?.Invoke(this, EventArgs.Empty);     // Verbindung weg
    }
}
```

Wichtig ist die Hilfsmethode **`ReadExactly`**: Ein einzelner `Read` liefert evtl.
**weniger** Bytes als gewünscht (weil TCP ein Strom ist). Deshalb wird in einer
Schleife so lange nachgelesen, bis wirklich alle Bytes da sind:

```csharp
private void ReadExactly(byte[] buffer, int count)
{
    int totalRead = 0;
    while (totalRead < count)
    {
        int read = _stream.Read(buffer, totalRead, count - totalRead);
        if (read == 0)
            throw new IOException("Verbindung wurde von der Gegenstelle geschlossen.");
        totalRead += read;
    }
}
```

### 4.4 Der Empfangs‑Thread und das Event

* Der Empfang läuft in einem **eigenen Hintergrund‑Thread**, der schon im
  Konstruktor von `Transfer` über `ThreadPool.QueueUserWorkItem(_ => Receive())`
  gestartet wird. Dadurch blockiert das Empfangen nicht die GUI bzw. den
  Hauptthread.
* Sobald eine **vollständige** Nachricht da ist, wird das Event
  **`OnMessageReceived`** ausgelöst. Wer eine Nachricht haben will, abonniert
  dieses Event – es gibt **keine zweite, eigene Empfangsschleife**.

---

## 5. Serialisierung (Objekt ⇄ XML)

Zum Umwandeln verwendet `Transfer` den `XmlSerializer`:

* **Senden:** `XmlSerializer.Serialize(...)` macht aus dem `MSG`‑Objekt einen
  XML‑Text, z. B.:

  ```xml
  <?xml version="1.0" encoding="utf-16"?>
  <MSG ...>
    <Search>anna</Search>
    <Sex>F</Sex>
    <type>SEARCH</type>
  </MSG>
  ```

* **Empfangen:** `XmlSerializer.Deserialize(...)` baut aus dem XML wieder ein
  `MSG`‑Objekt.

Weil Client und Server dieselben Feldnamen verwenden und der `enum`‑Wert als
**Name** (z. B. `SEARCH`) serialisiert wird, passt das XML auf beiden Seiten
zusammen.

---

## 6. Server‑Logik (Anfragen beantworten)

Pro Verbindung wird **ein** `Transfer` erzeugt und an `OnMessageReceived`
gehängt – nicht pro Nachricht neu!

```csharp
private void HandleClient(TcpClient client)
{
    Transfer<MSG> transfer = new Transfer<MSG>(client);          // EINMAL pro Verbindung
    transfer.OnMessageReceived += (s, netMsg) => HandleMessage(transfer, netMsg);
    transfer.OnDisconnected   += (s, e) => Console.WriteLine("Client disconnected");
}
```

`HandleMessage` schaut auf `type` und verzweigt:

### 6.1 SEARCH → SEARCHRESULT

```csharp
var matches = db.Babynames
    .Where(x => x.Sex.Equals(sex) && x.Name.Contains(name.ToLower()))  // Teilstring-Suche (LIKE)
    .ToList();

var alternatives = db.Babynames
    .Where(x => x.Sex.Equals(sex))
    .SoundexOf(x => x.Name).Matching(name)   // klangähnliche Namen
    .Distinct().ToList();

MSG response = new MSG
{
    type = MSG.MessageType.SEARCHRESULT,
    Names = matches.Select(x => x.Name).Distinct().ToList(),
    AlternativeNames = alternatives.Select(x => x.Name).Distinct().ToList()
    // ACHTUNG: KEINE Details hier – die kommen erst bei der Detailabfrage!
};
transfer.Send(response);
```

* **`Contains`** → SQL `LIKE '%anna%'` → findet **alle Namen, die den Suchbegriff
  enthalten**. (SQLite‑`LIKE` ist standardmäßig **groß-/kleinschreibungs­unabhängig**,
  deshalb findet `anna` auch `Anna`, `Hannah`, `Johanna` …)
* **Soundex** liefert „klingt ähnlich"-Namen als Alternativen.
* Die **`Details` werden hier bewusst NICHT gesetzt.**

### 6.2 DETAIL → DETAILRESULT

```csharp
var details = db.Babynames
    .Where(x => x.Sex.Equals(sex) && x.Name == name)   // exakter Name -> "=="
    .ToList();

MSG response = new MSG
{
    type = MSG.MessageType.DETAILRESULT,
    Details = details        // EINE Nachricht mit der kompletten Liste
};
transfer.Send(response);
```

* Hier wird der **ausgewählte Name exakt** verglichen (`==`), weil der Benutzer
  einen konkreten Namen aus der Ergebnisliste gewählt hat.
* Es werden **alle Datensätze** zu diesem Namen (z. B. über mehrere Jahre) in
  **einer einzigen Nachricht** zurückgeschickt – **nicht** eine Nachricht pro Zeile.

---

## 7. Client‑Logik (GUI)

### 7.1 Oberfläche – ein `Grid` (kein ListBox‑Container!)

[Client/MainWindow.xaml](PA3_4B_2026/Client/MainWindow.xaml) – die Steuerelemente
liegen direkt im `Grid`:

* `TextBox suche` – Suchbegriff
* `TextBox geschlecht` – „F" oder „M"
* `Button` „Suchen"
* `ComboBox resultBox` – gefundene Namen
* `ComboBox alternativeBox` – Alternativen
* `ListBox detailsListBox` – Details, dargestellt über ein **`DataTemplate`**:

```xml
<ListBox x:Name="detailsListBox">
    <ListBox.ItemTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal">
                <TextBlock Text="{Binding Name}"  FontWeight="Bold" Width="120"/>
                <TextBlock Text="{Binding Sex}"   Width="50"/>
                <TextBlock Text="Jahr: "/>   <TextBlock Text="{Binding Year}"  Width="60"/>
                <TextBlock Text="Anzahl: "/> <TextBlock Text="{Binding Count}"/>
            </StackPanel>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

Das `DataTemplate` legt fest, **wie ein einzelner `Babyname`** (Name/Sex/Jahr/Anzahl)
in der Liste dargestellt wird.

### 7.2 Suche starten (Button)

```csharp
private void Button_Click(object sender, RoutedEventArgs e)
{
    MSG msg = new MSG { type = MSG.MessageType.SEARCH, Search = suche.Text, Sex = geschlecht.Text };
    _transfer.Send(msg);
}
```

### 7.3 Antworten verarbeiten (`OnMessageReceived`)

```csharp
private void OnMessageReceived(object? sender, MSG msg)
{
    Dispatcher.Invoke(() =>                      // zurück auf den GUI-Thread!
    {
        if (msg.type == MSG.MessageType.SEARCHRESULT)
        {
            _suppressSelection = true;           // beim Befüllen keine Detailabfrage auslösen
            resultBox.ItemsSource = msg.Names;          // Liste als ItemsSource (NICHT Items.Add!)
            resultBox.SelectedIndex = -1;
            alternativeBox.ItemsSource = msg.AlternativeNames;
            alternativeBox.SelectedIndex = -1;
            detailsListBox.ItemsSource = null;          // alte Details löschen
            _suppressSelection = false;
        }
        else if (msg.type == MSG.MessageType.DETAILRESULT)
        {
            detailsListBox.ItemsSource = msg.Details;   // DataTemplate übernimmt die Anzeige
        }
    });
}
```

* **`Dispatcher.Invoke`** ist nötig, weil `OnMessageReceived` im
  **Empfangs‑Thread** läuft. Auf GUI‑Elemente darf aber nur der **GUI‑Thread**
  zugreifen.
* Die Namensliste wird als **`ItemsSource`** gesetzt (jeder Name wird ein eigener
  Eintrag) – **nicht** die ganze Liste als ein einziges Item via `Items.Add`.

### 7.4 Detailabfrage bei Auswahl (`SelectionChanged`)

```csharp
private void resultBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (_suppressSelection) return;              // programmatische Änderungen ignorieren
    if (resultBox.SelectedItem is string name)
    {
        MSG msg = new MSG { type = MSG.MessageType.DETAIL, Search = name, Sex = geschlecht.Text };
        _transfer.Send(msg);                     // Detailabfrage senden
    }
}
```

Sobald der Benutzer einen Namen auswählt, wird automatisch eine **DETAIL‑Anfrage**
an den Server geschickt. Das Flag `_suppressSelection` verhindert, dass das
**Befüllen** der Liste (neue Suchergebnisse) fälschlich eine Detailabfrage auslöst.

---

## 8. Kompletter Ablauf – „Was passiert beim Suchen?" (Sequenz)

```
  CLIENT (GUI)                         NETZ (TCP)                       SERVER (DB)
  ────────────                         ──────────                       ───────────
  1. "anna" + "F" eingeben,
     Button "Suchen" klicken
  2. Send( MSG{type=SEARCH,    ───►  [Länge][<MSG>…SEARCH…</MSG>]  ───►  3. OnMessageReceived
            Search="anna",                                                  -> HandleSearch
            Sex="F"} )                                                   4. DB: Name LIKE '%anna%'
                                                                            + Soundex-Alternativen
  6. OnMessageReceived          ◄──  [Länge][<MSG>…SEARCHRESULT…</MSG>] ◄─ 5. Send( SEARCHRESULT{
     -> resultBox.ItemsSource                                                   Names, AlternativeNames} )
        = Names
  7. Benutzer wählt "Anna"
  8. Send( MSG{type=DETAIL,     ───►  [Länge][<MSG>…DETAIL…</MSG>]   ───►  9. OnMessageReceived
            Search="Anna",                                                   -> HandleDetail
            Sex="F"} )                                                   10. DB: Name = 'Anna'
                                                                             (alle Jahre)
 12. OnMessageReceived          ◄──  [Länge][<MSG>…DETAILRESULT…</MSG>] ◄─ 11. Send( DETAILRESULT{
     -> detailsListBox.ItemsSource                                              Details=[…]} )
        = Details (DataTemplate)
```

---

## 9. Threading – wer läuft wo?

| Thread                        | Aufgabe |
|-------------------------------|---------|
| **GUI‑Thread (Client)**       | Reagiert auf Klicks, baut Nachrichten, `Send`. |
| **Empfangs‑Thread (`Transfer.Receive`)** | Liest dauernd vom Stream, deserialisiert, feuert `OnMessageReceived`. |
| **GUI‑Update**                | Im Handler muss mit **`Dispatcher.Invoke`** auf den GUI‑Thread zurückgewechselt werden. |
| **Server**                    | Hauptthread nimmt Verbindungen an (`AcceptTcpClient`); jede Verbindung hat ihren eigenen `Transfer`‑Empfangs‑Thread. |

---

## 10. Datenbank‑Zugriff (Server)

* Datenbank: **`Babynames.db`** (SQLite), Spalten `Name`, `Year`, `Sex`, `Count`.
* Zugriff über **linq2db**; das Datenmodell `Babyname` ist in
  [Server/DataModel.cs](PA3_4B_2026/Server/DataModel.cs) (per T4 generiert).
* `db.Babynames.Where(...)` wird **in SQL übersetzt** und auf der DB ausgeführt
  (kein Laden der ganzen Tabelle in den Speicher).
* **Soundex** (Paket `NinjaNye.SearchExtensions.Soundex`) liefert klangähnliche
  Namen als „Alternativen".

---

## 11. Was wurde gegenüber der Bewertung korrigiert?

| Aufgabe | Bemängelt | Korrektur |
|---------|-----------|-----------|
| **2** | „Transfer und eigene Empfangs­funktion parallel" | Es gibt **nur noch** den `Transfer`‑Empfang (`OnMessageReceived`). Die manuellen `ReceiveLoop`/Stream‑Leseschleifen in Client **und** Server wurden entfernt. |
| **2** | „Transfer sendet keine Länge vor dem XML" | `Transfer.Send` schickt jetzt **4 Byte Länge**, dann das XML. `Transfer.Receive` liest entsprechend Länge + Daten. |
| **2** | „Transfer im Server bei jeder Nachricht neu angelegt" | Pro Verbindung wird **genau ein** `Transfer` in `HandleClient` erzeugt. |
| **3** | „GUI entspricht nicht der Abbildung; Elemente in ListBox statt Grid" | Alle Steuerelemente liegen jetzt in einem **`Grid`**, nicht mehr als Items in einer ListBox. |
| **4** | „Details erst bei der Detailabfrage setzen" | `SEARCHRESULT` enthält **keine** `Details` mehr – nur `Names` + `AlternativeNames`. |
| **5** | „Namen als ItemsSource statt in Items einfügen" | `resultBox.ItemsSource = msg.Names` (statt `Items.Add(msg.Names)`). |
| **5** | „SelectionChanged fehlt" | `resultBox_SelectionChanged` hinzugefügt. |
| **5** | „Detailabfrage senden fehlt" | Bei Auswahl wird eine `DETAIL`‑Nachricht gesendet. |
| **5** | „== statt contains" | Suche verwendet **`Contains`** (Teilstring); nur die exakte Detailabfrage verwendet `==`. |
| **6** | „Details in ListBox inkorrekt; DataTemplate fehlt" | Details werden über `ItemsSource` an die ListBox gebunden und mit einem **`DataTemplate`** dargestellt. |
| **7** | „nur bei der Detailabfrage" | `Details` werden ausschließlich in `DETAILRESULT` gesetzt; die Antwort ist **eine** Nachricht mit der ganzen Liste (nicht pro Zeile). |

---

## 12. Starten & Testen

> WPF läuft nur unter **Windows** (Visual Studio). Auf macOS/Linux lässt sich das
> Client‑Projekt nicht ausführen.

1. **Server zuerst starten** (Projekt `Server` als Startprojekt) – es erscheint
   `Listening on port 12345. Waiting for clients...`.
2. **Client starten** (Projekt `Client`). Das Fenster „3. praktische Arbeit" öffnet sich.
3. In **Namenssuche** z. B. `anna` eingeben, in **Geschlecht** `F` oder `M`
   (genau so, wie in der DB: `F`/`M`), auf **Suchen** klicken.
4. In **Gefundene Namen** einen Namen auswählen → unten erscheinen die **Details**
   (Jahr, Anzahl …).

**Tipp für Visual Studio:** Solution rechtsklicken → *Configure Startup Projects* →
*Multiple startup projects* → `Server` und `Client` auf *Start* setzen, damit beide
gemeinsam starten (Server vor Client).
