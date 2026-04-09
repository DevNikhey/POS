using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace TodoApp
{
    // Statisk klass som hanterar all kommunikation med SQLite-databasen.
    // Visar CRUD-operationer: Create, Read, Update, Delete.
    public static class Database
    {
        // Sokvag till databasfilen (skapas i samma mapp som exe-filen)
        private const string DbPath = "todo.db";

        // Hjalpmetod: skapar och returnerar en ny databasanslutning
        private static SqliteConnection GetConnection()
        {
            return new SqliteConnection($"Data Source={DbPath}");
        }

        // Skapar tabellen om den inte redan finns.
        // Kors vid programstart for att sakerstalla att databasen ar redo.
        public static void InitializeDb()
        {
            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();

                // SQL: CREATE TABLE IF NOT EXISTS - skapar tabellen bara om den saknas
                string sql = @"
                    CREATE TABLE IF NOT EXISTS todos (
                        id              INTEGER PRIMARY KEY AUTOINCREMENT,
                        title           TEXT NOT NULL,
                        description     TEXT,
                        priority        INTEGER NOT NULL DEFAULT 0,
                        due_date        TEXT,
                        is_completed    INTEGER NOT NULL DEFAULT 0,
                        created_at      TEXT NOT NULL
                    )";

                using (SqliteCommand command = new SqliteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        // READ: Hamtar alla uppgifter fran databasen.
        // Sorterar: oavklarade forst, hog prioritet forst, sedan efter forfallodatum.
        public static List<TodoItem> GetAll()
        {
            List<TodoItem> items = new List<TodoItem>();

            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();

                string sql = @"
                    SELECT id, title, description, priority, due_date, is_completed, created_at
                    FROM todos
                    ORDER BY is_completed ASC, priority DESC, due_date ASC";

                using (SqliteCommand command = new SqliteCommand(sql, connection))
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TodoItem item = new TodoItem
                        {
                            Id = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Priority = (Priority)reader.GetInt32(3),
                            DueDate = reader.IsDBNull(4) ? null : DateTime.Parse(reader.GetString(4)),
                            IsCompleted = reader.GetInt32(5) == 1,
                            CreatedAt = DateTime.Parse(reader.GetString(6))
                        };
                        items.Add(item);
                    }
                }
            }

            return items;
        }

        // CREATE: Laggar till en ny uppgift i databasen.
        // Satter item.Id till det auto-genererade id:t fran databasen.
        public static void Add(TodoItem item)
        {
            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();

                string sql = @"
                    INSERT INTO todos (title, description, priority, due_date, is_completed, created_at)
                    VALUES (@title, @description, @priority, @dueDate, @isCompleted, @createdAt);
                    SELECT last_insert_rowid();";

                using (SqliteCommand command = new SqliteCommand(sql, connection))
                {
                    // Parametrar skyddar mot SQL-injection
                    command.Parameters.AddWithValue("@title", item.Title);
                    command.Parameters.AddWithValue("@description", item.Description ?? "");
                    command.Parameters.AddWithValue("@priority", (int)item.Priority);
                    command.Parameters.AddWithValue("@dueDate",
                        item.DueDate.HasValue ? item.DueDate.Value.ToString("yyyy-MM-dd") : (object)DBNull.Value);
                    command.Parameters.AddWithValue("@isCompleted", item.IsCompleted ? 1 : 0);
                    command.Parameters.AddWithValue("@createdAt", item.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));

                    // ExecuteScalar returnerar forsta kolumnen i forsta raden (det nya id:t)
                    object? result = command.ExecuteScalar();
                    if (result != null)
                    {
                        item.Id = Convert.ToInt32(result);
                    }
                }
            }
        }

        // UPDATE: Uppdaterar en befintlig uppgift i databasen baserat pa id.
        public static void Update(TodoItem item)
        {
            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();

                string sql = @"
                    UPDATE todos
                    SET title = @title,
                        description = @description,
                        priority = @priority,
                        due_date = @dueDate,
                        is_completed = @isCompleted
                    WHERE id = @id";

                using (SqliteCommand command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", item.Id);
                    command.Parameters.AddWithValue("@title", item.Title);
                    command.Parameters.AddWithValue("@description", item.Description ?? "");
                    command.Parameters.AddWithValue("@priority", (int)item.Priority);
                    command.Parameters.AddWithValue("@dueDate",
                        item.DueDate.HasValue ? item.DueDate.Value.ToString("yyyy-MM-dd") : (object)DBNull.Value);
                    command.Parameters.AddWithValue("@isCompleted", item.IsCompleted ? 1 : 0);

                    command.ExecuteNonQuery();
                }
            }
        }

        // DELETE: Tar bort en uppgift fran databasen baserat pa id.
        public static void Delete(int id)
        {
            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();

                string sql = "DELETE FROM todos WHERE id = @id";

                using (SqliteCommand command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        // TOGGLE: Vandrar is_completed mellan 0 och 1 for en uppgift.
        // Anvander CASE-uttryck i SQL for att flippa vardet.
        public static void ToggleComplete(int id)
        {
            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();

                string sql = @"
                    UPDATE todos
                    SET is_completed = CASE WHEN is_completed = 0 THEN 1 ELSE 0 END
                    WHERE id = @id";

                using (SqliteCommand command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
