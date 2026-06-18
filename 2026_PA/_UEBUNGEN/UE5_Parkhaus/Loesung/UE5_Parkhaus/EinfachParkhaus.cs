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
        // SemaphoreSlim mit KAPAZITAET (=3) Plaetzen: anfaenglicher und maximaler
        // Zaehlerstand ist KAPAZITAET -> es koennen nie mehr als 3 Autos parken.
        private readonly SemaphoreSlim _plaetze = new SemaphoreSlim(KAPAZITAET, KAPAZITAET);

        public override void Einfahren(Auto a)
        {
            // 1. Auto wartet (zunaechst), bis ein Platz frei ist.
            a.Status = "Wartet";

            // 2. Auf einen freien Platz warten. Wait() blockiert, solange alle
            //    KAPAZITAET Plaetze belegt sind.
            _plaetze.Wait();
            try
            {
                // 3. Parken erledigt Status ("Parkt"/"Laedt"), Anzeige und Parkdauer.
                Parken(a, MainWindow.parkBox);
            }
            finally
            {
                // 4. Platz IN JEDEM FALL wieder freigeben - auch bei Exception.
                _plaetze.Release();
            }
        }
    }
}
