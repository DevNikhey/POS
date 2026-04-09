using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Collections.ObjectModel;
using System.Threading;
using System.ComponentModel;

namespace Sorting
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        ObservableCollection<Int32> sortList = new ObservableCollection<Int32>();
        int _checks = 0;
        int _swaps = 0;
        int _selected = -1;

        public ObservableCollection<Int32> List
        {
            set
            {
                sortList = value;
                NotifyPropertyChanged(x => x.List);
            }
            get
            {
                return sortList;
            }
        }
        public int Checks {
            set
            {
                _checks = value;
                NotifyPropertyChanged(x => x.Checks);
            }
            get
            {
                return _checks;
            }
        }
        public int Swaps
        {
            set
            {
                _swaps = value;
                NotifyPropertyChanged(x => x.Swaps);
            }
            get
            {
                return _swaps;
            }
        }

        public int Selected
        {
            set
            {
                _selected = value;
                NotifyPropertyChanged(x => x.Selected);
            }
            get
            {
                return _selected;
            }
        }
        Random rand = new Random();

        public MainWindow()
        {
            InitializeComponent();
            for (int i = 0; i < 50; i++)
            {
                sortList.Add(rand.Next(200));
            }
            Checks = 0;
            Swaps = 0;
            this.DataContext = this;
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            int size = sortList.Count;
            Checks = 0;
            Swaps = 0;
            ThreadPool.QueueUserWorkItem(o =>
            {
                bool swapped = false;
                do{
                    swapped = false;
                    for (int i = 0; i < size - 1; ++i, Selected = i)
                    {
                        try
                        {
                            this.Dispatcher.Invoke(
                              System.Windows.Threading.DispatcherPriority.Normal
                              , new System.Windows.Threading.DispatcherOperationCallback(delegate
                              {
                                  Checks++;
                                  if (sortList[i] > sortList[i + 1])
                                  {
                                      Swaps++;
                                      int temp = sortList[i];
                                      sortList[i] = sortList[i + 1];
                                      sortList[i + 1] = temp;
                                      swapped = true;
                                  }
                                  return null;
                              }), null);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.ToString());
                        }
                        Thread.Sleep(50);
                    }
                    size = size-1;
              } while (swapped == true);

            });
        }

        private void reverse_Click(object sender, RoutedEventArgs e)
        {
            int size = sortList.Count;
            Checks = 0;
            Swaps = 0;
            ThreadPool.QueueUserWorkItem(o =>
            {
                bool swapped = false;
                do
                {
                    swapped = false;
                    for (int i = 0; i < size - 1; ++i, Selected = i)
                    {
                        try
                        {
                            this.Dispatcher.Invoke(
                              System.Windows.Threading.DispatcherPriority.Normal
                              , new System.Windows.Threading.DispatcherOperationCallback(delegate
                              {
                                  Checks++;
                                  if (sortList[i] < sortList[i + 1])
                                  {
                                      Swaps++;
                                      int temp = sortList[i];
                                      sortList[i] = sortList[i + 1];
                                      sortList[i + 1] = temp;
                                      swapped = true;
                                  }
                                  return null;
                              }), null);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.ToString());
                        }
                        Thread.Sleep(50);
                    }
                    size = size - 1;
                } while (swapped == true);

            });
        }

        private void startC_Click(object sender, RoutedEventArgs e)
        {
            bool swapped = false;
            int size = sortList.Count;
            Checks = 0;
            Swaps = 0;
            int start = 0;
            int end = size - 1;

            ThreadPool.QueueUserWorkItem(o =>
            {
                do
                {
                    swapped = false;
                    for (int i = start; i < end - 1; ++i)
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            Checks++;
                            Selected = i;
                            if (sortList[i] > sortList[i + 1])
                            {
                                Swaps++;
                                int temp = sortList[i];
                                sortList[i] = sortList[i + 1];
                                sortList[i + 1] = temp;
                                swapped = true;
                            }
                        }));
                        Thread.Sleep(50);
                    }

                    if (!swapped)
                        break;

                    swapped = false;
                    --end;

                    for (int i = end - 1; i >= start; i--)
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            Checks++;
                            Selected = i;
                            if (sortList[i] > sortList[i + 1])
                            {
                                Swaps++;
                                int temp = sortList[i];
                                sortList[i] = sortList[i + 1];
                                sortList[i + 1] = temp;
                                swapped = true;
                            }
                        }));
                        Thread.Sleep(50);
                    }
                    ++start;
                } while (swapped);
            });

        }

        #region INotifyPropertyChanged Member

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged<TValue>
                     (System.Linq.Expressions.Expression<Func<MainWindow, TValue>> propertySelector)
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

        private void startS_Click(object sender, RoutedEventArgs e)
        {
            int n = sortList.Count;
            Checks = 0;
            Swaps = 0;

            ThreadPool.QueueUserWorkItem(o =>
            {
                for (int i = 0; i < n - 1; i++)
                {
                    int min_idx = i;
                    Selected = i;

                    this.Dispatcher.Invoke(new Action(() => {
                        for (int j = i + 1; j < n; j++)
                        {
                            if (sortList[j] < sortList[min_idx])
                            {
                                min_idx = j;
                                Selected = j;
                            }
                        }

                        int temp = sortList[i];
                        sortList[i] = sortList[min_idx];
                        sortList[min_idx] = temp;
                    }));

                    Thread.Sleep(50);
                    Checks++;
                }
            });
        }

        private void startI_Click(object sender, RoutedEventArgs e)
        {
            int i = 1;
            int size = sortList.Count;
            ThreadPool.QueueUserWorkItem(o =>
            {
                while(i < size)
                {
                    int x = sortList[i];
                    int j = i;
                    while(j > 0 && sortList[j - 1] > x)
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            sortList[j] = sortList[j - 1];
                            Swaps++;
                            Selected = j - 1;
                        }));
                        j--;
                        Checks++;
                        Thread.Sleep(50);
                    }
                }
            });
        }
    }
}
