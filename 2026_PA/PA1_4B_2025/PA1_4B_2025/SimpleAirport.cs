using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PA1_4B_2025
{
    internal class SimpleAirport : Airport
    {
        private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        // Event für Tag und Nicht, Tag = true, nicht = false
        private ManualResetEventSlim dayNightEvent = new ManualResetEventSlim(true);

        public SimpleAirport()
        {
            // Day night thread
            new Thread(() =>
            {
                while(true)
                {
                    dayNightCycle();
                }
            }).Start();
        }

        public override void Land(Plane p)
        {
            semaphoreSlim.Wait();
            dayNightEvent.Wait();
            //TODO: here 
            p.Status = "Landing";
            Application.Current.Dispatcher.Invoke(new Action(() => {
                MainWindow.flyingBox.Items.Remove(p);
                MainWindow.simpleBox.Items.Add(p);
            }));

            Thread.Sleep(1000); // Landung

            p.Status = "Landed";
            Application.Current.Dispatcher.Invoke(new Action(() => {
                MainWindow.simpleBox.Items.Remove(p);
                MainWindow.simpleBox.Items.Add(p);
            }));

            semaphoreSlim.Release();
        }

        private void dayNightCycle()
        {
            // tag
            dayNightEvent.Set();
            updateUIDay("Tag");
            Thread.Sleep(5000);
            // nacht
            dayNightEvent.Reset();
            updateUIDay("Nacht");
            Thread.Sleep(5000);
        }

        private void updateUIDay(String time)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.simpleLabel.Content = "Simple Airport - " + time;
            });
        }
    }
}
