using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimonSays
{
    public partial class MainWindow : Window
    {
        private readonly SimonModel _model = new SimonModel();
        private Button[] _buttons = null!;
        private bool _inputEnabled;

        public MainWindow()
        {
            InitializeComponent();
            _buttons = new Button[] { btn0, btn1, btn2, btn3 };
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            _model.Reset();
            SetButtonsEnabled(false);
            await NextRound();
        }

        private async Task NextRound()
        {
            _inputEnabled = false;
            SetButtonsEnabled(false);

            _model.IsShowingSequence = true;
            _model.StartNextRound();
            UpdateStatus();

            await Task.Delay(500);

            foreach (int colorIndex in _model.Sequence)
            {
                _buttons[colorIndex].Opacity = 1.0;
                await Task.Delay(SimonModel.FlashMs);
                _buttons[colorIndex].Opacity = 0.5;
                await Task.Delay(200);
            }

            _model.IsShowingSequence = false;
            UpdateStatus();

            _inputEnabled = true;
            SetButtonsEnabled(true);
        }

        private async void Color_Click(object sender, RoutedEventArgs e)
        {
            if (!_inputEnabled)
                return;

            Button btn = (Button)sender;
            int colorIndex = int.Parse(btn.Tag.ToString()!);

            btn.Opacity = 1.0;
            await Task.Delay(150);
            btn.Opacity = 0.5;

            bool correct = _model.PlayerInput(colorIndex);
            UpdateStatus();

            if (!correct)
            {
                _inputEnabled = false;
                SetButtonsEnabled(false);
                return;
            }

            if (_model.RoundComplete)
            {
                _inputEnabled = false;
                SetButtonsEnabled(false);
                await Task.Delay(500);
                await NextRound();
            }
        }

        private void SetButtonsEnabled(bool enabled)
        {
            foreach (Button btn in _buttons)
            {
                btn.IsEnabled = enabled;
            }
        }

        private void UpdateStatus()
        {
            txtStatus.Text = _model.StatusText;
        }
    }
}
