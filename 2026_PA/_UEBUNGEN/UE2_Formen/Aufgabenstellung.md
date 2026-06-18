# UE2 – Eigene WPF-Zeichen-Controls: Vieleck & Stern

## Szenario

Du baust für eine kleine Grafik-Bibliothek zwei eigene WPF-Formen, die – wie die eingebauten `Ellipse` oder `Rectangle` – direkt in XAML verwendet werden können. Beide leiten von der vorgegebenen Basisklasse `ShapeBase : Shape` ab und liefern ihre Geometrie über eine `PathGeometry` mit einer einzigen `PathFigure`.

1. **`Vieleck`** – ein **regelmäßiges Vieleck** (Polygon) mit frei wählbarer **Eckenzahl** und **Radius** (Umkreisradius). Aus 3 Ecken wird ein Dreieck, aus 6 ein Sechseck usw.
2. **`Stern`** – ein **Sternpolygon** mit `Zacken` Spitzen, einem **Außenradius** und einem **Innenradius**. Die Punkte liegen abwechselnd auf Außen- und Innenradius.

Wie bei echten WPF-Shapes sollen die Werte (`X1`, `Y1`, `Radius`, `Ecken`, …) in XAML gesetzt werden können und beim Ändern automatisch ein Neuzeichnen auslösen.

## Vorgegeben

- `ShapeBase.cs` (vom Toolkit): Basisklasse, die von `Shape` erbt, `DefiningGeometry` überschreibt und `protected virtual PathFigure CreatePathFigure()` bereitstellt. Schon vorhandene DependencyProperties: `X1`, `Y1` (beide `double`, Längen, mit `LengthConverter` – korrekt). Standard-`CreatePathFigure` zeichnet ein kleines Kreuz.
- `Vieleck.cs` – **Skelett**: leitet von `ShapeBase` ab, DependencyProperties und `CreatePathFigure` sind als TODO gestubbt (`NotImplementedException` bzw. leere Figur). Kompiliert bereits.
- `Stern.cs` – **Skelett**: analog zu `Vieleck`, ebenfalls gestubbt.
- `MainWindow.xaml` – bindet `Vieleck` und `Stern` mit Beispielwerten ein (zeigt anfangs nichts bzw. nur den Default, bis du die Logik implementierst).

> Das Skelett **kompiliert** und startet. Die Formen sind aber noch **leer / unfertig** – das ist deine Aufgabe.

## Aufgaben (Summe: 58 Punkte)

### Aufgabe 1 – DependencyProperties für `Vieleck` (14 P)

In `Vieleck.cs` sollen zwei neue Eigenschaften als `DependencyProperty` registriert werden:

- `Radius` vom Typ **`double`** (Umkreisradius, eine **Länge**).
- `Ecken` vom Typ **`int`** (Anzahl der Ecken, eine **Stückzahl**).

Achte auf:
- DP-Typ **muss** zum Property-Typ passen (`typeof(double)` bzw. `typeof(int)`; Default `0.0` bzw. `0`).
- `FrameworkPropertyMetadataOptions.AffectsRender | AffectsMeasure` setzen, damit Änderungen neu zeichnen.
- `[TypeConverter(typeof(LengthConverter))]` **nur** bei `Radius` (Länge). **Nicht** bei `Ecken` (Anzahl)!

### Aufgabe 2 – `Vieleck` zeichnen (16 P)

Implementiere `CreatePathFigure()` in `Vieleck.cs`:

- Mittelpunkt = `(X1, Y1)`.
- Die `Ecken` Eckpunkte liegen gleichmäßig auf einem Kreis mit `Radius` um den Mittelpunkt. Punkt `i` liegt bei Winkel `2π · i / Ecken`: `center + new Vector(cos α, sin α) · Radius`.
- Setze den `StartPoint` auf den ersten Eckpunkt und füge für jeden weiteren Eckpunkt ein `LineSegment` hinzu.
- `IsClosed = true`, damit die letzte Kante zurück zum Start automatisch gezogen wird.
- Robust bleiben: bei `Ecken < 3` eine sinnvolle Untergrenze verwenden (mind. 3), damit nichts abstürzt.

### Aufgabe 3 – DependencyProperties für `Stern` (12 P)

In `Stern.cs` drei Eigenschaften registrieren:

- `AussenRadius` (`double`, Länge) und `InnenRadius` (`double`, Länge).
- `Zacken` (`int`, Anzahl der Spitzen).

`LengthConverter` nur auf die beiden Radien, **nicht** auf `Zacken`. DP-Typ == Property-Typ.

### Aufgabe 4 – `Stern` zeichnen (12 P)

Implementiere `CreatePathFigure()` in `Stern.cs`:

- Ein Stern mit `Zacken` Spitzen hat `2 · Zacken` Punkte, die abwechselnd auf `AussenRadius` und `InnenRadius` liegen.
- Punkt `i` (für `i = 0 .. 2·Zacken − 1`): Winkel `π · i / Zacken`; Radius = `AussenRadius` wenn `i` gerade, sonst `InnenRadius`.
- Punkte wieder über `StartPoint` + `LineSegment` verbinden, `IsClosed = true`.

### Aufgabe 5 – XAML & Beispiel (4 P)

In `MainWindow.xaml` sind `Vieleck` und `Stern` bereits eingebunden. Stelle sicher, dass die Beispielwerte sinnvoll gesetzt sind (z. B. `Vieleck` mit `Ecken="6"`, `Stern` mit `Zacken="5"`) und beide Formen mit `Stroke`/`StrokeThickness` sichtbar gezeichnet werden. Füge ein zweites `Vieleck` mit anderer Eckenzahl hinzu, um zu zeigen, dass es generisch funktioniert.

## Hinweise

- **`LengthConverter` nur für Längen.** Winkel und Anzahlen (`Ecken`, `Zacken`) bekommen **keinen** `[TypeConverter(typeof(LengthConverter))]`. Sonst wird `"5"` als Länge interpretiert und es kommt zu falschen Werten / `Auto` → `NaN`.
- **DP-Typ == Property-Typ.** Eine `int`-Property mit `typeof(double)` zu registrieren führt beim `(int)GetValue(...)` zur Laufzeit zu einer `InvalidCastException` (geboxter `double`).
- **Nur Segmente zeichnen etwas.** Ein `PathFigure` mit nur `StartPoint` bleibt unsichtbar – immer `LineSegment` hinzufügen.
- **`IsClosed = true`** schließt die Figur automatisch (letztes Ende → `StartPoint`); für Vieleck und Stern ideal.
- **Mittelpunkt ist Mittelpunkt:** `center = (X1, Y1)`, nicht `(X1 + Radius, Y1)`.
- **`AffectsRender | AffectsMeasure`** nicht vergessen, sonst zeichnet WPF bei geänderten Werten nicht neu.
- **WPF baut nur unter Windows.** Auf macOS/Linux dient das Setup nur zum Anlegen der Struktur.

## Setup

```bash
./_TOOLKIT/new-pa.sh UE2_Formen --wpf --shapes
```

Danach die Vorgabe-Dateien ins Projekt kopieren (in den Ordner `UE2_Formen/`):
- `UE2_Formen/Vieleck.cs`
- `UE2_Formen/Stern.cs`
- `UE2_Formen/MainWindow.xaml` (überschreibt die generierte Datei)

`ShapeBase.cs` wird vom Toolkit bereits durch `--shapes` angelegt – nicht überschreiben.