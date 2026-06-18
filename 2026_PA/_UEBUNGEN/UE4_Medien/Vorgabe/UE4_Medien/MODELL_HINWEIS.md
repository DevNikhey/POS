# Datenmodell erzeugen (NICHT von Hand schreiben)

Das DB-Modell (`MoviesDB` + Entity `DataModels.Film`) wird **generiert**, nicht
getippt. Vorgehen:

1. DB aus dem Schema erzeugen (im Ordner `UE4_Medien/UE4_Medien/`):

   ```
   sqlite3 movies.db < ../schema.sql
   ```

2. linq2db-Modell generieren:

   ```
   ./_TOOLKIT/scaffold-db.sh ./UE4_Medien/UE4_Medien movies.db
   ```

   bzw. unter Visual Studio per `movies.tt` (Run Custom Tool).

Das erzeugt sinngemaess:

```csharp
namespace DataModels
{
    public partial class MoviesDB : LinqToDB.Data.DataConnection
    {
        public ITable<Film> Films { get { return this.GetTable<Film>(); } }
        // ... .ctor (DataOptions options) usw.
    }

    [Table(Schema="main", Name="films")]
    public partial class Film
    {
        [Column("id"),       PrimaryKey, Identity, Nullable] public long?  Id       { get; set; }
        [Column("title"),                          Nullable] public string Title    { get; set; }
        [Column("director"),                       Nullable] public string Director { get; set; }
        [Column("year"),                           Nullable] public long?  Year     { get; set; }
        [Column("rating"),                         Nullable] public long?  Rating   { get; set; }
    }
}
```

## Wichtig

- Das generierte Entity `DataModels.Film` heisst **gleich** wie das ViewModel `Film`
  im Code-Behind -> beim DB-Zugriff `DataModels.Film` voll qualifizieren.
- Alle DB-Felder sind `Nullable` (`long?`). Beim Uebernehmen ins ViewModel
  casten/konvertieren, z.B. `(long)item.Id`, `(int)(item.Year ?? 0)`,
  `(int)(item.Rating ?? 0)`.
- `movies.db` in der `.csproj` auf `CopyToOutputDirectory=PreserveNewest` setzen,
  sonst zur Laufzeit `SQLite Error: no such table: films`.
- Erst nach dem Generieren die `using`-Zeilen in `MainWindow.xaml.cs` aktivieren
  (`using LinqToDB;` und `using DataModels;`).
