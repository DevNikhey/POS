# 2026_PA – Praktische Arbeiten 4BHIF (POS)

Sammelordner für die Praktischen Arbeiten samt **Erklär-Dateien** zu jedem Thema und einem
**Toolkit** zum Scaffolden der nächsten PA.

## Inhalt

| PA | Ordner | Thema | Punkte | Erklärung |
|----|--------|-------|:------:|-----------|
| **PA2** | [`PA2_Wagner-Baumgartner_Niklas`](PA2_Wagner-Baumgartner_Niklas) | Eigene WPF-Controls: Zeichnen mit `Shape`/`PathGeometry` (Slice, Spirale) + templatisiertes Control (RatingControl) | 63 | [ERKLAERUNG_PA2.md](PA2_Wagner-Baumgartner_Niklas/ERKLAERUNG_PA2.md) |
| **PA3** | [`PA3_4B_2026`](PA3_4B_2026) | Client/Server über TCP (Babynames-Suche), `Transfer<T>`, linq2db/SQLite | 48 | [CLIENT-SERVER-ERKLAERUNG.md](PA3_4B_2026/CLIENT-SERVER-ERKLAERUNG.md) |
| **PA4** | [`PA4_4B`](PA4_4B) | Zwei Teile: **Robot** (Tokenizer/Parser/Interpreter) + **Bilder** (WPF-Weltkarte, FileDialog, linq2db) | 62 | [Parser](PA4_4B/PA4_Robot/ERKLAERUNG_Parser.md) · [Bilder](PA4_4B/PA4_Bilder/ERKLAERUNG_Bilder.md) |
| _PA1_ | _(noch in Downloads)_ | Threads & Synchronisation (Flughafen: Landebahnen, Flugzeuge, VIPs) | 54 | _folgt_ |

> **PA3** wurde zusätzlich **korrigiert** (alle Bewertungspunkte behoben). Die anderen PAs sind im
> Originalzustand; die Erklär-Dateien zeigen jeweils **Vorher/Nachher** der korrigierten Stellen.
> **PA1** liegt vorerst noch unter `~/Downloads/PA1_4B_2025` und kann später ergänzt werden.

## Toolkit

[`_TOOLKIT/`](_TOOLKIT) – legt dir per Befehl eine neue Solution mit den passenden Modulen an
(WPF, Console, Network/`Transfer<T>`, MVVM, Shapes, Parser, linq2db). Siehe
[_TOOLKIT/README.md](_TOOLKIT/README.md).

```bash
# Beispiele
./_TOOLKIT/new-pa.sh Babynames --wpf --console --network --db   # wie PA3
./_TOOLKIT/new-pa.sh Formen     --wpf --shapes                  # wie PA2
./_TOOLKIT/new-pa.sh Robot      --wpf --parser                  # wie PA4_Robot
./_TOOLKIT/new-pa.sh Fotos      --wpf --db                      # wie PA4_Bilder
```
(Windows: `_TOOLKIT\new-pa.bat ...`)

## Die wichtigsten Lehren aus den Bewertungen

- **WPF-Layout:** `Grid`/`DockPanel` statt `StackPanel`; keine fixen Pixel-Höhen.
- **`ActualWidth`/`Height` erst im `Loaded`-Event** verfügbar (im Konstruktor 0).
- **`Transfer<T>`:** Länge vor dem XML, nur ein Empfangsweg, ein Transfer pro Verbindung.
- **Daten an die GUI:** `ItemsSource` setzen (nicht ganze Liste per `Items.Add`), `SelectionChanged` für Folgeaktionen.
- **`LengthConverter` nur für Längen**; DependencyProperty-Typ == Property-Typ.
- **Parser:** optionale Keywords (`ELSE`) nur bei exakter Übereinstimmung konsumieren; gemeinsamer Zustand in die Basisklasse.
