using RobotLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PA4_Robot
{
    internal class IfExpression : Expression
    {
        Condition condition = new Condition();
        Block block = new Block();
        Block elseblock = new Block();

        internal override void Parse(List<Token> tokens)
        {
            condition.Parse(tokens);
            block.Parse(tokens);

            if (tokens.Count == 0 || tokens[0].Type != Token.TokenType.KEYWORD)
            {
                return;
            }
            tokens.RemoveAt(0);

            elseblock.Parse(tokens);
        }

        internal override void Execute(RobotField roboter)
        {
            if(condition.Evaluate(roboter))
            {
                block.Execute(roboter);
            } else
            {
                elseblock.Execute(roboter);
            }
        }
    }
}
