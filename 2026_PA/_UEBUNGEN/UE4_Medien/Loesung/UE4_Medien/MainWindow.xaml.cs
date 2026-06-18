using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using LinqToDB;          // Aufgabe 1: nach dem Generieren des Modells aktiviert
using DataModels;        // Aufgabe 1: enthaelt MoviesDB + Entity DataModels.Film

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

            // Aufgabe 4: LoadFilms() und das erste Zeichnen der Bewertungsleiste
            // duerfen NICHT im Konstruktor passieren, weil ratingCanvas.ActualWidth
            // hier noch 0 ist. Beides wird ueber das Loaded-Event angestossen.
            this.Loaded += (s, e) =>
            {
                LoadFilms();

                // Erste Bewertungsleiste fuer den ersten Film anzeigen
                // (sofern es ueberhaupt Filme gibt).
                if (filmListe.Items.Count > 0)
                {
                    Film ersterFilm = (Film)filmListe.Items[0];
                    DrawRatingBar(ersterFilm.Rating);
                }
            };
        }

        /// <summary>
        /// Laedt alle Filme aus movies.db und zeigt sie in der ListBox an.
        /// </summary>
        private void LoadFilms()
        {
            // Aufgabe 1: Verbindung aufbauen
            var options = new DataOptions().UseSQLite("Data Source=movies.db");
            using var db = new MoviesDB(options);

            // Aufgabe 2: Alle Filme laden und pro Zeile ein ViewModel Film bauen.
            // ALLE Felder fuellen (Id nicht vergessen!), DB-Felder sind long? ->
            // casten/konvertieren.
            List<Film> liste = new List<Film>();

            foreach (var item in db.Films.ToList())
            {
                Film film = new Film
                {
                    Id = (long)item.Id,
                    Title = item.Title ?? "",
                    Director = item.Director ?? "",
                    Year = (int)(item.Year ?? 0),
                    Rating = (int)(item.Rating ?? 0)
                };

                liste.Add(film);
            }

            filmListe.ItemsSource = liste;
        }

        /// <summary>
        /// Auswahl in der ListBox -> Detailansicht fuellen + Bewertungsleiste zeichnen.
        /// </summary>
        private void filmListe_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Aufgabe 3: SelectedItem holen und auf null pruefen (z.B. bei Programmstart
            // oder nach dem Neuladen der Liste ist nichts ausgewaehlt).
            if (filmListe.SelectedItem == null)
            {
                return;
            }

            Film film = (Film)filmListe.SelectedItem;

            detailTitle.Text = film.Title;
            detailDirector.Text = "Regie: " + film.Director;
            detailYear.Text = "Jahr: " + film.Year;
            detailRating.Text = "Bewertung: " + film.Rating + " / 10";

            // Bewertungsleiste passend zum ausgewaehlten Film zeichnen.
            DrawRatingBar(film.Rating);
        }

        /// <summary>
        /// Zeichnet die Bewertungsleiste (0..10) als Rechteck auf ratingCanvas.
        /// Breite = rating/10 * ratingCanvas.ActualWidth, Hoehe = ratingCanvas.ActualHeight.
        /// </summary>
        private void DrawRatingBar(int rating)
        {
            // Aufgabe 4: alte Inhalte entfernen
            ratingCanvas.Children.Clear();

            Rectangle balken = new Rectangle
            {
                // rating == 10 -> volle Breite, rating == 5 -> halbe Breite
                Width = rating / 10.0 * ratingCanvas.ActualWidth,
                Height = ratingCanvas.ActualHeight,
                Fill = Brushes.SeaGreen
            };

            Canvas.SetLeft(balken, 0);
            Canvas.SetTop(balken, 0);

            ratingCanvas.Children.Add(balken);
        }

        /// <summary>
        /// Neuen Film aus den Eingabefeldern in die DB einfuegen.
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Aufgabe 5: Eingaben lesen und pruefen
            string title = neuTitle.Text.Trim();
            string director = neuDirector.Text.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Bitte einen Titel eingeben.", "Ungueltige Eingabe",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(neuYear.Text.Trim(), out int year))
            {
                MessageBox.Show("Das Jahr muss eine ganze Zahl sein.", "Ungueltige Eingabe",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(neuRating.Text.Trim(), out int rating))
            {
                MessageBox.Show("Die Bewertung muss eine ganze Zahl sein.", "Ungueltige Eingabe",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (rating < 0 || rating > 10)
            {
                MessageBox.Show("Die Bewertung muss zwischen 0 und 10 liegen.", "Ungueltige Eingabe",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // DataModels.Film voll qualifiziert anlegen (Namenskollision mit ViewModel Film).
            DataModels.Film film = new DataModels.Film
            {
                Title = title,
                Director = director,
                Year = year,
                Rating = rating
            };

            var options = new DataOptions().UseSQLite("Data Source=movies.db");
            using (var db = new MoviesDB(options))
            {
                // Liefert die neue Id direkt zurueck.
                long neueId = db.InsertWithInt64Identity(film);
            }

            // Liste neu laden, damit der neue Film sofort erscheint.
            LoadFilms();

            // Eingabefelder leeren.
            neuTitle.Clear();
            neuDirector.Clear();
            neuYear.Clear();
            neuRating.Clear();
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
