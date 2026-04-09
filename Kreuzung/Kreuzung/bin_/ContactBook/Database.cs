using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace ContactBook
{
    public static class Database
    {
        private const string DbPath = "contacts.db";

        private static string ConnectionString => $"Data Source={DbPath}";

        public static void InitializeDb()
        {
            using SqliteConnection connection = new SqliteConnection(ConnectionString);
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS contacts (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    first_name TEXT,
                    last_name TEXT,
                    email TEXT,
                    phone TEXT,
                    category INTEGER,
                    address TEXT,
                    notes TEXT
                )";
            command.ExecuteNonQuery();
        }

        public static List<Contact> GetAll()
        {
            List<Contact> contacts = new List<Contact>();

            using SqliteConnection connection = new SqliteConnection(ConnectionString);
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT id, first_name, last_name, email, phone, category, address, notes FROM contacts ORDER BY first_name, last_name";

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                contacts.Add(ReadContact(reader));
            }

            return contacts;
        }

        public static List<Contact> Search(string query)
        {
            List<Contact> contacts = new List<Contact>();

            using SqliteConnection connection = new SqliteConnection(ConnectionString);
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
                SELECT id, first_name, last_name, email, phone, category, address, notes
                FROM contacts
                WHERE first_name LIKE @query
                   OR last_name LIKE @query
                   OR email LIKE @query
                   OR phone LIKE @query
                ORDER BY first_name, last_name";
            command.Parameters.AddWithValue("@query", "%" + query + "%");

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                contacts.Add(ReadContact(reader));
            }

            return contacts;
        }

        public static List<Contact> GetByCategory(Category category)
        {
            List<Contact> contacts = new List<Contact>();

            using SqliteConnection connection = new SqliteConnection(ConnectionString);
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
                SELECT id, first_name, last_name, email, phone, category, address, notes
                FROM contacts
                WHERE category = @category
                ORDER BY first_name, last_name";
            command.Parameters.AddWithValue("@category", (int)category);

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                contacts.Add(ReadContact(reader));
            }

            return contacts;
        }

        public static void Add(Contact contact)
        {
            using SqliteConnection connection = new SqliteConnection(ConnectionString);
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO contacts (first_name, last_name, email, phone, category, address, notes)
                VALUES (@firstName, @lastName, @email, @phone, @category, @address, @notes)";
            command.Parameters.AddWithValue("@firstName", contact.FirstName);
            command.Parameters.AddWithValue("@lastName", contact.LastName);
            command.Parameters.AddWithValue("@email", contact.Email);
            command.Parameters.AddWithValue("@phone", contact.Phone);
            command.Parameters.AddWithValue("@category", (int)contact.Category);
            command.Parameters.AddWithValue("@address", contact.Address);
            command.Parameters.AddWithValue("@notes", contact.Notes);
            command.ExecuteNonQuery();
        }

        public static void Update(Contact contact)
        {
            using SqliteConnection connection = new SqliteConnection(ConnectionString);
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE contacts SET
                    first_name = @firstName,
                    last_name = @lastName,
                    email = @email,
                    phone = @phone,
                    category = @category,
                    address = @address,
                    notes = @notes
                WHERE id = @id";
            command.Parameters.AddWithValue("@id", contact.Id);
            command.Parameters.AddWithValue("@firstName", contact.FirstName);
            command.Parameters.AddWithValue("@lastName", contact.LastName);
            command.Parameters.AddWithValue("@email", contact.Email);
            command.Parameters.AddWithValue("@phone", contact.Phone);
            command.Parameters.AddWithValue("@category", (int)contact.Category);
            command.Parameters.AddWithValue("@address", contact.Address);
            command.Parameters.AddWithValue("@notes", contact.Notes);
            command.ExecuteNonQuery();
        }

        public static void Delete(int id)
        {
            using SqliteConnection connection = new SqliteConnection(ConnectionString);
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.CommandText = "DELETE FROM contacts WHERE id = @id";
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }

        private static Contact ReadContact(SqliteDataReader reader)
        {
            return new Contact
            {
                Id = reader.GetInt32(0),
                FirstName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                LastName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Email = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                Phone = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                Category = (Category)reader.GetInt32(5),
                Address = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                Notes = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
            };
        }
    }
}
