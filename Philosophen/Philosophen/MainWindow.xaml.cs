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

namespace Philosophen
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

        private void start_Click(object sender, RoutedEventArgs e)
        {
            // Define static stuff 
            Philosopher.dThink = int.Parse(ddenken.Text);
            Philosopher.vThink = int.Parse(vdenken.Text);
            Philosopher.dEat = int.Parse(dessen.Text);
            Philosopher.vEat = int.Parse(vessen.Text);
            Philosopher.counterBox = eatCounter;

            // Create forks
            Fork fork1 = new Fork(1);
            Fork fork2 = new Fork(2);
            Fork fork3 = new Fork(3);
            Fork fork4 = new Fork(4);
            Fork fork5 = new Fork(5);

            // Create philosophs and parse objects
            Philosopher philosopher1 = new Philosopher(name1.Text, fork1, fork2, phil1);
            Philosopher philosopher2 = new Philosopher(name2.Text, fork2, fork3, phil2);
            Philosopher philosopher3 = new Philosopher(name3.Text, fork3, fork4, phil3);
            Philosopher philosopher4 = new Philosopher(name4.Text, fork4, fork5, phil4);
            Philosopher philosopher5 = new Philosopher(name5.Text, fork5, fork1, phil5);

            philosopher1.startThread();
            philosopher2.startThread();
            philosopher3.startThread();
            philosopher4.startThread();
            philosopher5.startThread();

            start.IsEnabled = false;
            stop.IsEnabled = true;
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            start.IsEnabled = true;
            Philosopher.running = false;
            stop.IsEnabled = false;
        }
    }
}