using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Hangman
{
    public partial class MainWindow : Window
    {
        private HangmanModel _model;

        public MainWindow()
        {
            InitializeComponent();
            _model = new HangmanModel();
            DataContext = _model;
            GenerateLetterButtons();
        }

        private void GenerateLetterButtons()
        {
            LetterPanel.Children.Clear();

            for (char c = 'A'; c <= 'Z'; c++)
            {
                Button btn = new Button
                {
                    Content = c.ToString(),
                    Tag = c,
                    Width = 42,
                    Height = 42,
                    Margin = new Thickness(3),
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB")),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                btn.Click += Letter_Click;
                LetterPanel.Children.Add(btn);
            }
        }

        private void Letter_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is char letter)
            {
                _model.Guess(letter);
                btn.IsEnabled = false;
                btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D"));
            }
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            _model.NewGame();
            GenerateLetterButtons();
        }
    }
}
