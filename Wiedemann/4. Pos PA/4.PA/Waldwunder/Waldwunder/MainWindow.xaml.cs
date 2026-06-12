using DataModels;
using LinqToDB;
using LinqToDB.Common;
using LinqToDB.Mapping;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Waldwunder.Models;

namespace Waldwunder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<DataModels.Waldwunder> foundWonders = new();
        public MainWindow()
        {
            InitializeComponent();
            SizeChanged += MainWindow_SizeChanged;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AddMarker(48.2082m, 16.3738m);
        }

        private void NewWonder_Click(object sender, RoutedEventArgs e)
        {
            NewWonderWindow window = new NewWonderWindow();
            window.ShowDialog();
        }

        private void search(object sender, RoutedEventArgs e)
        {
            var options = new DataOptions().UseSQLite("Data Source=Waldwunder.db");
            using var db = new WaldwunderDB(options);
            var searchString = searchTextBox.Text;
            string searchType = ((ComboBoxItem)searchComboBox.SelectedItem).Content.ToString();

            switch (searchType)
            {
                case "Stichwort":
                    foundWonders = db.Waldwunders.Where(w => w.Name.Contains(searchString) || w.Description.Contains(searchString)).ToList();
                    break;
                case "Art":
                    foundWonders = db.Waldwunders.Where(w => w.Type == searchString).ToList();
                    break;
                case "Ort":
                    decimal latitude =
            Convert.ToDecimal(LatitudeTextBox.Text);

                    decimal longitude =
                        Convert.ToDecimal(LongitudeTextBox.Text);
                    foundWonders = db.Waldwunders.Where(w => w.Latitude.Value >= latitude - 0.5m && w.Latitude.Value <= latitude + 0.5m && w.Longitude.Value >= longitude - 0.5m && w.Longitude.Value <= longitude + 0.5m).ToList();
                    break;
            }

            showListbox.Items.Clear();
            
            foreach(var w in foundWonders)
            {
                showListbox.Items.Add(w);
            }
            RedrawMarkers();


        }
        private void searchTypeChanged(object sender, SelectionChangedEventArgs e)
        {

            if (searchTextBox == null)
            {
                return;
            }

            ComboBoxItem selected =
                (ComboBoxItem)searchComboBox.SelectedItem;

            string searchType =
                selected.Content.ToString();

            if (searchType == "Ort")
            {
                searchTextBox.Visibility = Visibility.Collapsed;

                LatitudeLabel.Visibility = Visibility.Visible;
                LatitudeTextBox.Visibility = Visibility.Visible;

                LongitudeLabel.Visibility = Visibility.Visible;
                LongitudeTextBox.Visibility = Visibility.Visible;
            }
            else
            {
                searchTextBox.Visibility = Visibility.Visible;

                LatitudeLabel.Visibility = Visibility.Collapsed;
                LatitudeTextBox.Visibility = Visibility.Collapsed;

                LongitudeLabel.Visibility = Visibility.Collapsed;
                LongitudeTextBox.Visibility = Visibility.Collapsed;
            }
        }

        private void anzeigen(object sender, RoutedEventArgs e)
        {
            if (showListbox.SelectedItem == null)
            {
                MessageBox.Show("Bitte ein Waldwunder auswählen.");
                return;
            }

            DataModels.Waldwunder selectedWonder = (DataModels.Waldwunder)showListbox.SelectedItem;
            WonderDetailsWindow window = new WonderDetailsWindow(selectedWonder);
            window.ShowDialog();
        }

        private void AddMarker(decimal latitude, decimal longitude)
        {
            double topLatitude = 49.063175;
            double bottomLatitude = 46.308597;

            double leftLongitude = 9.362383;
            double rightLongitude = 17.231941;

            double mapWidth = AustriaMap.ActualWidth;
            double mapHeight = AustriaMap.ActualHeight;

            double x =
                ((double)longitude - leftLongitude)
                / (rightLongitude - leftLongitude)
                * mapWidth;

            double y =
                (topLatitude - (double)latitude)
                / (topLatitude - bottomLatitude)
                * mapHeight;

            Ellipse marker = new Ellipse();

            marker.Width = 10;
            marker.Height = 10;
            marker.Fill = Brushes.Red;

            Canvas.SetLeft(marker, x);
            Canvas.SetTop(marker, y);

            MapCanvas.Children.Add(marker);
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawMarkers();
        }

        private void RedrawMarkers()
        {
            MapCanvas.Children.Clear();

            foreach (var wonder in foundWonders)
            {
                if (wonder.Latitude == null ||
                    wonder.Longitude == null)
                {
                    continue;
                }

                AddMarker(
                    wonder.Latitude.Value,
                    wonder.Longitude.Value);
            }
        }
    }
}