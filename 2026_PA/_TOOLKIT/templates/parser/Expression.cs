namespace __NS__;

// =====================================================================
//  Recursive-Descent-Parser-Skelett (aus PA4_Robot).
//
//  Idee: Jede Grammatik-Regel ist eine Expression-Unterklasse mit
//   * Parse(List<Token>)  – "frisst" die zu ihr gehörenden Tokens vorne
//                            aus der Liste (tokens.RemoveAt(0)).
//   * Execute(...)        – führt den geparsten Knoten aus (Interpreter).
//
//  Gemeinsamer Zustand (z.B. Errors, "previouslyCollected") gehört in
//  DIESE Basisklasse – NICHT in die GUI/MainWindow!  (PA4-Bewertung A7)
// =====================================================================
public abstract class Expression
{
    public static List<string> Errors = new List<string>();

    public abstract void Parse(List<Token> tokens);

    // Optional-Block-Muster (z.B. IF ... [ELSE ...]):
    // ELSE darf NUR konsumiert werden, wenn das nächste Token wirklich ELSE
    // ist – sonst ist es der Beginn der nächsten Anweisung!  (PA4-Bewertung A5)
    //
    //   public override void Parse(List<Token> tokens)
    //   {
    //       condition.Parse(tokens);
    //       block.Parse(tokens);
    //       if (tokens.Count > 0
    //           && tokens[0].Type == Token.TokenType.KEYWORD
    //           && tokens[0].Value == "ELSE")
    //       {
    //           tokens.RemoveAt(0);
    //           elseBlock.Parse(tokens);
    //       }
    //   }
}
