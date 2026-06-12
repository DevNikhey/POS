using Painter;
using System.Collections.Generic;

namespace PainterSolution
{
    internal class ColorExpression : Expression
    {
        private Token _color;

        internal override void Parse(List<Token> tokens)
        {
            if (tokens.Count == 0)
            {
                Errors.Add(
                    "Incorrect Color Statement, expecting Colorname and found End Of File");
                return;
            }

            if (tokens[0].Type == Token.TokenType.Color)
            {
                _color = tokens[0];
                tokens.RemoveAt(0);
            }
            else
            {
                Errors.Add(
                    "Incorrect Color Statement, expecting Colorname and found "
                    + tokens[0].Type + ": "
                    + tokens[0].Value);

                return;
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