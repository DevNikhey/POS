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

namespace PainterSolution
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Regex regex = new Regex(@"FOR|TURN|COLOR|DRAW|RIGHT|LEFT|\d+|{|}|\S+");
        private Regex numberRegex = new Regex(@"\d+");
        private Regex keywordRegex = new Regex(@"FOR|TURN|COLOR|DRAW");
        private Regex directionRegex = new Regex(@"LEFT|RIGHT");
        private Regex colorRegex = new Regex(@"Red|White|Blue|Green", RegexOptions.IgnoreCase);

        private List<Token> tokens = new List<Token>();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void RunButton(object sender, RoutedEventArgs e)
        {
            // 1. Schritt: Tokenisieren
            tokens.Clear();
            foreach (Match match in regex.Matches(codeTextBox.Text))
            {
                Token token = new Token() { Value = match.Value };
                tokens.Add(token);
                switch (match.Value)
                {
                    case var _ when numberRegex.IsMatch(match.Value):
                        token.Type = Token.TokenType.Number;
                        break;

                    case var _ when keywordRegex.IsMatch(match.Value):
                        token.Type = Token.TokenType.Keyword;
                        break;

                    case var _ when directionRegex.IsMatch(match.Value):
                        token.Type = Token.TokenType.Direction;
                        break;

                    case "{":
                        token.Type = Token.TokenType.OpenBracket;
                        break;

                    case "}":
                        token.Type = Token.TokenType.CloseBracket;
                        break;
                    case var _ when colorRegex.IsMatch(match.Value):
                        token.Type = Token.TokenType.Color;
                        break;
                }
            }
            MessageBox.Show(tokens.ElementAt(1).Type.ToString());
            var errors = tokens.Where(t => t.Type == Token.TokenType.Error).ToList();
            if (errors.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Fehlerhafte Tokens:");
                foreach (var error in errors)
                {
                    sb.AppendLine(error.Value);
                }
                MessageBox.Show(sb.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //Schritt 2: Parsen
            Programm programm = new();
            programm.Parse(tokens);

            //Schritt 2.5: Fehlerhafte Anweisungen ausgeben
            if (Expression.Errors.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Fehlerhafte Anweisungen:");
                foreach (var error in Expression.Errors)
                {
                    builder.AppendLine(error);
                }
                MessageBox.Show(builder.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Expression.Errors.Clear();

            // Schritt 3: Ausführen
            programm.Run(Field);

            //Schritt 3.5: Fehlerhafte Ausführung ausgeben
            if (Expression.Errors.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Fehlerhafte Ausführung:");
                foreach (var error in Expression.Errors)
                {
                    builder.AppendLine(error);
                }
                MessageBox.Show(builder.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Expression.Errors.Clear();
        }
    }
}