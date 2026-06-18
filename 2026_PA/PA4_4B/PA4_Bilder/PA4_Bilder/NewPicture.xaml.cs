using DataModels;
using ExifPhotoReader;
using LinqToDB;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
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

namespace PA4_Bilder
{
    /// <summary>
    /// Interaction logic for NewPicture.xaml
    /// </summary>
    public partial class NewPicture : Window
    {

        private string selectedFile = "";

        public NewPicture()
        {
            InitializeComponent();
        }

        private void addImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Bilddateien (*.jpg)|*.jpg";
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string file in openFileDialog.FileNames)
                {
                    ImagesListBox.Items.Add(System.IO.Path.GetFileName(file));
                    selectedFile = file;
                }
            }

        }

        private void register(object sender, RoutedEventArgs e)
        {

            //Für bild in ordner speicher
            string imagesFolder = "images";

            if (!Directory.Exists(imagesFolder))
            {
                Directory.CreateDirectory(imagesFolder);
            }
            //--

            DataModels.Photo photo = new DataModels.Photo();


            { //foto laden
                ExifImageProperties exifPhoto = ExifPhoto.GetExifDataPhoto(selectedFile);


                photo.Name = selectedFile;
                photo.Lat = exifPhoto.GPSInfo.Latitude;
                photo.Lng = exifPhoto.GPSInfo.Longitude;
            }

            var options = new DataOptions().UseSQLite("Data Source=photoworld.db");
            using var db = new PhotoworldDB(options);
            var wonderId = db.InsertWithInt64Identity(photo);

            { // neues Bild in den images ordner laden (nach dem insert, damit wir die id direkt haben)
                string fileName = System.IO.Path.GetFileName(selectedFile);

                string destinationPath =
                    System.IO.Path.Combine(imagesFolder, fileName);


                while (File.Exists(destinationPath))
                {
                    string imageName =
                        System.IO.Path.GetFileNameWithoutExtension(fileName);

                    string extension =
                        System.IO.Path.GetExtension(fileName);

                    destinationPath =
                        System.IO.Path.Combine(
                            imagesFolder,
                            $"{imageName}_{wonderId}{extension}");
                }

                File.Copy(selectedFile, destinationPath);

            }

            Debug.WriteLine("Saved new image with the name: " + photo.Name + " and id: " + wonderId);
        }
    }
}
