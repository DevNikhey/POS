using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PA1_4B_2025
{
    internal class VipAirport : Airport
    {
        //ob gerade ein normales flugzeug landen kann
        private AutoResetEvent landingevent = new AutoResetEvent(true);
        // Es kann trotzdem immer nur ein Flugzeug gleichzeitig landen
        private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public override void Land(Plane p)
        {
            if (p.IsVip) {
                landingevent.Reset();
                // vip landet
                Landen(p);

                landingevent.Set();
            } else
            {
                landingevent.WaitOne();
                // 08/15 landet
                Landen(p);
                landingevent.Set();
            }    
        }

        private void Landen(Plane p)
        {
            semaphoreSlim.Wait();
            //TODO: here 
            p.Status = "Landing";
            Application.Current.Dispatcher.Invoke(new Action(() => {
                MainWindow.flyingBox.Items.Remove(p);
                MainWindow.vipBox.Items.Add(p);
            }));

            Thread.Sleep(1000); // Landung

            p.Status = "Landed";
            Application.Current.Dispatcher.Invoke(new Action(() => {
                MainWindow.vipBox.Items.Remove(p);
                MainWindow.vipBox.Items.Add(p);
            }));

            semaphoreSlim.Release();
        }
    }
}
