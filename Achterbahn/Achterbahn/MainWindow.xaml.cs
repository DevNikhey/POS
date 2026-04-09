using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Achterbahn
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static int anzahlFahrten = 0;
        public static ListBox walkingBox;
        public static ListBox ridingBox;
        public static Label counterLabel;

        public MainWindow()
        {
            InitializeComponent();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int pass = int.Parse(Pass.Text);
            int platz = int.Parse(Platz.Text);
            int fahrten = int.Parse(Fahrten.Text);

            ridingBox = riding;
            walkingBox = walking;
            counterLabel = fahrtCounter;
            anzahlFahrten = fahrten;

            Wagon wagon = new Wagon(platz, fahrten);

            for (int i = 0; i < pass; i++)
            {
                Passagiere p = new Passagiere(wagon, $"P{i + 1}");
                new Thread(p.Run).Start();
            }
        }
    }
}