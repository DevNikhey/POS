using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PainterSolution
{
    internal class Token
    {
        public enum TokenType { Keyword, Number, Direction, OpenBracket, CloseBracket, Color, Error }
        public string Value { get; set; }
        public TokenType Type { get; set; } = TokenType.Error;
    }
}
