using System;
using System.Threading;

namespace UE5_Parkhaus
{
    /// <summary>
    /// Aufgabe 3: 6 Plaetze, davon nur 2 mit Ladesaeule.
    /// Elektroautos brauchen zwingend einen Ladeplatz und warten ggf. darauf
    /// (Monitor.Wait in while-Schleife, Monitor.Pulse/PulseAll beim Verlassen).
    /// </summary>
    public class LadeParkhaus : Parkhaus
    {
        private const int GESAMT = 6;
        private const int LADE = 2;

        // TODO (Aufgabe 3): privates readonly Lock-Objekt + SemaphoreSlim(GESAMT)
        //   private readonly object _lock = new object();
        //   private readonly SemaphoreSlim _plaetze = new SemaphoreSlim(GESAMT, GESAMT);
        //   private int _freieLadeplaetze = LADE;

        public override void Einfahren(Auto a)
        {
            // TODO (Aufgabe 3):
            //  - Mit SemaphoreSlim auf einen der GESAMT Plaetze warten.
            //  - Wenn a.IstElektro: im lock(_lock) mit
            //        while (_freieLadeplaetze == 0) Monitor.Wait(_lock);
            //    auf eine freie Ladesaeule warten, dann _freieLadeplaetze--.
            //  - a.Platz / a.Status sinnvoll setzen.
            //  - Parken(a, MainWindow.parkBox) aufrufen.
            //  - Im finally: Ladeplatz zurueckgeben (lock + _freieLadeplaetze++
            //    + Monitor.Pulse/PulseAll) UND _plaetze.Release().
            throw new NotImplementedException();
        }
    }
}
