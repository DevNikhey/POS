# WPF + SQLite + LINQ — Cheatsheet fuer die Pruefung

---

## 1. Projekt erstellen

```bash
dotnet new wpf -n MeinProjekt
cd MeinProjekt
```

---

## 2. NuGet Packages

### Option A: Raw SQLite (einfacher, weniger Code)
```bash
dotnet add package Microsoft.Data.Sqlite
```

### Option B: Entity Framework Core (auto Models, LINQ statt SQL)
```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet tool install --global dotnet-ef
```

---

## 3. Models aus data.db generieren (EF Core)

Wenn du eine `data.db` Datei bekommst:

```bash
# data.db in den Projektordner kopieren, dann:
dotnet ef dbcontext scaffold "Data Source=data.db" Microsoft.EntityFrameworkCore.Sqlite -o Models
```

Das generiert automatisch:
- Eine **Model-Klasse pro Tabelle** im `Models/` Ordner
- Einen **DbContext** mit allen `DbSet<T>` Properties

Danach sofort benutzbar:
```csharp
using var db = new DataContext();
var alle = db.Students.ToList();
```

---

## 4. LINQ — using System.Linq;

LINQ ist in C# eingebaut. Kein NuGet noetig — nur dieses using:

```csharp
using System.Linq;
```

### Filtern
```csharp
list.Where(x => x.Age > 18)
list.Where(x => x.Name.Contains("Max"))
list.Where(x => x.Category == "IT" && x.Salary > 3000)
```

### Sortieren
```csharp
list.OrderBy(x => x.Name)                  // A → Z
list.OrderByDescending(x => x.Score)        // 5 → 1
list.OrderBy(x => x.Category).ThenBy(x => x.Name)  // mehrere Kriterien
```

### Transformieren (Select)
```csharp
list.Select(x => x.Name)                              // → List<string>
list.Select(x => new { x.Name, x.Score })             // → anonymes Objekt
list.Select(x => $"{x.FirstName} {x.LastName}")       // → formatierter String
```

### Erstes Element finden
```csharp
list.First()                                // erstes Element (wirft Exception wenn leer)
list.First(x => x.Id == 5)                 // erstes mit Bedingung
list.FirstOrDefault(x => x.Id == 5)        // null wenn nicht gefunden
list.SingleOrDefault(x => x.Id == 5)       // wie First, aber wirft wenn mehrere
```

### Aggregation (Zusammenfassung)
```csharp
list.Count()                                // Anzahl
list.Count(x => x.Score > 3)               // Anzahl mit Bedingung
list.Sum(x => x.Amount)                    // Summe
list.Average(x => x.Score)                 // Durchschnitt
list.Min(x => x.Price)                     // Minimum
list.Max(x => x.Price)                     // Maximum
```

### Gruppieren (GroupBy)
```csharp
var gruppen = list.GroupBy(x => x.Category);
foreach (var g in gruppen)
{
    Console.WriteLine($"{g.Key}: {g.Count()} Eintraege, Avg: {g.Average(x => x.Score)}");
}

// Oder direkt:
list.GroupBy(x => x.Department)
    .Select(g => new { Dept = g.Key, Count = g.Count(), AvgSalary = g.Average(x => x.Salary) })
    .OrderByDescending(x => x.AvgSalary)
    .ToList();
```

### Pruefen (Any / All)
```csharp
list.Any()                                  // gibt es ueberhaupt Elemente?
list.Any(x => x.Score > 4)                 // gibt es mindestens eines mit Score > 4?
list.All(x => x.IsCompleted)               // sind ALLE completed?
```

### Menge begrenzen
```csharp
list.Take(10)                               // erste 10
list.Skip(5)                                // ueberspringe 5
list.Skip(10).Take(5)                       // Seite 3 bei 5 pro Seite
list.Distinct()                             // keine Duplikate
```

### Verketten (Chaining)
```csharp
var ergebnis = studenten
    .Where(s => s.Klasse == "3A")
    .OrderBy(s => s.Name)
    .Select(s => new {
        s.Name,
        Schnitt = s.Noten.Average(n => n.Score)
    })
    .ToList();
```

---

## 5. Raw SQLite — Schnellstart

```csharp
using Microsoft.Data.Sqlite;

// Verbinden
var conn = new SqliteConnection("Data Source=data.db");
conn.Open();

// Tabellen anzeigen (wenn du die Struktur nicht kennst)
var cmd = conn.CreateCommand();
cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
var reader = cmd.ExecuteReader();
while (reader.Read()) Console.WriteLine(reader.GetString(0));
reader.Close();

// Spalten einer Tabelle anzeigen
cmd.CommandText = "PRAGMA table_info(students)";
reader = cmd.ExecuteReader();
while (reader.Read()) Console.WriteLine($"{reader["name"]} ({reader["type"]})");
reader.Close();

// SELECT
cmd.CommandText = "SELECT * FROM students WHERE class = $c ORDER BY name";
cmd.Parameters.AddWithValue("$c", "3A");
reader = cmd.ExecuteReader();
while (reader.Read())
{
    int id = reader.GetInt32(0);
    string name = reader.GetString(1);
}
reader.Close();

// INSERT
cmd.CommandText = "INSERT INTO students(name, class) VALUES($n, $c)";
cmd.Parameters.Clear();
cmd.Parameters.AddWithValue("$n", "Max Mustermann");
cmd.Parameters.AddWithValue("$c", "3A");
cmd.ExecuteNonQuery();

// Letzte eingefuegte ID holen
cmd.CommandText = "SELECT last_insert_rowid()";
long newId = (long)cmd.ExecuteScalar();

// UPDATE
cmd.CommandText = "UPDATE students SET name = $n WHERE id = $id";
cmd.Parameters.Clear();
cmd.Parameters.AddWithValue("$n", "Neuer Name");
cmd.Parameters.AddWithValue("$id", 1);
cmd.ExecuteNonQuery();

// DELETE
cmd.CommandText = "DELETE FROM students WHERE id = $id";
cmd.Parameters.Clear();
cmd.Parameters.AddWithValue("$id", 1);
cmd.ExecuteNonQuery();

// JOIN
cmd.CommandText = @"
    SELECT s.name, g.subject, g.score
    FROM students s
    JOIN grades g ON g.student_id = s.id
    WHERE s.class = $c";
cmd.Parameters.Clear();
cmd.Parameters.AddWithValue("$c", "3A");
reader = cmd.ExecuteReader();
while (reader.Read())
{
    Console.WriteLine($"{reader.GetString(0)}: {reader.GetString(1)} = {reader.GetDouble(2)}");
}
reader.Close();

conn.Close();
```

---

## 6. EF Core DbContext — Schnellstart

### Models (manuell oder per scaffold generiert)
```csharp
public class Student
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Class { get; set; }
    public List<Grade> Grades { get; set; }     // 1:N Beziehung
}

public class Grade
{
    public int Id { get; set; }
    public string Subject { get; set; }
    public double Score { get; set; }
    public int StudentId { get; set; }          // Fremdschluessel
    public Student Student { get; set; }
}
```

### DbContext
```csharp
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<Student> Students { get; set; }
    public DbSet<Grade> Grades { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite("Data Source=data.db");
    }
}
```

### CRUD mit EF Core
```csharp
using var db = new AppDbContext();
db.Database.EnsureCreated();    // Tabellen erstellen falls noetig

// SELECT (mit JOIN automatisch ueber Include)
var studenten = db.Students
    .Include(s => s.Grades)
    .Where(s => s.Class == "3A")
    .OrderBy(s => s.Name)
    .ToList();

// INSERT
db.Students.Add(new Student { Name = "Max", Class = "3A" });
db.SaveChanges();

// UPDATE
var student = db.Students.Find(1);
student.Name = "Neuer Name";
db.SaveChanges();

// DELETE
db.Students.Remove(student);
db.SaveChanges();

// Aggregation
double schnitt = db.Grades
    .Where(g => g.StudentId == 1)
    .Average(g => g.Score);

int anzahl = db.Students.Count(s => s.Class == "3A");
```

---

## 7. CSV lesen mit LINQ

```csharp
// CSV einlesen (semikolon-getrennt)
var zeilen = File.ReadAllLines("data.csv");

var daten = zeilen
    .Skip(1)                                    // Header ueberspringen
    .Where(z => !string.IsNullOrWhiteSpace(z))  // Leerzeilen ignorieren
    .Select(z => z.Split(';'))                   // in Spalten aufteilen
    .Select(s => new Product
    {
        Name = s[0].Trim(),
        Category = s[1].Trim(),
        Price = decimal.Parse(s[2].Trim()),
        Stock = int.Parse(s[3].Trim())
    })
    .ToList();

// Sofort filtern/sortieren:
var teuer = daten.Where(p => p.Price > 100).OrderBy(p => p.Name).ToList();
var gruppen = daten.GroupBy(p => p.Category).ToList();
```

---

## 8. WPF Data Binding — die wichtigsten Patterns

### INotifyPropertyChanged (damit UI sich aktualisiert)
```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Student : INotifyPropertyChanged
{
    private string _name;
    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
```

### ObservableCollection (damit ListView/DataGrid sich aktualisiert)
```csharp
using System.Collections.ObjectModel;

public ObservableCollection<Student> Students { get; } = new();

// Add/Remove aktualisiert UI automatisch:
Students.Add(new Student { Name = "Max" });
Students.Remove(student);
Students.Clear();
```

### XAML Binding
```xml
<!-- ListView -->
<ListView ItemsSource="{Binding Students}" SelectionChanged="List_SelectionChanged">
    <ListView.View>
        <GridView>
            <GridViewColumn Header="Name" DisplayMemberBinding="{Binding Name}" Width="150"/>
            <GridViewColumn Header="Klasse" DisplayMemberBinding="{Binding Class}" Width="80"/>
        </GridView>
    </ListView.View>
</ListView>

<!-- DataGrid (auto-generiert Spalten) -->
<DataGrid ItemsSource="{Binding Students}" AutoGenerateColumns="True" IsReadOnly="True"/>

<!-- Detail-Anzeige -->
<TextBlock Text="{Binding SelectedItem.Name, ElementName=myListView}"/>
```

---

## 9. Schnellstart-Template

```bash
# Projekt erstellen
dotnet new wpf -n Pruefung
cd Pruefung

# data.db reinkopieren
cp ../data.db .

# Option A: Raw SQLite
dotnet add package Microsoft.Data.Sqlite

# Option B: EF Core mit Auto-Scaffold
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet ef dbcontext scaffold "Data Source=data.db" Microsoft.EntityFrameworkCore.Sqlite -o Models

# Starten
dotnet run
```

---

## 10. Haeufige Using-Statements

```csharp
// Immer gebraucht
using System;
using System.Collections.Generic;
using System.Linq;                          // ← LINQ!
using System.Windows;
using System.Windows.Controls;

// Fuer ObservableCollection + Binding
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// Fuer Raw SQLite
using Microsoft.Data.Sqlite;

// Fuer EF Core
using Microsoft.EntityFrameworkCore;

// Fuer CSV / Dateien
using System.IO;

// Fuer XML Export
using System.Xml.Linq;
```
