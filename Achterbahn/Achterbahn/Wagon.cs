using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Achterbahn
{
    internal class Wagon
    {
        private int plaetze;
        private int maxFahrten;
        private int aktuelleFahrt = 0;
        
        private List<Passagiere> aktuellePassagiere = new List<Passagiere>();

        public ManualResetEventSlim rideReadyEvent = new ManualResetEventSlim(true);
        public ManualResetEventSlim rideFinishedEvent = new ManualResetEventSlim(false);

        public Wagon(int plaetze, int maxFahrten)
        {
            this.plaetze = plaetze;
            this.maxFahrten = maxFahrten;
        }

        public void AddPassenger(Passagiere p)
        {
            lock (aktuellePassagiere)
            {
                if (aktuellePassagiere.Count < plaetze)
                {
                    aktuellePassagiere.Add(p);
                    UpdateUI_AddRiding(p.name);
                    if (aktuellePassagiere.Count == plaetze)
                    {
                        Task.Run(() => Drive());
                    }
                }
            }
        }

        private void Drive()
        {
            rideReadyEvent.Reset();
            aktuelleFahrt++;

            MainWindow.counterLabel.Dispatcher.Invoke(() => {
                MainWindow.counterLabel.Content = $"Fahrt: {aktuelleFahrt}/{MainWindow.anzahlFahrten}";
            });

            Console.WriteLine($"Fahrt {aktuelleFahrt} gestartet..");

            Thread.Sleep(new Random().Next(2000, 4000));
            Console.WriteLine($"Fahrt {aktuelleFahrt} beendet!");

            lock (aktuellePassagiere)
            {
                aktuellePassagiere.Clear();
                ClearRidingList();
            }

            rideFinishedEvent.Set();
            Thread.Sleep(100);

            if (aktuelleFahrt < maxFahrten)
            {
                rideFinishedEvent.Reset();
                rideReadyEvent.Set();
            }
            else
            {
                Console.WriteLine("Achterbahn fertig!");

                MainWindow.counterLabel.Dispatcher.Invoke(() => {
                    MainWindow.counterLabel.Content = $"Fertig";
                });
            }
        }


        public void UpdateUI_AddRiding(string name)
        {
            MainWindow.ridingBox.Dispatcher.Invoke(() => {
                MainWindow.ridingBox.Items.Add(name);
                MainWindow.walkingBox.Items.Remove(name);
            });
        }

        public void ClearRidingList()
        {
            MainWindow.ridingBox.Dispatcher.Invoke(() => MainWindow.ridingBox.Items.Clear());
        }
    }
}