using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PA4_Robot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            RobotFieldControl.LoadField("Aufgabe2.xml");
            //InputTextBox.Text = "REPEAT 4 {\r\n    UNTIL DOWN IS-A OBSTACLE {\r\n        MOVE DOWN\r\n        IF DOWN IS-A A {\r\n            MOVE DOWN\r\n            COLLECT\r\n        }\r\n    }\r\n    UNTIL RIGHT IS-A OBSTACLE {\r\n        MOVE RIGHT\r\n    }\r\n    UNTIL UP IS-A OBSTACLE {\r\n        MOVE UP\r\n        IF UP IS-A B {\r\n            MOVE UP\r\n            COLLECT\r\n        }\r\n    }\r\n    UNTIL RIGHT IS-A OBSTACLE {\r\n        MOVE RIGHT\r\n    }\r\n}\r\nMOVE DOWN\r\nMOVE DOWN\r\nCOLLECT  ";

            //Code with ELSE
            //InputTextBox.Text = "REPEAT 4 {\r\n    UNTIL DOWN IS-A OBSTACLE {\r\n        IF DOWN IS-A A {\r\n            MOVE DOWN\r\n            COLLECT\r\n        } ELSE {\r\n            MOVE DOWN\r\n        }\r\n    }\r\n    UNTIL RIGHT IS-A OBSTACLE {\r\n        MOVE RIGHT\r\n    }\r\n    UNTIL UP IS-A OBSTACLE {\r\n        IF UP IS-A B {\r\n            MOVE UP\r\n            COLLECT\r\n        } ELSE {\r\n            MOVE UP\r\n        }\r\n    }\r\n    UNTIL RIGHT IS-A OBSTACLE {\r\n        MOVE RIGHT\r\n    }\r\n}\r\nMOVE DOWN\r\nMOVE DOWN\r\nCOLLECT  ";

            //Code with PLACE
            InputTextBox.Text = "REPEAT 4 {\r\n    UNTIL DOWN IS-A OBSTACLE {\r\n        MOVE DOWN\r\n        IF DOWN IS-A A {\r\n            MOVE DOWN\r\n            COLLECT\r\n        }\r\n    }\r\n    UNTIL RIGHT IS-A OBSTACLE {\r\n        MOVE RIGHT\r\n    }\r\n    UNTIL UP IS-A OBSTACLE {\r\n        MOVE UP\r\n        IF UP IS-A B {\r\n            MOVE UP\r\n            COLLECT\r\n        }\r\n    }\r\n    UNTIL RIGHT IS-A OBSTACLE {\r\n        MOVE RIGHT\r\n    }\r\n}\r\nPLACE\r\nMOVE DOWN\r\nMOVE DOWN\r\nCOLLECT  ";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            //Schritt 1 : Tokenisierung
            Regex regex = new Regex(@"REPEAT|MOVE|RIGHT|DOWN|LEFT|UP|COLLECT|PLACE|UNTIL|IS-A|IF|ELSE|OBSTACLE|{|}|[A-Z]|\d+|\S+");
            Regex keywords = new Regex(@"REPEAT|MOVE|RIGHT|DOWN|LEFT|UP|COLLECT|PLACE|UNTIL|IS-A|IF|ELSE|OBSTACLE");
            MatchCollection matches = regex.Matches(InputTextBox.Text);
            List<Token> tokens = new List<Token>();
            foreach (Match match in matches)
            {
                Token token = new Token() { Value = match.Value };
                if (keywords.IsMatch(match.Value))
                {
                    token.Type = Token.TokenType.KEYWORD;
                }
                else if (match.Value == "{")
                {
                    token.Type = Token.TokenType.OPEN_BRACE;
                }
                else if (match.Value == "}")
                {
                    token.Type = Token.TokenType.CLOSE_BRACE;
                }
                else if (Regex.IsMatch(match.Value, @"[A-Z]"))
                {
                    token.Type = Token.TokenType.LETTER;
                }
                else if (Regex.IsMatch(match.Value, @"\d+"))
                {
                    token.Type = Token.TokenType.NUMBER;
                }
                else
                {
                    token.Type = Token.TokenType.ERROR;
                }
                tokens.Add(token);
            }
            TokensListBox.ItemsSource = tokens;

            //Schritt 1.5 : Ausgabe der Fehler falls vorhanden
            if (tokens.Any(t => t.Type == Token.TokenType.ERROR))
            {
                StringBuilder sb = new StringBuilder();
                foreach (Token token in tokens.Where(t => t.Type == Token.TokenType.ERROR))
                {
                    sb.AppendLine($"Ungültiges Token: {token.Value}");
                }
                MessageBox.Show(sb.ToString(), "Fehler beim Tokenisieren", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //Schritt 2 : Parsing
            Programm programm = new Programm();
            programm.Parse(tokens.Where(x => x.Type != Token.TokenType.ERROR).ToList());

            //Schritt 2.5 : Ausgabe der Fehler falls vorhanden
            if(Expression.Errors.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string error in Expression.Errors)
                {
                    sb.AppendLine(error);
                }
                MessageBox.Show(sb.ToString(), "Fehler beim Parsen", MessageBoxButton.OK, MessageBoxImage.Error);
                Expression.Errors.Clear();
            }


            //Schritt 3 : Ausführen
            ThreadPool.QueueUserWorkItem(_ =>
            {
                programm.Execute(RobotFieldControl);
            

                //Schritt 3.5 : Ausgabe der Fehler falls vorhanden
                if (Expression.Errors.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string error in Expression.Errors)
                    {
                        sb.AppendLine(error);
                    }
                    MessageBox.Show(sb.ToString(), "Fehler beim Ausführen", MessageBoxButton.OK, MessageBoxImage.Error);
                    Expression.Errors.Clear();
                }
            });
        }
    }
}