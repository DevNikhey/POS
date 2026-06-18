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
            // AUFGABE 5: GUI verdrahten (Tokenize -> Parse -> Execute)
            //
            // Schritt 1  : tokens = Token.Tokenize(InputTextBox.Text);
            //              TokensListBox.ItemsSource = tokens;
            // Schritt 1.5: gibt es ERROR-Tokens? -> MessageBox + return.
            // Schritt 2  : Programm programm = new Programm();
            //              programm.Parse(<Liste OHNE ERROR-Tokens>);
            //              Expression.Errors pruefen -> MessageBox + Clear().
            // Schritt 3  : Expression.Variables.Clear();
            //              programm.Evaluate();
            //              programm.Results in OutputTextBox schreiben
            //              (eine Zeile pro Anweisung). Danach Fehler erneut
            //              pruefen/anzeigen/Clear().
            //
            // Tipp: Eine StringBuilder-Schleife ueber Errors bzw. Results
            //       fuer die Ausgabe.
            // -------------------------------------------------------------
            throw new System.NotImplementedException("Button_Click noch nicht implementiert (Aufgabe 5)");
        }
    }
}
