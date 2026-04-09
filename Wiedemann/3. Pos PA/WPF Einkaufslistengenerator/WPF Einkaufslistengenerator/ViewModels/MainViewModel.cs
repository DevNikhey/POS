using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using WPF_Einkaufslistengenerator.Models;

namespace WPF_Einkaufslistengenerator.ViewModels
{
    public class MainViewModel
    {
        public ObservableCollection<string> ProductGroups { get; set; }
        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<ShoppingItem> ShoppingList { get; set; }

        private Dictionary<string, List<Product>> _productsByGroup;
        private List<Product> _allProducts;

        public Product? SelectedProduct { get; set; }
        public string? CustomProductName { get; set; }
        public int Quantity { get; set; } = 1;

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand PrintCommand { get; }

        private string _selectedGroup;

        private string? _searchText;

        public string? SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                UpdateSearch();
            }
        }

        public ObservableCollection<ProductGroup> GroupedItems { get; set; }
    = new ObservableCollection<ProductGroup>();

        public ObservableCollection<Product> SearchResults { get; set; }
            = new ObservableCollection<Product>();

        public Product? SelectedSearchProduct { get; set; }

        public string SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                _selectedGroup = value;

                UpdateProducts();
            }
        }

        private void UpdateProducts()
        {
            Products.Clear();

            if (_selectedGroup != null && _productsByGroup.ContainsKey(_selectedGroup))
            {
                foreach (var p in _productsByGroup[_selectedGroup])
                {
                    Products.Add(p);
                }
            }
        }
        private void AddItem()
        {
            string? name = null;
            string? group = null;

            // 1️⃣ eigenes Produkt
            if (!string.IsNullOrWhiteSpace(CustomProductName))
            {
                name = CustomProductName;
                group = "Eigen";
            }
            // 2️⃣ SUCH-ComboBox
            else if (SelectedSearchProduct != null)
            {
                name = SelectedSearchProduct.Name;
                group = SelectedSearchProduct.Group;
            }
            // 3️⃣ normale ComboBox
            else if (SelectedProduct != null)
            {
                name = SelectedProduct.Name;
                group = SelectedProduct.Group;
            }

            if (string.IsNullOrWhiteSpace(name))
                return;

            var existing = ShoppingList
                .FirstOrDefault(x => x.Name == name);

            if (existing != null)
            {
                existing.Quantity += Quantity;
            }
            else
            {
                ShoppingList.Add(new ShoppingItem
                {
                    Name = name,
                    Group = group,
                    Quantity = Quantity
                });
            }
            UpdateGroupedItems();
        }

        public MainViewModel()
        {
            ProductGroups = new ObservableCollection<string>();
            Products = new ObservableCollection<Product>();
            ShoppingList = new ObservableCollection<ShoppingItem>();

            AddCommand = new RelayCommand(AddItem);
            DeleteCommand = new RelayCommand(DeleteItems);
            NewCommand = new RelayCommand(NewList);
            SaveCommand = new RelayCommand(SaveList);
            LoadCommand = new RelayCommand(LoadList);
            PrintCommand = new RelayCommand(PrintList);

            // ALLE Produkte
            _allProducts = File.ReadAllLines("Produkte.csv")
                .Skip(1) // Header überspringen
                .Select(line => line.Split(';'))
                .Select(parts => new Product
                {
                    Group = parts[0],
                    Name = parts[1]
                })
                .ToList();

            // LINQ Gruppierung
            _productsByGroup = _allProducts
                .GroupBy(p => p.Group)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Gruppen füllen
            foreach (var group in _productsByGroup.Keys)
            {
                ProductGroups.Add(group);
            }
        }

        private void DeleteItems()
        {
            ShoppingList.Clear();
            UpdateGroupedItems();
        }

        private void NewList()
        {
            ShoppingList.Clear();
            UpdateGroupedItems();
        }

        private void SaveList()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "XML Dateien (*.xml)|*.xml";

            if (dialog.ShowDialog() == true)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<ShoppingItem>));

                using (var stream = File.Create(dialog.FileName))
                {
                    serializer.Serialize(stream, ShoppingList.ToList());
                }
            }
        }
        private void LoadList()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "XML Dateien (*.xml)|*.xml";

            if (dialog.ShowDialog() == true)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<ShoppingItem>));

                using (var stream = File.OpenRead(dialog.FileName))
                {
                    var list = (List<ShoppingItem>)serializer.Deserialize(stream);

                    ShoppingList.Clear();

                    foreach (var item in list)
                    {
                        ShoppingList.Add(item);
                    }
                }
            }
            UpdateGroupedItems();
        }
        private void PrintList()
        {
            Application.Current.MainWindow.Dispatcher.Invoke(() =>
            {
                ((MainWindow)Application.Current.MainWindow).PrintList();
            });
        }
        private void UpdateSearch()
        {
            SearchResults.Clear();

            if (string.IsNullOrWhiteSpace(_searchText))
                return;

            var results = _allProducts
                .Where(p => p.Name != null &&
                            p.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var item in results)
            {
                SearchResults.Add(item);
            }
        }
        private void UpdateGroupedItems()
        {
            GroupedItems.Clear();

            var groups = ShoppingList
                .GroupBy(x => x.Group)
                .Select(g => new ProductGroup
                {
                    Name = g.Key,
                    Items = new ObservableCollection<ShoppingItem>(g.ToList())
                });

            foreach (var group in groups)
            {
                GroupedItems.Add(group);
            }
        }

    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;

        public RelayCommand(Action execute)
        {
            _execute = execute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => _execute();
    }
}
