
using RobotLibrary;

namespace PA4_Robot
{
    internal class CollectExpression : Expression
    {
        internal override void Parse(List<Token> tokens)
        {
            throw new NotImplementedException();
        }
        internal override void Execute(RobotField roboter)
        {
            String result = roboter.Collect();
            if(result == null || result == "")
            {
                Errors.Add("Failed to collect. There might be no item to collect at the current position.");
            }
            previouslyCollected = result;   // geerbtes static-Feld aus Expression (A7)
        }
    }
}