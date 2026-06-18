# UE5 Parkhaus – Threading & Synchronisation

## Szenario

Du simulierst ein **Parkhaus** mit einer **begrenzten Anzahl an Stellplätzen**. Jedes Auto läuft in einem **eigenen Thread**: es fährt zum Parkhaus (zufällige Anfahrtszeit), möchte hinein, parkt eine Weile und fährt dann wieder weg. Wenn alle Plätze belegt sind, müssen weitere Autos **warten**, bis ein Platz frei wird.

Es gibt mehrere **Parkhaus-Varianten** mit unterschiedlichen Regeln. Alle erben von der abstrakten Klasse `Parkhaus` und implementieren `Einfahren(Auto a)`. Die GUI ist nur eine **Start-Shell**: ein ComboBox zur Auswahl der Variante, ein Eingabefeld für die Anzahl der Autos und ein Start-Button. Pro Auto wird ein Thread gestartet.

Deine Aufgabe ist die **korrekte Synchronisation** in den vier Parkhaus-Klassen. Die GUI, das `Auto`-Datenmodell, das Grundgerüst und die Dispatcher-Aufrufe sind bereits vorgegeben.

## Vorgegeben

- **`MainWindow.xaml` / `MainWindow.xaml.cs`** – fertige Start-Shell. Combobox mit den vier Varianten, Anzahl-Feld, Start-Button. Stellt die `ListBox`-Referenzen statisch zur Verfügung (`MainWindow.fahrendBox`, `MainWindow.parkBox`, `MainWindow.kassaBox`, `MainWindow.statusLabel`). Startet pro Auto einen `Thread`.
- **`Auto.cs`** – Datenmodell mit `INotifyPropertyChanged` (Properties `Id`, `IstElektro`, `Platz`, `Status`). Methode `Fahren()` simuliert die Anfahrt und ruft anschließend `ZielParkhaus.Einfahren(this)` auf.
- **`Parkhaus.cs`** – abstrakte Basisklasse mit `public abstract void Einfahren(Auto a)` und der **fertigen** Hilfsmethode `Parken(Auto a, ListBox box)` (zeigt das Auto über den Dispatcher in der GUI an, wartet eine Parkdauer und entfernt es wieder). **Diese Hilfsmethode musst du nutzen, nicht ändern.**
- Vier leere Parkhaus-Klassen mit `throw new NotImplementedException();` als Stub: `EinfachParkhaus`, `TagNachtParkhaus`, `LadeParkhaus`, `KassaParkhaus`.

## Aufgaben

### Aufgabe 1 – EinfachParkhaus (12 Punkte)
Implementiere `EinfachParkhaus` mit **`SemaphoreSlim`**.

- Das Parkhaus hat **genau `KAPAZITAET` (= 3) Stellplätze**. Es dürfen also nie mehr als 3 Autos gleichzeitig parken.
- In `Einfahren(Auto a)`: warte mit `Wait()` auf einen freien Platz, rufe dann `Parken(a, MainWindow.parkBox)` auf.
- **Wichtig:** Der Platz muss **in jedem Fall** wieder freigegeben werden (`Release()`), auch wenn beim Parken eine Exception fliegt → benutze **`try/finally`**.
- Setze `a.Status` sinnvoll (`"Wartet"`, beim Parken erledigt `Parken(...)` den Rest).

### Aufgabe 2 – TagNachtParkhaus (14 Punkte)
Das Parkhaus aus Aufgabe 1, aber zusätzlich mit **Öffnungszeiten**.

- Verwende zusätzlich zum `SemaphoreSlim` (Kapazität 3) ein **`ManualResetEventSlim`** als „Tor".
- Ein **eigener Hintergrund-Thread** (im Konstruktor gestartet) schaltet im Wechsel **Offen (5 s)** und **Geschlossen (5 s)** und aktualisiert `MainWindow.statusLabel` über den Dispatcher (`"Parkhaus: OFFEN"` / `"Parkhaus: GESCHLOSSEN"`).
- In `Einfahren(Auto a)`: Ein Auto darf **erst dann einen Platz belegen, wenn das Tor offen ist**. Überlege dir die **Reihenfolge** von „auf Tor warten" und „auf Platz warten" – ein wartendes Auto soll keinen Platz blockieren, während das Tor zu ist.
- `Release()` weiterhin im `finally`.

### Aufgabe 3 – LadeParkhaus (16 Punkte)
Ein Parkhaus mit **6 normalen Plätzen**, davon haben aber nur **2 eine Ladesäule**.

- Elektroautos (`a.IstElektro == true`) **brauchen zwingend einen Ladeplatz**. Verbrenner nehmen einen normalen Platz.
- Verwende ein **privates `readonly object` als Lock** und einen Zähler `freieLadeplaetze` (Start = 2) für die Ladeplätze sowie einen `SemaphoreSlim` (6) für die Gesamtkapazität.
- Ein Elektroauto, das einfahren will, aber gerade **keine freie Ladesäule** vorfindet, muss **warten, bis eine frei wird**. Setze das mit **`Monitor.Wait` in einer `while`-Schleife** um (Bedingung `while (freieLadeplaetze == 0)`), und wecke wartende Autos beim Verlassen mit **`Monitor.Pulse`/`PulseAll`**.
- Setze `a.Platz` auf eine sinnvolle Nummer und `a.Status` (z. B. `"Lädt"` vs. `"Parkt"`).
- Achte auch hier auf korrektes Freigeben (`SemaphoreSlim.Release()` **und** Ladeplatz zurückgeben) im `finally`.

> **Tipp:** `Monitor.Wait`/`Pulse` dürfen nur **innerhalb** eines `lock`-Blocks auf **demselben** Objekt aufgerufen werden. Die `while`-Schleife (kein `if`!) schützt vor *spurious wakeups* und davor, dass mehrere geweckte Threads denselben Platz nehmen.

### Aufgabe 4 – KassaParkhaus / Producer-Consumer (14 Punkte)
Beim Verlassen muss jedes Auto an der **Kassa** bezahlen. Es gibt **eine einzige Kassa** (ein Consumer-Thread), die Autos der Reihe nach abarbeitet.

- Autos parken wie im EinfachParkhaus (SemaphoreSlim, Kapazität 3). **Nach** dem Parken legen sie sich als **„Bezahl-Auftrag" in eine gemeinsame `Queue<Auto>`** (Producer).
- Ein **einziger Kassa-Thread** (Consumer, im Konstruktor gestartet) nimmt Autos aus der Queue und „kassiert" (kurzes `Thread.Sleep`), setzt `a.Status = "Bezahlt"` und zeigt das Auto kurz in `MainWindow.kassaBox` an.
- Synchronisiere Queue-Zugriff über ein **privates `readonly object`**. Der Consumer wartet mit **`Monitor.Wait`**, solange die Queue leer ist; der Producer weckt ihn mit **`Monitor.Pulse`**.
- **Achtung:** Kein *Busy-Waiting* (keine `while(true){ if(queue.Count>0)... }`-Schleife ohne `Wait`).

### Aufgabe 5 – Fragen zur Synchronisation (6 Punkte)
Beantworte als Kommentar am Ende von `Parkhaus.cs` kurz (je 1–2 Sätze):

1. Warum muss `Release()` im `finally` stehen und nicht einfach nach `Parken(...)`?
2. Warum `while` statt `if` rund um `Monitor.Wait`?
3. Was passiert, wenn man in `LadeParkhaus` den Zugriff auf `freieLadeplaetze` **ohne** `lock` macht (Race Condition – nenne ein konkretes Fehlszenario)?

**Summe: 62 Punkte**

## Hinweise

- **GUI nur über den Dispatcher:** Jeder Zugriff auf ein WPF-Control aus einem Auto-/Kassa-Thread **muss** über `Application.Current.Dispatcher.Invoke(...)` laufen. In `Parken(...)` ist das schon erledigt – wenn du selbst die GUI anfasst (z. B. `statusLabel`), denke daran.
- **Release im `finally`:** Wenn ein Platz nach einer Exception nicht freigegeben wird, „verschwindet" er dauerhaft → das Parkhaus läuft voll und blockiert für immer (Deadlock-ähnliches Verhalten).
- **Lock-Objekt:** Immer ein **privates `readonly object`** sperren (`private readonly object _lock = new();`), niemals `this`, einen String oder ein öffentliches Objekt.
- **`Monitor.Wait` / `Pulse`:** nur innerhalb `lock(sameObject)`. `Wait` gibt das Lock temporär frei. Immer in `while(!bedingung)` prüfen, nicht in `if`.
- **`SemaphoreSlim(KAPAZITAET, KAPAZITAET)`** für eine feste Kapazität anlegen; `Wait()` blockiert, bis ein Platz frei ist.
- **`ActualWidth/ActualHeight`** sind im Konstruktor noch 0 – hier nicht relevant, aber merke dir: größenabhängiges Zeichnen gehört ins `Loaded`-Event.
- Setze die `Status`-Property der `Auto`-Objekte fleißig (`"Anfahrt"`, `"Wartet"`, `"Parkt"`, `"Lädt"`, `"Bezahlt"`, `"Weg"`), damit du in der GUI siehst, ob die Synchronisation stimmt. Teste mit **mehr Autos als Plätzen** (z. B. 15 Autos bei 3 Plätzen)!

## Setup

```bash
./_TOOLKIT/new-pa.sh UE5_Parkhaus --wpf
```

Danach die **Vorgabe-Dateien ins Projekt kopieren** (überschreibt die von `dotnet new wpf` erzeugten `MainWindow.xaml`/`MainWindow.xaml.cs`) und die neuen Dateien (`Auto.cs`, `Parkhaus.cs`, `EinfachParkhaus.cs`, `TagNachtParkhaus.cs`, `LadeParkhaus.cs`, `KassaParkhaus.cs`) in den Ordner `UE5_Parkhaus/` legen. Das Skelett **kompiliert** sofort (die Stubs werfen `NotImplementedException`). Bauen/Starten geht nur unter **Windows**.

```bash
dotnet build      # nur unter Windows
```