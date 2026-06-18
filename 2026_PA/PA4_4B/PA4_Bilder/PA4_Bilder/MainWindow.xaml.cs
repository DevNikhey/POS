using DataModels;
using ExifPhotoReader;
using LinqToDB;
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
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            ExifImageProperties exifImage = ExifPhoto.GetExifDataPhoto("images/1.jpg");
            Debug.WriteLine(exifImage.GPSInfo.Longitude + " / " + exifImage.GPSInfo.Latitude);

            var options = new DataOptions().UseSQLite("Data Source=photoworld.db");
            using var db = new PhotoworldDB(options);

            List<Bild> list = new();

            foreach (var item in db.Photos.ToList())
            {
                Bild person = new Bild();
                person.longitude = item.Lng.ToString();
                person.latitude = item.Lat.ToString();
                person.name = item.Name;

                list.Add(person);
                AddMarker((double) item.Lat, (double) item.Lng, person);
            }

            showListbox.ItemsSource = list;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NewPicture newPicture = new NewPicture();
            newPicture.ShowDialog();
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