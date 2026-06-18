# UE4 Medien - Filmsammlung

Du baust eine WPF-Anwendung, die eine **Filmsammlung** aus einer SQLite-Datenbank
(`movies.db`) verwaltet. Links eine Liste aller Filme, rechts die Detailansicht des
ausgewaehlten Films inklusive einer **grafischen Bewertungsleiste** (0-10 Punkte).
Ueber einen Button kann ein neuer Film hinzugefuegt und in die Datenbank
eingetragen werden.

Die Tabelle `films` hat die Spalten `id` (Primaerschluessel, Identity), `title`,
`director`, `year` und `rating` (0-10).

## Vorgegeben

Im Skelett ist bereits enthalten / vorbereitet:

- `schema.sql` - das SQL-Skript zum Anlegen der Tabelle `films` inkl. 7 Beispiel-Datensaetzen.
  Daraus erzeugst du `movies.db` (siehe **Setup**).
- `UE4_Medien/MainWindow.xaml` - die GUI-Shell: zweispaltiges `Grid`, links eine
  `ListBox` (`x:Name="filmListe"`) mit einem leeren `DataTemplate` (`FilmTemplate`),
  rechts ein Detailbereich mit `TextBlock`s und einem leeren `Canvas`
  (`x:Name="ratingCanvas"`) fuer die Bewertungsleiste sowie ein Button "Neuer Film".
- `UE4_Medien/MainWindow.xaml.cs` - das Code-Behind mit dem ViewModel `Film`,
  der Methode `LoadFilms()`, dem `SelectionChanged`-Handler, dem `Button_Click`-Handler
  und `DrawRatingBar(...)` - alles **gestubbt** mit `// TODO` bzw.
  `throw new NotImplementedException();`.
- Hinweise zum **Datenmodell**: das echte Modell (`MoviesDB` + Entity `Film`) wird
  NICHT von Hand geschrieben, sondern mit linq2db aus der `.db`-Datei generiert
  (siehe **Setup**). Solange das Modell noch nicht generiert ist, kompiliert das
  Projekt trotzdem, weil im Code-Behind nur ein lokales ViewModel `Film` verwendet
  wird und die DB-Zugriffe gestubbt sind.

> ACHTUNG Namenskollision: Das **generierte** Entity heisst ebenfalls `Film`
> (aus Tabelle `films`) und liegt im Namespace `DataModels`. Das **ViewModel** im
> Code-Behind heisst auch `Film`. Sprich das Entity beim DB-Zugriff voll qualifiziert
> als `DataModels.Film` an, um die beiden auseinanderzuhalten.

## Aufgaben

### Aufgabe 1 - Datenmodell erzeugen & DB-Verbindung (8 Punkte)

- Erzeuge aus `movies.db` das linq2db-Modell (`scaffold-db`, siehe **Setup**).
  Es entsteht eine `DataConnection`-Klasse `MoviesDB` und ein Entity `DataModels.Film`
  mit `Id` (`long?`), `Title`, `Director`, `Year` (`long?`), `Rating` (`long?`).
- Stelle sicher, dass `movies.db` in der `.csproj` auf
  `CopyToOutputDirectory=PreserveNewest` steht (sonst findet die App die DB zur
  Laufzeit nicht).
- Implementiere in `LoadFilms()` den Verbindungsaufbau:
  `new DataOptions().UseSQLite("Data Source=movies.db")` und
  `using var db = new MoviesDB(options);`.

### Aufgabe 2 - Filme laden & in der ListBox anzeigen (12 Punkte)

- Lies in `LoadFilms()` alle Filme mit `db.Films.ToList()`.
- Baue fuer jede DB-Zeile ein ViewModel `Film` und fuelle **alle** Felder
  (`Id`, `Title`, `Director`, `Year`, `Rating`). Vergiss `Id` nicht - sonst zeigt
  die Detailansicht spaeter die falschen Daten an.
- Setze die Liste als `filmListe.ItemsSource`.
- Ergaenze das `DataTemplate` `FilmTemplate` in `MainWindow.xaml` so, dass pro
  Eintrag mindestens **Titel** und **Jahr** angezeigt werden (`{Binding ...}`).

### Aufgabe 3 - Auswahl -> Detailansicht (10 Punkte)

- Implementiere `filmListe_SelectionChanged(...)`: hole den ausgewaehlten `Film`
  und zeige `Title`, `Director`, `Year` und `Rating` in den rechten `TextBlock`s an.
- Rufe danach `DrawRatingBar(rating)` auf, damit die Bewertungsleiste zum
  ausgewaehlten Film passt.
- Beachte: Bei Programmstart bzw. wenn nichts ausgewaehlt ist, darf nichts abstuerzen
  (Null-Pruefung auf `SelectedItem`).

### Aufgabe 4 - Bewertungsleiste zeichnen (groessenabhaengig!) (14 Punkte)

- Implementiere `DrawRatingBar(int rating)`: Zeichne auf `ratingCanvas` ein
  Rechteck (`Rectangle`), dessen **Breite** dem Verhaeltnis `rating / 10`
  multipliziert mit `ratingCanvas.ActualWidth` entspricht
  (also `rating == 10` -> volle Breite, `rating == 5` -> halbe Breite).
  Die Hoehe entspricht `ratingCanvas.ActualHeight`. Loesche vorher alte Inhalte
  (`ratingCanvas.Children.Clear()`).
- Zeige beim **ersten Laden** (ohne Auswahl) die Bewertungsleiste fuer den ersten
  Film an. Da `DrawRatingBar` von `ratingCanvas.ActualWidth` abhaengt und diese im
  Konstruktor `0` ist, MUSST du das erste Zeichnen (und das `LoadFilms()`) ueber das
  **`Loaded`-Event** anstossen - nicht im Konstruktor.

### Aufgabe 5 - Neuen Film einfuegen (12 Punkte)

- Implementiere `Button_Click(...)`: lies die Eingaben aus den drei `TextBox`es
  (`neuTitle`, `neuDirector`, `neuYear`) und der `neuRating`-TextBox.
- Lege ein `DataModels.Film`-Objekt an und fuege es mit
  `db.InsertWithInt64Identity(film)` ein. Die Methode liefert die neue `Id` zurueck.
- Lade danach die Liste neu (`LoadFilms()`), damit der neue Film sofort erscheint.
- Robustheit: ungueltige Eingaben (leerer Titel, Jahr/Bewertung nicht numerisch,
  Bewertung ausserhalb 0-10) sollen abgefangen werden (`MessageBox` + `return`).

## Hinweise

- **`ActualWidth`/`ActualHeight` sind im Konstruktor `0`.** Alles Groessenabhaengige
  (hier: `DrawRatingBar`) gehoert ins `Loaded`-Event, nicht in den Konstruktor.
- **`StackPanel` vs. `Grid`.** Damit die ListBox den Platz fuellt und bei vielen
  Eintraegen von selbst scrollt, gehoert sie in eine `Grid`-Zeile mit `Height="*"` -
  nicht in ein `StackPanel` mit fixer Pixel-Hoehe.
- **Namenskollision `Film`.** Das generierte Entity (`DataModels.Film`) und das
  ViewModel (`Film` im Code-Behind) heissen gleich. Beim DB-Zugriff
  `DataModels.Film` voll qualifizieren.
- **`Nullable` aus der DB.** `Id`, `Year`, `Rating` sind `long?`. Beim Uebernehmen
  ins ViewModel casten/konvertieren (`(long)item.Id`, `(int)(item.Rating ?? 0)`).
- **`InsertWithInt64Identity`** gibt die neue `Id` direkt zurueck - kein erneutes
  Abfragen noetig.
- **`movies.db` muss kopiert werden** (`CopyToOutputDirectory=PreserveNewest`),
  sonst `SQLite Error: no such table` zur Laufzeit.
- **WPF ist Windows-only** - bauen/starten nur unter Windows.

## Setup

1. Basis erzeugen (kompilierbares Skelett):

   ```
   ./_TOOLKIT/new-pa.sh UE4_Medien --wpf --db
   ```

2. Die Vorgabe-Dateien dieser Aufgabe ins Projekt kopieren bzw. ueberschreiben:
   - `schema.sql` -> in den Solution-Ordner `UE4_Medien/`
   - `UE4_Medien/MainWindow.xaml` und `UE4_Medien/MainWindow.xaml.cs` ueberschreiben
   - `Db.cs` (vom Toolkit erzeugt) kannst du als Notiz behalten oder loeschen.

3. Datenbank aus dem Schema erzeugen (im Projektordner `UE4_Medien/UE4_Medien/`):

   ```
   sqlite3 movies.db < ../schema.sql
   ```

   (oder mit einem beliebigen SQLite-Tool). Danach `movies.db` in der `.csproj`
   auf `CopyToOutputDirectory=PreserveNewest` setzen.

4. linq2db-Modell aus der DB generieren (erzeugt `MoviesDB` + Entity `DataModels.Film`):

   ```
   ./_TOOLKIT/scaffold-db.sh ./UE4_Medien/UE4_Medien movies.db
   ```

   (Alternativ unter Visual Studio per `*.tt` / "Run Custom Tool".)

5. `dotnet build` (nur unter Windows) und implementieren.