using RobotLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PA4_Robot
{
    internal class Condition : Expression
    {
        Token direction = new Token() { Type = Token.TokenType.KEYWORD, Value = "" };
        Token objectToken = new Token() { Type = Token.TokenType.KEYWORD, Value = "" };

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
            if(tokens.Count > 0 && tokens[0].Type == Token.TokenType.KEYWORD && tokens[0].Value == "IS-A")
            {
                tokens.RemoveAt(0);
            }
            else
            {
                Errors.Add("Expected a IS-A keyword, got " + (tokens.Count > 0 ? tokens[0].Value : "end of input"));
            }
            if(tokens.Count > 0 && (tokens[0].Type == Token.TokenType.KEYWORD && tokens[0].Value == "OBSTACLE") || tokens[0].Type == Token.TokenType.LETTER)
            {
                objectToken = tokens[0];
                tokens.RemoveAt(0);
            }
            else
            {
                Errors.Add("Expected an OBSTACLE or a Letter-Token, got " + (tokens.Count > 0 ? tokens[0].Value : "end of input"));
            }

        }

        internal bool Evaluate(RobotField roboter)
        {
            if(objectToken.Value == "OBSTACLE")
            {
                switch(direction.Value)
                {
                    case "UP":
                        return roboter.IsObstacle(RobotField.Direction.Up);
                    case "DOWN":
                        return roboter.IsObstacle(RobotField.Direction.Down);
                    case "LEFT":
                        return roboter.IsObstacle(RobotField.Direction.Left);
                    case "RIGHT":
                        return roboter.IsObstacle(RobotField.Direction.Right);
                    default:
                        Errors.Add("Missing direction in condition!");
                        break;
                }
            }
            else if(objectToken.Type == Token.TokenType.LETTER)
            {
                switch(direction.Value)
                {
                    case "UP":
                        return roboter.IsLetter(objectToken.Value, RobotField.Direction.Up);
                    case "DOWN":
                        return roboter.IsLetter(objectToken.Value, RobotField.Direction.Down);
                    case "LEFT":
                        return roboter.IsLetter(objectToken.Value, RobotField.Direction.Left);
                    case "RIGHT":
                        return roboter.IsLetter(objectToken.Value, RobotField.Direction.Right);
                    default:
                        Errors.Add("Missing direction in condition!");
                        break;
                }
            }
            else
            {
                Errors.Add("Missing object in condition!");
            }

            return false;
        }
    }
}
