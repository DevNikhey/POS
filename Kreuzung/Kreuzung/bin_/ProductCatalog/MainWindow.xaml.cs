using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProductCatalog
{
    /// <summary>
    /// Hauptfenster: Produktkatalog mit Suche, Filterung, Sortierung und Warenkorb.
    /// Demonstriert CSV-Parsing, LINQ und ObservableCollection.
    /// </summary>
    public partial class MainWindow : Window
    {
        // Alle Produkte aus der CSV-Datei
        private List<Product> _allProducts = new List<Product>();

        // Aktuell angezeigte (gefilterte/sortierte) Produkte
        private List<Product> _filteredProducts = new List<Product>();

        // Warenkorb als ObservableCollection fuer automatische UI-Aktualisierung
        private ObservableCollection<CartItem> _cartItems = new ObservableCollection<CartItem>();

        // Ausgewaehlte Menge fuer "Zum Warenkorb hinzufuegen"
        private int _selectedQuantity = 1;

        public MainWindow()
        {
            InitializeComponent();

            // CSV parsen und Daten laden
            LoadProducts();

            // Filter- und Sortieroptionen initialisieren
            InitializeFilters();

            // Warenkorb an ListView binden
            CartList.ItemsSource = _cartItems;

            // Initiale Produktliste anzeigen
            ApplyFilterAndSort();
        }

        /// <summary>
        /// Laedt die Produkte aus dem eingebetteten CSV-String.
        /// </summary>
        private void LoadProducts()
        {
            _allProducts = CsvData.Parse();
        }

        /// <summary>
        /// Initialisiert die ComboBoxen fuer Kategorie-Filter und Sortierung.
        /// </summary>
        private void InitializeFilters()
        {
            // Kategorien aus den Produkten extrahieren (LINQ Distinct)
            List<string> categories = _allProducts
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            // "Alle Kategorien" als erste Option
            categories.Insert(0, "Alle Kategorien");
            CategoryFilter.ItemsSource = categories;
            CategoryFilter.SelectedIndex = 0;

            // Sortieroptionen
            List<string> sortOptions = new List<string>
            {
                "Name (A-Z)",
                "Name (Z-A)",
                "Preis (aufsteigend)",
                "Preis (absteigend)",
                "Lagerbestand",
                "Kategorie"
            };
            SortCombo.ItemsSource = sortOptions;
            SortCombo.SelectedIndex = 0;
        }

        /// <summary>
        /// Wendet Suchtext, Kategorie-Filter und Sortierung mit LINQ an.
        /// Zentrale Methode die bei jeder Aenderung aufgerufen wird.
        /// </summary>
        private void ApplyFilterAndSort()
        {
            // Startet mit allen Produkten
            IEnumerable<Product> query = _allProducts;

            // 1. Suchtext-Filter (Name oder Beschreibung)
            string searchText = SearchBox.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(searchText))
            {
                string searchLower = searchText.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchLower) ||
                    p.Description.ToLower().Contains(searchLower));
            }

            // 2. Kategorie-Filter
            if (CategoryFilter.SelectedIndex > 0) // 0 = "Alle Kategorien"
            {
                string selectedCategory = CategoryFilter.SelectedItem as string ?? string.Empty;
                query = query.Where(p => p.Category == selectedCategory);
            }

            // 3. Sortierung
            switch (SortCombo.SelectedIndex)
            {
                case 0: // Name A-Z
                    query = query.OrderBy(p => p.Name);
                    break;
                case 1: // Name Z-A
                    query = query.OrderByDescending(p => p.Name);
                    break;
                case 2: // Preis aufsteigend
                    query = query.OrderBy(p => p.Price);
                    break;
                case 3: // Preis absteigend
                    query = query.OrderByDescending(p => p.Price);
                    break;
                case 4: // Lagerbestand
                    query = query.OrderByDescending(p => p.Stock);
                    break;
                case 5: // Kategorie
                    query = query.OrderBy(p => p.Category).ThenBy(p => p.Name);
                    break;
                default:
                    query = query.OrderBy(p => p.Name);
                    break;
            }

            // Ergebnis materialisieren und anzeigen
            _filteredProducts = query.ToList();
            ProductList.ItemsSource = _filteredProducts;

            // StatusBar aktualisieren
            UpdateStatusBar();
        }

        // ==================== Event-Handler: Filter ====================

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilterAndSort();
        }

        private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
                ApplyFilterAndSort();
        }

        private void SortCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
                ApplyFilterAndSort();
        }

        // ==================== Event-Handler: Produktauswahl ====================

        /// <summary>
        /// Zeigt die Details des ausgewaehlten Produkts im mittleren Panel an.
        /// </summary>
        private void ProductList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Product? selectedProduct = ProductList.SelectedItem as Product;

            if (selectedProduct != null)
            {
                // Detail-Panel sichtbar machen
                DetailPanel.Visibility = Visibility.Visible;
                DetailPlaceholder.Visibility = Visibility.Collapsed;

                // Details befuellen
                DetailName.Text = selectedProduct.Name;
                DetailCategory.Text = selectedProduct.Category;
                DetailPrice.Text = selectedProduct.PriceText;
                DetailStock.Text = selectedProduct.Stock.ToString();

                // Verfuegbarkeit mit Farbe
                if (selectedProduct.InStock)
                {
                    DetailAvailability.Text = $"Verfuegbar ({selectedProduct.Stock} Stueck)";
                    DetailAvailability.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    DetailAvailability.Text = "Nicht verfuegbar";
                    DetailAvailability.Foreground = System.Windows.Media.Brushes.Red;
                }

                DetailDescription.Text = selectedProduct.Description;

                // Menge zuruecksetzen
                _selectedQuantity = 1;
                QuantityText.Text = _selectedQuantity.ToString();

                // Button nur aktiv wenn auf Lager
                BtnAddToCart.IsEnabled = selectedProduct.InStock;
            }
            else
            {
                DetailPanel.Visibility = Visibility.Collapsed;
                DetailPlaceholder.Visibility = Visibility.Visible;
            }
        }

        // ==================== Event-Handler: Menge ====================

        private void BtnQuantityDown_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedQuantity > 1)
            {
                _selectedQuantity--;
                QuantityText.Text = _selectedQuantity.ToString();
            }
        }

        private void BtnQuantityUp_Click(object sender, RoutedEventArgs e)
        {
            Product? selectedProduct = ProductList.SelectedItem as Product;
            if (selectedProduct != null && _selectedQuantity < selectedProduct.Stock)
            {
                _selectedQuantity++;
                QuantityText.Text = _selectedQuantity.ToString();
            }
        }

        // ==================== Event-Handler: Warenkorb ====================

        /// <summary>
        /// Fuegt das ausgewaehlte Produkt zum Warenkorb hinzu.
        /// Wenn das Produkt bereits im Warenkorb ist, wird die Menge erhoeht.
        /// </summary>
        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            Product? selectedProduct = ProductList.SelectedItem as Product;
            if (selectedProduct == null || !selectedProduct.InStock)
                return;

            // Pruefen ob Produkt bereits im Warenkorb (LINQ FirstOrDefault)
            CartItem? existingItem = _cartItems
                .FirstOrDefault(ci => ci.Product.Id == selectedProduct.Id);

            if (existingItem != null)
            {
                // Menge erhoehen
                existingItem.Quantity += _selectedQuantity;
            }
            else
            {
                // Neues CartItem erstellen
                CartItem newItem = new CartItem
                {
                    Product = selectedProduct,
                    Quantity = _selectedQuantity
                };
                _cartItems.Add(newItem);
            }

            // Menge zuruecksetzen
            _selectedQuantity = 1;
            QuantityText.Text = _selectedQuantity.ToString();

            // UI aktualisieren
            RefreshCartList();
            UpdateStatusBar();
        }

        /// <summary>
        /// Entfernt ein Produkt aus dem Warenkorb ueber den X-Button.
        /// </summary>
        private void BtnRemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            Button? button = sender as Button;
            if (button?.Tag == null)
                return;

            int productId = (int)button.Tag;

            // Element mit LINQ finden und entfernen
            CartItem? itemToRemove = _cartItems
                .FirstOrDefault(ci => ci.Product.Id == productId);

            if (itemToRemove != null)
            {
                _cartItems.Remove(itemToRemove);
            }

            UpdateStatusBar();
        }

        /// <summary>
        /// Leert den gesamten Warenkorb.
        /// </summary>
        private void BtnClearCart_Click(object sender, RoutedEventArgs e)
        {
            if (_cartItems.Count == 0)
                return;

            MessageBoxResult result = MessageBox.Show(
                "Moechten Sie den Warenkorb wirklich leeren?",
                "Warenkorb leeren",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _cartItems.Clear();
                UpdateStatusBar();
            }
        }

        // ==================== Hilfsmethoden ====================

        /// <summary>
        /// Erzwingt eine Aktualisierung der Warenkorb-Anzeige.
        /// Noetig weil ObservableCollection keine Property-Aenderungen innerhalb von Items meldet.
        /// </summary>
        private void RefreshCartList()
        {
            CartList.ItemsSource = null;
            CartList.ItemsSource = _cartItems;
        }

        /// <summary>
        /// Aktualisiert die StatusBar mit aktuellen Zaehlerstaenden.
        /// Verwendet LINQ Sum und Count.
        /// </summary>
        private void UpdateStatusBar()
        {
            // Angezeigte Produkte
            int productCount = _filteredProducts.Count;
            int totalCount = _allProducts.Count;
            StatusProducts.Text = $"Produkte: {productCount} von {totalCount}";

            // Warenkorb-Artikel (Summe der Mengen)
            int cartItemCount = _cartItems.Sum(ci => ci.Quantity);
            StatusCartItems.Text = $"Warenkorb: {cartItemCount} Artikel";

            // Warenkorb-Gesamtsumme (LINQ Sum)
            decimal cartTotal = _cartItems.Sum(ci => ci.Total);
            string cartTotalFormatted = $"\u20ac{cartTotal:F2}";
            StatusCartTotal.Text = $"Gesamt: {cartTotalFormatted}";
            CartTotalText.Text = cartTotalFormatted;
        }
    }
}
