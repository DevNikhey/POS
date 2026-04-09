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

namespace DreiRaucher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // the one and only table :)
        Table table;

        Dealer dealer;
        Raucher raucher1;
        Raucher raucher2;
        Raucher raucher3;

        private int smokeTime;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            smokeTime = Int32.Parse(SmokeTime.Text);

            Table table = new Table(TableContentLabel);
            dealer = new Dealer(table);
            raucher1 = new Raucher(table, RESOURCE.TABACCO, smokeTime, Smoker1Resource);
            raucher2 = new Raucher(table, RESOURCE.FLINT_AND_STEEL, smokeTime, Smoker2Resource);
            raucher3 = new Raucher(table, RESOURCE.PAPER, smokeTime, Smoker3Resource);


            // start the threads
            new Thread(dealer.Run).Start();
            new Thread(raucher1.runThread).Start();
            new Thread(raucher2.runThread).Start();
            new Thread(raucher3.runThread).Start();

            setDisplayContent();
        }

        private void setDisplayContent()
        {
            Smoker1.Content = "Raucher 1 (" + raucher1.getUnlimitedResource() +")" ;
            Smoker2.Content = "Raucher 2 (" + raucher2.getUnlimitedResource() + ")";
            Smoker3.Content = "Raucher 3 (" + raucher3.getUnlimitedResource() + ")";
        }
    }
}