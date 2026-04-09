using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Kreuzung
{
    internal class Car
    {
        public string carname { get; }
        public int id;
        public Direction fromDirection { get;  }

        public Car(int id)
        {
            this.carname = "Car " + id;
            this.id = id;
            this.fromDirection = (Direction)new Random().Next(0, 4);

            if(this.fromDirection == Direction.West || this.fromDirection == Direction.East)
            {
                // Horizontal
                BKreuzung.hAmount++;
            }

            UpdateUI(true);
        }

        public void Drive(BKreuzung kreuzung)
        {
            Thread.Sleep(new Random().Next(1000, 10000));
            kreuzung.Cross(this);
        }

        public void UpdateUI(bool add)
        {
            MainWindow.kListBox.Dispatcher.BeginInvoke(new Action(() => {
                switch (fromDirection)
                {
                    case Direction.North:
                        if (add)
                            MainWindow.NorthListBox.Items.Add(carname);
                        else
                            MainWindow.NorthListBox.Items.Remove(carname);
                        break;
                    case Direction.East:
                        if (add)
                            MainWindow.EastListBox.Items.Add(carname);
                        else
                            MainWindow.EastListBox.Items.Remove(carname);
                        break;
                    case Direction.South:
                        if (add)
                            MainWindow.SouthListBox.Items.Add(carname);
                        else
                            MainWindow.SouthListBox.Items.Remove(carname);
                        break;
                    case Direction.West:
                        if (add)
                            MainWindow.WestListBox.Items.Add(carname);
                        else
                            MainWindow.WestListBox.Items.Remove(carname);
                        break;
                }
            }));
        }
    }
}
