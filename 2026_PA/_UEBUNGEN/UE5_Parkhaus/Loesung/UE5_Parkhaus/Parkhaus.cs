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
    // Aufgabe 5 - Antworten:
    //
    // 1) Warum Release() im finally?
    //    Damit der belegte Platz IN JEDEM FALL wieder freigegeben wird - auch
    //    wenn beim Parken eine Exception fliegt. Stuende Release() einfach nach
    //    Parken(...), wuerde es bei einer Exception uebersprungen; der Platz waere
    //    dauerhaft "verloren", das Parkhaus liefe nach und nach voll und blockierte
    //    schliesslich fuer immer (deadlock-aehnliches Verhalten).
    //
    // 2) Warum while statt if rund um Monitor.Wait?
    //    Erstens kann ein Thread durch "spurious wakeups" aufwachen, ohne dass die
    //    Bedingung erfuellt ist. Zweitens kann zwischen dem Pulse und dem erneuten
    //    Erlangen des Locks ein anderer geweckter Thread den frei gewordenen Platz
    //    bereits genommen haben. Mit while wird die Bedingung nach dem Aufwachen
    //    erneut geprueft und ggf. weiter gewartet; ein if wuerde faelschlich
    //    fortfahren und z. B. _freieLadeplaetze unter 0 druecken.
    //
    // 3) freieLadeplaetze ohne lock - konkretes Fehlszenario?
    //    _freieLadeplaetze-- ist keine atomare Operation (Lesen, Verringern,
    //    Schreiben). Beispiel: Es ist nur noch 1 Ladeplatz frei. Zwei Elektroautos
    //    lesen gleichzeitig den Wert 1, beide sehen "frei > 0", beide dekrementieren
    //    und parken an derselben (einzigen) Ladesaeule. Der Zaehler steht danach auf
    //    0 oder -1, und zwei Autos belegen physisch denselben Ladeplatz -> die
    //    Kapazitaetsbeschraenkung ist verletzt. Ausserdem koennen Monitor.Wait/Pulse
    //    ohne gehaltenes Lock gar nicht korrekt verwendet werden (SynchronizationLockException).
    // ============================================================
}
