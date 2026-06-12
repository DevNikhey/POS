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
    internal class TurnExpression : Expression
    {
        private Token _direction;
        private int _angle;

        internal override void Parse(List<Token> tokens)
        {
            if (tokens.Count > 0)
            {
                if (tokens[0].Type == Token.TokenType.Direction)
                {
                    _direction = tokens[0];
                    tokens.RemoveAt(0);
                }
                else
                {
                    //Fehler
                    Errors.Add("Unexpected Token Type " + tokens[0].Type + ", expected Direction");
                }
            }
            else
            {
                //Fehler
                Errors.Add("Unexpected end of TurnExpression, expected Direction");
            }

            if (tokens.Count > 0)
            {
                if (tokens[0].Type == Token.TokenType.Number)
                {
                    _angle = int.Parse(tokens[0].Value);
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
                Errors.Add("Unexpected end of TurnExpression, expected Number");
            }
        }
        internal override void Run(PainterControl painter)
        {
            switch (_direction.Value)
            {
                case "LEFT":
                    painter.Rotate(-_angle);
                    break;
                case "RIGHT":
                    painter.Rotate(_angle);
                    break;
            }
        }
    }
}
