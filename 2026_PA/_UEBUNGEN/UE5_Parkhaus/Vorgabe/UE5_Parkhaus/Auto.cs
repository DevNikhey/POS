using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace UE5_Parkhaus
{
    /// <summary>
    /// Datenmodell fuer ein Auto. Laeuft pro Instanz in einem eigenen Thread.
    /// VOLLSTAENDIG VORGEGEBEN - hier ist nichts zu tun.
    /// </summary>
    public class Auto : INotifyPropertyChanged
    {
        private int _Id;
        public int Id
        {
            get => _Id;
            set { _Id = value; NotifyPropertyChanged(nameof(Id)); }
        }

        private bool _IstElektro;
        public bool IstElektro
        {
            get => _IstElektro;
            set { _IstElektro = value; NotifyPropertyChanged(nameof(IstElektro)); }
        }

        private int _Platz = -1;
        public int Platz
        {
            get => _Platz;
            set { _Platz = value; NotifyPropertyChanged(nameof(Platz)); }
        }

        private string _Status = "Anfahrt";
        public string Status
        {
            get => _Status;
            set { _Status = value; NotifyPropertyChanged(nameof(Status)); }
        }

        public Parkhaus ZielParkhaus { get; set; } = null!;

        /// <summary>
        /// Simuliert die Anfahrt (zufaellige Zeit) und faehrt dann ins Parkhaus.
        /// </summary>
        public void Fahren()
        {
            Status = "Anfahrt";
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.fahrendBox.Items.Add(this);
            });

            Thread.Sleep(new Random().Next(500, 4000));

            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.fahrendBox.Items.Remove(this);
            });

            ZielParkhaus.Einfahren(this);
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion
    }
}
