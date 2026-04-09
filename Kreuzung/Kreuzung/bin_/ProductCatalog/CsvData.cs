using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ProductCatalog
{
    /// <summary>
    /// Statische Klasse mit eingebetteten CSV-Produktdaten und Parse-Methode.
    /// Format: id;name;category;price;stock;description
    /// </summary>
    public static class CsvData
    {
        public const string EmbeddedCsv =
@"id;name;category;price;stock;description
1;Laptop Pro 15;Elektronik;1299.99;12;Leistungsstarker Laptop mit 15 Zoll Display und 16GB RAM
2;Smartphone X12;Elektronik;899.50;25;Flaggschiff-Smartphone mit 128GB Speicher und OLED Display
3;Bluetooth Kopfhoerer;Elektronik;79.99;50;Kabellose Kopfhoerer mit Noise-Cancelling und 30h Akku
4;USB-C Hub 7in1;Elektronik;45.90;80;Multiport-Adapter mit HDMI, USB-A, SD-Kartenleser
5;Mechanische Tastatur;Elektronik;129.00;30;RGB Gaming-Tastatur mit Cherry MX Switches
6;Winterjacke Premium;Kleidung;189.95;15;Wasserdichte Winterjacke mit Dauenfuellung
7;Laufschuhe Aero;Kleidung;134.90;40;Leichte Laufschuhe mit Daempfungstechnologie
8;Jeans Classic Fit;Kleidung;69.99;60;Klassische Jeans aus Bio-Baumwolle
9;Wollpullover Nordic;Kleidung;89.50;20;Handgestrickter Pullover aus Merinowolle
10;Sport-T-Shirt Dry;Kleidung;29.99;100;Atmungsaktives Funktionsshirt fuer Sport
11;Bio-Olivenoel Extra;Lebensmittel;12.90;200;Kaltgepresstes Olivenoel aus Griechenland 500ml
12;Espresso Bohnen;Lebensmittel;18.50;150;Premium Arabica Bohnen aus Kolumbien 1kg
13;Protein Riegel Mix;Lebensmittel;24.99;75;Box mit 12 Proteinriegeln verschiedene Sorten
14;Bio-Honig Natur;Lebensmittel;9.80;90;Naturbelassener Blueten-Honig vom Imker 500g
15;Vollkorn Pasta;Lebensmittel;3.49;300;Italienische Vollkorn-Spaghetti aus Hartweizen 500g
16;Clean Code;Buecher;34.99;45;Handbuch fuer agile Software-Entwicklung
17;C# in der Praxis;Buecher;44.90;30;Umfassendes Lehrbuch fuer C# und .NET Entwicklung
18;Design Patterns;Buecher;39.95;25;Entwurfsmuster fuer wiederverwendbare Software
19;Kochbuch Mediterran;Buecher;28.50;35;200 Rezepte aus der mediterranen Kueche
20;Fitness Guide 2024;Buecher;22.99;55;Trainingsplaene und Ernaehrungstipps
21;Yoga Matte Premium;Sport;49.90;40;Rutschfeste Yogamatte 6mm mit Tragegurt
22;Hanteln Set 2-10kg;Sport;89.99;20;Kurzhantel-Set mit Staender 5 Paar
23;Widerstandsbaender;Sport;19.90;120;5er Set Fitnessbaender verschiedene Staerken
24;Trinkflasche 1L;Sport;16.50;85;BPA-freie Edelstahl-Trinkflasche isoliert
25;Springseil Speed;Sport;14.99;60;Profi-Springseil mit Kugellager und Stahlseil
26;Staubsauger Roboter;Haushalt;349.00;10;Saugroboter mit Laser-Navigation und App-Steuerung
27;Kaffeemaschine Deluxe;Haushalt;199.90;18;Vollautomatische Kaffeemaschine mit Milchschaeumer
28;Mixer Smoothie Pro;Haushalt;69.99;35;Hochleistungsmixer 1200W mit 1.5L Glasbehaelter
29;Buegeleisen Dampf;Haushalt;54.90;25;Dampfbuegeleisen 2800W mit Antikalk-System
30;LED Schreibtischlampe;Haushalt;39.95;50;Dimmbare LED-Lampe mit USB-Ladeanschluss";

        /// <summary>
        /// Parst den eingebetteten CSV-String und gibt eine Liste von Produkten zurueck.
        /// Verwendet LINQ und string.Split fuer die Verarbeitung.
        /// </summary>
        public static List<Product> Parse()
        {
            // CSV zeilenweise aufteilen, Header ueberspringen, leere Zeilen ignorieren
            List<Product> products = EmbeddedCsv
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Skip(1) // Header-Zeile ueberspringen
                .Select(line => line.Split(';'))
                .Where(parts => parts.Length >= 6)
                .Select(parts => new Product
                {
                    Id = int.Parse(parts[0].Trim()),
                    Name = parts[1].Trim(),
                    Category = parts[2].Trim(),
                    Price = decimal.Parse(parts[3].Trim(), CultureInfo.InvariantCulture),
                    Stock = int.Parse(parts[4].Trim()),
                    Description = parts[5].Trim()
                })
                .ToList();

            return products;
        }
    }
}
