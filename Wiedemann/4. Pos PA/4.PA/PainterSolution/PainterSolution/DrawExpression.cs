using AbcRobotCore;
using Painter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AbcRobotCore.RobotField;

namespace PainterSolution
{
    internal class DrawExpression : Expression
    {
        private int _number;
        internal override void Parse(List<Token> tokens)
        {
            if (tokens.Count > 0)
            {
                if (tokens[0].Type == Token.TokenType.Number)
                {
                    _number = int.Parse(tokens[0].Value);
                    tokens.RemoveAt(0);
                }
                else
                {
                    //Fehler
                    Errors.Add("Unexpected Token Type " + tokens[0].Type + ", expected Number");
                }
            }
            else
            {
                //Fehler
                Errors.Add("Unexpected end of DrawExpression, expected Draw");
            }
        }

        internal override void Run(PainterControl painter)
        {
            painter.Draw(_number);
        }
    }
}
