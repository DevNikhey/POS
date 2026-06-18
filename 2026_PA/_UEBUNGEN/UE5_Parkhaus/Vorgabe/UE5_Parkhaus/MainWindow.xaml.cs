using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace UE5_Parkhaus
{
    /// <summary>
    /// Start-Shell. VORGEGEBEN - hier ist nichts zu implementieren.
    /// Stellt die ListBox-Referenzen statisch bereit und startet pro Auto
    /// einen eigenen Thread. (Wie im Referenz-Projekt PA1_4B_2025.)
    /// </summary>
    public partial class MainWindow : Window
    {
        public static ListBox fahrendBox = null!;
        public static ListBox parkBox = null!;
        public static ListBox kassaBox = null!;
        public static Label statusLabel = null!;

        public MainWindow()
        {
            InitializeComponent();
            fahrendBox = fahrend;
            parkBox = park;
            kassaBox = kassa;
            statusLabel = this.statusLabel; // gleichnamiges Label aus XAML
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            Parkhaus? ziel = (comboVariante.Text) switch
            {
                "EinfachParkhaus" => new EinfachParkhaus(),
                "TagNachtParkhaus" => new TagNachtParkhaus(),
                "LadeParkhaus" => new LadeParkhaus(),
                "KassaParkhaus" => new KassaParkhaus(),
                _ => null
            };

            if (ziel == null)
            {
                Debug.WriteLine("Bitte eine Variante waehlen.");
                return;
            }

            if (!int.TryParse(anzahlBox.Text, out int anzahl) || anzahl < 1)
            {
                Debug.WriteLine("Ungueltige Anzahl.");
                return;
            }

            var rnd = new Random();
            for (int i = 0; i < anzahl; i++)
            {
                var auto = new Auto
                {
                    Id = i,
                    ZielParkhaus = ziel,
                    IstElektro = rnd.Next(0, 100) < 30 // ca. 30% Elektroautos
                };
                new Thread(() => auto.Fahren()) { IsBackground = true }.Start();
            }
        }
    }
}
