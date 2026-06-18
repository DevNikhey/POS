using LinqToDB;
using LinqToDB.Data;

namespace __NS__;

// =====================================================================
//  linq2db / SQLite – Schnellstart (aus PA3 & PA4).
//
//  1) NuGet-Pakete (macht das Toolkit-Skript mit --db automatisch):
//       linq2db  linq2db.SQLite  Microsoft.Data.Sqlite
//
//  2) Modell aus einer .db-Datei erzeugen:  ./scaffold-db.sh (bzw. .bat)
//     -> erzeugt eine DataConnection (z.B. MyDB) + Entity-Klassen.
//
//  3) Verwendung (Muster):
//       var options = new DataOptions().UseSQLite("Data Source=mydata.db");
//       using var db = new MyDB(options);
//       var rows  = db.SomeTable.Where(x => x.Name.Contains("abc")).ToList();
//       long id   = db.InsertWithInt64Identity(entity);   // Insert + neue Id
//
//  Merke:
//   * Abfragen auf db.Table sind IQueryable -> werden in SQL übersetzt.
//   * SQLite-"LIKE" (von .Contains) ist standardmäßig case-INsensitive.
//   * .db-Datei in der .csproj auf "CopyToOutputDirectory=PreserveNewest"
//     setzen, damit sie neben der EXE liegt.
// =====================================================================
public static class Db
{
    // Verbindungsstring zentral halten -> in einer Methode öffnen.
    public const string ConnectionString = "Data Source=mydata.db";

    // Beispiel-Factory; ersetze "DataConnection" durch deine generierte Klasse:
    //   public static MyDB Open() => new MyDB(new DataOptions().UseSQLite(ConnectionString));
    public static DataConnection Open()
        => new DataConnection(new DataOptions().UseSQLite(ConnectionString));
}
