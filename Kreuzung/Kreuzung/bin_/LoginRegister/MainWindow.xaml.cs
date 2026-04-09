using System.Windows;

namespace LoginRegister;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        loginControl.LoginSucceeded += (_, _) =>
        {
            loginControl.Visibility = Visibility.Collapsed;
            welcomeOverlay.Visibility = Visibility.Visible;
            txtWelcome.Text = $"Welcome, {loginControl.Identifier}!";
        };

        loginControl.SwitchToRegistration += (_, _) =>
        {
            loginControl.Visibility = Visibility.Collapsed;
            registerControl.Visibility = Visibility.Visible;
        };

        registerControl.RegisterSucceeded += (_, _) =>
        {
            registerControl.Visibility = Visibility.Collapsed;
            welcomeOverlay.Visibility = Visibility.Visible;
            txtWelcome.Text = $"Welcome, {registerControl.FirstName} {registerControl.LastName}!";
        };

        registerControl.SwitchToLogin += (_, _) =>
        {
            registerControl.Visibility = Visibility.Collapsed;
            loginControl.Visibility = Visibility.Visible;
        };
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        welcomeOverlay.Visibility = Visibility.Collapsed;
        loginControl.Reset();
        loginControl.Visibility = Visibility.Visible;
    }
}
