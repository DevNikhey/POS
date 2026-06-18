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
            // 1) ersten Term lesen.
            first = new MulExpression();
            first.Parse(tokens);

            // 2) solange vorne ein "+" oder "-" steht: Operator + weiterer Term.
            while (tokens.Count > 0
                   && tokens[0].Type == Token.TokenType.OPERATOR
                   && (tokens[0].Value == "+" || tokens[0].Value == "-"))
            {
                string op = tokens[0].Value;
                tokens.RemoveAt(0);

                Expression term = new MulExpression();
                term.Parse(tokens);
                rest.Add((op, term));
            }
        }

        public override double Evaluate()
        {
            double result = first.Evaluate();
            foreach (var (op, term) in rest)
            {
                double value = term.Evaluate();
                if (op == "+")
                {
                    result += value;
                }
                else // "-"
                {
                    result -= value;
                }
            }
            return result;
        }
    }

    // <term> ::= <factor> { (*|/) <factor> }
    public class MulExpression : Expression
    {
        private Expression first = new FactorExpression();
        private List<(string op, Expression factor)> rest = new List<(string, Expression)>();

        public override void Parse(List<Token> tokens)
        {
            // 1) ersten Faktor lesen.
            first = new FactorExpression();
            first.Parse(tokens);

            // 2) solange vorne ein "*" oder "/" steht: Operator + weiterer Faktor.
            while (tokens.Count > 0
                   && tokens[0].Type == Token.TokenType.OPERATOR
                   && (tokens[0].Value == "*" || tokens[0].Value == "/"))
            {
                string op = tokens[0].Value;
                tokens.RemoveAt(0);

                Expression factor = new FactorExpression();
                factor.Parse(tokens);
                rest.Add((op, factor));
            }
        }

        public override double Evaluate()
        {
            double result = first.Evaluate();
            foreach (var (op, factor) in rest)
            {
                double value = factor.Evaluate();
                if (op == "*")
                {
                    result *= value;
                }
                else // "/"
                {
                    result /= value;
                }
            }
            return result;
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
            if (tokens.Count == 0)
            {
                Errors.Add("Faktor erwartet, aber Ende der Eingabe erreicht.");
                return;
            }

            Token token = tokens[0];

            switch (token.Type)
            {
                case Token.TokenType.NUMBER:
                    kind = Kind.Number;
                    number = double.Parse(token.Value, CultureInfo.InvariantCulture);
                    tokens.RemoveAt(0);
                    break;

                case Token.TokenType.NAME:
                    kind = Kind.Name;
                    name = token.Value;
                    tokens.RemoveAt(0);
                    break;

                case Token.TokenType.OPEN_PAREN:
                    // '(' entfernen -> wieder oben in der Grammatik (AddExpression) anfangen.
                    kind = Kind.Inner;
                    tokens.RemoveAt(0);
                    inner = new AddExpression();
                    inner.Parse(tokens);

                    // ')' erwarten und entfernen.
                    if (tokens.Count > 0 && tokens[0].Type == Token.TokenType.CLOSE_PAREN)
                    {
                        tokens.RemoveAt(0);
                    }
                    else
                    {
                        Errors.Add("')' erwartet, gefunden: "
                            + (tokens.Count > 0 ? tokens[0].Value : "Ende der Eingabe"));
                    }
                    break;

                case Token.TokenType.KEYWORD when token.Value == "IF":
                    // Fuehrendes "IF" hier entfernen; IfExpression startet bei der Bedingung.
                    kind = Kind.Inner;
                    tokens.RemoveAt(0);
                    inner = new IfExpression();
                    inner.Parse(tokens);
                    break;

                default:
                    Errors.Add("Unerwartetes Token im Faktor: " + token.Value);
                    tokens.RemoveAt(0); // konsumieren, um Endlosschleifen zu vermeiden.
                    break;
            }
        }

        public override double Evaluate()
        {
            switch (kind)
            {
                case Kind.Number:
                    return number;

                case Kind.Name:
                    if (Variables.TryGetValue(name, out double value))
                    {
                        return value;
                    }
                    Errors.Add("Unbekannte Variable: " + name);
                    return 0;

                case Kind.Inner:
                    return inner != null ? inner.Evaluate() : 0;

                default:
                    return 0;
            }
        }
    }

    // <if> ::= IF <expr> THEN <expr> [ ELSE <expr> ]
    // (Das fuehrende "IF" wird vom FactorExpression-Aufrufer entfernt.)
    public class IfExpression : Expression
    {
        private Expression condition = new AddExpression();
        private Expression thenExpr = new AddExpression();
        private Expression? elseExpr = null;   // optional!

        public override void Parse(List<Token> tokens)
        {
            // 1) Bedingung parsen.
            condition = new AddExpression();
            condition.Parse(tokens);

            // 2) THEN erwarten und entfernen.
            if (tokens.Count > 0
                && tokens[0].Type == Token.TokenType.KEYWORD
                && tokens[0].Value == "THEN")
            {
                tokens.RemoveAt(0);
            }
            else
            {
                Errors.Add("IF: 'THEN' erwartet, gefunden: "
                    + (tokens.Count > 0 ? tokens[0].Value : "Ende der Eingabe"));
            }

            // 3) Then-Ausdruck parsen.
            thenExpr = new AddExpression();
            thenExpr.Parse(tokens);

            // 4) FALLSTRICK: ELSE NUR konsumieren, wenn das naechste Token
            //    WIRKLICH das Keyword "ELSE" ist - sonst gehoert das Keyword
            //    bereits zur naechsten Anweisung (z.B. LET/IF).
            if (tokens.Count > 0
                && tokens[0].Type == Token.TokenType.KEYWORD
                && tokens[0].Value == "ELSE")
            {
                tokens.RemoveAt(0);
                elseExpr = new AddExpression();
                elseExpr.Parse(tokens);
            }
        }

        public override double Evaluate()
        {
            if (condition.Evaluate() != 0)
            {
                return thenExpr.Evaluate();
            }
            return elseExpr != null ? elseExpr.Evaluate() : 0;
        }
    }
}
