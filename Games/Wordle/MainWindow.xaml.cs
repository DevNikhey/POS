using System;
using System.Windows;
using System.Windows.Input;

namespace Wordle
{
    public partial class MainWindow : Window
    {
        private WordleModel _model;

        public MainWindow()
        {
            InitializeComponent();
            _model = new WordleModel();
            DataContext = _model;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _model.SubmitGuess();
            }
            else if (e.Key == Key.Back)
            {
                _model.Backspace();
            }
            else if (e.Key >= Key.A && e.Key <= Key.Z)
            {
                char letter = (char)('A' + (e.Key - Key.A));
                _model.TypeLetter(letter);
            }
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            _model.NewGame();
            this.Focus();
        }
    }
}
