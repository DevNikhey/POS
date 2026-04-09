using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

namespace StudentGrades
{
    public static class Database
    {
        private static string DbPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "grades.db");

        private static SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();
            return connection;
        }

        public static void InitializeDb()
        {
            using var connection = GetConnection();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS students (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    first_name TEXT NOT NULL,
                    last_name TEXT NOT NULL,
                    class TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS grades (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    student_id INTEGER NOT NULL REFERENCES students(id),
                    subject TEXT NOT NULL,
                    score REAL NOT NULL,
                    date TEXT NOT NULL
                );
            ";
            cmd.ExecuteNonQuery();
        }

        public static void SeedSampleData()
        {
            using var connection = GetConnection();

            // Check if data already exists
            var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM students";
            var count = Convert.ToInt64(checkCmd.ExecuteScalar());
            if (count > 0) return;

            // Sample students
            var students = new (string First, string Last, string Class)[]
            {
                ("Anna", "Mueller", "3A"),
                ("Ben", "Schmidt", "3A"),
                ("Clara", "Weber", "3A"),
                ("David", "Fischer", "3B"),
                ("Emma", "Wagner", "3B"),
                ("Felix", "Bauer", "3B"),
                ("Greta", "Hoffmann", "4A"),
                ("Hans", "Koch", "4A"),
                ("Ida", "Richter", "4A"),
                ("Jan", "Klein", "4B")
            };

            foreach (var s in students)
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = "INSERT INTO students (first_name, last_name, class) VALUES (@fn, @ln, @cl)";
                cmd.Parameters.AddWithValue("@fn", s.First);
                cmd.Parameters.AddWithValue("@ln", s.Last);
                cmd.Parameters.AddWithValue("@cl", s.Class);
                cmd.ExecuteNonQuery();
            }

            // Sample grades
            var subjects = new string[] { "Mathematik", "Deutsch", "Englisch", "Physik", "Informatik" };
            var random = new Random(42); // Fixed seed for reproducible data

            for (int studentId = 1; studentId <= 10; studentId++)
            {
                // Each student gets 3-5 grades
                int gradeCount = random.Next(3, 6);
                for (int g = 0; g < gradeCount; g++)
                {
                    var subject = subjects[g % subjects.Length];
                    var score = random.Next(1, 6); // 1-5
                    var date = new DateTime(2026, random.Next(1, 4), random.Next(1, 29));

                    var cmd = connection.CreateCommand();
                    cmd.CommandText = "INSERT INTO grades (student_id, subject, score, date) VALUES (@sid, @sub, @sc, @dt)";
                    cmd.Parameters.AddWithValue("@sid", studentId);
                    cmd.Parameters.AddWithValue("@sub", subject);
                    cmd.Parameters.AddWithValue("@sc", (double)score);
                    cmd.Parameters.AddWithValue("@dt", date.ToString("yyyy-MM-dd"));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<StudentWithAvg> GetStudentsWithAvg()
        {
            var list = new List<StudentWithAvg>();
            using var connection = GetConnection();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT s.id, s.first_name, s.last_name, s.class,
                       COALESCE(AVG(g.score), 0) AS average,
                       COUNT(g.id) AS grade_count
                FROM students s
                LEFT JOIN grades g ON s.id = g.student_id
                GROUP BY s.id, s.first_name, s.last_name, s.class
                ORDER BY s.last_name, s.first_name
            ";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new StudentWithAvg
                {
                    Id = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Class = reader.GetString(3),
                    Average = Math.Round(reader.GetDouble(4), 2),
                    GradeCount = reader.GetInt32(5)
                });
            }

            return list;
        }

        public static List<Grade> GetGradesForStudent(int studentId)
        {
            var list = new List<Grade>();
            using var connection = GetConnection();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT id, student_id, subject, score, date FROM grades WHERE student_id = @sid ORDER BY date DESC";
            cmd.Parameters.AddWithValue("@sid", studentId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Grade
                {
                    Id = reader.GetInt32(0),
                    StudentId = reader.GetInt32(1),
                    Subject = reader.GetString(2),
                    Score = reader.GetDouble(3),
                    Date = DateTime.Parse(reader.GetString(4))
                });
            }

            return list;
        }

        public static void AddStudent(Student student)
        {
            using var connection = GetConnection();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO students (first_name, last_name, class) VALUES (@fn, @ln, @cl)";
            cmd.Parameters.AddWithValue("@fn", student.FirstName);
            cmd.Parameters.AddWithValue("@ln", student.LastName);
            cmd.Parameters.AddWithValue("@cl", student.Class);
            cmd.ExecuteNonQuery();
        }

        public static void AddGrade(Grade grade)
        {
            using var connection = GetConnection();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO grades (student_id, subject, score, date) VALUES (@sid, @sub, @sc, @dt)";
            cmd.Parameters.AddWithValue("@sid", grade.StudentId);
            cmd.Parameters.AddWithValue("@sub", grade.Subject);
            cmd.Parameters.AddWithValue("@sc", grade.Score);
            cmd.Parameters.AddWithValue("@dt", grade.Date.ToString("yyyy-MM-dd"));
            cmd.ExecuteNonQuery();
        }

        public static void DeleteStudent(int id)
        {
            using var connection = GetConnection();

            // Delete associated grades first
            var cmd1 = connection.CreateCommand();
            cmd1.CommandText = "DELETE FROM grades WHERE student_id = @id";
            cmd1.Parameters.AddWithValue("@id", id);
            cmd1.ExecuteNonQuery();

            // Delete student
            var cmd2 = connection.CreateCommand();
            cmd2.CommandText = "DELETE FROM students WHERE id = @id";
            cmd2.Parameters.AddWithValue("@id", id);
            cmd2.ExecuteNonQuery();
        }

        public static void DeleteGrade(int id)
        {
            using var connection = GetConnection();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM grades WHERE id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
