using System;
using System.Collections.Generic;

namespace UE3_Rechner
{
    // =================================================================
    //  Recursive-Descent-Parser/Interpreter-Skelett (Rechner-Domaene).
    //
    //  Jede Grammatik-Regel ist eine Expression-Unterklasse mit:
    //    * Parse(List<Token>)  -> "frisst" ihre Tokens vorne aus der Liste
    //                             (tokens.RemoveAt(0)).
    //    * Evaluate()          -> wertet den geparsten Knoten aus und
    //                             liefert einen double zurueck.
    //
    //  Gemeinsamer Zustand (Fehlerliste, Variablen-Umgebung) gehoert in
    //  DIESE Basisklasse - NICHT in die GUI/MainWindow!
    // =================================================================
    public abstract class Expression
    {
        // Fehler, die beim Parsen/Auswerten auftreten.
        public static List<string> Errors = new List<string>();

        // Variablen-Umgebung: name -> wert. Wird von LET gefuellt und
        // von einem Namens-Faktor gelesen. Vor jedem Lauf in der GUI leeren.
        public static Dictionary<string, double> Variables = new Dictionary<string, double>();

        // Baut den Teilbaum dieser Regel; konsumiert die zugehoerigen
        // Tokens vorne aus der Liste.
        public abstract void Parse(List<Token> tokens);

        // Wertet den geparsten Knoten aus. Default: 0.
        public virtual double Evaluate()
        {
            return 0;
        }
    }
}
