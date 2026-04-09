using System;

namespace QuizApp
{
    /// <summary>
    /// Represents a single quiz question with four answer options.
    /// Demonstrates: auto-properties, value types (char), string properties.
    /// </summary>
    public class QuizQuestion
    {
        public string Question { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public char CorrectAnswer { get; set; }   // 'A', 'B', 'C', or 'D'
        public string Category { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty; // Easy, Medium, Hard
    }
}
