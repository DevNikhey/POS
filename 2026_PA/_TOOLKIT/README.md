# PA-Toolkit – Vorsprung für die nächste Praktische Arbeit

Ein kleines Scaffolding-Toolkit für die typischen POS-PAs (C#/.NET, WPF, TCP, linq2db/SQLite, Parser).
Es legt dir per Befehl eine fertige Solution mit den passenden Projekten an und kopiert die
wiederverwendbaren Bausteine hinein – damit du in der PA sofort mit der eigentlichen Aufgabe loslegen kannst.

> **Voraussetzung:** Das **.NET SDK** muss installiert sein (`dotnet --version`).
> **WPF** lässt sich nur unter **Windows** bauen/starten. Auf macOS/Linux kannst du die Struktur und
> die Nicht-WPF-Module trotzdem erzeugen.

---

## Schnellstart – der Launcher

Am einfachsten **einen** Befehl starten, der **alles** per Pfeiltasten-Menü anbietet:
```bash
./_TOOLKIT/pa.sh        # macOS / Linux
```
```bat
_TOOLKIT\pa.bat         REM Windows (Doppelklick geht auch)
```
Pfeiltasten ↑/↓ + Enter → Kategorie wählen → Befehl wählen → Argumente eintippen (Enter = Beispiel) → läuft.

Oder die Befehle direkt aufrufen, z. B. neue Solution anlegen:
```bash
./_TOOLKIT/new-pa.sh MeinePA --wpf --mvvm --shapes      # macOS / Linux
```
```bat
_TOOLKIT\new-pa.bat MeinePA --wpf --mvvm --shapes
```

Danach:
```bash
cd MeinePA
dotnet build        # WPF nur unter Windows
```

---

## `new-pa` – Module

| Flag | Was es anlegt |
|------|----------------|
| `--wpf` | WPF-App-Projekt (Name = Projektname). Default, wenn kein Modul gewählt. |
| `--console` | Konsolen-Projekt `Server` (z. B. für einen TCP-Server). |
| `--network` | Klassenbibliothek `Network` mit der fertigen **`Transfer<T>`** (Length-Prefix + XML). Verweise auf App/Server werden automatisch gesetzt. |
| `--mvvm` | `ViewModelBase` (INotifyPropertyChanged), `RelayCommand` (ICommand), `BoolToVisibilityConverter` ins WPF-Projekt. |
| `--shapes` | `ShapeBase` (eigene Zeichen-Controls via `PathGeometry`) ins WPF-Projekt. |
| `--parser` | `Token.cs` + `Expression.cs` (Recursive-Descent-Skelett) ins Server- bzw. WPF-Projekt. |
| `--db` | NuGet `linq2db`, `linq2db.SQLite`, `Microsoft.Data.Sqlite` + `Db.cs`-Helfer. |
| `--out DIR` | Zielordner (Default: aktuelles Verzeichnis). |

**Presets** – ein Flag → fertig **verdrahtetes, lauffähiges** Skelett (kombiniert die Module + generiert den Code):

| Preset | Ergebnis |
|--------|----------|
| `--clientserver` | `Network`(Transfer)+`MSG` + **Broadcast-Server** + **Client** (Konsole), alles referenziert & verbunden — sofort startbar. |
| `--drawing` | WPF + `ShapeBase` + Beispiel-Shape `Stern` (PA2). |
| `--threading` | WPF + `ThreadingDemo` (lock / SemaphoreSlim / Monitor / Producer-Consumer als Referenz, PA1). |

```bash
./new-pa.sh Chat --clientserver      # Server-Projekt starten, dann mehrere Client-Projekte
./new-pa.sh Formen --drawing
./new-pa.sh Airport --threading
```

Module sind frei kombinierbar. Beispiele:

```bash
# Client/Server wie PA3 (Babynames):
./new-pa.sh Babynames --wpf --console --network --db

# Zeichnen wie PA2 (Spirale/Slice):
./new-pa.sh Formen --wpf --shapes

# Parser/Interpreter wie PA4_Robot:
./new-pa.sh Robot --wpf --parser

# WPF + Datenbank wie PA4_Bilder:
./new-pa.sh Fotos --wpf --db
```

---

## Was die Vorlagen können (und welche PA sie abdeckt)

| Vorlage | Thema / aus PA | Kerninhalt |
|---------|----------------|------------|
| `templates/network/Transfer.cs` | **PA3** Client/Server | `Transfer<T>`: ein Empfangs-Thread + Event, sendet **4-Byte-Länge + XML**. Pro Verbindung **ein** Objekt. |
| `templates/mvvm/ViewModelBase.cs` | WPF allgemein | `INotifyPropertyChanged` + `SetProperty(...)`. |
| `templates/mvvm/RelayCommand.cs` | WPF allgemein | `ICommand` für Button-Bindungen. |
| `templates/converters/BoolToVisibilityConverter.cs` | WPF allgemein | Beispiel-`IValueConverter` (≠ `TypeConverter`!). |
| `templates/shapes/ShapeBase.cs` | **PA2** Zeichnen | `Shape` + `DefiningGeometry` + DP-Muster. Mit Merkhilfen: LengthConverter nur für Längen, DP-Typ == Property-Typ, Segmente hinzufügen. |
| `templates/parser/Token.cs` + `Expression.cs` | **PA4_Robot** Parser | Tokenizer + Recursive-Descent-Skelett. Mit dem korrekten **ELSE-Prüf-Muster** und „gemeinsamer Zustand in die Basisklasse". |
| `templates/db/Db.cs` | **PA3/PA4** Datenbank | linq2db/SQLite-Schnellstart (Connection, Insert mit Identity, LIKE case-insensitive). |

Der Platzhalter `__NS__` in den Vorlagen wird beim Kopieren automatisch durch den Namespace des
Zielprojekts ersetzt (`Transfer.cs` nutzt fix `namespace Network`).

---

## Helfer **während** der PA

Diese Scripts laufen auf einem bestehenden Projekt – ideal, wenn die PA mit einem Lehrer-Skelett startet.

### `pa-check` – Fehler-Linter (der wichtigste!)
Durchsucht `.cs`/`.xaml` nach den typischen Bewertungsfehlern und gibt eine Checkliste mit Fundstellen aus.
Kurz vor der Abgabe drüberlaufen lassen!

```bash
./pa-check.sh .              # aktuelles Verzeichnis
./pa-check.sh ./MeinePA
```
```bat
pa-check.bat .
```
Geprüft wird u. a.: `StackPanel`/fixe Listen-Höhe *(PA4/A1)*, `ActualWidth/Height` im Code-Behind *(PA4/A4)*,
`Items.Add(` statt `ItemsSource` *(PA3/A5)*, `LengthConverter` auf Winkel/Anzahl *(PA2/A3,A6)*,
`CreatePathFigure` ohne `Segments.Add` *(PA2/A3)*, `Transfer` ohne Längen-Präfix *(PA3/A2)*,
`IF/ELSE`-Keyword-Check *(PA4/A5)*, absolute Pfade in T4. Es ist eine **Heuristik** – Treffer prüfen, nicht blind ändern.
*(Die `.bat` nutzt intern `pa-check.ps1`/PowerShell, weil `findstr` für diese Muster zu schwach ist.)*

### `lock-check` – Threading-Linter (PA1)
Schwester von `pa-check` für Nebenläufigkeit:
```bash
./lock-check.sh ./Server
```
Findet `lock(this)`/`lock(typeof)`/`lock("…")`, `new Thread(() => …)` (Lambda unnötig), `if(... .WaitOne())`
statt `while`, `Monitor.Wait` außerhalb einer `while`-Schleife, `Release()/Set()` (im `finally`?), und
WPF-Code-Behind mit Threading **ohne** `Dispatcher`. *(`.bat` → `lock-check.ps1`.)*

### Abgabe-Workflow – `pa-ready`, `find-todo`, `pa-submit`
Kurz vor der Abgabe, der Reihe nach:
```bash
./pa-ready.sh .                    # Sammel-Check: Build + pa-check + lock-check + find-todo + .sln-Check -> Ampel
./find-todo.sh .                   # NotImplementedException / TODO / Debug-Ausgaben / absolute Pfade
./pa-submit.sh . PA1_Hey_Niklas    # sauberes ZIP (ohne bin/obj/.vs/.git/*.user/.DS_Store)
```
`pa-ready` orchestriert die anderen Checks **und** `dotnet build` und sagt **BEREIT / NICHT BEREIT**
(rot u. a. bei Build-Fehler oder `NotImplementedException`). `pa-submit` zeigt, dass `.sln`/`.csproj`
im ZIP sind. *(Windows: `pa-ready.bat`, `find-todo.bat`, `pa-submit.bat`.)*

### `pa-snap` – Checkpoints (unabhängig von git)
Vor einem riskanten Umbau sichern, notfalls in Sekunden zurück:
```bash
./pa-snap.sh save works .          # aktuellen Stand als 'works' sichern (ohne bin/obj/.vs/.git)
./pa-snap.sh list                  # Snapshots auflisten
./pa-snap.sh restore works .       # 'works' wiederherstellen (vorheriger Stand wird auto-gesichert)
```
*(Windows: `pa-snap.bat`, via robocopy. Snapshots liegen im Toolkit-Ordner unter `.snapshots/`.)*

### `snip` – Schnipsel-Bibliothek (mit Zwischenablage)
```bash
./snip.sh                # Liste der Namen
./snip.sh dispatcher     # Snippet ausgeben + in die Zwischenablage kopieren
./snip.sh -i             # interaktiv per Pfeiltasten auswählen
```
Enthält die häufigsten Muster (Dispatcher, Loaded, ItemsSource, InsertWithInt64Identity, ArcSegment,
XmlSerializer, ELSE-Guard, OverrideMetadata, Semaphore-finally, DataTemplate). *(Windows: `snip.bat`.)*

### `add` – Baustein in ein bestehendes Projekt einfügen
```bash
./add.sh ./MeinePA/MeinePA mvvm converter      # in das Projekt im Ordner
./add.sh ./Server transfer
./add.sh ./MeinePA/MeinePA all                 # alles
```
```bat
add.bat .\MeinePA\MeinePA mvvm converter
```
Bausteine: `transfer | mvvm | converter | shapes | parser | db | all`. Der Namespace wird automatisch auf den
Projektnamen gesetzt; vorhandene Dateien werden nicht überschrieben. Bei `db` werden auch die NuGet-Pakete ergänzt.

### `db-inspect` / `db-query` – die vorgegebene DB schnell verstehen
```bash
./db-inspect.sh ./Fotos/photoworld.db                       # Tabellen + Schema + Beispieldaten
./db-query.sh   ./Babynames.db "SELECT * FROM Babynames LIMIT 5;"
```
```bat
db-inspect.bat .\photoworld.db
db-query.bat   .\Babynames.db "SELECT * FROM Babynames LIMIT 5;"
```
> Benötigt das `sqlite3`-Kommandozeilentool. macOS hat es vorinstalliert. Unter **Windows** ist es oft nicht dabei:
> `sqlite-tools` von sqlite.org herunterladen und `sqlite3.exe` in den PATH legen (oder in den Toolkit-Ordner).

## Code-Generatoren – `gen`

Erzeugt wiederkehrende Boilerplate (Namespace wird automatisch gesetzt; vorhandene Dateien bleiben unangetastet).

**Snippets zum Kopieren** (Ausgabe ins Terminal):
```bash
./gen.sh dp <Klasse> <Name> <Typ> [--render]    # DependencyProperty (Typ==Property-Typ, kein LengthConverter-Fallstrick)
./gen.sh prop <Name> <Typ>                       # INotifyPropertyChanged-Property (SetProperty)
./gen.sh cmd <Name>                              # RelayCommand-Property + Handler
./gen.sh datatemplate <Typ> [feld...] [optionen] # XAML-DataTemplate für Listen
```
`datatemplate`-Optionen:
- `-i` / `--interactive` — **Felder, Layout & Optionen per Pfeiltasten auswählen** (Leertaste = an/aus, Enter = fertig)
- `--from <Klasse.cs>` — Properties **direkt aus der Klasse lesen** (Typ wird auto-erkannt); ohne `--from` Feldnamen angeben
- `-v` / `--vertical` — untereinander (statt nebeneinander)
- `-g` / `--grid` — ausgerichtete Tabelle (`Label | Wert`)
- `-l` / `--labels` — `"Feld:"`-Beschriftung vor jedem Wert
- `-b` / `--border` — in einen Rahmen (`Border`) packen
- `--width <n>` — feste Breite der Wert-Spalte
- `--image <Feld>` — Feld als `<Image>` rendern (mehrfach möglich)
- `--checkbox <Feld>` — Feld als `<CheckBox>` rendern (mehrfach möglich)
- `--click <Handler>` — Klick-Event am Wurzelelement (`MouseLeftButtonDown`)

```bash
# Interaktiv aus einer Klasse zusammenklicken:
./gen.sh datatemplate --from Server/DataModel.cs -i

# Oder direkt: Tabelle aus Klasse, mit Bild-Spalte, Rahmen und Klick:
./gen.sh datatemplate --from Bild.cs --grid --image foto -b --click Row_Click
```
> Der interaktive Modus (`-i`) braucht ein echtes Terminal. Unter macOS funktioniert er auch mit der alten Bash 3.2.

**Client/Server & Threading generieren:**
```bash
# Multi-Client-Broadcast-Server + passender Client für eine Nachrichtenklasse auf einem Port:
./gen.sh server ./Chat/Chat --msg ChatMessage --port 5000
./gen.sh client ./Chat/Chat --msg ChatMessage --port 5000
# (braucht Transfer<T> via ./add.sh <proj> transfer und die Klasse: ./gen.sh model <proj> ChatMessage text:string)

# Threading-Primitive (PA1) mit korrekten Idiomen (Release im finally, while statt if):
./gen.sh sync semaphore Landebahn --count 3
./gen.sh sync monitor Puffer
./gen.sh sync prodcons Plane                    # Producer/Consumer (BlockingCollection)
```
Der Server nimmt **mehrere** Clients an und verteilt jede empfangene Nachricht per **Broadcast** an alle.
**Dateien im Projekt anlegen:**
```bash
./gen.sh window <projekt-ordner> <Name>          # WPF-Window (.xaml + .xaml.cs)
./gen.sh vm <projekt-ordner> <Name>              # ViewModel-Stub (braucht ViewModelBase)
./gen.sh converter <projekt-ordner> <Name>       # IValueConverter-Stub
./gen.sh shape <projekt-ordner> <Name>           # Zeichen-Control (braucht ShapeBase)
./gen.sh control <projekt-ordner> <Name>         # Templated Control + Generic.xaml-Snippet
./gen.sh model <projekt-ordner> <Klasse> [feld:typ...]   # POCO-Klasse (z.B. name:string jahr:int)
./gen.sh class <projekt-ordner> <Name>           # leere Klasse
./gen.sh enum <projekt-ordner> <Name> <Werte...> # Enum-Datei
./gen.sh grid <zeilen> <spalten>                 # Grid mit Row/Column-Definitionen (stdout)
./gen.sh handlers <projekt-ordner>               # fehlende Event-Handler-Stubs ins Code-Behind ergaenzen
```
**`gen handlers`** scannt alle `.xaml` nach `Click`/`SelectionChanged`/… und ergänzt **fehlende**
Code-Behind-Methoden mit dem richtigen `EventArgs` — ein vergessener Handler = `CS1061` = Build kaputt.

**`register`** trägt einen Converter in `App.xaml` ein und ergänzt `xmlns:local` (den Hand-Edit, den `gen converter` offen lässt):
```bash
./register.sh <projekt-ordner> converter <Name> [--window Datei.xaml]
```
```bat
gen.bat dp Spirale Umdrehung int
gen.bat model .\MeinePA\MeinePA Person name:string jahr:int
```
`dp`: DP-Typ == Property-Typ (verhindert den PA2-Bug); mit `--render` inkl. `AffectsRender | AffectsMeasure` fürs Zeichnen.
`prop`/`cmd` brauchen `ViewModelBase`/`RelayCommand` (via `./add.sh <proj> mvvm`). *(Die `.bat` nutzt intern `gen.ps1`/PowerShell.)*

## Client/Server & DB-Diagnose

```bash
./port-check.sh 12345                          # lauscht der Server auf dem Port?
./net-probe.sh localhost 12345 "PING"          # roher TCP-Test (OHNE Längen-Präfix)
./frame-probe.sh localhost 12345 --root MSG --field type=SEARCH --field Search=Anna   # ECHTES Transfer-Format!
./frame-probe.sh --listen 12345                # Echo-Server im Transfer-Format (Client testen, bevor der Server fertig ist)
./db-create.sh schema.sql meine.db             # neue SQLite-DB aus .sql-Schema
./cheatsheet.sh                                # Merkkarte der wichtigsten Muster ins Terminal
```
```bat
port-check.bat 12345
frame-probe.bat localhost 12345 --root MSG --field type=SEARCH --field Search=Anna
frame-probe.bat --listen 12345
db-create.bat schema.sql meine.db
```
> **`frame-probe`** spricht das echte `Transfer<T>`-Protokoll `[4-Byte-Länge LE][UTF-8-XML]` – `net-probe`
> sendet *ohne* Längen-Präfix und hängt deshalb im `ReadExactly` des Servers. `frame-probe.sh` nutzt Python3,
> `frame-probe.bat` nutzt PowerShell (`System.Net.Sockets`).
> `net-probe` braucht `nc`; `db-create` braucht `sqlite3`.

---

## `scaffold-db` – DB-Modell erzeugen (optional)

Erzeugt die linq2db-Entity-Klassen aus einer bestehenden SQLite-Datei – als Alternative zum
T4-Template (`.tt`) in Visual Studio.

```bash
./scaffold-db.sh <ziel-ordner> <pfad/zur.db> [Namespace]
# z. B.
./scaffold-db.sh ./Fotos/Fotos ./Fotos/Fotos/photoworld.db DataModels
```

Installiert bei Bedarf das globale Tool `linq2db.cli`. Wird `dotnet linq2db` danach nicht gefunden,
ein neues Terminal öffnen (PATH `~/.dotnet/tools`).

> **In Visual Studio (der übliche Weg im Unterricht):** Datenbank-`.db` ins Projekt, ein
> `*.tt`-Template (linq2db T4) hinzufügen, Connection/Provider eintragen, „Run Custom Tool" →
> `*.generated.cs` entsteht. Siehe `PA3_4B_2026` (`DataModel.tt`) und `PA4_Bilder` (`photoworld.tt`).

### `db-tt` – das `.tt`-Template fertig generieren (für den VS-Weg)

Schreibt ein korrektes linq2db-`.tt` mit **relativem** Pfad (kein absoluter `C:\Users\…`-Pfad – genau
der Fehler, der in PA4 Punkte gekostet hat) und gibt das passende `.csproj`-Snippet aus.

```bash
./db-tt.sh <ziel-ordner> <pfad/zur.db> [Namespace] [tt-name]
# z. B.
./db-tt.sh ./Fotos/Fotos ./Fotos/Fotos/photoworld.db DataModels
```
```bat
db-tt.bat .\Fotos\Fotos .\Fotos\Fotos\photoworld.db DataModels
```
Ergebnis: `DataModel.tt` mit `LoadSQLiteMetadata(@".", "photoworld.db")`. Dann in VS „Run Custom Tool".
Zum **sofort ausführen** ohne VS: `./scaffold-db.sh …` (nutzt `linq2db.cli`).

### `tt-fixpath` – kaputten `.tt`-Pfad reparieren
Wenn ein vorgegebenes/kopiertes `.tt` einen fremden absoluten Pfad enthält (`C:\Users\…`):
```bash
./tt-fixpath.sh ./Proj/DataModel.tt     # ersetzt den Pfad durch @"." (Backup: .tt.bak)
```

### `db-copycheck` – `PreserveNewest` prüfen/fixen
Fehlt das, öffnet die App die DB in `bin/` (leer) → **jede Query gibt still nichts zurück**:
```bash
./db-copycheck.sh ./Proj/Proj.csproj            # prüfen
./db-copycheck.sh ./Proj/Proj.csproj --fix      # fehlende <None Update PreserveNewest> einfügen
```

---

## Typische Stolpersteine (aus den Bewertungen)

- **Layout:** `Grid`/`DockPanel` statt `StackPanel`; keine fixen Pixel-Höhen für Listen. *(PA4)*
- **`ActualWidth`/`ActualHeight` sind im Konstruktor 0** → größenabhängiges Zeichnen ins `Loaded`-Event. *(PA4)*
- **`Transfer`:** Länge **vor** dem XML senden, **nur** den eingebauten Empfang nutzen, **ein** Transfer pro Verbindung. *(PA3)*
- **`ItemsSource` statt `Items.Add(liste)`**; `SelectionChanged` für Folge-Aktionen. *(PA3)*
- **`LengthConverter` nur für Längen**, nicht für Winkel/Anzahlen; DP-Typ == Property-Typ. *(PA2)*
- **Parser:** optionale Keywords (`ELSE`) nur bei exakter Übereinstimmung konsumieren. *(PA4)*

Details stehen in den `ERKLAERUNG_*.md` der jeweiligen PA.
