using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace UE5_Parkhaus
{
    /// <summary>
    /// Abstrakte Basisklasse fuer alle Parkhaus-Varianten.
    /// Die Methode Parken(...) ist VORGEGEBEN und darf NICHT veraendert werden.
    /// </summary>
    public abstract class Parkhaus
    {
        /// <summary>Standard-Kapazitaet fuer die einfachen Varianten.</summary>
        protected const int KAPAZITAET = 3;

        /// <summary>
        /// Wird pro Auto aus dessen Thread aufgerufen. Hier passiert die
        /// Synchronisation (das ist DEINE Aufgabe in den abgeleiteten Klassen).
        /// </summary>
        public abstract void Einfahren(Auto a);

        /// <summary>
        /// VORGEGEBEN: zeigt das Auto in der angegebenen ListBox an, simuliert
        /// die Parkdauer und entfernt es wieder. GUI-Zugriff erfolgt korrekt
        /// ueber den Dispatcher. NICHT veraendern - einfach aufrufen.
        /// </summary>
        protected void Parken(Auto a, ListBox box)
        {
            a.Status = a.IstElektro ? "Laedt" : "Parkt";
            Application.Current.Dispatcher.Invoke(() =>
            {
                box.Items.Add(a);
            });

            Thread.Sleep(new Random().Next(1500, 3500)); // Parkdauer

            a.Status = "Weg";
            Application.Current.Dispatcher.Invoke(() =>
            {
                box.Items.Remove(a);
            });
        }
    }

    // ============================================================
    // Aufgabe 5 - Antworten hier als Kommentar eintragen:
    //
    // 1) Warum Release() im finally?
    //    TODO: ...
    //
    // 2) Warum while statt if rund um Monitor.Wait?
    //    TODO: ...
    //
    // 3) freieLadeplaetze ohne lock - konkretes Fehlszenario?
    //    TODO: ...
    // ============================================================
}
