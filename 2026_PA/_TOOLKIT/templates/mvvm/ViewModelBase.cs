using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace __NS__;

// Basisklasse für ViewModels: meldet Property-Änderungen an die GUI.
// Verwendung:
//   public string Name { get => _name; set => SetProperty(ref _name, value); }
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // Setzt das Feld nur, wenn sich der Wert ändert, und feuert dann PropertyChanged.
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }
}
