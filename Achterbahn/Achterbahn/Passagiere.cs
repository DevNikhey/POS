using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

namespace Achterbahn
{
    class Passagiere
    {
        private Wagon wagon;

        public string name { get; }

        public Passagiere(Wagon wagon, string name)
        {
            this.wagon = wagon;
            this.name = name;
        }

        public void Run()
        {
            while (true)
            {
                wagon.rideReadyEvent.Wait();
                wagon.AddPassenger(this);

                wagon.rideFinishedEvent.Wait();
                UpdateUI_AddWalking(name);
                Console.WriteLine($"{name} spaziert..");

                Thread.Sleep(new Random().Next(1000, 3000));
            }
        }

        public void UpdateUI_AddWalking(string name)
        {
            MainWindow.walkingBox.Dispatcher.Invoke(() =>
            {
                MainWindow.walkingBox.Items.Add(name);
                MainWindow.ridingBox.Items.Remove(name);
            });
        }
    }
}
