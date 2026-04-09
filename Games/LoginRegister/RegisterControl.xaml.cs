using System;
using System.Windows;
using System.Windows.Controls;

namespace LoginRegister;

public partial class RegisterControl : UserControl
{
    public event EventHandler? RegisterSucceeded;
    public event EventHandler? SwitchToLogin;
    public event EventHandler? Cancelled;

    public string FirstName => txtFirstName.Text;
    public string LastName => txtLastName.Text;
    public string Email => txtEmail.Text;
    public string Password => txtRegPassword.Password;
    public string Address => txtAddress.Text;

    public RegisterControl()
    {
        InitializeComponent();
    }

    private void Register_Click(object sender, RoutedEventArgs e)
    {
        var error = Validators.ValidateRegister(
            txtFirstName.Text, txtLastName.Text, txtEmail.Text,
            txtRegPassword.Password, txtConfirm.Password, txtAddress.Text);

        if (error != null)
        {
            txtRegError.Text = error;
            txtRegError.Visibility = Visibility.Visible;
            return;
        }
        txtRegError.Visibility = Visibility.Collapsed;
        RegisterSucceeded?.Invoke(this, EventArgs.Empty);
    }

    private void SwitchToLogin_Click(object sender, RoutedEventArgs e)
    {
        SwitchToLogin?.Invoke(this, EventArgs.Empty);
    }

    public void Reset()
    {
        txtFirstName.Clear();
        txtLastName.Clear();
        txtEmail.Clear();
        txtRegPassword.Clear();
        txtConfirm.Clear();
        txtAddress.Clear();
        txtRegError.Visibility = Visibility.Collapsed;
    }
}
