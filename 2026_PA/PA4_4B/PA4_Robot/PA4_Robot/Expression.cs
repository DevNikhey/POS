using RobotLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PA4_Robot
{
    internal abstract class Expression
    {
        internal static List<String> Errors = new List<string>();

        // A7: gemeinsamer Zustand (zuletzt eingesammeltes Objekt) gehoert in die
        // Expression-Basis, NICHT in die GUI (MainWindow).
        internal static String previouslyCollected = String.Empty;

        internal abstract void Parse(List<Token> tokens);
        internal virtual void Execute(RobotField roboter)        {
            //Default implementation does nothing
        }
    }
}
