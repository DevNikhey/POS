using System.Collections.Generic;
using System.Globalization;

namespace UE3_Rechner
{
    // <expr> ::= <term> { (+|-) <term> }
    public class AddExpression : Expression
    {
        // Tipp zur Struktur: erster Term + Liste aus (Operator, Term).
        private Expression first = new MulExpression();
        private List<(string op, Expression term)> rest = new List<(string, Expression)>();

        public override void Parse(List<Token> tokens)
        {
            // ---------------------------------------------------------
            // AUFGABE 3: Strich-Rechnung (+, -)
            //   1) first = new MulExpression(); first.Parse(tokens);
            //   2) solange vorne ein OPERATOR "+" oder "-" steht:
            //        Operator merken + RemoveAt(0);
            //        new MulExpression().Parse(tokens) lesen und in rest legen.
            // ---------------------------------------------------------
            throw new System.NotImplementedException("AddExpression.Parse noch nicht implementiert (Aufgabe 3)");
        }

        public override double Evaluate()
        {
            // TODO: first.Evaluate() nehmen und die rest-Liste mit +/-
            //       der Reihe nach verrechnen.
            throw new System.NotImplementedException("AddExpression.Evaluate noch nicht implementiert (Aufgabe 3)");
        }
    }

    // <term> ::= <factor> { (*|/) <factor> }
    public class MulExpression : Expression
    {
        private Expression first = new FactorExpression();
        private List<(string op, Expression factor)> rest = new List<(string, Expression)>();

        public override void Parse(List<Token> tokens)
        {
            // ---------------------------------------------------------
            // AUFGABE 3: Punkt-Rechnung (*, /)  - analog zu AddExpression,
            //   aber mit FactorExpression und den Operatoren "*" / "/".
            //   Dass * und / staerker binden als + und -, ergibt sich
            //   automatisch aus der Aufruf-Reihenfolge Add -> Mul -> Factor.
            // ---------------------------------------------------------
            throw new System.NotImplementedException("MulExpression.Parse noch nicht implementiert (Aufgabe 3)");
        }

        public override double Evaluate()
        {
            // TODO: first.Evaluate() nehmen und die rest-Liste mit *//
            //       der Reihe nach verrechnen.
            throw new System.NotImplementedException("MulExpression.Evaluate noch nicht implementiert (Aufgabe 3)");
        }
    }

    // <factor> ::= <number> | <name> | '(' <expr> ')' | <if>
    public class FactorExpression : Expression
    {
        private double number = 0;          // falls Zahl
        private string name = "";           // falls Variable
        private Expression? inner = null;   // falls Klammer oder IF
        private enum Kind { Number, Name, Inner }
        private Kind kind = Kind.Number;

        public override void Parse(List<Token> tokens)
        {
            // ---------------------------------------------------------
            // AUFGABE 3: Faktor
            //   Schau auf tokens[0].Type:
            //     NUMBER     -> double.Parse(value, CultureInfo.InvariantCulture); RemoveAt(0)
            //     NAME       -> Variablen-Name merken; RemoveAt(0)
            //     OPEN_PAREN -> '(' entfernen, inner = new AddExpression();
            //                   inner.Parse(tokens); danach ')' erwarten + entfernen
            //     KEYWORD "IF" -> inner = new IfExpression(); inner.Parse(tokens)
            //   Bei unerwartetem Token: Errors.Add(...).
            // ---------------------------------------------------------
            throw new System.NotImplementedException("FactorExpression.Parse noch nicht implementiert (Aufgabe 3)");
        }

        public override double Evaluate()
        {
            // TODO: je nach kind den Zahlenwert / Variablenwert (aus
            //       Variables) / inner.Evaluate() zurueckgeben.
            //       Unbekannte Variable -> Errors.Add(...), 0 zurueckgeben.
            throw new System.NotImplementedException("FactorExpression.Evaluate noch nicht implementiert (Aufgabe 3)");
        }
    }

    // <if> ::= IF <expr> THEN <expr> [ ELSE <expr> ]
    // (Das fuehrende "IF" wird vom FactorExpression-Aufrufer ODER hier
    //  entfernt - lege das konsistent fest.)
    public class IfExpression : Expression
    {
        private Expression condition = new AddExpression();
        private Expression thenExpr = new AddExpression();
        private Expression? elseExpr = null;   // optional!

        public override void Parse(List<Token> tokens)
        {
            // ---------------------------------------------------------
            // AUFGABE 4: Bedingung mit OPTIONALEM ELSE
            //   1) condition = new AddExpression(); condition.Parse(tokens);
            //   2) THEN erwarten (Keyword "THEN") und entfernen.
            //   3) thenExpr = new AddExpression(); thenExpr.Parse(tokens);
            //   4) FALLSTRICK: ELSE NUR konsumieren, wenn das naechste Token
            //      WIRKLICH das Keyword "ELSE" ist:
            //        if (tokens.Count > 0
            //            && tokens[0].Type == Token.TokenType.KEYWORD
            //            && tokens[0].Value == "ELSE")
            //        { RemoveAt(0); elseExpr = new AddExpression(); elseExpr.Parse(tokens); }
            //      Sonst Tokens UNANGETASTET lassen - das naechste Keyword
            //      gehoert schon zur naechsten Anweisung!
            // ---------------------------------------------------------
            throw new System.NotImplementedException("IfExpression.Parse noch nicht implementiert (Aufgabe 4)");
        }

        public override double Evaluate()
        {
            // TODO: condition.Evaluate() != 0  -> thenExpr.Evaluate()
            //       sonst -> elseExpr?.Evaluate() ?? 0
            throw new System.NotImplementedException("IfExpression.Evaluate noch nicht implementiert (Aufgabe 4)");
        }
    }
}
