using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Einkaufslistengenerator.Models
{
    public class ProductGroup
    {
        public string? Name { get; set; }
        public ObservableCollection<ShoppingItem> Items { get; set; }
            = new ObservableCollection<ShoppingItem>();
    }
}
