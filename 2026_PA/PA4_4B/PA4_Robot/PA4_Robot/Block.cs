using RobotLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PA4_Robot
{
    internal class Block : Expression
    {
        Programm programm = new Programm();

        internal override void Parse(List<Token> tokens)
        {
            if (tokens.Count == 0 || tokens[0].Type != Token.TokenType.OPEN_BRACE)
            {
                Errors.Add("Expected '{' at the beginning of a block.");
                return;
            }
            tokens.RemoveAt(0); // Entferne das '{'-Token, um die Analyse fortzusetzen


            programm.Parse(tokens);

            if (tokens.Count == 0 || tokens[0].Type != Token.TokenType.CLOSE_BRACE)
            {
                Errors.Add("Expected '}' at the end of a block.");
                return;
            }
            tokens.RemoveAt(0); // Entferne das '}'-Token, um die Analyse fortzusetzen
        }

        internal override void Execute(RobotField roboter)
        {
            programm.Execute(roboter);
        }
    }
}
