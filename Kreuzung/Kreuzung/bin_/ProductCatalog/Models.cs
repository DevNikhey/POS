using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProductCatalog
{
    /// <summary>
    /// Produkt-Modell mit INotifyPropertyChanged fuer Datenbindung.
    /// </summary>
    public class Product : INotifyPropertyChanged
    {
        private int _id;
        private string _name = string.Empty;
        private string _category = string.Empty;
        private decimal _price;
        private int _stock;
        private string _description = string.Empty;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(); }
        }

        public decimal Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); OnPropertyChanged(nameof(PriceText)); }
        }

        public int Stock
        {
            get => _stock;
            set { _stock = value; OnPropertyChanged(); OnPropertyChanged(nameof(InStock)); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        // Berechnete Eigenschaften
        public string PriceText => $"\u20ac{Price:F2}";
        public bool InStock => Stock > 0;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Warenkorb-Element: Referenz auf ein Produkt mit Menge.
    /// </summary>
    public class CartItem : INotifyPropertyChanged
    {
        private Product _product = null!;
        private int _quantity;

        public Product Product
        {
            get => _product;
            set { _product = value; OnPropertyChanged(); OnPropertyChanged(nameof(Total)); OnPropertyChanged(nameof(TotalText)); }
        }

        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); OnPropertyChanged(nameof(Total)); OnPropertyChanged(nameof(TotalText)); }
        }

        // Berechnete Eigenschaften
        public decimal Total => Product.Price * Quantity;
        public string TotalText => $"\u20ac{Total:F2}";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
