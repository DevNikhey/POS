# UE3_Rechner — Mini-Taschenrechner-Sprache

## Szenario

Du baust einen kleinen **Interpreter für eine Rechen-Sprache**. Der Benutzer tippt
ein „Programm" aus mehreren Zeilen in eine TextBox, z. B.:

```
LET a = 3 + 4 * 2
LET b = ( a - 1 ) / 2
IF b THEN a + 100 ELSE a - 100
```

Die App macht daraus — wie bei jedem echten Compiler/Interpreter — in **drei Schritten** ein Ergebnis:

1. **Tokenisieren** — den Text mit einer Regex in Tokens (Zahlen, Operatoren, Klammern, Namen, Schlüsselwörter) zerlegen.
2. **Parsen** — aus den Tokens nach der Grammatik (siehe `Grammatik.txt`) einen Baum aus `Expression`-Objekten bauen (Recursive Descent).
3. **Ausführen** — den Baum mit `Evaluate(...)` auswerten und das Ergebnis jeder Anweisung anzeigen.

Die Sprache kennt:
- **Punkt-vor-Strich** (`*`, `/` binden stärker als `+`, `-`),
- **Klammern** `( ... )`,
- **Variablen** über `LET name = expr` (Wert wird in einer `Dictionary<string,double>`-Umgebung gemerkt und in späteren Zeilen wieder verwendet),
- einen **Bedingungs-Ausdruck** `IF expr THEN expr [ELSE expr]` (der ELSE-Teil ist **optional**; Bedingung gilt als „wahr", wenn der Wert ungleich 0 ist; ohne ELSE ist der Else-Wert 0).

## Vorgegeben

Vom Toolkit (`--parser`) bzw. in diesem Skelett bereits enthalten und **kompilierbar**:

- `Token.cs` — `Token` mit `Type` (`KEYWORD`, `OPEN_BRACE`, `CLOSE_BRACE`, `LETTER`, `NUMBER`, `ERROR`) und `Value`. Für diese Übung sind die Token-Typen sinnvoll umbenannt/erweitert (Operatoren, Klammern, Zahl, Name, Keyword). Die Methode `Token.Tokenize(...)` ist als **Stub** vorgegeben.
- `Expression.cs` — abstrakte Basisklasse mit `abstract Parse(List<Token>)`, einer **statischen** Fehlerliste `Errors` und einer **statischen** Variablen-Umgebung `Variables` (`Dictionary<string,double>`). Dazu eine virtuelle `Evaluate(...)`, die einen `double` zurückgibt.
- `Programm.cs`, `Calc.cs` — die Expression-Unterklassen (Dispatcher, `LetStatement`, `AddExpression`, `MulExpression`, `FactorExpression`, `IfExpression`) als **Stubs** mit TODOs.
- `MainWindow.xaml` / `MainWindow.xaml.cs` — die GUI-Shell mit Eingabe-TextBox, Token-Liste, Ausgabe und „Run"-Button. Der `Button_Click`-Ablauf (Tokenisieren → Parsen → Ausführen) ist als **Stub** vorgegeben.
- `Grammatik.txt` — die vollständige Grammatik in BNF.

> Das Skelett **kompiliert** sofort (alle TODO-Stellen werfen `NotImplementedException` oder liefern Defaultwerte). Deine Aufgabe ist es, die Logik zu füllen.

## Aufgaben (Summe: 60 Punkte)

### Aufgabe 1 — Tokenizer (10 P)
Implementiere `Token.Tokenize(string input)` in `Token.cs`.
- Zerlege den Text mit **einer** Regex in: Schlüsselwörter (`LET`, `IF`, `THEN`, `ELSE`), Operatoren (`+ - * /`), Klammern (`(` `)`), Zuweisung (`=`), Zahlen (`\d+` ggf. mit Dezimalpunkt) und Namen (`[a-zA-Z]\w*`).
- Setze für jeden Match den passenden `TokenType`. Alles Unbekannte wird `ERROR`.
- **Reihenfolge in der Regex beachten:** Schlüsselwörter müssen vor dem allgemeinen Namens-Muster stehen, sonst „klaut" `[a-zA-Z]\w*` z. B. das `IF`.

### Aufgabe 2 — Programm/Statements & LET (12 P)
Implementiere `Programm.Parse` und `LetStatement` in `Programm.cs`.
- `Programm.Parse` liest **so lange Anweisungen**, bis die Tokenliste leer (bzw. ein `}`/Ende) ist, und sammelt sie in einer `List<Expression>`.
- Beginnt eine Zeile mit dem Keyword `LET`, wird ein `LetStatement` erzeugt; sonst ein arithmetischer Ausdruck (`AddExpression`).
- Jede `Parse`-Methode **konsumiert ihre Tokens vorne** aus der Liste (`tokens.RemoveAt(0)`).
- `LetStatement.Parse` liest `LET name = <expr>`; `Evaluate` berechnet `<expr>`, speichert das Ergebnis in `Variables[name]` und gibt es zurück.

### Aufgabe 3 — Arithmetik mit Punkt-vor-Strich (16 P)
Implementiere `AddExpression`, `MulExpression` und `FactorExpression` in `Calc.cs`.
- `AddExpression`: `<term> { (+|-) <term> }` — ruft `MulExpression` für die Terme auf.
- `MulExpression`: `<factor> { (*|/) <factor> }` — ruft `FactorExpression` für die Faktoren auf.
- `FactorExpression`: eine **Zahl**, ein **Name** (Variable aus `Variables`), eine **Klammer** `( <AddExpression> )` oder ein **IfExpression** (Keyword `IF`).
- `Evaluate` wertet den jeweiligen Knoten aus und liefert den `double`-Wert. Achte darauf, dass `*`/`/` stärker binden als `+`/`-` — das ergibt sich automatisch aus der Aufruf-Reihenfolge Add → Mul → Factor.

### Aufgabe 4 — Bedingung mit optionalem ELSE (14 P)
Implementiere `IfExpression` in `Calc.cs` (Grammatik: `IF <expr> THEN <expr> [ ELSE <expr> ]`).
- `Parse`: lies die Bedingung (`AddExpression`), erwarte/konsumiere `THEN`, lies den Then-Ausdruck.
- **Fallstrick (wird streng bewertet):** Den `ELSE`-Teil **nur dann** parsen, wenn das nächste Token **wirklich** das Keyword `ELSE` ist. Prüfe vor `RemoveAt(0)` explizit `tokens[0].Value == "ELSE"` — sonst frisst dein Parser das Keyword der **nächsten Anweisung** (z. B. `LET`/`IF`) fälschlich als ELSE.
- `Evaluate`: ist die Bedingung ≠ 0, liefere den Then-Wert, sonst den Else-Wert (ohne ELSE: 0).

### Aufgabe 5 — GUI-Verdrahtung (8 P)
Implementiere `Button_Click` in `MainWindow.xaml.cs`.
- Schritt 1: `Token.Tokenize(InputTextBox.Text)` aufrufen, Ergebnis an `TokensListBox.ItemsSource` binden.
- Schritt 1.5: Gibt es `ERROR`-Tokens, per `MessageBox` melden und abbrechen.
- Schritt 2: ein `Programm` erzeugen und `Parse` aufrufen (Liste ohne `ERROR`-Tokens übergeben). Danach `Expression.Errors` prüfen und ggf. anzeigen + `Clear()`.
- Schritt 3: vor dem Lauf `Expression.Variables.Clear()`; dann `Evaluate` aufrufen und das/die Ergebnis(se) in `OutputTextBox` schreiben.

## Hinweise

- **Lange/spezielle Schlüsselwörter zuerst in der Regex.** `LET|IF|THEN|ELSE` vor `[a-zA-Z]\w*`, sonst matcht der Namens-Teil die Keywords.
- **Recursive Descent = eine Klasse pro Grammatik-Regel.** Add ruft Mul, Mul ruft Factor, Factor ruft (bei Klammern) wieder Add → so entsteht die Rekursion und automatisch die korrekte Vorrang-Reihenfolge.
- **Tokens immer vorne konsumieren** (`tokens[0]` lesen, dann `tokens.RemoveAt(0)`). Vor jedem Zugriff `tokens.Count > 0` prüfen, sonst `IndexOutOfRange`.
- **Optionales ELSE:** vor dem `RemoveAt(0)` gezielt auf `Value == "ELSE"` prüfen, nicht nur auf `Type == KEYWORD`. Das ist der klassische Fehler.
- **Gemeinsamer Zustand gehört in `Expression`** (statische `Errors`-Liste, `Variables`-Umgebung), **nicht** in die GUI/`MainWindow`. Greife aus den Knoten direkt auf `Variables`/`Errors` zu.
- **WPF baut/startet nur unter Windows.** Auf macOS/Linux kannst du die Struktur erzeugen; gebaut/getestet wird unter Windows.

## Setup

```bash
./_TOOLKIT/new-pa.sh UE3_Rechner --wpf --parser
```

Danach die mitgelieferten **Vorgabe-Dateien ins Projekt kopieren** (sie ersetzen die vom Toolkit erzeugten `Token.cs`/`Expression.cs` und ergänzen die übrigen Dateien) — alle liegen im Projektordner `UE3_Rechner/`:

- `UE3_Rechner/Token.cs`
- `UE3_Rechner/Expression.cs`
- `UE3_Rechner/Programm.cs`
- `UE3_Rechner/Calc.cs`
- `UE3_Rechner/MainWindow.xaml`
- `UE3_Rechner/MainWindow.xaml.cs`
- `UE3_Rechner/Grammatik.txt`

Build (unter Windows):

```bash
dotnet build
```

Das Skelett kompiliert sofort; an den TODO-Stellen wird zur Laufzeit `NotImplementedException` geworfen, bis du sie implementiert hast.