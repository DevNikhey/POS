using System.Text.RegularExpressions;

namespace LoginRegister;

public static class Validators
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$");

    public static string? ValidateLogin(string identifier, string password, bool useEmail)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return "Please enter your " + (useEmail ? "email" : "username");
        if (useEmail && !EmailRegex.IsMatch(identifier))
            return "Enter a valid email address";
        if (string.IsNullOrEmpty(password))
            return "Password is required";
        return null;
    }

    public static string? ValidateRegister(string firstName, string lastName, string email,
        string password, string confirm, string address)
    {
        if (string.IsNullOrWhiteSpace(firstName)) return "First name is required";
        if (string.IsNullOrWhiteSpace(lastName)) return "Last name is required";
        if (string.IsNullOrWhiteSpace(email) || !EmailRegex.IsMatch(email)) return "Valid email is required";
        if (string.IsNullOrEmpty(password)) return "Password is required";
        if (password != confirm) return "Passwords do not match";
        if (string.IsNullOrWhiteSpace(address)) return "Address is required";
        return null;
    }
}
