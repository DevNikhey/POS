using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using PenteNetwork;

namespace PenteServer
{
    public class Database
    {
        private readonly string _connectionString;

        public Database(string dbPath = "scores.db")
        {
            _connectionString = $"Data Source={dbPath}";
        }

        public void EnsureCreated()
        {
            using SqliteConnection conn = new SqliteConnection(_connectionString);
            conn.Open();

            string sql = @"
                CREATE TABLE IF NOT EXISTS Players (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL UNIQUE
                );
                CREATE TABLE IF NOT EXISTS Games (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    WhitePlayerId INTEGER NOT NULL,
                    BlackPlayerId INTEGER NOT NULL,
                    WinnerId INTEGER,
                    WinReason TEXT,
                    WhiteCaptures INTEGER NOT NULL DEFAULT 0,
                    BlackCaptures INTEGER NOT NULL DEFAULT 0,
                    TotalMoves INTEGER NOT NULL DEFAULT 0,
                    PlayedAt TEXT NOT NULL,
                    FOREIGN KEY (WhitePlayerId) REFERENCES Players(Id),
                    FOREIGN KEY (BlackPlayerId) REFERENCES Players(Id),
                    FOREIGN KEY (WinnerId) REFERENCES Players(Id)
                );";

            using SqliteCommand cmd = new SqliteCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        public int GetOrCreatePlayer(string name)
        {
            using SqliteConnection conn = new SqliteConnection(_connectionString);
            conn.Open();

            // Try to find existing
            using (SqliteCommand selectCmd = new SqliteCommand("SELECT Id FROM Players WHERE Name = @name", conn))
            {
                selectCmd.Parameters.AddWithValue("@name", name);
                object? result = selectCmd.ExecuteScalar();
                if (result != null)
                    return Convert.ToInt32(result);
            }

            // Insert new
            using (SqliteCommand insertCmd = new SqliteCommand("INSERT INTO Players (Name) VALUES (@name); SELECT last_insert_rowid();", conn))
            {
                insertCmd.Parameters.AddWithValue("@name", name);
                object? result = insertCmd.ExecuteScalar();
                return Convert.ToInt32(result!);
            }
        }

        public void SaveGame(int whitePlayerId, int blackPlayerId, int winnerId, string winReason,
            int whiteCaptures, int blackCaptures, int totalMoves)
        {
            using SqliteConnection conn = new SqliteConnection(_connectionString);
            conn.Open();

            string sql = @"INSERT INTO Games (WhitePlayerId, BlackPlayerId, WinnerId, WinReason,
                           WhiteCaptures, BlackCaptures, TotalMoves, PlayedAt)
                           VALUES (@white, @black, @winner, @reason, @wCap, @bCap, @moves, @played)";

            using SqliteCommand cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@white", whitePlayerId);
            cmd.Parameters.AddWithValue("@black", blackPlayerId);
            cmd.Parameters.AddWithValue("@winner", winnerId);
            cmd.Parameters.AddWithValue("@reason", winReason);
            cmd.Parameters.AddWithValue("@wCap", whiteCaptures);
            cmd.Parameters.AddWithValue("@bCap", blackCaptures);
            cmd.Parameters.AddWithValue("@moves", totalMoves);
            cmd.Parameters.AddWithValue("@played", DateTime.UtcNow.ToString("o"));
            cmd.ExecuteNonQuery();
        }

        public List<ScoreEntry> GetScoreboard()
        {
            using SqliteConnection conn = new SqliteConnection(_connectionString);
            conn.Open();

            string sql = @"
                SELECT
                    p.Name,
                    COALESCE(SUM(CASE WHEN g.WinnerId = p.Id THEN 1 ELSE 0 END), 0) AS Wins,
                    COALESCE(SUM(CASE WHEN g.WinnerId != p.Id AND g.WinnerId IS NOT NULL THEN 1 ELSE 0 END), 0) AS Losses,
                    COALESCE(
                        SUM(CASE WHEN g.WhitePlayerId = p.Id THEN g.WhiteCaptures ELSE 0 END) +
                        SUM(CASE WHEN g.BlackPlayerId = p.Id THEN g.BlackCaptures ELSE 0 END), 0
                    ) AS TotalCaptures
                FROM Players p
                LEFT JOIN Games g ON g.WhitePlayerId = p.Id OR g.BlackPlayerId = p.Id
                GROUP BY p.Id, p.Name
                ORDER BY Wins DESC, Losses ASC";

            List<ScoreEntry> scores = new List<ScoreEntry>();
            using SqliteCommand cmd = new SqliteCommand(sql, conn);
            using SqliteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                scores.Add(new ScoreEntry
                {
                    PlayerName = reader.GetString(0),
                    Wins = reader.GetInt32(1),
                    Losses = reader.GetInt32(2),
                    TotalCaptures = reader.GetInt32(3)
                });
            }

            return scores;
        }
    }
}
