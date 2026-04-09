using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace ExpenseTracker
{
    public static class Database
    {
        private static readonly string DbPath = "expenses.db";

        private static string ConnectionString => $"Data Source={DbPath}";

        public static void InitializeDb()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS transactions (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        description TEXT,
                        amount REAL,
                        category INTEGER,
                        is_income INTEGER,
                        date TEXT
                    )";
                command.ExecuteNonQuery();
            }
        }

        public static void SeedSampleData()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                // Check if table already has data
                var countCmd = connection.CreateCommand();
                countCmd.CommandText = "SELECT COUNT(*) FROM transactions";
                long count = (long)countCmd.ExecuteScalar()!;
                if (count > 0) return;

                var samples = new (string desc, decimal amount, int cat, bool income, string date)[]
                {
                    ("Salary", 3200.00m, 6, true, "2026-04-01"),
                    ("Freelance Project", 850.00m, 6, true, "2026-04-03"),
                    ("Grocery Store", 67.50m, 0, false, "2026-04-02"),
                    ("Bus Monthly Pass", 45.00m, 1, false, "2026-04-01"),
                    ("Rent", 950.00m, 2, false, "2026-04-01"),
                    ("Netflix Subscription", 15.99m, 3, false, "2026-04-01"),
                    ("Pharmacy", 23.40m, 4, false, "2026-04-02"),
                    ("New Shoes", 89.99m, 5, false, "2026-04-03"),
                    ("Restaurant Dinner", 42.80m, 0, false, "2026-04-04"),
                    ("Taxi Ride", 18.50m, 1, false, "2026-04-04"),
                    ("Electricity Bill", 78.00m, 2, false, "2026-04-05"),
                    ("Cinema Tickets", 24.00m, 3, false, "2026-04-05"),
                    ("Doctor Visit", 55.00m, 4, false, "2026-04-06"),
                    ("Book Purchase", 19.99m, 5, false, "2026-04-06"),
                    ("Coffee Shop", 8.50m, 0, false, "2026-04-07"),
                    ("Gas Station", 52.00m, 1, false, "2026-04-07"),
                    ("Bonus Payment", 500.00m, 6, true, "2026-04-05"),
                    ("Lunch Takeaway", 12.90m, 0, false, "2026-04-08"),
                    ("Gym Membership", 35.00m, 4, false, "2026-04-01"),
                    ("Online Shopping", 145.00m, 5, false, "2026-04-08"),
                };

                foreach (var s in samples)
                {
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = @"
                        INSERT INTO transactions (description, amount, category, is_income, date)
                        VALUES ($desc, $amount, $cat, $income, $date)";
                    cmd.Parameters.AddWithValue("$desc", s.desc);
                    cmd.Parameters.AddWithValue("$amount", (double)s.amount);
                    cmd.Parameters.AddWithValue("$cat", s.cat);
                    cmd.Parameters.AddWithValue("$income", s.income ? 1 : 0);
                    cmd.Parameters.AddWithValue("$date", s.date);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<Transaction> GetAll()
        {
            var list = new List<Transaction>();
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT id, description, amount, category, is_income, date FROM transactions ORDER BY date DESC";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(ReadTransaction(reader));
                    }
                }
            }
            return list;
        }

        public static List<Transaction> GetByDateRange(DateTime from, DateTime to)
        {
            var list = new List<Transaction>();
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    SELECT id, description, amount, category, is_income, date
                    FROM transactions
                    WHERE date >= $from AND date <= $to
                    ORDER BY date DESC";
                cmd.Parameters.AddWithValue("$from", from.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("$to", to.ToString("yyyy-MM-dd"));
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(ReadTransaction(reader));
                    }
                }
            }
            return list;
        }

        public static List<Transaction> GetByCategory(Category category)
        {
            var list = new List<Transaction>();
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    SELECT id, description, amount, category, is_income, date
                    FROM transactions
                    WHERE category = $cat
                    ORDER BY date DESC";
                cmd.Parameters.AddWithValue("$cat", (int)category);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(ReadTransaction(reader));
                    }
                }
            }
            return list;
        }

        public static void Add(Transaction t)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO transactions (description, amount, category, is_income, date)
                    VALUES ($desc, $amount, $cat, $income, $date)";
                cmd.Parameters.AddWithValue("$desc", t.Description);
                cmd.Parameters.AddWithValue("$amount", (double)t.Amount);
                cmd.Parameters.AddWithValue("$cat", (int)t.Category);
                cmd.Parameters.AddWithValue("$income", t.IsIncome ? 1 : 0);
                cmd.Parameters.AddWithValue("$date", t.Date.ToString("yyyy-MM-dd"));
                cmd.ExecuteNonQuery();
            }
        }

        public static void Delete(int id)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "DELETE FROM transactions WHERE id = $id";
                cmd.Parameters.AddWithValue("$id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public static decimal GetBalance()
        {
            return GetTotalIncome() - GetTotalExpenses();
        }

        public static decimal GetTotalIncome()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT COALESCE(SUM(amount), 0) FROM transactions WHERE is_income = 1";
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

        public static decimal GetTotalExpenses()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT COALESCE(SUM(amount), 0) FROM transactions WHERE is_income = 0";
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

        private static Transaction ReadTransaction(SqliteDataReader reader)
        {
            return new Transaction
            {
                Id = reader.GetInt32(0),
                Description = reader.GetString(1),
                Amount = Convert.ToDecimal(reader.GetDouble(2)),
                Category = (Category)reader.GetInt32(3),
                IsIncome = reader.GetInt32(4) == 1,
                Date = DateTime.Parse(reader.GetString(5))
            };
        }
    }
}
