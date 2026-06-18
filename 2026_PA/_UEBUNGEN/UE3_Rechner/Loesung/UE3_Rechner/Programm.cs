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
            // Solange Tokens da sind: Anweisung fuer Anweisung lesen.
            // Ein schliessendes ')' am Anfang beendet den aktuellen
            // (Klammer-)Block - hier i.d.R. nicht relevant, schadet aber nicht.
            while (tokens.Count > 0)
            {
                Token token = tokens.First();

                if (token.Type == Token.TokenType.CLOSE_PAREN)
                {
                    // Gehoert zu einer umgebenden Klammer -> Block beenden.
                    return;
                }

                if (token.Type == Token.TokenType.KEYWORD && token.Value == "LET")
                {
                    // Fuehrendes "LET" hier entfernen; LetStatement startet beim Namen.
                    tokens.RemoveAt(0);
                    LetStatement let = new LetStatement();
                    let.Parse(tokens);
                    statements.Add(let);
                }
                else
                {
                    // Jede andere Anweisung ist ein arithmetischer Ausdruck.
                    AddExpression expr = new AddExpression();
                    expr.Parse(tokens);
                    statements.Add(expr);
                }
            }
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
    // (Das fuehrende "LET" wurde bereits vom Programm-Dispatcher entfernt.)
    public class LetStatement : Expression
    {
        private string name = "";
        private Expression value = new AddExpression();

        public override void Parse(List<Token> tokens)
        {
            // 1) Name-Token lesen und merken.
            if (tokens.Count > 0 && tokens[0].Type == Token.TokenType.NAME)
            {
                name = tokens[0].Value;
                tokens.RemoveAt(0);
            }
            else
            {
                Errors.Add("LET: Variablenname erwartet, gefunden: "
                    + (tokens.Count > 0 ? tokens[0].Value : "Ende der Eingabe"));
                return;
            }

            // 2) '=' erwarten und entfernen.
            if (tokens.Count > 0 && tokens[0].Type == Token.TokenType.ASSIGN)
            {
                tokens.RemoveAt(0);
            }
            else
            {
                Errors.Add("LET " + name + ": '=' erwartet, gefunden: "
                    + (tokens.Count > 0 ? tokens[0].Value : "Ende der Eingabe"));
                return;
            }

            // 3) Den zuzuweisenden Ausdruck parsen.
            value = new AddExpression();
            value.Parse(tokens);
        }

        public override double Evaluate()
        {
            // Wert berechnen, in der Umgebung merken und zurueckgeben.
            double result = value.Evaluate();
            Variables[name] = result;
            return result;
        }
    }
}
