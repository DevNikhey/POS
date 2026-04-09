using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ExpenseTracker
{
    public enum Category
    {
        Food,
        Transport,
        Housing,
        Entertainment,
        Health,
        Shopping,
        Other
    }

    public class Transaction : INotifyPropertyChanged
    {
        private int _id;
        private string _description = string.Empty;
        private decimal _amount;
        private Category _category;
        private bool _isIncome;
        private DateTime _date;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public decimal Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(); OnPropertyChanged(nameof(AmountText)); }
        }

        public Category Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(); OnPropertyChanged(nameof(CategoryColor)); }
        }

        public bool IsIncome
        {
            get => _isIncome;
            set { _isIncome = value; OnPropertyChanged(); OnPropertyChanged(nameof(TypeText)); OnPropertyChanged(nameof(AmountText)); }
        }

        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); }
        }

        // Computed properties
        public string TypeText => IsIncome ? "Income" : "Expense";

        public string AmountText => IsIncome ? $"+\u20ac{Amount:F2}" : $"-\u20ac{Amount:F2}";

        public string CategoryColor
        {
            get
            {
                switch (Category)
                {
                    case Category.Food: return "#E67E22";
                    case Category.Transport: return "#3498DB";
                    case Category.Housing: return "#9B59B6";
                    case Category.Entertainment: return "#E74C3C";
                    case Category.Health: return "#2ECC71";
                    case Category.Shopping: return "#F1C40F";
                    case Category.Other: return "#95A5A6";
                    default: return "#95A5A6";
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
