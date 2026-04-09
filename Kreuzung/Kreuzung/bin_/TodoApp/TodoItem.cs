using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TodoApp
{
    // Prioritet-enum for att ange hur viktig en uppgift ar
    public enum Priority
    {
        Low,
        Medium,
        High
    }

    // Modellklass som representerar en Todo-uppgift
    // Implementerar INotifyPropertyChanged sa att WPF:s databindning
    // automatiskt uppdaterar UI nar en egenskap andras
    public class TodoItem : INotifyPropertyChanged
    {
        private int _id;
        private string _title = "";
        private string _description = "";
        private Priority _priority;
        private DateTime? _dueDate;
        private bool _isCompleted;
        private DateTime _createdAt;

        // Unik identifierare fran databasen (PRIMARY KEY)
        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        // Titel / namn pa uppgiften
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        // Langre beskrivning (valfri)
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        // Prioritetsniva: Low, Medium eller High
        public Priority Priority
        {
            get => _priority;
            set
            {
                _priority = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PriorityColor)); // Uppdatera farg nar prioritet andras
            }
        }

        // Valfritt forfallodatum
        public DateTime? DueDate
        {
            get => _dueDate;
            set { _dueDate = value; OnPropertyChanged(); }
        }

        // Om uppgiften ar avklarad eller ej
        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                _isCompleted = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusText)); // Uppdatera statustext nar completion andras
            }
        }

        // Tidpunkt da uppgiften skapades
        public DateTime CreatedAt
        {
            get => _createdAt;
            set { _createdAt = value; OnPropertyChanged(); }
        }

        // Beraknad egenskap: returnerar en farg baserat pa prioritet
        // Anvands i XAML for att visa prioritet med fargkodning
        public string PriorityColor
        {
            get
            {
                switch (Priority)
                {
                    case Priority.Low: return "Green";
                    case Priority.Medium: return "Orange";
                    case Priority.High: return "Red";
                    default: return "Gray";
                }
            }
        }

        // Beraknad egenskap: visar textstatus
        public string StatusText
        {
            get => IsCompleted ? "Completed" : "Active";
        }

        // ---- INotifyPropertyChanged-implementation ----
        // Detta event triggas varje gang en egenskap andras,
        // och WPF lyssnar pa det for att uppdatera UI automatiskt.
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
