using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using Waldwunder.Models;
using System.IO;
using DataModels;
using LinqToDB;

namespace Waldwunder
{
    /// <summary>
    /// Interaktionslogik für NewWonderWindow.xaml
    /// </summary>
    public partial class NewWonderWindow : Window
    {
        private List<string> selectedFiles = new();
        public NewWonderWindow()
        {
            InitializeComponent();
            ProvinceComboBox.ItemsSource = Enum.GetValues(typeof(Province));
        }

        private void addImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Bilddateien (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";
            if(openFileDialog.ShowDialog() == true)
            {
                foreach(string file in openFileDialog.FileNames)
                {
                    ImagesListBox.Items.Add(System.IO.Path.GetFileName(file));
                    selectedFiles.Add(file);
                }
            }
            
        }

        private void deleteImage(object sender, RoutedEventArgs e)
        {
            ImagesListBox.Items.Remove(ImagesListBox.SelectedItem);
        }

        private void register(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            string beschreibung = BeschreibungTextBox.Text;
            string type = TypeTextBox.Text;
            string latitudeS = LatitudeTextBox.Text;
            string longitudeS = LongitudeTextBox.Text;
            decimal latitude = Convert.ToDecimal(latitudeS);
            decimal longitude = Convert.ToDecimal(longitudeS);
            
            //Für bild in ordner speicher
            string imagesFolder = "Images";

            if (!Directory.Exists(imagesFolder))
            {
                Directory.CreateDirectory(imagesFolder);
            }
            //--

            DataModels.Waldwunder wonder = new DataModels.Waldwunder();
            wonder.Name = name;
            wonder.Description = beschreibung;
            if (ProvinceComboBox.SelectedItem == null)
            {
                MessageBox.Show("Bitte ein Bundesland auswählen.");
                return;
            }
            wonder.Province = ProvinceComboBox.SelectedItem.ToString();
            wonder.Latitude = latitude;
            wonder.Longitude = longitude;
            wonder.Type = type;
            wonder.Votes = 0;

            var options = new DataOptions().UseSQLite("Data Source=Waldwunder.db");
            //MessageBox.Show(System.IO.Path.GetFullPath("Waldwunder.db"));
            using var db = new WaldwunderDB(options);
            var wonderId = db.InsertWithInt64Identity(wonder);
            //MessageBox.Show("Gespeichert!");

            foreach (string file in selectedFiles)
            {
                DataModels.Bilder picture = new DataModels.Bilder();
                picture.Name = System.IO.Path.GetFileName(file);
                picture.Wonder = wonderId;
                //für speichern in images
                string fileName = System.IO.Path.GetFileName(file);

                string destinationPath =
                    System.IO.Path.Combine(imagesFolder, fileName);

                int counter = 1;

                while (File.Exists(destinationPath))
                {
                    string imageName =
                        System.IO.Path.GetFileNameWithoutExtension(fileName);

                    string extension =
                        System.IO.Path.GetExtension(fileName);

                    destinationPath =
                        System.IO.Path.Combine(
                            imagesFolder,
                            $"{imageName}_{counter}{extension}");

                    counter++;
                }

                File.Copy(file, destinationPath);

                DataModels.Bilder imagePicture =
                    new DataModels.Bilder();

                imagePicture.Name =
                    System.IO.Path.GetFileName(destinationPath);

                imagePicture.Wonder =
                    (decimal)wonderId;

                db.Insert(imagePicture);
            }
        }
    }
}
