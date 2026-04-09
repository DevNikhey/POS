using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Microsoft.Win32;

namespace Einkaufsliste;

public partial class MainWindow : Window
{
    private readonly List<Product> _products;
    private readonly Dictionary<string, List<Product>> _byGroup;
    private readonly ObservableCollection<ShoppingItem> _items = new();

    private const string CsvData = @"group;name;unit
Obst;Apfel;Stk
Obst;Banane;Stk
Obst;Orange;Stk
Obst;Erdbeere;Schale
Obst;Traube;kg
Gemüse;Karotte;kg
Gemüse;Tomate;kg
Gemüse;Gurke;Stk
Gemüse;Paprika;Stk
Gemüse;Zwiebel;kg
Milchprodukte;Milch;Liter
Milchprodukte;Käse;g
Milchprodukte;Joghurt;Becher
Milchprodukte;Butter;Stk
Milchprodukte;Sahne;Becher
Backwaren;Brot;Stk
Backwaren;Brötchen;Stk
Backwaren;Croissant;Stk
Getränke;Wasser;Liter
Getränke;Saft;Liter
Getränke;Kaffee;Packung
Getränke;Tee;Packung";

    public MainWindow()
    {
        InitializeComponent();

        _products = CsvData.Split('\n')
            .Skip(1)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => l.Split(';'))
            .Select(f => new Product { Group = f[0].Trim(), Name = f[1].Trim(), Unit = f[2].Trim() })
            .ToList();

        _byGroup = _products
            .GroupBy(p => p.Group)
            .ToDictionary(g => g.Key, g => g.ToList());

        cmbGroup.ItemsSource = _byGroup.Keys.ToList();
        if (_byGroup.Count > 0) cmbGroup.SelectedIndex = 0;

        lstItems.ItemsSource = _items;
        _items.CollectionChanged += (_, _) => RefreshTree();
    }

    private void Group_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (cmbGroup.SelectedItem is string group && _byGroup.TryGetValue(group, out var products))
        {
            cmbProduct.ItemsSource = products;
            if (products.Count > 0) cmbProduct.SelectedIndex = 0;
        }
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        if (cmbProduct.SelectedItem is not Product product) return;
        if (!int.TryParse(txtAmount.Text, out int amount) || amount < 1) amount = 1;

        var existing = _items.FirstOrDefault(i => i.Name == product.Name);
        if (existing != null)
        {
            existing.Amount += amount;
        }
        else
        {
            _items.Add(new ShoppingItem
            {
                Name = product.Name,
                Group = product.Group,
                Unit = product.Unit,
                Amount = amount
            });
        }
        RefreshTree();
    }

    private void Search_Changed(object sender, TextChangedEventArgs e)
    {
        string query = txtSearch.Text;
        if (string.IsNullOrWhiteSpace(query))
        {
            cmbSearch.ItemsSource = null;
            return;
        }
        var matches = _products
            .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(20)
            .ToList();
        cmbSearch.ItemsSource = matches;
        cmbSearch.IsDropDownOpen = matches.Count > 0;
    }

    private void SearchResult_Selected(object sender, SelectionChangedEventArgs e)
    {
        if (cmbSearch.SelectedItem is Product p)
        {
            cmbGroup.SelectedItem = p.Group;
            cmbProduct.SelectedItem = _byGroup[p.Group].FirstOrDefault(x => x.Name == p.Name);
        }
    }

    private void Item_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (lstItems.SelectedItem is ShoppingItem item)
        {
            _items.Remove(item);
            RefreshTree();
        }
    }

    private void RefreshTree()
    {
        var grouped = _items
            .GroupBy(i => i.Group)
            .OrderBy(g => g.Key)
            .Select(g => new GroupedItems { GroupName = g.Key, Items = g.ToList() })
            .ToList();
        treeGrouped.ItemsSource = grouped;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new SaveFileDialog { Filter = "XML|*.xml", FileName = "einkaufsliste.xml" };
        if (dlg.ShowDialog() != true) return;

        var doc = new XDocument(
            new XElement("ShoppingList",
                _items.Select(i => new XElement("Item",
                    new XAttribute("name", i.Name),
                    new XAttribute("group", i.Group),
                    new XAttribute("unit", i.Unit),
                    new XAttribute("amount", i.Amount)))));
        doc.Save(dlg.FileName);
        MessageBox.Show($"Saved {_items.Count} items.", "Saved");
    }

    private void Load_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog { Filter = "XML|*.xml" };
        if (dlg.ShowDialog() != true) return;

        var doc = XDocument.Load(dlg.FileName);
        _items.Clear();
        foreach (var el in doc.Root!.Elements("Item"))
        {
            _items.Add(new ShoppingItem
            {
                Name = el.Attribute("name")!.Value,
                Group = el.Attribute("group")!.Value,
                Unit = el.Attribute("unit")?.Value ?? "",
                Amount = int.Parse(el.Attribute("amount")!.Value)
            });
        }
        RefreshTree();
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        _items.Clear();
        RefreshTree();
    }
}
