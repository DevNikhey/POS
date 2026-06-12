using DataModels;
using LinqToDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Waldwunder
{
    /// <summary>
    /// Interaktionslogik für WonderDetailsWindow.xaml
    /// </summary>
    public partial class WonderDetailsWindow : Window
    {

        public WonderDetailsWindow(DataModels.Waldwunder selectedWunder)
        {
            InitializeComponent();

            NameTextBlock.Text = selectedWunder.Name;
            BeschreibungTextBlock.Text = selectedWunder.Description;
            BundeslandTextBlock.Text = selectedWunder.Province;
            ArtTextBlock.Text = selectedWunder.Type;
            LatitudeTextBlock.Text = selectedWunder.Latitude.ToString();
            LongitudeTextBlock.Text = selectedWunder.Longitude.ToString();

            LoadImages(selectedWunder);
        }

        public void LoadImages(DataModels.Waldwunder selectedWunder)
        {
            var options = new DataOptions()
                .UseSQLite("Data Source=Waldwunder.db");

            using var db = new WaldwunderDB(options);

            var images = db.Bilders
                .Where(b => b.Wonder == selectedWunder.Id)
                .ToList();

            ImagesPanel.Children.Clear();

            foreach (var image in images)
            {
                string path = System.IO.Path.GetFullPath(
                    System.IO.Path.Combine(
                        "Images",
                        image.Name));

                if (!File.Exists(path))
                {
                    MessageBox.Show($"Bild nicht gefunden:\n{path}");
                    continue;
                }

                Image img = new Image();

                img.Width = 200;
                img.Height = 150;
                img.Margin = new Thickness(10);
                img.Stretch = Stretch.Uniform;

                img.Source = new BitmapImage(
                    new Uri(path, UriKind.Absolute));

                ImagesPanel.Children.Add(img);
            }
        }
    }
}
