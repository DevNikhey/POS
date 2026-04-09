using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kreuzung
{
    internal class BKreuzung
    {
        Car middleCar;
        ManualResetEventSlim kLock = new ManualResetEventSlim(true);
        public static int hAmount = 0;

        public virtual void Cross(Car car)
        {
            kLock.Wait();
            kLock.Reset();

            middleCar = car;
            car.UpdateUI(false);
            this.updateUI();

            Thread.Sleep(1000);

            Leave();
            moveToExitBox(car);

            kLock.Set();
        }


        private void updateUI()
        {
            MainWindow.kListBox.Dispatcher.BeginInvoke(new Action(() => {
                MainWindow.kListBox.Items.Clear();
                if (middleCar != null)
                    MainWindow.kListBox.Items.Add(middleCar.carname);
            }));
        }

        private void Leave()
        {
            middleCar = null;
            updateUI();
        }

        public void moveToExitBox(Car car)
        {
            MainWindow.EastListBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                switch (car.fromDirection)
                {
                    case Direction.North:
                        MainWindow.SouthListBox.Items.Add(car.carname + " Exited");
                        break;
                    case Direction.East:
                        MainWindow.WestListBox.Items.Add(car.carname + " Exited");
                        break;
                    case Direction.South:
                        MainWindow.NorthListBox.Items.Add(car.carname + " Exited");
                        break;
                    case Direction.West:
                        MainWindow.EastListBox.Items.Add(car.carname + " Exited");
                        break;
                }
            }));
        }
    }
}
