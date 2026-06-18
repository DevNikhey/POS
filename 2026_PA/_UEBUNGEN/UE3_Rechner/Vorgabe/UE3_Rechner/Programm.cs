using System.Collections.Generic;
using System.Linq;

namespace UE3_Rechner
{
    // <program>   ::= { <statement> }
    // <statement> ::= LET <name> = <expr> | <expr>
    //
    // Der "Dispatcher": liest Anweisung fuer Anweisung und erzeugt je
    // nach erstem Token entweder ein LetStatement oder einen Ausdruck.
    public class Programm : Expression
    {
        private List<Expression> statements = new List<Expression>();

        // Ergebnis der zuletzt ausgewerteten Anweisung (fuer die GUI).
        public List<double> Results { get; } = new List<double>();

        public override void Parse(List<Token> tokens)
        {
            // ---------------------------------------------------------
            // AUFGABE 2: Programm/Statements
            //
            // Solange Tokens da sind (und kein schliessendes ')' am Anfang
            // steht, falls du Bloecke ergaenzt):
            //   - beginnt die Anweisung mit Keyword "LET" -> LetStatement
            //   - sonst -> AddExpression (arithmetischer Ausdruck)
            //   jede Unter-Parse-Methode aufrufen und in "statements" sammeln.
            //   Denk daran: das "LET"-Token vor dem LetStatement entfernen,
            //   bzw. die Tokens werden von der jeweiligen Parse-Methode
            //   selbst vorne konsumiert (RemoveAt(0)).
            // ---------------------------------------------------------
            throw new System.NotImplementedException("Programm.Parse noch nicht implementiert (Aufgabe 2)");
        }

        public override double Evaluate()
        {
            Results.Clear();
            double last = 0;
            foreach (Expression s in statements)
            {
                last = s.Evaluate();
                Results.Add(last);
            }
            return last;
        }
    }

    // <statement> ::= LET <name> = <expr>
    // (Das fuehrende "LET" wurde bereits vom Programm-Dispatcher entfernt
    //  ODER wird hier entfernt - lege das im Dispatcher konsistent fest.)
    public class LetStatement : Expression
    {
        private string name = "";
        private Expression value = new AddExpression();

        public override void Parse(List<Token> tokens)
        {
            // ---------------------------------------------------------
            // AUFGABE 2: LET <name> = <expr>
            //   1) Name-Token lesen (Type == NAME) und merken; RemoveAt(0).
            //   2) '='-Token erwarten (Type == ASSIGN); RemoveAt(0).
            //   3) value = new AddExpression(); value.Parse(tokens);
            //   Bei fehlenden/falschen Tokens: Errors.Add(...).
            // ---------------------------------------------------------
            throw new System.NotImplementedException("LetStatement.Parse noch nicht implementiert (Aufgabe 2)");
        }

        public override double Evaluate()
        {
            // TODO: Wert berechnen (value.Evaluate()), in Variables[name]
            //       speichern und zurueckgeben.
            throw new System.NotImplementedException("LetStatement.Evaluate noch nicht implementiert (Aufgabe 2)");
        }
    }
}
