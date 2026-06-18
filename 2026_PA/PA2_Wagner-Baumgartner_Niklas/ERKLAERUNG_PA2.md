# Eigene WPF-Controls: Zeichnen mit Geometry & ein Templated Control (PA2)

Diese PA besteht aus zwei WPF-Themen: (1) **eigene Zeichen-Controls**, die von `Shape` erben und über eine `PathGeometry` Figuren wie ein Tortenstück (`Slice`) oder eine Spirale (`Spirale`) zeichnen, und (2) ein **templatisiertes Custom Control** (`RatingControl` – die Sterne-Bewertung) in einer eigenen Bibliothek.

## Worum geht es?

- **Zeichnen über `Shape`:** Eine Basisklasse `Basis : Shape` liefert die Geometrie; Unterklassen (`Slice`, `Spirale`) bestimmen die konkrete Figur. Werte wie `X1`, `Y1`, `Radius`, `Angle`, `Steigung`, `Umdrehung`, `Ecken` sind **DependencyProperties**, damit man sie in XAML setzen kann und WPF bei Änderungen automatisch neu zeichnet.
- **Templated Control:** `RatingControl` ist ein „lookless" Control – die Logik steckt im C#-Code, das Aussehen in einem `ControlTemplate` in `Themes/Generic.xaml`. Über benannte Template-Teile (`PART_...`) verbindet der Code Logik und Optik.

## Dateien & Aufbau

| Datei | Rolle |
|-------|-------|
| `Basis.cs` | `Shape`-Basisklasse: überschreibt `DefiningGeometry`, baut eine `PathGeometry` aus `CreatePathFigure()`. DPs `X1`, `Y1`. |
| `Slice.cs` | Zeichnet ein Tortenstück (Kreissektor) mit `Radius` und `Angle` (`ArcSegment`). |
| `Spirale.cs` | Soll eine Spirale aus `Steigung`, `Umdrehung`, `Ecken` zeichnen. |
| `MainWindow.xaml` | Verwendet `Slice`, `Spirale` und `RatingControl`. |
| `Library/CustomControl1.cs` | `RatingControl : Control` – DPs `Rating`, `ShowNumber`, `OnApplyTemplate`, Logik zum Umschalten der Sterne. |
| `Library/Themes/Generic.xaml` | Default-`Style`/`ControlTemplate` des `RatingControl` (die 5 Stern-Bilder, der Text, die Buttons). |

## Wie funktioniert es?

### 1. `Shape` + `DefiningGeometry`

Eine eigene Form entsteht, indem man von `Shape` erbt und die Eigenschaft `DefiningGeometry` überschreibt. WPF zeichnet dann genau diese Geometrie (mit `Stroke`, `StrokeThickness`, `Fill` …):

```csharp
protected override Geometry DefiningGeometry
{
    get
    {
        PathGeometry geometry = new PathGeometry { FillRule = FillRule.EvenOdd };
        geometry.Figures.Add(CreatePathFigure());   // konkrete Figur aus der Unterklasse
        return geometry;
    }
}
```

Eine `PathGeometry` besteht aus einer oder mehreren `PathFigure`. Eine `PathFigure` hat einen `StartPoint` und eine Liste von **Segmenten** (`LineSegment`, `ArcSegment`, …). `IsClosed = true` verbindet das letzte Segment-Ende automatisch wieder mit dem `StartPoint`.

### 2. DependencyProperty-Muster

Damit eine Eigenschaft in XAML gesetzt und animiert werden kann und ein Neuzeichnen auslöst, registriert man sie als `DependencyProperty`:

```csharp
public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(
    "Radius", typeof(double), typeof(Slice),
    new FrameworkPropertyMetadata(0.0,
        FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

public double Radius
{
    get => (double)GetValue(RadiusProperty);
    set => SetValue(RadiusProperty, value);
}
```

`AffectsRender | AffectsMeasure` sorgt dafür, dass WPF bei jeder Änderung neu misst und neu zeichnet.

### 3. `Slice` – ein Tortenstück mit `ArcSegment`

`Slice` baut aus Mittelpunkt, Bogenanfang und Bogenende einen Kreissektor (siehe Korrektur unten).

### 4. `RatingControl` – templatisiertes Control

Ein Control ohne festes Aussehen meldet seinen Default-Style an und holt sich die benannten Template-Teile zur Laufzeit:

```csharp
static RatingControl()
{
    DefaultStyleKeyProperty.OverrideMetadata(typeof(RatingControl),
        new FrameworkPropertyMetadata(typeof(RatingControl)));   // sucht Style in Themes/Generic.xaml
}

public override void OnApplyTemplate()
{
    base.OnApplyTemplate();
    var full1  = GetTemplateChild("PART_STARF_1") as Image;      // Teil aus dem Template holen
    var empty1 = GetTemplateChild("PART_STARE_1") as Image;
    empty1.MouseLeftButtonDown += (s, e) => changeTo(1);
    // ...
}
```

`Rating` ist eine `DependencyProperty` mit `PropertyChangedCallback`: ändert sich der Wert, blendet der Code die „vollen" bzw. „leeren" Stern-Bilder passend ein/aus. Die Bilder kommen per `pack://`-URI aus der Bibliothek:

```xml
<Image x:Name="PART_STARF_1" Source="pack://application:,,,/Library;component/Themes/Images/Star_selected.png"/>
```

(`RatingControl` hat in der Bewertung die volle Punktzahl bekommen – hier nur als Konzept erklärt.)

## Die wichtigsten Konzepte

- **`Shape` + `DefiningGeometry`:** der einfachste Weg, etwas Eigenes zu zeichnen. Unterklassen liefern nur die `PathFigure`.
- **`PathFigure` & Segmente:** `StartPoint` + `LineSegment`/`ArcSegment`. Erst **Segmente hinzufügen** ergibt eine sichtbare Figur – ein bloßer `StartPoint` zeichnet nichts.
- **`IsClosed`:** schließt die Figur (letztes Ende → `StartPoint`). Praktisch, um z. B. die zweite Kante eines Tortenstücks automatisch zu ziehen.
- **DependencyProperty:** Voraussetzung für XAML-Werte, Bindung, Animation und automatisches Neuzeichnen (`AffectsRender`). **DP-Typ und Property-Typ müssen identisch sein.**
- **`TypeConverter` vs. Wertekonverter:** Ein `[TypeConverter(...)]` wandelt einen **XAML-String** in den Property-Wert um. `LengthConverter` ist **nur für Längen** gedacht (px/cm/in, `"Auto"` → `NaN`). Für Winkel oder Anzahlen ist er falsch.
- **Templated Control:** Logik in C#, Aussehen im `ControlTemplate` (`Themes/Generic.xaml`). Verbindung über `PART_`-Namen und `GetTemplateChild` in `OnApplyTemplate`.

## Bewertung: Fehler & Korrekturen

| Aufgabe | Bemängelt | Korrektur |
|---------|-----------|-----------|
| A3 (5) – Spirale | „keinen LengthConverter für Umdrehungen und Ecken, Zeichnen unvollständig". | `LengthConverter` von `Umdrehung`/`Ecken` entfernen; DP-Typ an Property-Typ anpassen; Spirale tatsächlich mit Segmenten zeichnen. |
| A6 (8) – Slice | „keinen LengthConverter beim Angle, LineSegment fehlt". | `LengthConverter` von `Angle` entfernen; fehlende `LineSegment`-Kante(n) für ein geschlossenes Tortenstück ergänzen. |

### A3 — Spirale: kein LengthConverter & richtig zeichnen

**Warum (LengthConverter)?** `Umdrehung` (Anzahl Umdrehungen) und `Ecken` (Anzahl Ecken) sind **Stückzahlen**, keine Längen. `[TypeConverter(typeof(LengthConverter))]` gehört nur auf echte Längen (`X1`, `Y1`, `Steigung`).

**Warum (Typ-Mismatch)?** Die Properties `Umdrehung`/`Ecken` sind `int`, die DependencyProperty wurde aber als `typeof(double)` registriert. `(int)GetValue(...)` entpackt dann einen geboxten `double` → **`InvalidCastException`** zur Laufzeit. DP-Typ und Property-Typ müssen zusammenpassen.

**Warum (Zeichnen)?** `CreatePathFigure` fügt der Figur **keine Segmente** hinzu – es wird also gar nichts gezeichnet.

**Vorher** (`Spirale.cs`):

```csharp
[TypeConverter(typeof(LengthConverter))]   // falsch: Anzahl, keine Länge
public int Umdrehung { get => (int)GetValue(UmdrehungProperty); set => SetValue(UmdrehungProperty, value); }
// UmdrehungProperty ist als typeof(double) registriert  -> Typ-Mismatch

protected override PathFigure CreatePathFigure()
{
    Point Center = new Point(X1, Y1);
    PathFigure pathFigure = new() { StartPoint = Center, IsClosed = true };
    return pathFigure;     // KEINE Segmente -> nichts wird gezeichnet
}
```

**Nachher** – Property korrekt (Typ passt, kein LengthConverter; `Ecken` analog):

```csharp
public static readonly DependencyProperty UmdrehungProperty = DependencyProperty.Register(
    "Umdrehung", typeof(int), typeof(Spirale),
    new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender
                                   | FrameworkPropertyMetadataOptions.AffectsMeasure));

public int Umdrehung
{
    get => (int)GetValue(UmdrehungProperty);
    set => SetValue(UmdrehungProperty, value);
}
```

**Nachher** – die Spirale tatsächlich zeichnen (Polylinie aus `LineSegment`):

```csharp
protected override PathFigure CreatePathFigure()
{
    Point center = new Point(X1, Y1);
    PathFigure fig = new PathFigure { StartPoint = center, IsClosed = false };

    int ecken = Math.Max(3, Ecken);     // Stützpunkte pro Umdrehung
    int steps = Umdrehung * ecken;      // Gesamtanzahl Punkte
    for (int i = 1; i <= steps; i++)
    {
        double angle  = 2 * Math.PI * i / ecken;   // Winkel nimmt zu
        double radius = Steigung * i / ecken;      // Radius waechst -> Spirale
        Point p = center + new Vector(Math.Cos(angle), Math.Sin(angle)) * radius;
        fig.Segments.Add(new LineSegment(p, true));   // Spirale aus LineSegments
    }
    return fig;
}
```

### A6 — Slice: kein LengthConverter beim Angle & fehlende `LineSegment`

**Warum (LengthConverter)?** `Angle` ist ein **Winkel**, keine Länge → `LengthConverter` entfernen. (`Radius` ist eine Länge und darf ihn behalten.)

**Warum (LineSegment)?** Das Original zeichnet nur den Bogen (`ArcSegment`). Für ein **geschlossenes Tortenstück** fehlen die geraden Radius-Kanten (von der Mitte zum Bogenanfang und zurück). Außerdem war `Center` falsch auf `(X1 + Radius, Y1)` gesetzt – das ist ein Punkt **auf** dem Kreis, nicht die Mitte.

**Vorher** (`Slice.cs`):

```csharp
[TypeConverter(typeof(LengthConverter))]   // falsch: Winkel, keine Länge
public double Angle { get => (double)GetValue(AngleProperty); set => SetValue(AngleProperty, value); }

protected override PathFigure CreatePathFigure()
{
    Point Center = new Point(X1 + Radius, Y1);   // das ist NICHT die Mitte
    PathFigure pathFigure = new() { StartPoint = Center, IsClosed = true };
    // ... nur ein ArcSegment, keine geraden Kanten
    pathFigure.Segments.Add(new ArcSegment(p1, new Size(Radius, Radius), 0.0, large, d, true));
    return pathFigure;
}
```

**Nachher** – Property ohne LengthConverter:

```csharp
public static readonly DependencyProperty AngleProperty = DependencyProperty.Register(
    "Angle", typeof(double), typeof(Slice),
    new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender
                                     | FrameworkPropertyMetadataOptions.AffectsMeasure));
public double Angle
{
    get => (double)GetValue(AngleProperty);
    set => SetValue(AngleProperty, value);
}
```

**Nachher** – Tortenstück mit gerader Kante + Bogen, sauber geschlossen:

```csharp
protected override PathFigure CreatePathFigure()
{
    Point center = new Point(X1, Y1);
    double a = Angle * Math.PI / 180.0;        // Grad -> Bogenmass

    Point arcStart = center + new Vector(Radius, 0);                          // bei 0 Grad
    Point arcEnd   = center + new Vector(Math.Cos(a), Math.Sin(a)) * Radius;  // bei Angle

    PathFigure fig = new PathFigure { StartPoint = center, IsClosed = true };
    fig.Segments.Add(new LineSegment(arcStart, true));   // Kante: Mitte -> Bogenanfang (LineSegment!)
    fig.Segments.Add(new ArcSegment(arcEnd, new Size(Radius, Radius),
                                    0, Angle > 180, SweepDirection.Clockwise, true));  // Bogen
    // IsClosed = true zieht die zweite Kante (Bogenende -> Mitte) automatisch
    return fig;
}
```

## Was du fürs nächste Mal mitnimmst

- **Nur Segmente zeichnen etwas.** Ein `PathFigure` mit nur `StartPoint` bleibt unsichtbar – immer `LineSegment`/`ArcSegment` hinzufügen.
- **`LengthConverter` ausschließlich für Längen.** Winkel (`Angle`) und Anzahlen (`Ecken`, `Umdrehung`) bekommen **keinen** LengthConverter.
- **DP-Typ == Property-Typ.** Sonst kracht es beim `(int)GetValue(...)` (`InvalidCastException`).
- **`IsClosed = true` spart eine Kante.** Es verbindet das Ende automatisch mit dem `StartPoint` – ideal für geschlossene Formen wie Tortenstücke.
- **Mittelpunkt ist Mittelpunkt.** Variablennamen ehrlich halten (`center` = `(X1, Y1)`), sonst rechnet man mit dem falschen Punkt.
- **Templated Control:** `DefaultStyleKey` + `Themes/Generic.xaml` + `PART_`-Namen + `GetTemplateChild` in `OnApplyTemplate` – dieses Muster wiederholt sich bei jedem eigenen Control.
