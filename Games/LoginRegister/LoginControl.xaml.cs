using System;
using System.Windows;
using System.Windows.Controls;

namespace LoginRegister;

public partial class LoginControl : UserControl
{
    public event EventHandler? LoginSucceeded;
    public event EventHandler? SwitchToRegistration;

    public string Identifier => txtIdentifier.Text;
    public string Password => txtPassword.Password;

    public LoginControl()
    {
        InitializeComponent();
    }

    private void Login_Click(object sender, RoutedEventArgs e)
    {
        var error = Validators.ValidateLogin(txtIdentifier.Text, txtPassword.Password, true);
        if (error != null)
        {
            txtError.Text = error;
            txtError.Visibility = Visibility.Visible;
            return;
        }
        txtError.Visibility = Visibility.Collapsed;
        LoginSucceeded?.Invoke(this, EventArgs.Empty);
    }

    private void SwitchToRegister_Click(object sender, RoutedEventArgs e)
    {
        SwitchToRegistration?.Invoke(this, EventArgs.Empty);
    }

    public void Reset()
    {
        txtIdentifier.Clear();
        txtPassword.Clear();
        txtError.Visibility = Visibility.Collapsed;
    }
}
