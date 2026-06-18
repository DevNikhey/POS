using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
// using LinqToDB;          // TODO (Aufgabe 1): nach dem Generieren des Modells aktivieren
// using DataModels;        // TODO (Aufgabe 1): enthaelt MoviesDB + Entity DataModels.Film

namespace UE4_Medien
{
    /// <summary>
    /// Interaktionslogik fuer MainWindow.xaml
    ///
    /// Aufgabe: Filme aus movies.db laden, in der ListBox anzeigen, Auswahl ->
    /// Detailansicht + Bewertungsleiste, neuen Film per InsertWithInt64Identity
    /// einfuegen.
    ///
    /// HINWEIS Namenskollision: Das generierte Entity heisst ebenfalls 'Film'
    /// (DataModels.Film). Beim DB-Zugriff voll qualifizieren: DataModels.Film.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // TODO (Aufgabe 4): LoadFilms() und das erste Zeichnen der
            // Bewertungsleiste duerfen NICHT hier im Konstruktor passieren,
            // weil ratingCanvas.ActualWidth hier noch 0 ist.
            // Stosse beides ueber das Loaded-Event an, z.B.:
            //   this.Loaded += (s, e) => { LoadFilms(); /* erste Bewertungsleiste */ };
        }

        /// <summary>
        /// Laedt alle Filme aus movies.db und zeigt sie in der ListBox an.
        /// </summary>
        private void LoadFilms()
        {
            // TODO (Aufgabe 1): Verbindung aufbauen:
            //   var options = new DataOptions().UseSQLite("Data Source=movies.db");
            //   using var db = new MoviesDB(options);
            //
            // TODO (Aufgabe 2): Alle Filme laden (db.Films.ToList()), pro Zeile
            //   ein ViewModel Film bauen und ALLE Felder fuellen (Id nicht vergessen!),
            //   dann filmListe.ItemsSource = liste; setzen.
            //   (item.Id / item.Year / item.Rating sind long? -> casten/konvertieren)
            throw new NotImplementedException();
        }

        /// <summary>
        /// Auswahl in der ListBox -> Detailansicht fuellen + Bewertungsleiste zeichnen.
        /// </summary>
        private void filmListe_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO (Aufgabe 3):
            //  - SelectedItem holen (auf null pruefen!) und als Film casten
            //  - detailTitle/detailDirector/detailYear/detailRating fuellen
            //  - DrawRatingBar(film.Rating) aufrufen
            throw new NotImplementedException();
        }

        /// <summary>
        /// Zeichnet die Bewertungsleiste (0..10) als Rechteck auf ratingCanvas.
        /// Breite = rating/10 * ratingCanvas.ActualWidth, Hoehe = ratingCanvas.ActualHeight.
        /// </summary>
        private void DrawRatingBar(int rating)
        {
            // TODO (Aufgabe 4):
            //  - ratingCanvas.Children.Clear();
            //  - Rectangle erzeugen, Width = rating / 10.0 * ratingCanvas.ActualWidth,
            //    Height = ratingCanvas.ActualHeight, Fill = z.B. Brushes.SteelblueGreen
            //  - dem Canvas hinzufuegen (Canvas.SetLeft/SetTop = 0)
            //  ACHTUNG: ActualWidth ist im Konstruktor 0 -> erstes Zeichnen ins Loaded-Event!
            throw new NotImplementedException();
        }

        /// <summary>
        /// Neuen Film aus den Eingabefeldern in die DB einfuegen.
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // TODO (Aufgabe 5):
            //  - neuTitle.Text / neuDirector.Text lesen, neuYear.Text und neuRating.Text
            //    mit int.TryParse pruefen (ungueltige Eingaben -> MessageBox + return)
            //  - Bewertung muss im Bereich 0..10 liegen
            //  - DataModels.Film anlegen und mit db.InsertWithInt64Identity(film) einfuegen
            //    (liefert die neue Id zurueck)
            //  - danach LoadFilms() aufrufen, damit der neue Film erscheint
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// ViewModel fuer die Anzeige in der ListBox / Detailansicht.
    /// (Bewusst getrennt vom generierten DB-Entity DataModels.Film.)
    /// </summary>
    public class Film
    {
        public long Id { get; set; }
        public string Title { get; set; } = "";
        public string Director { get; set; } = "";
        public int Year { get; set; }
        public int Rating { get; set; }
    }
}
