using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace __NS__;

// Beispiel-IValueConverter: bool -> Visibility (true => Visible, false => Collapsed).
// In XAML:
//   <Window.Resources>
//     <local:BoolToVisibilityConverter x:Key="BoolVis"/>
//   </Window.Resources>
//   <TextBlock Visibility="{Binding IstSichtbar, Converter={StaticResource BoolVis}}"/>
//
// Merke: Ein Converter wandelt einen gebundenen Wert für die Anzeige um.
// Ein TypeConverter (z.B. LengthConverter) wandelt dagegen XAML-STRINGS in
// Property-Werte um und gehört NUR zu passenden Typen (LengthConverter = Längen!).
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => (value is bool b && b) ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility v && v == Visibility.Visible;
}
