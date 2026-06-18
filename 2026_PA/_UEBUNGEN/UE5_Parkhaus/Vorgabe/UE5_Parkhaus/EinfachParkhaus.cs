using System;
using System.Threading;

namespace UE5_Parkhaus
{
    /// <summary>
    /// Aufgabe 1: Parkhaus mit genau KAPAZITAET (=3) Plaetzen.
    /// Synchronisation ausschliesslich mit SemaphoreSlim.
    /// </summary>
    public class EinfachParkhaus : Parkhaus
    {
        // TODO (Aufgabe 1): SemaphoreSlim mit KAPAZITAET Plaetzen anlegen.
        //   private readonly SemaphoreSlim _plaetze = new SemaphoreSlim(KAPAZITAET, KAPAZITAET);

        public override void Einfahren(Auto a)
        {
            // TODO (Aufgabe 1):
            //  1. a.Status = "Wartet" setzen.
            //  2. Mit Wait() auf einen freien Platz warten.
            //  3. try { Parken(a, MainWindow.parkBox); }
            //  4. finally { Release(); }   <-- IMMER freigeben!
            throw new NotImplementedException();
        }
    }
}
