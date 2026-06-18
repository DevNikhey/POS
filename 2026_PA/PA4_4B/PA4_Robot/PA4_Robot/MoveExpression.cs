
using RobotLibrary;

namespace PA4_Robot
{
    internal class MoveExpression : Expression
    {
        Token direction = new Token() { Type = Token.TokenType.KEYWORD, Value = "" };

        internal override void Parse(List<Token> tokens)
        {
            if (tokens.Count > 0 && tokens[0].Type == Token.TokenType.KEYWORD && (tokens[0].Value == "UP" || tokens[0].Value == "DOWN" || tokens[0].Value == "LEFT" || tokens[0].Value == "RIGHT"))
            {
                direction = tokens[0];
                tokens.RemoveAt(0);
            }
            else
            {
                Errors.Add("Expected a direction keyword (UP, DOWN, LEFT, RIGHT), got " + (tokens.Count > 0 ? tokens[0].Value : "end of input"));
            }
        }

        internal override void Execute(RobotField roboter)
        {
            bool success = true;
            switch (direction.Value)
            {
                case "UP":
                    success = roboter.Move(RobotField.Direction.Up);
                    break;
                case "DOWN":
                    success = roboter.Move(RobotField.Direction.Down);
                    break;
                case "LEFT":
                    success = roboter.Move(RobotField.Direction.Left);
                    break;
                case "RIGHT":
                    success = roboter.Move(RobotField.Direction.Right);
                    break;
                default:
                    Errors.Add("Direction not set");
                    // Sollte nicht passieren, da die Parse-Methode bereits Fehler hinzufügt, wenn die Richtung ungültig ist
                    break;
            }
            if(!success)
            {
                Errors.Add($"Failed to move in direction {direction.Value}. There might be an obstacle or the robot is at the edge of the field.");
            }
        }
    }
}