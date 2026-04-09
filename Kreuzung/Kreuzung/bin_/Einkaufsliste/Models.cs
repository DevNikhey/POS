using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Einkaufsliste;

public class Product
{
    public string Group { get; set; } = "";
    public string Name { get; set; } = "";
    public string Unit { get; set; } = "";
    public override string ToString() => $"{Name} ({Unit})";
}

public class ShoppingItem : INotifyPropertyChanged
{
    public string Name { get; set; } = "";
    public string Group { get; set; } = "";
    public string Unit { get; set; } = "";

    private int _amount = 1;
    public int Amount
    {
        get => _amount;
        set { _amount = value; OnPropertyChanged(); OnPropertyChanged(nameof(Display)); }
    }

    public string Display => $"{Amount} x {Name} ({Unit})";

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class GroupedItems
{
    public string GroupName { get; set; } = "";
    public System.Collections.Generic.List<ShoppingItem> Items { get; set; } = new();
}
