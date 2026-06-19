
using RobotLibrary;

namespace PA4_Robot
{
    internal class PlaceExpression: Expression
    {
        internal override void Parse(List<Token> tokens)
        {
            throw new NotImplementedException();
        }
        internal override void Execute(RobotField roboter)
        {

            string result = previouslyCollected;   // geerbtes static-Feld aus Expression (A7)

            if(result == null || result == "")
            {
                Errors.Add("Failed to place. There is no item previously collected which now can be placed.");
            } 
            roboter.Place(result);
        }
    }
}