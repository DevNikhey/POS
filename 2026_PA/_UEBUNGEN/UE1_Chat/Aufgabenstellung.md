# UE1 - Live-Umfrage (Client/Server, TCP)

## Szenario

Bei Vorträgen oder im Unterricht soll das Publikum live über eine Frage abstimmen
(z. B. "Wie verständlich war der Vortrag?" mit den Optionen *Sehr gut*, *Ok*, *Schlecht*).

Es gibt **einen Server** (Konsole) und **beliebig viele Clients** (WPF-App). Jeder Client
verbindet sich mit einem Benutzernamen. Sobald jemand abstimmt, schickt der Client seine
Stimme an den Server. Der Server zählt alle Stimmen pro Option und **verteilt das aktuelle
Ergebnis an ALLE verbundenen Clients (Broadcast)**. Jeder Client zeigt das Live-Ergebnis sofort an.

Es handelt sich also nicht um eine einfache Frage/Antwort wie bei einer Suche, sondern jeder
Client muss auch dann ein Update bekommen, wenn ein *anderer* Client abgestimmt hat.

## Vorgegeben

- **`Network/Transfer.cs`** (vom Toolkit, NICHT ändern): generische, bidirektionale
  Kommunikations-Hülle `Transfer<T>` über eine TCP-Verbindung.
  - Konstruktor `Transfer(TcpClient client)` - startet selbst einen Empfangs-Thread.
  - Event `EventHandler<T>? OnMessageReceived` - feuert pro vollständig empfangener Nachricht.
  - Event `EventHandler? OnDisconnected` - feuert, wenn die Gegenstelle die Verbindung schließt.
  - Methode `void Send(T data)` - serialisiert nach XML und sendet (Length-Prefix-Protokoll).
- **`Network/MSG.cs`** (gegeben): die Nachrichtenklasse mit `MessageType`-Enum und den nötigen
  Feldern (`UserName`, `Option`, `Tally`). Felder dürfen ergänzt, aber nicht entfernt werden.
- **`Server/Program.cs`**: Gerüst mit `TcpListener` und Accept-Loop. Die Stellen für
  Registrierung, Stimmenzählung und **Broadcast** sind als `// TODO` markiert.
- **`UE1_Chat/MainWindow.xaml`** + **`.xaml.cs`**: GUI-Shell (Benutzername, Verbinden-Button,
  Abstimm-Buttons, Ergebnis-Anzeige). Senden/Empfangen sind als `// TODO` markiert.

## Aufgaben

### Aufgabe 1 - Verbindungen verwalten (Server) (12 P)
- Implementiere im Server die Methode, die pro angenommener TCP-Verbindung **genau ein**
  `Transfer<MSG>` anlegt (NICHT pro Nachricht ein neues!).
- Verkable `OnMessageReceived` mit deiner Nachrichten-Verarbeitung und `OnDisconnected`.
- Halte alle aktiven `Transfer<MSG>`-Objekte in einer gemeinsamen Liste, damit später an alle
  gesendet werden kann. Entferne einen Transfer aus der Liste, sobald `OnDisconnected` feuert.
- Der Zugriff auf diese gemeinsame Liste erfolgt aus mehreren Threads -> sichere ihn ab (`lock`).

### Aufgabe 2 - Beitritt (JOIN) (8 P)
- Verarbeite eine `JOIN`-Nachricht (enthält den Benutzernamen). Gib am Server in der Konsole
  aus, wer beigetreten ist.
- Schicke dem neu verbundenen Client sofort den **aktuellen Stand** des Ergebnisses, damit er
  nicht bei leerer Anzeige startet.

### Aufgabe 3 - Abstimmen und Broadcast (Server) (18 P)
- Verarbeite eine `VOTE`-Nachricht: erhöhe den Zähler der gewählten Option.
- Verhindere, dass derselbe Benutzer doppelt zählt: Speichere pro Benutzername die zuletzt
  gewählte Option und korrigiere die Zähler, wenn jemand seine Stimme ändert (alte Option -1,
  neue Option +1).
- Baue eine `RESULT`-Nachricht mit der aktuellen Auszählung (`Tally`) und sende sie per
  **Broadcast an ALLE** verbundenen Clients (über die Liste aus Aufgabe 1).

### Aufgabe 4 - Client: Verbinden und Senden (10 P)
- Implementiere im `Connect`-Button: TCP-Verbindung zu `localhost:12345` aufbauen, **genau ein**
  `Transfer<MSG>` anlegen, `OnMessageReceived` abonnieren, eine `JOIN`-Nachricht mit dem
  eingegebenen Benutzernamen senden.
- Implementiere die Abstimm-Buttons: jeweils eine `VOTE`-Nachricht mit der gewählten Option senden.

### Aufgabe 5 - Client: Ergebnis anzeigen (10 P)
- Implementiere den `OnMessageReceived`-Handler: bei `RESULT` die Auszählung (`Tally`) in der
  Ergebnis-ListBox darstellen (Option + Anzahl).
- **Wichtig:** Der Handler läuft im Empfangs-Thread des Transfers, nicht im GUI-Thread -> alle
  UI-Zugriffe über `Dispatcher.Invoke(...)`.

### Aufgabe 6 (optional, Bonus) - Robustheit (5 P)
- Behandle `OnDisconnected` am Client (z. B. Statuszeile "Verbindung getrennt").
- Verhindere am Client das Abstimmen, solange noch nicht verbunden wurde (Buttons deaktiviert).

**Summe: ca. 58 Punkte (+5 Bonus)**

## Hinweise

- **Ein Transfer pro Verbindung.** Niemals pro Nachricht ein neues `Transfer<MSG>` anlegen und
  niemals eine eigene zweite Empfangsschleife schreiben - der Transfer empfängt selbst im
  Hintergrund-Thread.
- **Broadcast = an ALLE senden**, auch an den Absender. Iteriere über die gemeinsame Liste der
  Transfers und rufe bei jedem `Send(...)` auf.
- **Threading am Server:** Die Liste der Verbindungen und die Zähler werden aus mehreren
  Empfangs-Threads gleichzeitig berührt -> mit `lock` schützen. Beim Broadcast nicht über die
  Originalliste iterieren, während sie evtl. verändert wird (Kopie ziehen oder im `lock`).
- **Dispatcher-Falle (WPF):** GUI-Elemente dürfen NUR vom GUI-Thread verändert werden. Im
  `OnMessageReceived`-Handler des Clients alles in `Dispatcher.Invoke(() => { ... })` kapseln,
  sonst gibt es eine `InvalidOperationException`.
- **XML-Serialisierung:** `Transfer<T>` serialisiert `MSG` per `XmlSerializer`. Daher braucht
  `MSG` einen parameterlosen Konstruktor (ist da) und nur öffentliche, serialisierbare Felder/
  Properties. `Dictionary` ist NICHT XML-serialisierbar -> für `Tally` eine `List<VoteCount>`
  verwenden (siehe Vorgabe).
- Server zuerst starten, dann einen oder mehrere Clients.

## Setup

```bash
./_TOOLKIT/new-pa.sh UE1_Chat --wpf --console --network
```

Danach die Vorgabe-Dateien ins Projekt kopieren bzw. überschreiben:
- `Network/MSG.cs`
- `Server/Program.cs`
- `UE1_Chat/MainWindow.xaml`
- `UE1_Chat/MainWindow.xaml.cs`

`Network/Transfer.cs` wird vom Toolkit bereits angelegt und bleibt unverändert.
Beide Projekte (`UE1_Chat`, `Server`) referenzieren `Network` (richtet das Toolkit ein).
Build/Start des WPF-Projekts nur unter Windows.