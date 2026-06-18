using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PA1_4B_2025
{
    internal class Plane : INotifyPropertyChanged
    {
        private int _Id;
        public int Id
        {
            set
            {
                _Id = value;
                NotifyPropertyChanged(x => x.Id);
            }
            get
            {
                return _Id;
            }
        }
        private bool _IsVip;
        public bool IsVip
        {
            set
            {
                _IsVip = value;
                NotifyPropertyChanged(x => x.IsVip);
            }
            get
            {
                return _IsVip;
            }
        }
        private int _LandingStrip = -1;
        public int LandingStrip
        {
            set
            {
                _LandingStrip = value;
                NotifyPropertyChanged(x => x.LandingStrip);
            }
            get
            {
                return _LandingStrip;
            }
        }
        private String _Status = "Flying";
        public String Status
        {
            set
            {
                _Status = value;
                NotifyPropertyChanged(x => x.Status);
            }
            get
            {
                return _Status;
            }
        }

        public Airport TargetAirport { get; set; }

        public void Fly()
        {
            // Random Zeit bis zur Ankunft an dem Flughafen (1000 - 10000ms)
            Application.Current.Dispatcher.Invoke(new Action(() => {
                MainWindow.flyingBox.Items.Add(this);
            }));
            Thread.Sleep(new Random().Next(1000, 10000));
            TargetAirport.Land(this);
        }

        #region INotifyPropertyChanged Member

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged<TValue>
                     (System.Linq.Expressions.Expression<Func<Plane, TValue>> propertySelector)
        {
            if (PropertyChanged != null)
            {
                var memberExpression = propertySelector.Body as System.Linq.Expressions.MemberExpression;
                if (memberExpression != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(memberExpression.Member.Name));
                }
            }
        }
        #endregion
    }
}
