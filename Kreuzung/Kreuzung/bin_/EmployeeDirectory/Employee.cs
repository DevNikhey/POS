using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EmployeeDirectory
{
    public class Employee : INotifyPropertyChanged
    {
        private int _id;
        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private string _email = string.Empty;
        private string _department = string.Empty;
        private string _position = string.Empty;
        private decimal _salary;
        private DateTime _hireDate;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(); OnPropertyChanged(nameof(FullName)); }
        }

        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(); OnPropertyChanged(nameof(FullName)); }
        }

        public string FullName => $"{FirstName} {LastName}";

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Department
        {
            get => _department;
            set { _department = value; OnPropertyChanged(); }
        }

        public string Position
        {
            get => _position;
            set { _position = value; OnPropertyChanged(); }
        }

        public decimal Salary
        {
            get => _salary;
            set { _salary = value; OnPropertyChanged(); OnPropertyChanged(nameof(SalaryText)); }
        }

        public string SalaryText => $"\u20ac{Salary:N0}";

        public DateTime HireDate
        {
            get => _hireDate;
            set { _hireDate = value; OnPropertyChanged(); OnPropertyChanged(nameof(Tenure)); }
        }

        public int Tenure
        {
            get
            {
                DateTime today = DateTime.Today;
                int years = today.Year - HireDate.Year;
                if (HireDate.Date > today.AddYears(-years))
                    years--;
                return years;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public override string ToString() => FullName;
    }
}
