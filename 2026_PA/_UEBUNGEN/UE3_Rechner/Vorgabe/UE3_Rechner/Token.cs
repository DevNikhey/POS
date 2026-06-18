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
            // TODO: Regex bauen, ueber alle Matches iterieren und pro
            //       Match ein Token mit korrektem Type erzeugen.
            //       Tipp: separate Keyword-Regex zum Pruefen des Typs.
            throw new System.NotImplementedException("Token.Tokenize noch nicht implementiert (Aufgabe 1)");
        }
    }
}
