using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace UE5_Parkhaus
{
    /// <summary>
    /// Aufgabe 4: Producer/Consumer. Autos parken wie im EinfachParkhaus und
    /// legen sich danach als Bezahl-Auftrag in eine gemeinsame Queue (Producer).
    /// Ein einziger Kassa-Thread (Consumer) arbeitet die Queue der Reihe nach ab.
    /// </summary>
    public class KassaParkhaus : Parkhaus
    {
        private readonly SemaphoreSlim _plaetze = new SemaphoreSlim(KAPAZITAET, KAPAZITAET);
        private readonly Queue<Auto> _kassaQueue = new Queue<Auto>();
        private readonly object _lock = new object();

        public KassaParkhaus()
        {
            // Ein einziger Kassa-Thread (Consumer). Kein zweites Lambda
            // (new Thread(Methode) statt new Thread(() => Methode())).
            new Thread(KassaSchleife) { IsBackground = true }.Start();
        }

        public override void Einfahren(Auto a)
        {
            // --- Parken wie im EinfachParkhaus ---
            a.Status = "Wartet";
            _plaetze.Wait();
            try
            {
                Parken(a, MainWindow.parkBox);
            }
            finally
            {
                _plaetze.Release();
            }

            // --- Producer: Auto als Bezahl-Auftrag in die Queue legen ---
            a.Status = "Wartet auf Kassa";
            lock (_lock)
            {
                _kassaQueue.Enqueue(a);
                // Kassa-Thread wecken (es gibt nur einen Consumer -> Pulse reicht).
                Monitor.Pulse(_lock);
            }
        }

        // Consumer-Schleife der (einzigen) Kassa.
        private void KassaSchleife()
        {
            while (true)
            {
                Auto a;
                lock (_lock)
                {
                    // Kein Busy-Waiting: solange leer, mit Monitor.Wait blockieren
                    // (gibt das Lock temporaer frei). while (kein if!) gegen spurious wakeups.
                    while (_kassaQueue.Count == 0)
                    {
                        Monitor.Wait(_lock);
                    }
                    a = _kassaQueue.Dequeue();
                }

                // Kassieren ausserhalb des Locks (haelt die Kassa nicht unnoetig lange gesperrt).
                a.Status = "Zahlt";
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow.kassaBox.Items.Add(a);
                });

                Thread.Sleep(800); // Bezahlvorgang

                a.Status = "Bezahlt";
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow.kassaBox.Items.Remove(a);
                });
            }
        }
    }
}
