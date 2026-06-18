using RobotLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PA4_Robot
{
    internal class UntilExpression : Expression
    {
        Condition condition = new Condition();
        Block block = new Block();

        internal override void Parse(List<Token> tokens)
        {
            condition.Parse(tokens);
            block.Parse(tokens);
        }

        internal override void Execute(RobotField roboter)
        {
            while (!condition.Evaluate(roboter))
            {
                block.Execute(roboter);
            }
        }
    }
}
