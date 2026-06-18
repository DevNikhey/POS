using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PA1_4B_2025
{
    internal class BigAirport : Airport
    {
        private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);
        private object lockObj2 = new();

        // wie viele gerade warten
        private int counter = 0;
        // anzahl der aktuellen erlaubten landebahnen, für berechnungen
        private int anzahlLande = 1;

        public override void Land(Plane p)
        {
            lock(lockObj2)
            {
                counter++;
                // wenn mehr als 5 * landebahnen sind, neue landebahn aufmachen. => für 5, 10, 20 steps
                if(counter > 5 * anzahlLande)
                {
                    semaphoreSlim.Release(1);
                    anzahlLande++;
                    p.LandingStrip = anzahlLande;
                }
            }

            semaphoreSlim.Wait();
            this.Landen(p);
            semaphoreSlim.Release();

            lock (lockObj2)
            {
                counter--;
                if(counter < 5 * anzahlLande)
                {
                    anzahlLande--;
                }
            }
        }

        private void Landen(Plane p)
        {
            p.Status = "Landing";
            Application.Current.Dispatcher.Invoke(new Action(() => {
                MainWindow.flyingBox.Items.Remove(p);
                MainWindow.bigBox.Items.Add(p);
            }));

            Thread.Sleep(1000); // Landung

            p.Status = "Landed";
            Application.Current.Dispatcher.Invoke(new Action(() => {
                MainWindow.bigBox.Items.Remove(p);
                MainWindow.bigBox.Items.Add(p);
            }));

        }
    }
}
