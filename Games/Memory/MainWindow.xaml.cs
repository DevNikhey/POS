using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Memory
{
    public partial class MainWindow : Window
    {
        private readonly MemoryModel _model;

        public MainWindow()
        {
            InitializeComponent();
            _model = new MemoryModel();
            DataContext = _model;
        }

        private async void Card_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Card card)
            {
                await _model.FlipCard(card);
            }
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            _model.NewGame();
        }
    }
}
