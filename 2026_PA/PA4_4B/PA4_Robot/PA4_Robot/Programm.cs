using RobotLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PA4_Robot
{
    internal class Programm : Expression
    {
        private List<Expression> expressions = new List<Expression>();


        internal override void Parse(List<Token> tokens)
        {
            while(tokens.Count > 0)
            {
                Token token = tokens.First();
                if (token.Type == Token.TokenType.KEYWORD)
                {
                    switch (token.Value)
                    {
                        case "MOVE":
                            MoveExpression moveExpression = new MoveExpression();
                            tokens.RemoveAt(0); // Entferne das "MOVE"-Token, um die Analyse fortzusetzen
                            moveExpression.Parse(tokens);
                            expressions.Add(moveExpression);
                            break;
                        case "COLLECT":
                            CollectExpression collectExpression = new CollectExpression();
                            tokens.RemoveAt(0); // Entferne das "COLLECT"-Token, um die Analyse fortzusetzen
                            expressions.Add(collectExpression);
                            break;
                        case "PLACE":
                            PlaceExpression placeExpression = new PlaceExpression();
                            tokens.RemoveAt(0); // Entferne das "PLACE"-Token, um die Analyse fortzusetzen
                            expressions.Add(placeExpression);
                            break;
                        case "REPEAT":
                            RepeatExpression repeatExpression = new RepeatExpression();
                            tokens.RemoveAt(0); // Entferne das "REPEAT"-Token, um die Analyse fortzusetzen
                            repeatExpression.Parse(tokens);
                            expressions.Add(repeatExpression);
                            break;
                        case "UNTIL":
                            UntilExpression untilExpression = new UntilExpression();
                            tokens.RemoveAt(0); // Entferne das "UNTIL"-Token, um die Analyse fortzusetzen
                            untilExpression.Parse(tokens);
                            expressions.Add(untilExpression);
                            break;
                        case "IF":
                            IfExpression ifExpression = new IfExpression();
                            tokens.RemoveAt(0); // Entferne das "IF"-Token, um die Analyse fortzusetzen
                            ifExpression.Parse(tokens);
                            expressions.Add(ifExpression);
                            break;
                        default:
                            Errors.Add($"Unbekanntes Schlüsselwort: {token.Value}");
                            tokens.RemoveAt(0); // Entferne das unbekannte Token, um die Analyse fortzusetzen
                            break;
                    }
                }
                else
                {
                    if (token.Type == Token.TokenType.CLOSE_BRACE)
                    {
                        return; // Schließe die Analyse des aktuellen Blocks ab, wenn eine schließende Klammer gefunden wird
                    }
                    Errors.Add($"Unerwartetes Token: {token.Value}");
                    tokens.RemoveAt(0); // Entferne das unbekannte Token, um die Analyse fortzusetzen
                }
            }
            
        }

        internal override void Execute(RobotField roboter)
        {
            foreach (Expression expression in expressions)
            {
                expression.Execute(roboter);
            }
        }
    }
}
