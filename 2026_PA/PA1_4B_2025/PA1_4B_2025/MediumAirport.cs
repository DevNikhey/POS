using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PA1_4B_2025
{
    internal class MediumAirport : Airport
    {
        private Object runway1Lock = new object();
        private Object runway2Lock = new object();

        private Object counterLock = new object();
        private int lastRunWay = 1;

        public override void Land(Plane p)
        {
            lock(counterLock)
            {
                if (lastRunWay == 2)
                {
                    lock (runway1Lock)
                    {
                        p.LandingStrip = 1;
                        this.Landen(p);
                        lastRunWay = 1;
                    }
                }
                else
                {
                    lock (runway2Lock)
                    {
                        p.LandingStrip = 2;
                        this.Landen(p);
                        lastRunWay = 2;
                    }
                }
            }
        }

        private void Landen(Plane p)
        {
            p.Status = "Landing";
            Application.Current.Dispatcher.Invoke(new Action(() => {
                MainWindow.flyingBox.Items.Remove(p);
                MainWindow.mediumBox.Items.Add(p);
            }));

            Thread.Sleep(1000); // Landung

            p.Status = "Landed";
            Application.Current.Dispatcher.Invoke(new Action(() => {
                MainWindow.mediumBox.Items.Remove(p);
                MainWindow.mediumBox.Items.Add(p);
            }));

        }
    }
}
