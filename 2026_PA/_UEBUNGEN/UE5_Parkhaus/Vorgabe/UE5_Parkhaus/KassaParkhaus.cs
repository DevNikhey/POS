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
        // TODO (Aufgabe 4): SemaphoreSlim(KAPAZITAET) fuer die Plaetze,
        //   Queue<Auto> als Bezahl-Schlange und ein privates readonly Lock-Objekt.
        //   private readonly SemaphoreSlim _plaetze = new SemaphoreSlim(KAPAZITAET, KAPAZITAET);
        //   private readonly Queue<Auto> _kassaQueue = new Queue<Auto>();
        //   private readonly object _lock = new object();

        public KassaParkhaus()
        {
            // TODO (Aufgabe 4): Kassa-Thread (Consumer) starten.
            //   In Endlosschleife: lock(_lock) { while (_kassaQueue.Count == 0)
            //   Monitor.Wait(_lock); auto = _kassaQueue.Dequeue(); }
            //   Danach "kassieren": kurzes Thread.Sleep, a.Status = "Bezahlt",
            //   Auto kurz in MainWindow.kassaBox anzeigen (Dispatcher!).
            //
            //   new Thread(KassaSchleife) { IsBackground = true }.Start();
        }

        public override void Einfahren(Auto a)
        {
            // TODO (Aufgabe 4):
            //  - Auf einen Platz warten (SemaphoreSlim), Parken(a, MainWindow.parkBox),
            //    Release() im finally.
            //  - DANACH (Producer): a in die Queue legen
            //        lock(_lock) { _kassaQueue.Enqueue(a); Monitor.Pulse(_lock); }
            throw new NotImplementedException();
        }

        // TODO (Aufgabe 4): Consumer-Schleife der Kassa.
        // private void KassaSchleife() { ... }
    }
}
