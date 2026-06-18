using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;

namespace UE3_Rechner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Beispiel-Programm zum Ausprobieren:
            InputTextBox.Text =
                "LET a = 3 + 4 * 2\r\n" +
                "LET b = ( a - 1 ) / 2\r\n" +
                "IF b THEN a + 100 ELSE a - 100";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // -------------------------------------------------------------
            // Schritt 1: Tokenisieren und in der Liste anzeigen.
            // -------------------------------------------------------------
            List<Token> tokens = Token.Tokenize(InputTextBox.Text);
            TokensListBox.ItemsSource = tokens;

            // Schritt 1.5: Gibt es ERROR-Tokens? -> melden und abbrechen.
            if (tokens.Any(t => t.Type == Token.TokenType.ERROR))
            {
                StringBuilder sb = new StringBuilder();
                foreach (Token t in tokens.Where(t => t.Type == Token.TokenType.ERROR))
                {
                    sb.AppendLine($"Ungueltiges Token: {t.Value}");
                }
                MessageBox.Show(sb.ToString(), "Fehler beim Tokenisieren",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // -------------------------------------------------------------
            // Schritt 2: Parsen (Liste OHNE ERROR-Tokens uebergeben).
            // -------------------------------------------------------------
            Programm programm = new Programm();
            programm.Parse(tokens.Where(t => t.Type != Token.TokenType.ERROR).ToList());

            if (Expression.Errors.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string error in Expression.Errors)
                {
                    sb.AppendLine(error);
                }
                MessageBox.Show(sb.ToString(), "Fehler beim Parsen",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Expression.Errors.Clear();
                return;
            }

            // -------------------------------------------------------------
            // Schritt 3: Ausfuehren. Vor dem Lauf die Variablen-Umgebung leeren.
            // -------------------------------------------------------------
            Expression.Variables.Clear();
            programm.Evaluate();

            StringBuilder output = new StringBuilder();
            foreach (double result in programm.Results)
            {
                output.AppendLine(result.ToString(CultureInfo.InvariantCulture));
            }
            OutputTextBox.Text = output.ToString();

            // Schritt 3.5: zur Laufzeit aufgetretene Fehler anzeigen.
            if (Expression.Errors.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string error in Expression.Errors)
                {
                    sb.AppendLine(error);
                }
                MessageBox.Show(sb.ToString(), "Fehler beim Ausfuehren",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Expression.Errors.Clear();
            }
        }
    }
}
