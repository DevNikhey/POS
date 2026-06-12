using AbcRobotCore;
using Painter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AbcRobotCore.RobotField;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PainterSolution
{
    internal class ColorExpression : Expression
    {
        private Token _color;
        internal override void Parse(List<Token> tokens)
        {
            if (tokens.Count > 0)
            {
                if (tokens[0].Type == Token.TokenType.Color)
                {
                    _color = tokens[0];
                    tokens.RemoveAt(0);
                }
                else
                {
                    //Fehler
                    Errors.Add("Unexpected Token Type " + tokens[0].Type + ", expected Color");
                }
            }
            else
            {
                //Fehler
                Errors.Add("Unexpected end of ColorExpression, expected Color");
            }
        }

        internal override void Run(PainterControl painter)
        {
            switch (_color.Value)
            {
                case "White":
                    painter.ChangeColor("White");
                    break;
                case "Red":
                    painter.ChangeColor("Red");
                    break;
                case "Blue":
                    painter.ChangeColor("Blue");
                    break;
                case "Green":
                    painter.ChangeColor("Green");
                    break;
            }
        }
    }
}
