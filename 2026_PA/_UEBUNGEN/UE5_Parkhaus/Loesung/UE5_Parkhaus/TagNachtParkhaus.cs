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
        private readonly SemaphoreSlim _plaetze = new SemaphoreSlim(KAPAZITAET, KAPAZITAET);

        // Tor: gesetzt (Set) = offen, zurueckgesetzt (Reset) = geschlossen.
        // Startwert true -> Parkhaus startet im Zustand OFFEN.
        private readonly ManualResetEventSlim _tor = new ManualResetEventSlim(true);

        public TagNachtParkhaus()
        {
            // Hintergrund-Thread: oeffnet/schliesst das Tor im 5-Sekunden-Takt
            // und aktualisiert das Status-Label ueber den Dispatcher.
            new Thread(() =>
            {
                while (true)
                {
                    // OFFEN
                    _tor.Set();
                    SetzeStatus("Parkhaus: OFFEN");
                    Thread.Sleep(5000);

                    // GESCHLOSSEN
                    _tor.Reset();
                    SetzeStatus("Parkhaus: GESCHLOSSEN");
                    Thread.Sleep(5000);
                }
            })
            { IsBackground = true }.Start();
        }

        public override void Einfahren(Auto a)
        {
            // REIHENFOLGE: erst auf das offene Tor warten, DANN einen Platz belegen.
            // So blockiert ein wartendes Auto keinen Stellplatz, solange das Tor zu ist.
            a.Status = "Wartet";

            // 1. Warten, bis das Tor offen ist (blockiert, solange Reset/geschlossen).
            _tor.Wait();

            // 2. Erst jetzt auf einen freien Platz warten.
            _plaetze.Wait();
            try
            {
                Parken(a, MainWindow.parkBox);
            }
            finally
            {
                // Platz in jedem Fall wieder freigeben.
                _plaetze.Release();
            }
        }

        private void SetzeStatus(string text)
        {
            // GUI-Zugriff aus dem Hintergrund-Thread nur ueber den Dispatcher.
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.statusLabel.Content = text;
            });
        }
    }
}
