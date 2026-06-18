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

        // Privates readonly Lock-Objekt nur fuer die Synchronisation der Ladeplaetze.
        private readonly object _lock = new object();

        // SemaphoreSlim fuer die Gesamtkapazitaet (6 Stellplaetze).
        private readonly SemaphoreSlim _plaetze = new SemaphoreSlim(GESAMT, GESAMT);

        // Anzahl der aktuell freien Ladesaeulen (Start = 2).
        private int _freieLadeplaetze = LADE;

        public override void Einfahren(Auto a)
        {
            a.Status = "Wartet";

            // 1. Zuerst auf einen der GESAMT (=6) Stellplaetze warten.
            _plaetze.Wait();

            bool ladeplatzBelegt = false;
            try
            {
                if (a.IstElektro)
                {
                    // 2. Elektroauto braucht zwingend eine Ladesaeule. Im lock auf
                    //    eine freie Ladesaeule warten. while (kein if!) schuetzt vor
                    //    spurious wakeups und davor, dass mehrere geweckte Autos
                    //    denselben Ladeplatz nehmen.
                    lock (_lock)
                    {
                        while (_freieLadeplaetze == 0)
                        {
                            Monitor.Wait(_lock);
                        }
                        _freieLadeplaetze--;
                        ladeplatzBelegt = true;
                        // Platznummer der belegten Ladesaeule (1..LADE) - innerhalb
                        // des Locks bestimmen, damit kein anderes Auto dazwischenfunkt.
                        a.Platz = LADE - _freieLadeplaetze;
                    }
                    a.Status = "Laedt";
                }
                else
                {
                    // Verbrenner nimmt einen normalen Platz (Nummern ab LADE+1).
                    a.Platz = 0;
                    a.Status = "Parkt";
                }

                Parken(a, MainWindow.parkBox);
            }
            finally
            {
                // 3. Ladeplatz zurueckgeben und wartende Autos wecken - nur falls
                //    dieses Auto tatsaechlich einen Ladeplatz belegt hatte.
                if (ladeplatzBelegt)
                {
                    lock (_lock)
                    {
                        _freieLadeplaetze++;
                        // Wartende Elektroautos wecken (PulseAll, da mehrere warten koennen).
                        Monitor.PulseAll(_lock);
                    }
                }

                // 4. Stellplatz in jedem Fall wieder freigeben.
                _plaetze.Release();
            }
        }
    }
}
