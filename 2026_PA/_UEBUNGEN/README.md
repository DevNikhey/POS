# Übungs-PAs – eine pro Typ zum Selbermachen

Fünf **ungelöste** Übungs-PAs (Skelett mit `// TODO` / `throw new NotImplementedException();`),
je eine pro Archetyp, jeweils mit **anderer Domäne** als die Original-PAs. Jede Übung hat:
- `Aufgabenstellung.md` – Szenario, „Vorgegeben", **Aufgaben mit Punkten**, Hinweise (Fallstricke), Setup.
- `Vorgabe/` – die Skelett-Dateien, die du ins erzeugte Projekt kopierst.

| Übung | Typ | Was du übst | Setup |
|-------|-----|-------------|-------|
| **UE1_Chat** | Client/Server (TCP) | Live-Umfrage: mehrere Clients, **Broadcast**, `Transfer<T>`, `Dispatcher` | `new-pa.sh UE1_Chat --wpf --console --network` |
| **UE2_Formen** | WPF-Zeichnen | Eigene `Shape`s (Vieleck, Stern): DependencyProperties + `CreatePathFigure` | `new-pa.sh UE2_Formen --wpf --shapes` |
| **UE3_Rechner** | Parser/Interpreter | Mini-Rechner-Sprache: Tokenizer, Recursive-Descent, Execute (inkl. optionalem Keyword) | `new-pa.sh UE3_Rechner --wpf --parser` |
| **UE4_Medien** | WPF + linq2db/SQLite | Filmsammlung: laden/anzeigen (`ItemsSource`+`DataTemplate`), Auswahl, `InsertWithInt64Identity`, `Loaded` | `new-pa.sh UE4_Medien --wpf --db` |
| **UE5_Parkhaus** | Threading/Sync | Parkhaus: `SemaphoreSlim`, `lock`, Tag/Nacht, Laden, Kassa | `new-pa.sh UE5_Parkhaus --wpf` |

## So gehst du eine Übung an

```bash
# 1) Buildfähige Basis erzeugen (Beispiel UE1):
./_TOOLKIT/new-pa.sh UE1_Chat --wpf --console --network

# 2) Vorgabe-Dateien in den erzeugten Solution-Ordner kopieren (Inhalt von Vorgabe/!):
cp -R 2026_PA/_UEBUNGEN/UE1_Chat/Vorgabe/* UE1_Chat/        # macOS/Linux
#  Windows:  xcopy /E /Y 2026_PA\_UEBUNGEN\UE1_Chat\Vorgabe\* UE1_Chat\
```
> **Wichtig zur Kopier-Basis:** Der Inhalt von `Vorgabe/` ist **relativ zum Solution-Ordner** aufgebaut
> (z. B. `Vorgabe/Network/MSG.cs`, `Vorgabe/UE1_Chat/MainWindow.xaml.cs`). Beim Kopieren von `Vorgabe/*`
> **in** den von `new-pa` erzeugten Ordner `UE1_Chat/` landen alle Dateien automatisch im richtigen
> Teilprojekt (überschreiben die Default-`MainWindow` usw.).

```bash
# 3) Aufgabenstellung lesen, TODOs implementieren. Vor der Abgabe:
./_TOOLKIT/pa-ready.sh ./UE1_Chat
```

- **UE4_Medien**: Die fertige `movies.db` liegt schon in `Vorgabe/UE4_Medien/`. Das **Datenmodell**
  erzeugst du dir (wie in der echten PA) mit `./_TOOLKIT/scaffold-db.sh` bzw. `./_TOOLKIT/db-tt.sh`
  (siehe `MODELL_HINWEIS.md` in der Übung). `CopyToOutputDirectory=PreserveNewest` nicht vergessen
  (`./_TOOLKIT/db-copycheck.sh … --fix`).
- WPF baut/läuft nur unter **Windows**.

## Lösungen
Bewusst **nicht** dabei — es ist zum Üben. Wenn du eine Übung gemacht hast und sie prüfen lassen
willst, oder eine **Musterlösung** brauchst, sag einfach Bescheid (am besten pro Übung einzeln).
Zum Selbst-Check während des Lösens: `pa-check`, `lock-check` (UE5), `find-todo`, `pa-ready`.
