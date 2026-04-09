using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace WeatherViewer
{
    /// <summary>
    /// Provides embedded Vienna weather CSV data and parsing methods.
    /// Demonstrates CSV parsing with LINQ.
    /// </summary>
    public static class CsvData
    {
        // ~60 records of Vienna weather data spanning January-February (winter mix)
        // Format: date;temperature;humidity;windspeed;condition
        public const string EmbeddedCsv =
@"Date;Temperature;Humidity;WindSpeed;Condition
2026-01-01;-2.3;78;12.5;Snowy
2026-01-02;-3.8;82;18.0;Snowy
2026-01-03;-1.5;75;10.2;Cloudy
2026-01-04;0.2;70;8.5;Cloudy
2026-01-05;1.8;65;6.3;Sunny
2026-01-06;-0.5;72;14.8;Snowy
2026-01-07;-4.1;85;22.0;Stormy
2026-01-08;-5.6;88;25.3;Stormy
2026-01-09;-3.2;80;15.7;Snowy
2026-01-10;-1.0;76;11.0;Cloudy
2026-01-11;0.5;68;7.5;Sunny
2026-01-12;2.1;63;5.8;Sunny
2026-01-13;3.4;60;9.2;Sunny
2026-01-14;1.2;67;12.0;Cloudy
2026-01-15;-0.8;74;16.5;Rainy
2026-01-16;-2.1;79;19.3;Snowy
2026-01-17;-3.5;83;21.7;Snowy
2026-01-18;-1.9;77;13.4;Cloudy
2026-01-19;0.8;71;10.1;Cloudy
2026-01-20;2.5;64;7.0;Sunny
2026-01-21;4.0;58;5.2;Sunny
2026-01-22;3.2;62;8.8;Sunny
2026-01-23;1.5;69;11.5;Cloudy
2026-01-24;0.0;73;14.2;Rainy
2026-01-25;-1.3;78;17.8;Rainy
2026-01-26;-2.7;81;20.5;Snowy
2026-01-27;-4.4;86;24.0;Stormy
2026-01-28;-2.0;79;16.3;Snowy
2026-01-29;0.3;72;12.8;Cloudy
2026-01-30;1.9;66;9.4;Sunny
2026-01-31;3.7;61;6.7;Sunny
2026-02-01;4.5;57;5.0;Sunny
2026-02-02;3.1;63;8.2;Cloudy
2026-02-03;1.6;68;11.9;Cloudy
2026-02-04;0.4;72;14.6;Rainy
2026-02-05;-0.9;76;17.3;Rainy
2026-02-06;-2.5;80;20.0;Snowy
2026-02-07;-1.2;75;13.8;Cloudy
2026-02-08;1.0;69;10.5;Cloudy
2026-02-09;3.3;62;7.3;Sunny
2026-02-10;5.8;55;4.5;Sunny
2026-02-11;7.2;50;6.0;Sunny
2026-02-12;5.5;56;8.7;Cloudy
2026-02-13;3.8;63;11.4;Cloudy
2026-02-14;2.0;70;14.1;Rainy
2026-02-15;4.6;60;9.8;Cloudy
2026-02-16;6.9;52;6.5;Sunny
2026-02-17;8.4;47;4.2;Sunny
2026-02-18;7.1;53;7.8;Sunny
2026-02-19;5.3;59;10.6;Cloudy
2026-02-20;3.0;66;13.3;Rainy
2026-02-21;1.4;71;16.0;Rainy
2026-02-22;4.2;61;9.1;Cloudy
2026-02-23;6.5;54;6.8;Sunny
2026-02-24;9.0;45;3.9;Sunny
2026-02-25;7.8;49;5.5;Sunny
2026-02-26;5.9;57;8.3;Cloudy
2026-02-27;3.6;64;12.2;Rainy
2026-02-28;6.1;55;7.6;Sunny";

        /// <summary>
        /// Parses the embedded CSV string into a list of WeatherRecord objects using LINQ.
        /// </summary>
        public static List<WeatherRecord> Parse()
        {
            return ParseCsvString(EmbeddedCsv);
        }

        /// <summary>
        /// Loads weather records from an external CSV file.
        /// </summary>
        public static List<WeatherRecord> LoadFromFile(string path)
        {
            string content = File.ReadAllText(path);
            return ParseCsvString(content);
        }

        /// <summary>
        /// Parses a CSV string (semicolon-separated) into WeatherRecord objects.
        /// Skips the header line and any lines that fail to parse.
        /// </summary>
        private static List<WeatherRecord> ParseCsvString(string csv)
        {
            string[] lines = csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            return lines
                .Skip(1) // skip header
                .Select(line => line.Split(';'))
                .Where(parts => parts.Length == 5)
                .Select(parts =>
                {
                    bool ok = DateTime.TryParseExact(parts[0].Trim(), "yyyy-MM-dd",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date);
                    ok &= double.TryParse(parts[1].Trim(), NumberStyles.Any,
                        CultureInfo.InvariantCulture, out double temp);
                    ok &= int.TryParse(parts[2].Trim(), out int humidity);
                    ok &= double.TryParse(parts[3].Trim(), NumberStyles.Any,
                        CultureInfo.InvariantCulture, out double wind);
                    string condition = parts[4].Trim();

                    if (!ok) return null;

                    return new WeatherRecord
                    {
                        Date = date,
                        Temperature = temp,
                        Humidity = humidity,
                        WindSpeed = wind,
                        Condition = condition
                    };
                })
                .Where(r => r != null)
                .Cast<WeatherRecord>()
                .OrderBy(r => r.Date)
                .ToList();
        }
    }
}
