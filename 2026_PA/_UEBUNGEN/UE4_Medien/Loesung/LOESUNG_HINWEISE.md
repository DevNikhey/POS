# Loesungshinweise - UE4_Medien

Beide Vorgabe-Dateien sind vollstaendig implementiert; keine TODOs/NotImplementedException mehr. Signaturen, x:Name-Felder und das ViewModel Film blieben unveraendert.

Umgesetzt:
- Aufgabe 1: DataOptions().UseSQLite("Data Source=movies.db") + using var db = new MoviesDB(options) in LoadFilms(). using LinqToDB; und using DataModels; aktiviert.
- Aufgabe 2: db.Films.ToList(), pro Zeile ViewModel Film mit ALLEN Feldern inkl. Id; long?->cast ((long)item.Id, (int)(item.Year ?? 0), (int)(item.Rating ?? 0)). Als ItemsSource gesetzt (nicht Items.Add). DataTemplate zeigt Title (fett) und Year.
- Aufgabe 3: SelectionChanged mit Null-Pruefung auf SelectedItem, fuellt detailTitle/detailDirector/detailYear/detailRating und ruft DrawRatingBar(film.Rating).
- Aufgabe 4: DrawRatingBar: Children.Clear(), Rectangle mit Width = rating/10.0 * ratingCanvas.ActualWidth, Height = ActualHeight, Canvas.SetLeft/SetTop = 0. Erstes Zeichnen UND LoadFilms() im Loaded-Event (nicht im ctor, da ActualWidth dort 0 ist) - zeigt die Leiste fuer den ersten Film.
- Aufgabe 5: Button_Click liest neuTitle/neuDirector/neuYear/neuRating, validiert (leerer Titel, Jahr/Bewertung nicht numerisch via int.TryParse, Bewertung 0..10) mit MessageBox + return; legt DataModels.Film (voll qualifiziert) an, db.InsertWithInt64Identity(film), danach LoadFilms() und Felder leeren.

WICHTIG zum Bauen (vom Schueler/von der Schuelerin per Setup zu erledigen, nicht Teil dieser Dateien):
- movies.db aus schema.sql erzeugen (sqlite3 movies.db < ../schema.sql).
- linq2db-Modell generieren (scaffold-db) -> erzeugt DataModels.MoviesDB + DataModels.Film. Das Modell wird NICHT mitgeliefert, der Code greift nur darauf zu.
- movies.db in der .csproj auf CopyToOutputDirectory=PreserveNewest setzen, sonst "no such table: films".
- WPF ist Windows-only: bauen/starten nur unter Windows. Die Loesung kompiliert hier nicht testbar (macOS), folgt aber exakt den Referenz-Patterns aus PA4_Bilder (DataOptions/UseSQLite, using var db, InsertWithInt64Identity, Canvas-Zeichnen ueber ActualWidth).
