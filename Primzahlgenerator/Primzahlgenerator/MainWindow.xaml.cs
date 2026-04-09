using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Primzahlgenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static bool isStarted = false;
        private static Storyboard storyboard;

        private static int startedThreads = 0;

        public MainWindow()
        {
            InitializeComponent();
            storyboard = (Storyboard)FindResource("loadingRotation");
            image1.Visibility = Visibility.Hidden;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int MAX_SIZE = Int32.Parse(input.Text);

            storyboard.Begin(this, true);
            image1.Visibility = Visibility.Visible;

            Thread thread = new Thread(() =>
            {
                startedThreads++;
                int primeCount = 0;
                int currentNumber = 2;
                int targetPrimeIndex = MAX_SIZE;

                while (primeCount < targetPrimeIndex)
                {
                    if (IsPrime(currentNumber))
                    {
                        primeCount++;
                        if (primeCount == targetPrimeIndex)
                        {
                            Debug.WriteLine($"The {targetPrimeIndex}th prime number is: {currentNumber}");
                            break;
                        }
                    }
                    currentNumber++;
                }

                ergNumbers.Dispatcher.BeginInvoke(() =>
                {
                    ergNumbers.Items.Add(targetPrimeIndex + "th: " + currentNumber);
                    if (--startedThreads <= 0)
                    {
                        storyboard.Stop(this);
                        image1.Visibility = Visibility.Hidden;
                    }
                });
            });

            thread.Start();
        }

        static bool IsPrime(int number)
        {
            if (number <= 1)
                return false;

            for (int i = 2; i <= Math.Sqrt(number); i++)
            {
                if (number % i == 0)
                    return false;
            }

            return true;
        }
    }
}
