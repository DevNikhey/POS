using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QuizApp
{
    /// <summary>
    /// Static class holding embedded CSV quiz data and a parser.
    /// Demonstrates: const strings, LINQ, string.Split, CSV parsing.
    /// </summary>
    public static class CsvData
    {
        // Format: question;optionA;optionB;optionC;optionD;correct;category;difficulty
        public const string EmbeddedCsv =
@"Which keyword declares a variable whose value cannot change?;var;let;const;readonly;C;Data Types;Easy
What is the default value of an int in C#?;null;0;1;-1;B;Data Types;Easy
Which type stores true or false?;int;string;bool;char;C;Data Types;Easy
What does 'string' inherit from?;ValueType;Object;Array;Enum;B;Data Types;Medium
Which suffix marks a double literal explicitly?;f;d;m;L;B;Data Types;Medium
What is the size of a 'long' in C#?;32 bit;16 bit;64 bit;128 bit;C;Data Types;Medium
Which loop checks the condition AFTER the first iteration?;for;while;do-while;foreach;C;Loops;Easy
What keyword exits a loop immediately?;continue;return;break;exit;C;Loops;Easy
Which loop is best for iterating over a collection?;for;while;do-while;foreach;D;Loops;Easy
What does 'continue' do inside a loop?;Exits the loop;Skips to the next iteration;Restarts the loop;Throws an exception;B;Loops;Medium
Which LINQ method filters a sequence?;Select;Where;OrderBy;GroupBy;B;LINQ;Easy
What does Select() do in LINQ?;Filters elements;Projects each element;Sorts elements;Groups elements;B;LINQ;Easy
Which LINQ method returns a single element or throws?;First;Single;FirstOrDefault;SingleOrDefault;B;LINQ;Medium
What does OrderByDescending() return?;A filtered list;A grouped list;A sorted sequence (desc);A projected list;C;LINQ;Medium
Which LINQ method counts elements matching a predicate?;Sum;Count;Any;Aggregate;B;LINQ;Medium
What are the four pillars of OOP?;Arrays Lists Stacks Queues;Class Struct Enum Interface;Encapsulation Inheritance Polymorphism Abstraction;Public Private Protected Internal;C;OOP;Easy
Which keyword prevents a class from being inherited?;static;abstract;sealed;partial;C;OOP;Medium
What is an abstract class?;A class with no methods;A class that cannot be instantiated directly;A class with only static members;A class without fields;B;OOP;Medium
Which access modifier makes a member visible only within its class?;public;internal;protected;private;D;OOP;Easy
What keyword is used to implement an interface?;implements;extends;inherits;: (colon);D;OOP;Medium
Which WPF element is used for data binding display?;Label;TextBlock;TextBox;Button;B;WPF;Easy
What does INotifyPropertyChanged enable?;Database access;Automatic UI updates on property changes;File I/O;Network calls;B;WPF;Medium
Which WPF layout panel stacks children vertically or horizontally?;Grid;Canvas;StackPanel;DockPanel;C;WPF;Easy
What is the purpose of a DataContext in WPF?;Stores files;Sets the binding source for a UI element;Creates animations;Handles routing;B;WPF;Medium
Which collection notifies the UI when items are added or removed?;List<T>;ArrayList;ObservableCollection<T>;Dictionary<K,V>;C;Collections;Medium
What interface must be implemented for a for-each loop?;IComparable;IEnumerable;IDisposable;ICloneable;B;Collections;Medium
Which collection stores key-value pairs?;List<T>;Queue<T>;Stack<T>;Dictionary<TKey, TValue>;D;Collections;Easy
What does a Queue<T> follow?;LIFO;FIFO;Random;Priority;B;Collections;Easy
Which block always executes whether or not an exception occurs?;try;catch;finally;throw;C;Exceptions;Easy
What is the base class for all exceptions in C#?;Error;Exception;Throwable;RuntimeException;B;Exceptions;Easy
Which keyword re-throws the current exception preserving the stack trace?;throw ex;throw;rethrow;raise;B;Exceptions;Hard
What happens if an exception is not caught?;Nothing;The program terminates;It is logged automatically;It retries;B;Exceptions;Medium
Which keyword is used to define a lambda expression?;func;delegate;=> (arrow);-> (arrow);C;LINQ;Medium
What does the 'as' keyword do?;Casts and returns null on failure;Casts and throws on failure;Creates an alias;Defines a property;A;Data Types;Hard
What is boxing in C#?;Converting reference to value type;Converting value type to object;Wrapping a class in a struct;Encrypting data;B;Data Types;Hard";

        /// <summary>
        /// Parses the embedded CSV string into a list of QuizQuestion objects.
        /// Demonstrates: Split, LINQ Select, char parsing.
        /// </summary>
        public static List<QuizQuestion> Parse()
        {
            return Parse(EmbeddedCsv);
        }

        /// <summary>
        /// Parses any CSV string (embedded or loaded from file) into quiz questions.
        /// </summary>
        public static List<QuizQuestion> Parse(string csvText)
        {
            return csvText
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line =>
                {
                    string[] parts = line.Split(';');
                    if (parts.Length < 8)
                        return null;

                    return new QuizQuestion
                    {
                        Question = parts[0].Trim(),
                        OptionA = parts[1].Trim(),
                        OptionB = parts[2].Trim(),
                        OptionC = parts[3].Trim(),
                        OptionD = parts[4].Trim(),
                        CorrectAnswer = parts[5].Trim().ToUpper()[0],
                        Category = parts[6].Trim(),
                        Difficulty = parts[7].Trim()
                    };
                })
                .Where(q => q != null)
                .Select(q => q!)   // null-forgiving after filter
                .ToList();
        }
    }
}
