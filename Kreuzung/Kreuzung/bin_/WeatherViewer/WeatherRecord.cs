using System;
using System.Windows.Media;

namespace WeatherViewer
{
    /// <summary>
    /// Represents a single weather measurement record.
    /// </summary>
    public class WeatherRecord
    {
        public DateTime Date { get; set; }
        public double Temperature { get; set; }   // Celsius
        public int Humidity { get; set; }          // %
        public double WindSpeed { get; set; }      // km/h
        public string Condition { get; set; } = string.Empty; // Sunny, Cloudy, Rainy, Snowy, Stormy

        /// <summary>
        /// Returns a color based on the temperature:
        /// Blue if below 0, Orange if above 30, Black otherwise.
        /// </summary>
        public SolidColorBrush TempColor
        {
            get
            {
                if (Temperature < 0)
                    return new SolidColorBrush(Colors.Blue);
                if (Temperature > 30)
                    return new SolidColorBrush(Colors.OrangeRed);
                return new SolidColorBrush(Colors.Black);
            }
        }
    }
}
