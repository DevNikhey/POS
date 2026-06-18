# Roboter-Sprache: Tokenizer, Parser & Interpreter (PA4_Robot)

Diese Anwendung liest einen Text in einer kleinen **Roboter-Befehlssprache** (z. B. `MOVE DOWN`, `REPEAT 4 { ... }`, `IF DOWN IS-A OBSTACLE { ... } ELSE { ... }`), zerlegt ihn in Tokens, baut daraus einen Befehlsbaum und führt ihn auf einem Roboter-Spielfeld aus.

## Worum geht es?

Der Benutzer tippt ein „Programm" in eine TextBox. Die App macht daraus in **drei Schritten** echte Roboter-Bewegungen:

1. **Tokenisieren** – den Text in einzelne Bausteine (Tokens) zerlegen.
2. **Parsen** – aus den Tokens einen Baum aus `Expression`-Objekten bauen (Recursive Descent nach der Grammatik in `Grammatik.txt`).
3. **Ausführen** – den Baum abarbeiten und dabei `RobotField`-Methoden (`Move`, `Collect`, `Place`, `IsObstacle`, `IsLetter`) aufrufen (Interpreter-Pattern).

Das ist ein Mini-**Compiler-/Interpreter-Bau** – dasselbe Prinzip wie bei echten Programmiersprachen, nur stark vereinfacht.

## Dateien & Aufbau

| Datei | Rolle |
|-------|-------|
| `Grammatik.txt` | Die Grammatik in BNF: welche Befehle es gibt und wie sie aufgebaut sind. |
| `MainWindow.xaml(.cs)` | GUI + die 3 Schritte: Tokenisieren (Regex), Parsen, Ausführen (im `ThreadPool`). |
| `Token.cs` | Ein Token: `Type` (KEYWORD, OPEN_BRACE, CLOSE_BRACE, LETTER, NUMBER, ERROR) + `Value`. |
| `Expression.cs` | Abstrakte Basisklasse: `Parse(...)`, `Execute(...)` und die **statische** Fehlerliste `Errors`. |
| `Programm.cs` | Der „Dispatcher": liest das nächste Keyword und erzeugt die passende Expression. Ist selbst eine `Expression`. |
| `Block.cs` | Ein Block `{ ... }` – enthält wieder ein `Programm`. |
| `Condition.cs` | Eine Bedingung `<direction> IS-A <object>`, plus `Evaluate(...)`. |
| `IfExpression.cs` | `IF <condition> <block> [ELSE <block>]`. |
| `RepeatExpression.cs` | `REPEAT <number> <block>` (Zählschleife). |
| `UntilExpression.cs` | `UNTIL <condition> <block>` (Schleife, bis die Bedingung wahr ist). |
| `MoveExpression.cs` | `MOVE <direction>`. |
| `CollectExpression.cs` / `PlaceExpression.cs` | `COLLECT` / `PLACE` (Gegenstand aufnehmen / ablegen). |
| `RobotLibrary.dll` | Vorgegeben: das Spielfeld-Control `RobotField` mit `Move/Collect/Place/IsObstacle/IsLetter`. |

## Wie funktioniert es?

### Schritt 1 – Tokenisieren (Regex)

Im `Button_Click` wird der Text mit einem regulären Ausdruck in Tokens zerlegt und jedem Token ein Typ gegeben:

```csharp
Regex regex = new Regex(@"REPEAT|MOVE|RIGHT|DOWN|LEFT|UP|COLLECT|PLACE|UNTIL|IS-A|IF|ELSE|OBSTACLE|{|}|[A-Z]|\d+|\S+");
Regex keywords = new Regex(@"REPEAT|MOVE|RIGHT|DOWN|LEFT|UP|COLLECT|PLACE|UNTIL|IS-A|IF|ELSE|OBSTACLE");
// ... pro Match ein Token erzeugen und Type setzen (KEYWORD / OPEN_BRACE / ... / ERROR)
```

Aus `MOVE DOWN` werden so zwei `KEYWORD`-Tokens. Aus `{` wird `OPEN_BRACE`, aus `4` ein `NUMBER`, aus `A` ein `LETTER`. Alles Unbekannte wird `ERROR`.

### Schritt 2 – Parsen (Recursive Descent)

Jede Grammatik-Regel ist eine eigene `Expression`-Klasse. `Programm.Parse` schaut auf das **erste** Token und erzeugt je nach Keyword die passende Expression. Wichtig: Jede `Parse`-Methode **entfernt** die Tokens, die zu ihr gehören, vorne aus der Liste (`tokens.RemoveAt(0)`):

```csharp
Token token = tokens.First();
switch (token.Value)
{
    case "MOVE":
        MoveExpression moveExpression = new MoveExpression();
        tokens.RemoveAt(0);                 // "MOVE" wegnehmen
        moveExpression.Parse(tokens);       // die Richtung lesen
        expressions.Add(moveExpression);
        break;
    case "REPEAT": /* ... */ break;
    // ...
}
```

Ein `Block` ist rekursiv definiert – er enthält wieder ein ganzes `Programm`:

```csharp
// Block.cs
tokens.RemoveAt(0);          // '{'
programm.Parse(tokens);      // Inhalt parsen (ruft sich indirekt selbst auf)
// danach muss '}' folgen
```

Daher der Name **Recursive Descent**: Die Parse-Methoden rufen sich gegenseitig entsprechend der Grammatik auf.

### Schritt 3 – Ausführen (Interpreter-Pattern)

Nach dem Parsen existiert ein Baum aus `Expression`-Objekten. `Execute` arbeitet ihn ab und ruft die Spielfeld-Methoden auf. Beispiele:

```csharp
// RepeatExpression: feste Anzahl
for (int i = 0; i < count; i++) block.Execute(roboter);

// UntilExpression: solange die Bedingung NICHT erfüllt ist
while (!condition.Evaluate(roboter)) block.Execute(roboter);

// MoveExpression: eine Bewegung
success = roboter.Move(RobotField.Direction.Down);
```

Ausgeführt wird im Hintergrund-Thread (`ThreadPool.QueueUserWorkItem`), damit die GUI während der Roboter-Animation nicht einfriert. Fehler landen währenddessen in der statischen Liste `Expression.Errors` und werden danach per `MessageBox` gezeigt.

## Die wichtigsten Konzepte

- **Tokenizer (lexikalische Analyse).** Rohtext → Liste typisierter Tokens. Eine Regex mit `|` probiert die Alternativen der Reihe nach; lange Schlüsselwörter stehen darum **vor** dem allgemeinen `\S+`.
- **Recursive-Descent-Parser.** Jede Grammatik-Regel = eine Methode/Klasse, die genau „ihre" Tokens konsumiert und sich für Unterregeln rekursiv aufruft. Das Token-Listen-Muster „vorne wegnehmen" (`RemoveAt(0)`) hält den Parse-Zustand simpel.
- **Interpreter-Pattern.** Der Syntaxbaum aus Objekten wird durch `Execute(...)` direkt ausgeführt (statt z. B. Maschinencode zu erzeugen). `Parse` baut den Baum, `Execute` läuft ihn ab.
- **Gemeinsamer Zustand in der Basisklasse.** Dinge, die alle Expressions brauchen (Fehlerliste, „zuletzt aufgesammelt"), gehören in die gemeinsame Basisklasse `Expression` – **nicht** in die GUI.
- **Optionale Teile vorsichtig parsen.** Bei optionalen Sprachteilen (wie `ELSE`) darf man nur dann ein Token konsumieren, wenn es **wirklich** dieses Schlüsselwort ist – sonst frisst man den Anfang der nächsten Anweisung.

## Bewertung: Fehler & Korrekturen

| Aufgabe | Bemängelt | Korrektur |
|---------|-----------|-----------|
| A5 (9) | „prüfen ob das Keyword ELSE ist, es können auch andere Keywords kommen". | In `IfExpression.Parse` nicht jedes Keyword als ELSE behandeln, sondern gezielt auf `"ELSE"` prüfen. |
| A7 (10) | „die static Variable besser in der Expression platzieren". | `previouslyCollected` von `MainWindow` (GUI) in die Basisklasse `Expression` verschieben. |

### A5 — Nur ein echtes `ELSE` konsumieren

**Warum?** Nach `IF <condition> <block>` prüft das Original nur, ob als Nächstes **irgendein** Keyword kommt, entfernt es und parst einen else-Block. Nach dem if-Block folgt aber sehr oft schon die **nächste Anweisung** (z. B. `MOVE`, `UNTIL`, `IF`), die ebenfalls mit einem Keyword beginnt. Dann frisst der Parser dieses Keyword fälschlich als „ELSE" und das Programm wird falsch interpretiert.

**Vorher** (`IfExpression.cs`):

```csharp
internal override void Parse(List<Token> tokens)
{
    condition.Parse(tokens);
    block.Parse(tokens);

    if (tokens.Count == 0 || tokens[0].Type != Token.TokenType.KEYWORD)
    {
        return;
    }
    tokens.RemoveAt(0);          // entfernt JEDES Keyword, als wäre es ELSE!

    elseblock.Parse(tokens);
}
```

**Nachher:**

```csharp
internal override void Parse(List<Token> tokens)
{
    condition.Parse(tokens);
    block.Parse(tokens);

    // Nur wenn WIRKLICH das Keyword ELSE folgt, einen else-Block parsen.
    // (Sonst ist das nächste Keyword schon der Beginn der nächsten Anweisung!)
    if (tokens.Count > 0
        && tokens[0].Type == Token.TokenType.KEYWORD
        && tokens[0].Value == "ELSE")
    {
        tokens.RemoveAt(0);     // das ELSE-Token entfernen
        elseblock.Parse(tokens);
    }
    // kein ELSE -> Tokens unangetastet lassen
}
```

`Execute` bleibt unverändert: Wurde kein `ELSE` geparst, ist `elseblock` ein leerer Block und tut beim Ausführen schlicht nichts.

### A7 — Statisches Feld in die `Expression`-Basisklasse

**Warum?** `COLLECT` merkt sich den aufgesammelten Gegenstand, `PLACE` legt ihn wieder ab. Dieser gemeinsame Zustand liegt im Original als `public static` in **MainWindow** (also in der GUI), und `CollectExpression`/`PlaceExpression` greifen über `MainWindow.previouslyCollected` darauf zu. Damit hängt der Interpreter an der GUI. Sauberer: das Feld dorthin legen, wo es fachlich hingehört – in die gemeinsame Basisklasse `Expression` (genau wie die schon vorhandene statische `Errors`-Liste).

**Vorher** (`MainWindow.xaml.cs` + Zugriffe):

```csharp
// MainWindow.xaml.cs
public static string previouslyCollected = String.Empty;

// CollectExpression.Execute
MainWindow.previouslyCollected = result;
// PlaceExpression.Execute
string result = MainWindow.previouslyCollected;
```

**Nachher** (`Expression.cs` + Zugriffe ohne `MainWindow.`):

```csharp
// Expression.cs
internal abstract class Expression
{
    internal static List<String> Errors = new List<string>();
    internal static string previouslyCollected = String.Empty;   // <-- hierher
    // ...
}

// CollectExpression.Execute
previouslyCollected = result;
// PlaceExpression.Execute
string result = previouslyCollected;
// und in MainWindow.xaml.cs das statische Feld entfernen.
```

## Was du fürs nächste Mal mitnimmst

- **Drei Phasen merken: Tokenize → Parse → Execute.** Das ist das Grundgerüst jedes Interpreters/Compilers.
- **Recursive Descent = eine Klasse/Methode pro Grammatik-Regel.** Jede konsumiert nur ihre Tokens; Blöcke rufen rekursiv das ganze Programm auf.
- **Optionales nur bei exakter Übereinstimmung konsumieren.** Vor `RemoveAt(0)` immer den konkreten `Value` prüfen (`== "ELSE"`), nicht nur den Token-Typ.
- **Gemeinsamer Zustand gehört in die Basisklasse, nicht in die GUI.** Parser/Interpreter sollen ohne MainWindow funktionieren.
- **Lange Schlüsselwörter zuerst in der Regex**, sonst „klaut" ein allgemeineres Muster (`\S+`, `[A-Z]`) den Match.
