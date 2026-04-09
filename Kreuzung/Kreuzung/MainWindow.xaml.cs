using System.Security.Cryptography.X509Certificates;
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

namespace Kreuzung
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static ListBox kListBox;
        public static ListBox NorthListBox; 
        public static ListBox EastListBox;
        public static ListBox SouthListBox;
        public static ListBox WestListBox;

        public MainWindow()
        {
            InitializeComponent();
            kListBox = K;
            NorthListBox = N;
            EastListBox = E;
            SouthListBox = S;
            WestListBox = W;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LargeKreuzung kreuzung = new LargeKreuzung();
            for (int i = 0; i < Int32.Parse(AI.Text); i++)
            {
                Car carN = new Car(i);
                Task.Run(() => carN.Drive(kreuzung));
            }
        }
    }
}