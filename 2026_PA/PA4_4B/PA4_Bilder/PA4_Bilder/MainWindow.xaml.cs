using DataModels;
using ExifPhotoReader;
using LinqToDB;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using PA4_Bilder;
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PA4_4B
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Anzeige-Liste als ObservableCollection -> neu hinzugefuegte Bilder erscheinen sofort.
        private readonly ObservableCollection<Bild> _bilder = new();

        // Marker-Daten der DB-Bilder; werden erst im Loaded gezeichnet (A4).
        private readonly List<(double lat, double lng, Bild bild)> _markers = new();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            var options = new DataOptions().UseSQLite("Data Source=photoworld.db");
            using var db = new PhotoworldDB(options);

            foreach (var item in db.Photos.ToList())
            {
                Bild person = new Bild();
                person.id = (long)item.Id;                 // A6: Id setzen (sonst laedt jedes Detail images/0.jpg)
                person.longitude = item.Lng.ToString();
                person.latitude = item.Lat.ToString();
                person.name = item.Name;

                _bilder.Add(person);
                _markers.Add(((double)item.Lat, (double)item.Lng, person));
            }

            showListbox.ItemsSource = _bilder;

            // A4: ActualWidth/ActualHeight sind im Konstruktor noch 0 -> Marker erst im Loaded zeichnen.
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var m in _markers)
            {
                AddMarker(m.lat, m.lng, m.bild);
            }
        }

        // A2: direkt per FileDialog auswaehlen (KEIN zusaetzlicher Dialog), jedes Bild
        //     sofort in DB + Ordner uebernehmen und in Liste/Karte anzeigen.
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Bilddateien (*.jpg)|*.jpg"
            };
            if (dialog.ShowDialog() == true)
            {
                foreach (string file in dialog.FileNames)
                {
                    RegisterImage(file);
                }
            }
        }

        // Speichert ein gewaehltes Bild: in die DB einfuegen, Datei mit der DB-Id
        // benennen (images/<id>.jpg) und in Liste + Karte aufnehmen.
        private void RegisterImage(string file)
        {
            string imagesFolder = "images";
            if (!Directory.Exists(imagesFolder))
                Directory.CreateDirectory(imagesFolder);

            ExifImageProperties exif = ExifPhoto.GetExifDataPhoto(file);

            DataModels.Photo photo = new DataModels.Photo
            {
                Name = System.IO.Path.GetFileName(file),
                Lat = exif.GPSInfo.Latitude,
                Lng = exif.GPSInfo.Longitude
            };

            var options = new DataOptions().UseSQLite("Data Source=photoworld.db");
            using var db = new PhotoworldDB(options);
            long id = db.InsertWithInt64Identity(photo);   // A2/A6: Id holen

            // Datei mit der Datenbank-Id benennen, damit changeSelected sie findet.
            File.Copy(file, System.IO.Path.Combine(imagesFolder, id + ".jpg"), true);

            Bild bild = new Bild
            {
                id = id,
                name = photo.Name,
                longitude = photo.Lng.ToString(),
                latitude = photo.Lat.ToString()
            };
            _bilder.Add(bild);
            AddMarker((double)photo.Lat, (double)photo.Lng, bild);
        }

        //TODO: man hätte auch lat und long aus bild nehmen können, hatte aber die zeit dafür nicht mehr
        private void AddMarker(double latitude, double longitude, Bild bild)
        {
            double topLatitude = 90;
            double bottomLatitude = -90;

            double leftLongitude = -180;
            double rightLongitude = 180;

            double mapWidth = Map.ActualWidth;
            double mapHeight = Map.ActualHeight;

            double x =
                ((double)longitude - leftLongitude)
                / (rightLongitude - leftLongitude)
                * mapWidth;

            double y =
                (topLatitude - (double)latitude)
                / (topLatitude - bottomLatitude)
                * mapHeight;

            Ellipse marker = new Ellipse();
            marker.MouseDown += (s, e) =>
            {
                //TODO: load click event here
                this.changeSelected(bild);
            };

            marker.Width = 10;
            marker.Height = 10;
            marker.Fill = Brushes.Red;

            Canvas.SetLeft(marker, x);
            Canvas.SetTop(marker, y);

            MapCanvas.Children.Add(marker);
        }

        private void changeSelected(Bild bild)
        {
            template.Source = new BitmapImage(
                    new Uri("images/" + bild.id + ".jpg", UriKind.Relative));
        }

        private void showListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Bild bild = (Bild)showListbox.SelectedItem;
            changeSelected(bild);
        }
    }

    class Bild
    {
        public long id { get; set; }
        public string name { get; set; }
        public string longitude { get; set; }
        public string latitude { get; set; }
    }
}