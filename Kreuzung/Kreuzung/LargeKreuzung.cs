using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kreuzung
{
    internal class LargeKreuzung : BKreuzung
    {
        ManualResetEventSlim vLock = new ManualResetEventSlim(true);
        ManualResetEventSlim hLock = new ManualResetEventSlim(false);

        public virtual void Cross(Car car)
        {
            bool isVert = car.fromDirection == Direction.North || car.fromDirection == Direction.South;

            if (isVert)
                vLock.Wait();
            else
                hLock.Wait();

            Update(car, true);
            Thread.Sleep(1000);
            Update(car, false);
            base.moveToExitBox(car);    
            if(isVert)
                BKreuzung.hAmount--;

            if(BKreuzung.hAmount == 0)
            {
                vLock.Reset();
                hLock.Set();
            }

        }

        private void Update(Car car, bool add)
        {
            MainWindow.kListBox.Dispatcher.BeginInvoke(new Action(() => {
                if(add)
                    MainWindow.kListBox.Items.Add(car.carname);
                else
                    MainWindow.kListBox.Items.Remove(car.carname);
            }));
        }
    }
}
