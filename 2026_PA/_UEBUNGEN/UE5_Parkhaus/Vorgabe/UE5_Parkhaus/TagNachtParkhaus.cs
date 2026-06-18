using System;
using System.Threading;
using System.Windows;

namespace UE5_Parkhaus
{
    /// <summary>
    /// Aufgabe 2: wie EinfachParkhaus, aber mit Oeffnungszeiten.
    /// Ein Hintergrund-Thread schaltet abwechselnd OFFEN (5s) / GESCHLOSSEN (5s).
    /// Ein Auto darf nur einfahren, wenn das Tor offen ist.
    /// </summary>
    public class TagNachtParkhaus : Parkhaus
    {
        // TODO (Aufgabe 2): SemaphoreSlim (Kapazitaet) + ManualResetEventSlim ("Tor").
        //   private readonly SemaphoreSlim _plaetze = new SemaphoreSlim(KAPAZITAET, KAPAZITAET);
        //   private readonly ManualResetEventSlim _tor = new ManualResetEventSlim(true);

        public TagNachtParkhaus()
        {
            // TODO (Aufgabe 2): Hintergrund-Thread starten, der in einer
            // Endlosschleife abwechselnd das Tor oeffnet (Set) und schliesst
            // (Reset), je 5000 ms, und MainWindow.statusLabel ueber den
            // Dispatcher aktualisiert ("Parkhaus: OFFEN" / "Parkhaus: GESCHLOSSEN").
            //
            //   new Thread(() => { while (true) { /* OFFEN / GESCHLOSSEN */ } })
            //       { IsBackground = true }.Start();
        }

        public override void Einfahren(Auto a)
        {
            // TODO (Aufgabe 2):
            //  - Auf das offene Tor warten (Wait des ManualResetEventSlim).
            //  - Auf einen freien Platz warten (SemaphoreSlim).
            //  - Ueberlege die REIHENFOLGE: ein wartendes Auto soll keinen Platz
            //    belegen, solange das Tor geschlossen ist.
            //  - Parken(a, MainWindow.parkBox) im try, Release() im finally.
            throw new NotImplementedException();
        }
    }
}
