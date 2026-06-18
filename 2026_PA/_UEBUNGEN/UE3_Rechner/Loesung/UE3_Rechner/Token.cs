using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UE3_Rechner
{
    // Ein Token unserer Rechen-Sprache: Typ + Textwert.
    // (Aus dem Parser-Toolkit, fuer die Rechner-Domaene angepasst.)
    public class Token
    {
        public enum TokenType
        {
            KEYWORD,      // LET, IF, THEN, ELSE
            OPERATOR,     // + - * /
            ASSIGN,       // =
            OPEN_PAREN,   // (
            CLOSE_PAREN,  // )
            NUMBER,       // 3 , 42 , 3.14
            NAME,         // a , summe , x1
            ERROR         // alles Unbekannte
        }

        public TokenType Type { get; set; }
        public string Value { get; set; } = "";

        public override string ToString() => $"{Type}: {Value}";

        // ---------------------------------------------------------------
        // AUFGABE 1: Tokenizer
        //
        // Zerlege "input" mit EINER Regex in Tokens und setze pro Match
        // den passenden TokenType. Vorschlag fuer die Bausteine:
        //   Schluesselwoerter : LET | IF | THEN | ELSE
        //   Operatoren        : + - * /
        //   Zuweisung         : =
        //   Klammern          : ( )
        //   Zahl              : \d+(\.\d+)?
        //   Name              : [a-zA-Z]\w*
        //   Rest              : \S+   -> ERROR
        //
        // WICHTIG: Schluesselwoerter MUESSEN in der Regex VOR dem
        // Namens-Muster stehen, sonst "klaut" [a-zA-Z]\w* z.B. das IF.
        // ---------------------------------------------------------------
        public static List<Token> Tokenize(string input)
        {
            // Eine Regex fuer alle Bausteine. Reihenfolge ist wichtig:
            //  - Schluesselwoerter VOR dem Namens-Muster ([a-zA-Z]\w*),
            //    sonst matcht der Name z.B. "IF" als gewoehnlichen Namen.
            //  - Wortgrenze \b nach den Keywords, damit "LETTER" nicht als
            //    "LET" + "TER" zerfaellt.
            //  - \S+ am Ende faengt alles Unbekannte als ERROR.
            Regex regex = new Regex(
                @"\b(?:LET|IF|THEN|ELSE)\b|[a-zA-Z]\w*|\d+(?:\.\d+)?|[+\-*/]|=|\(|\)|\S+");

            // Separate Keyword-Regex, um den Typ eines Matches zu bestimmen.
            Regex keywords = new Regex(@"^(?:LET|IF|THEN|ELSE)$");

            List<Token> tokens = new List<Token>();
            foreach (Match match in regex.Matches(input))
            {
                Token token = new Token { Value = match.Value };

                if (keywords.IsMatch(match.Value))
                {
                    token.Type = TokenType.KEYWORD;
                }
                else if (Regex.IsMatch(match.Value, @"^[a-zA-Z]\w*$"))
                {
                    token.Type = TokenType.NAME;
                }
                else if (Regex.IsMatch(match.Value, @"^\d+(?:\.\d+)?$"))
                {
                    token.Type = TokenType.NUMBER;
                }
                else if (match.Value == "+" || match.Value == "-"
                         || match.Value == "*" || match.Value == "/")
                {
                    token.Type = TokenType.OPERATOR;
                }
                else if (match.Value == "=")
                {
                    token.Type = TokenType.ASSIGN;
                }
                else if (match.Value == "(")
                {
                    token.Type = TokenType.OPEN_PAREN;
                }
                else if (match.Value == ")")
                {
                    token.Type = TokenType.CLOSE_PAREN;
                }
                else
                {
                    token.Type = TokenType.ERROR;
                }

                tokens.Add(token);
            }

            return tokens;
        }
    }
}
