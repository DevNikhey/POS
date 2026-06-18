# WPF Foto-Weltkarte (PA4_Bilder)

Diese Anwendung zeigt eine Weltkarte (`World.jpg`), auf der zu jedem Foto aus einer SQLite-Datenbank ein roter Punkt an der GPS-Position erscheint. Klickt man einen Punkt oder einen Listeneintrag an, wird das passende Bild angezeigt.

## Worum geht es?

Aus einer SQLite-Datenbank (`photoworld.db`) werden alle Fotos geladen. Jedes Foto hat einen Namen und GPS-Koordinaten (Longitude/Latitude). Die App soll:

1. Über einem Kartenbild für jedes Foto einen **roten Marker** an der richtigen Stelle zeichnen.
2. Alle Fotos in einer **ListBox** auflisten (Name, Longitude, Latitude).
3. Beim Klick auf einen Marker **oder** beim Auswählen in der ListBox das zugehörige Bild (`images/<id>.jpg`) anzeigen.
4. Neue Bilder über einen **OpenFileDialog** hinzufügen: GPS-Daten aus den EXIF-Infos lesen, das Foto in die DB einfügen und die Bilddatei im `images`-Ordner speichern.

Technisch steckt dahinter: WPF-Layout, ein `Canvas` als Overlay über der Karte, eine Koordinaten-Projektion (Lng/Lat -> Pixel), Datenbankzugriff mit **linq2db** und der `OpenFileDialog` aus `Microsoft.Win32`.

## Dateien & Aufbau

| Datei | Rolle |
|-------|-------|
| `MainWindow.xaml` | Layout des Hauptfensters: Karte + Canvas-Overlay (rechts), Button + ListBox + Vorschaubild (links), DataTemplate `PersonTemplate` für die ListBox-Einträge. |
| `MainWindow.xaml.cs` | Lädt die Fotos aus der DB, baut die `Bild`-Liste, zeichnet die Marker (`AddMarker`), reagiert auf Klick/Auswahl (`changeSelected`). Enthält auch die kleine Klasse `Bild` (ViewModel). |
| `NewPicture.xaml` | Layout des zweiten Fensters zum Hochladen (Zwischen-ListBox + Buttons). |
| `NewPicture.xaml.cs` | `OpenFileDialog`, EXIF-GPS lesen, Foto per linq2db einfügen, Datei in den `images`-Ordner kopieren. |
| `photoworld.generated.cs` | Von linq2db generierter DB-Code: `PhotoworldDB` (DataConnection) und die Entity `Photo`. **Nicht von Hand ändern.** |

## Wie funktioniert es?

### 1. Die Datenbank-Entity (generiert)

Die Tabelle `photos` wird auf die Klasse `Photo` abgebildet. Wichtig: `Id` ist Primärschlüssel und `Identity` (wird von SQLite automatisch vergeben) und alle Felder sind `Nullable` (`long?`, `double?`):

```csharp
[Table(Schema="main", Name="photos")]
public partial class Photo
{
    [Column("id"),   PrimaryKey, Identity, Nullable] public long?   Id   { get; set; }
    [Column("name"),                       Nullable] public string  Name { get; set; }
    [Column("lng"),                        Nullable] public double? Lng  { get; set; }
    [Column("lat"),                        Nullable] public double? Lat  { get; set; }
}
```

### 2. Fotos laden (DB-Zugriff mit linq2db)

Im Code wird eine Verbindung über `DataOptions` aufgebaut und die Tabelle wie eine normale C#-Liste abgefragt:

```csharp
var options = new DataOptions().UseSQLite("Data Source=photoworld.db");
using var db = new PhotoworldDB(options);

foreach (var item in db.Photos.ToList())
{
    Bild person = new Bild();
    person.longitude = item.Lng.ToString();
    person.latitude  = item.Lat.ToString();
    person.name      = item.Name;
    // ...
}
```

`db.Photos` ist eine `ITable<Photo>`; mit `.ToList()` wird die Abfrage ausgeführt und die Zeilen kommen als `Photo`-Objekte zurück.

### 3. Das ViewModel `Bild`

Für die Anzeige in der ListBox gibt es eine eigene kleine Klasse. Sie hält genau die Felder, die im DataTemplate gebraucht werden:

```csharp
class Bild
{
    public long id { get; set; }
    public string name { get; set; }
    public string longitude { get; set; }
    public string latitude { get; set; }
}
```

### 4. Das Layout: Karte + Canvas-Overlay

Im rechten Bereich liegen das Kartenbild und ein `Canvas` im **selben Grid** übereinander. Dadurch kann man die Marker frei über der Karte positionieren:

```xml
<Grid>
    <Image x:Name="Map" Source="/World.jpg" Stretch="Fill"/>
    <Canvas x:Name="MapCanvas"/>
</Grid>
```

### 5. Koordinaten-Projektion (Lng/Lat -> Pixel)

Eine Weltkarte deckt die Längengrade von -180 bis +180 und die Breitengrade von +90 (oben) bis -90 (unten) ab. Daraus berechnet man die Pixelposition:

```csharp
double x = ((double)longitude - leftLongitude)
           / (rightLongitude - leftLongitude)
           * mapWidth;                       // -180..180 -> 0..mapWidth

double y = (topLatitude - (double)latitude)
           / (topLatitude - bottomLatitude)
           * mapHeight;                       // 90..-90 -> 0..mapHeight (y ist invertiert!)
```

Wichtig: Bei `y` wird `90 - lat` gerechnet, weil der Nullpunkt von Pixelkoordinaten **oben links** liegt, der Breitengrad +90 aber den **oberen** Rand meint.

Der Marker selbst ist eine kleine rote `Ellipse`, die mit `Canvas.SetLeft`/`Canvas.SetTop` auf dem Canvas platziert wird:

```csharp
Ellipse marker = new Ellipse();
marker.Width = 10;
marker.Height = 10;
marker.Fill = Brushes.Red;

Canvas.SetLeft(marker, x);
Canvas.SetTop(marker, y);
MapCanvas.Children.Add(marker);
```

### 6. Auswahl / Klick -> Bild anzeigen

Sowohl der Klick auf den Marker als auch die Auswahl in der ListBox rufen `changeSelected(bild)` auf. Dort wird das Bild über eine **relative Uri** geladen:

```csharp
private void changeSelected(Bild bild)
{
    template.Source = new BitmapImage(
        new Uri("images/" + bild.id + ".jpg", UriKind.Relative));
}
```

Der Dateiname leitet sich also aus `bild.id` ab — die Datei muss `images/<id>.jpg` heißen, sonst wird nichts gefunden.

### 7. Neues Bild einfügen (Insert mit Identity)

Beim Hochladen wird ein `Photo` angelegt und per `InsertWithInt64Identity` eingefügt. Diese Methode fügt die Zeile ein **und liefert die neu vergebene Id zurück**:

```csharp
var options = new DataOptions().UseSQLite("Data Source=photoworld.db");
using var db = new PhotoworldDB(options);
var wonderId = db.InsertWithInt64Identity(photo);   // neue id
```

Diese `id` braucht man, um die Bilddatei korrekt als `images/<id>.jpg` zu benennen.

## Die wichtigsten Konzepte

- **WPF-Layout: `Grid`/`DockPanel` statt `StackPanel`.** Ein `StackPanel` gibt jedem Kind genau seine *Wunschgröße* und füllt den vorhandenen Platz **nicht** aus. Ein Element wie eine ListBox wächst dann ins Unendliche oder braucht eine fixe Höhe. Ein `Grid` mit `RowDefinition Height="*"` füllt den verfügbaren Platz und lässt die ListBox **von selbst scrollen** — ohne Pixel-Höhe im Code.
- **Canvas-Overlay + `Canvas.SetLeft`/`SetTop`.** Ein `Canvas` erlaubt absolute Positionierung über Koordinaten. Liegt er im selben `Grid` über einem Bild, kann man Marker pixelgenau auf das Bild legen.
- **`ItemsSource` + `DataTemplate`.** Die ListBox bekommt eine Liste von Objekten (`ItemsSource`). Das `DataTemplate` legt fest, **wie** ein einzelnes Objekt dargestellt wird; die `{Binding Path=...}`-Ausdrücke greifen auf die Properties des Objekts zu.
- **`OpenFileDialog` (Microsoft.Win32).** Standard-Dialog zum Auswählen von Dateien. `ShowDialog()` liefert `true`, wenn der Benutzer eine Datei bestätigt hat; danach steht der Pfad in `FileName` (bzw. `FileNames` bei Mehrfachauswahl).
- **linq2db Insert mit Identity.** `InsertWithInt64Identity(entity)` fügt eine Zeile ein und gibt sofort den vom Autoincrement vergebenen Primärschlüssel zurück. So muss man nicht erneut abfragen.
- **`BitmapImage` + relative Uri.** Bilder lädt man als `BitmapImage` mit einer `Uri`. Bei `UriKind.Relative` ist der Pfad relativ zum Arbeitsverzeichnis der laufenden App (dort, wo z. B. der `images`-Ordner liegt).
- **`Loaded`-Event vs. Konstruktor (`ActualWidth`).** Im Konstruktor ist das Layout **noch nicht** berechnet — `ActualWidth`/`ActualHeight` sind `0`. Erst nach dem `Loaded`-Event kennt WPF die tatsächlichen Größen. Alles, was von der gerenderten Größe abhängt (hier: die Markerpositionen), gehört deshalb in `Loaded`, nicht in den Konstruktor.

## Bewertung: Fehler & Korrekturen

| Aufgabe | Bemängelt | Korrektur |
|---------|-----------|-----------|
| A1 (9) | Linke Spalte nutzt `StackPanel` mit fixer ListBox-Höhe (`Height="214"`). | `Grid` mit Zeilen `Auto/*/Auto` verwenden, ListBox füllt den Rest und scrollt selbst. |
| A2 (9) | Extra-Fenster `NewPicture` mit Zwischen-ListBox; Datei wird unter Originalnamen gespeichert -> passt nicht zur id. | `OpenFileDialog` direkt im Hauptfenster, Datei als `images/<id>.jpg` speichern. |
| A4 (8) | Marker werden im **Konstruktor** gezeichnet -> `ActualWidth/Height == 0` -> alle Marker bei (0,0). | Marker erst im `Loaded`-Event zeichnen. |
| A6 (7) | `person.id = (long)item.Id;` fehlt -> `id` immer 0 -> immer `images/0.jpg`. | `id` in der Lade-Schleife setzen. |

### A1 — `Grid` statt `StackPanel`, keine fixe Höhe

**Warum?** In einem `StackPanel` bekommt die ListBox nur ihre Wunschhöhe, also braucht man eine fixe `Height="214"` — die passt aber nie zu jeder Fenstergröße. In einem `Grid` mit `Height="*"` füllt die ListBox automatisch den Platz und scrollt bei vielen Einträgen von selbst.

**Vorher** (`MainWindow.xaml`, linke Spalte):

```xml
<StackPanel Orientation="Vertical">
    <Button Click="Button_Click">Neue Bilder</Button>
    <ListBox ItemTemplate="{StaticResource PersonTemplate}"
             SelectionChanged="showListbox_SelectionChanged"
             ItemsSource="{Binding}" x:Name="showListbox" Height="214"/>
    <Image x:Name="template"></Image>
</StackPanel>
```

**Nachher:**

```xml
<Border Grid.Row="1" Grid.Column="0" BorderBrush="Black" BorderThickness="1">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>   <!-- Button -->
            <RowDefinition Height="*"/>      <!-- ListBox fuellt den Rest -->
            <RowDefinition Height="Auto"/>   <!-- Vorschaubild -->
        </Grid.RowDefinitions>
        <Button Grid.Row="0" Click="Button_Click">Neue Bilder</Button>
        <ListBox Grid.Row="1" x:Name="showListbox"
                 ItemTemplate="{StaticResource PersonTemplate}"
                 SelectionChanged="showListbox_SelectionChanged"/>
        <Image Grid.Row="2" x:Name="template"/>
    </Grid>
</Border>
```

### A2 — Direkt aus dem Hauptfenster, Datei mit DB-Id benennen

**Warum?** Im Original öffnet der Button ein zweites Fenster (`NewPicture`) mit einer Zwischen-ListBox — die Aufgabe verlangt aber eine **direkte** Auswahl per `OpenFileDialog`, ohne zusätzlichen Dialog. Zudem speichert das Original die Datei unter ihrem **Originalnamen** (nur bei Namenskollision wird die id angehängt). `changeSelected` lädt jedoch `images/<id>.jpg` — der Dateiname passt also nicht zur id. Lösung: Datei sofort als `images/<id>.jpg` ablegen.

**Vorher** (`MainWindow.xaml.cs` öffnet nur das Zwischenfenster):

```csharp
private void Button_Click(object sender, RoutedEventArgs e)
{
    NewPicture newPicture = new NewPicture();
    newPicture.ShowDialog();
}
```

und in `NewPicture.xaml.cs` wird der Originalname genutzt:

```csharp
string fileName = System.IO.Path.GetFileName(selectedFile);
string destinationPath = System.IO.Path.Combine(imagesFolder, fileName);
// ... nur bei Kollision wird die id angehaengt
File.Copy(selectedFile, destinationPath);
```

**Nachher** (alles direkt im Hauptfenster, Datei mit der DB-Id):

```csharp
private void Button_Click(object sender, RoutedEventArgs e)
{
    var dlg = new Microsoft.Win32.OpenFileDialog
    {
        Filter = "Bilddateien (*.jpg)|*.jpg"
    };
    if (dlg.ShowDialog() != true) return;

    // EXIF-GPS lesen und Photo einfuegen
    var exif = ExifPhoto.GetExifDataPhoto(dlg.FileName);
    var photo = new DataModels.Photo
    {
        Name = System.IO.Path.GetFileName(dlg.FileName),
        Lat = exif.GPSInfo.Latitude,
        Lng = exif.GPSInfo.Longitude
    };

    var options = new DataOptions().UseSQLite("Data Source=photoworld.db");
    using var db = new PhotoworldDB(options);
    long id = db.InsertWithInt64Identity(photo);   // neue DB-id

    // Datei MIT DER DB-ID benennen -> passt zu changeSelected ("images/<id>.jpg")
    Directory.CreateDirectory("images");
    System.IO.File.Copy(dlg.FileName, System.IO.Path.Combine("images", id + ".jpg"), true);

    LoadPhotos();   // Liste + Marker neu aufbauen
}
```

Damit wird das `NewPicture`-Fenster überflüssig.

### A4 — Marker im `Loaded`-Event zeichnen

**Warum?** `AddMarker` rechnet mit `Map.ActualWidth`/`Map.ActualHeight`. Im **Konstruktor** ist das Layout aber noch nicht passiert, also sind beide Werte `0`. Multipliziert man die Projektion mit `0`, landen **alle** Marker bei `(0,0)` in der linken oberen Ecke. Erst nach dem `Loaded`-Event sind die tatsächlichen Größen bekannt.

**Vorher** (`MainWindow.xaml.cs`, im Konstruktor):

```csharp
public MainWindow()
{
    InitializeComponent();
    this.DataContext = this;
    // ...
    foreach (var item in db.Photos.ToList())
    {
        Bild person = new Bild();
        // ...
        AddMarker((double) item.Lat, (double) item.Lng, person);  // ActualWidth == 0!
    }
    showListbox.ItemsSource = list;
}
```

**Nachher:**

```csharp
public MainWindow()
{
    InitializeComponent();
    this.DataContext = this;
    this.Loaded += (s, e) => LoadPhotos();   // erst NACH dem Layout -> ActualWidth/Height korrekt
}

private void LoadPhotos()
{
    MapCanvas.Children.Clear();
    var options = new DataOptions().UseSQLite("Data Source=photoworld.db");
    using var db = new PhotoworldDB(options);

    var list = new List<Bild>();
    foreach (var item in db.Photos.ToList())
    {
        var person = new Bild
        {
            id = (long)item.Id,            // <-- A6: id setzen!
            name = item.Name,
            longitude = item.Lng.ToString(),
            latitude = item.Lat.ToString()
        };
        list.Add(person);
        AddMarker((double)item.Lat, (double)item.Lng, person);
    }
    showListbox.ItemsSource = list;
}
```

Alternativ kann man das Event auch in XAML setzen: `<Window ... Loaded="Window_Loaded">`.

### A6 — `person.id` setzen

**Warum?** In der Original-Schleife werden `name`, `longitude` und `latitude` gesetzt, aber **nie** die `id`. Damit bleibt `bild.id` immer auf dem Default-Wert `0`, und `changeSelected` lädt für jedes Foto stur `images/0.jpg`. Da `item.Id` vom Typ `long?` ist, muss man es auf `long` casten.

**Vorher:**

```csharp
Bild person = new Bild();
person.longitude = item.Lng.ToString();
person.latitude = item.Lat.ToString();
person.name = item.Name;
// id wird NIE gesetzt -> bleibt 0
```

**Nachher** (siehe `LoadPhotos` oben):

```csharp
var person = new Bild
{
    id = (long)item.Id,            // <-- jetzt korrekt gesetzt
    name = item.Name,
    longitude = item.Lng.ToString(),
    latitude = item.Lat.ToString()
};
```

## Was du fürs nächste Mal mitnimmst

- **Fixe Pixel-Höhen sind ein Warnsignal.** Brauchst du eine `Height="214"`, ist meistens das falsche Layout-Panel im Spiel. Nimm ein `Grid` mit `Height="*"` (oder ein `DockPanel`) statt `StackPanel`, wenn etwas den Platz füllen und scrollen soll.
- **`ActualWidth`/`ActualHeight` sind im Konstruktor 0.** Alles, was von der gerenderten Größe abhängt, gehört ins `Loaded`-Event.
- **Ein Schlüssel muss überall zusammenpassen.** Wenn du Dateien als `images/<id>.jpg` lädst, müssen sie auch genau so gespeichert werden — und die `id` muss tatsächlich gesetzt sein (nicht im Default `0` hängen bleiben).
- **`InsertWithInt64Identity` gibt dir die neue Id direkt zurück** — nutze sie sofort für Dateinamen, Verweise usw., statt nachträglich neu abzufragen.
- **Keine unnötigen Zwischen-Dialoge.** Wenn die Aufgabe "direkt auswählen" sagt, reicht der `OpenFileDialog` im Hauptfenster — kein zweites Fenster bauen.
- **Lies die Aufgabenstellung wörtlich.** "Bild mit der Datenbank-ID benennen", "im Loaded zeichnen" — solche Sätze sind direkte Hinweise, wo Punkte vergeben werden.
