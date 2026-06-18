using System.Diagnostics;
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

namespace PA1_4B_2025
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static ListBox flyingBox;
        public static ListBox simpleBox;
        public static ListBox mediumBox;
        public static ListBox bigBox;
        public static Label simpleLabel;

        public static ListBox vipBox;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            simpleBox = simpleAirport;
            flyingBox = Flying;
            mediumBox = mediumAirport;
            simpleLabel = simpleLabelUI;
            vipBox = vipAirport;
            bigBox = bigAirportUI;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Airport targetAirport = null;

            switch (comboAirport.Text)
            {
                case "SimpleAirport":
                    targetAirport = new SimpleAirport();
                    break;
                case "MediumAirport":
                    targetAirport = new MediumAirport();
                    break;
                case "BigAirport":
                    targetAirport = new BigAirport();
                    break;
                case "VipAirport":
                    targetAirport = new VipAirport();
                    break;
            }

            if(targetAirport == null)
            {
                Debug.WriteLine("Airport Cannot be null. Please select an airport.");
                return;
            }

            for(int i = 0; i < Int32.Parse(anzahlNeu.Text); i++)
            {
                Plane plane = new Plane();
                plane.Id = i;
                plane.TargetAirport = targetAirport;

                // here calculate percentage for task 7
                // wert von 1-100, wenn unter 5 dann sind 5% erreicht
                if(new Random().Next(0, 100) <= 5)
                {
                    plane.IsVip = true;
                }

                new Thread(() => plane.Fly()).Start();
            }
        }
    }
}