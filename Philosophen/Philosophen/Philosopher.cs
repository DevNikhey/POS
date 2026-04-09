using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Philosophen
{
    internal class Philosopher(String name, Fork left, Fork right, TextBox textBox)
    {    
        private static Random random = new Random();
        public static int dThink { get; set; } = 1000;
        public static int vThink { get; set; } = 200;
 
        public static int dEat { get; set; } = 200;
        public static int vEat { get; set; } = 40;
        public static int take { get; set; } = 40;

        public static bool running = true;
        public static int count = 0;
        public static TextBox counterBox;


        private String name = name;
        private int position;
        private Fork leftFork = left;
        private Fork rightFork = right;
        private TextBox textbox = textBox;
        private Thread thread;

        public void startThread()
        {
            Thread thread = new Thread(new ThreadStart(run));
            thread.Start();
        }

        public void stopThread()
        {
            running = false;
            thread.Interrupt();
        }

        private void run()
        {
            try
            {
                while (running)
                {
                    think();
                    eat();
                }
            }
            catch (ThreadInterruptedException)
            {
                // Thread interrupted, exit gracefully
            }
        }

        void think()
        {
            Thread.Sleep(_GetRandomNumber(dThink, vThink));
            changeStatus(Brushes.LightGray, "denkt");
        }

        void eat()
        {
            changeStatus(Brushes.Tomato, "wartet");
            lock(rightFork)
            {
                changeStatus(Brushes.Green, "nimmt");
                Thread.Sleep(take);
                changeStatus(Brushes.Tomato, "wartet");
                lock(leftFork)
                {
                    changeStatus(Brushes.Azure, "nimmt");
                    Thread.Sleep(take);
                    changeStatus(Brushes.Green, "isst");
                    Thread.Sleep(_GetRandomNumber(dEat, vEat));
                }
            }
            changeStatus(Brushes.LightGray, "denkt");

            counterBox.Dispatcher.Invoke(() => {
                count++;
                counterBox.Text = "Counter: " + count.ToString();
            });
        }

        void changeStatus(SolidColorBrush color, String text)
        {
            textbox.Dispatcher.Invoke(() => {
                textbox.Text = text;
                textbox.Background = color;
                textbox.ScrollToEnd();
            });
        }

        private int _GetRandomNumber(int mean, int variance)
        {
            int min = Math.Max(0, mean - variance);
            int max = mean + variance;
            return random.Next(min, max);
        }
    }
}
