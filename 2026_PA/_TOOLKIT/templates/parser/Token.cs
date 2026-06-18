using System.Text.RegularExpressions;

namespace __NS__;

// Token einer einfachen Sprache (aus PA4_Robot). Typ + Textwert.
public class Token
{
    public enum TokenType { KEYWORD, OPEN_BRACE, CLOSE_BRACE, LETTER, NUMBER, ERROR }

    public TokenType Type { get; set; }
    public string Value { get; set; } = "";

    // Beispiel-Tokenizer: passe die Schlüsselwörter an deine Grammatik an.
    public static List<Token> Tokenize(string input, string keywordPattern)
    {
        // keywordPattern z.B.: "REPEAT|MOVE|IF|ELSE|UNTIL|IS-A|OBSTACLE"
        Regex split = new Regex(keywordPattern + @"|{|}|[A-Z]|\d+|\S+");
        Regex keywords = new Regex("^(?:" + keywordPattern + ")$");

        List<Token> tokens = new List<Token>();
        foreach (Match m in split.Matches(input))
        {
            Token t = new Token { Value = m.Value };
            if (keywords.IsMatch(m.Value)) t.Type = Token.TokenType.KEYWORD;
            else if (m.Value == "{") t.Type = Token.TokenType.OPEN_BRACE;
            else if (m.Value == "}") t.Type = Token.TokenType.CLOSE_BRACE;
            else if (Regex.IsMatch(m.Value, @"^[A-Z]$")) t.Type = Token.TokenType.LETTER;
            else if (Regex.IsMatch(m.Value, @"^\d+$")) t.Type = Token.TokenType.NUMBER;
            else t.Type = Token.TokenType.ERROR;
            tokens.Add(t);
        }
        return tokens;
    }
}
