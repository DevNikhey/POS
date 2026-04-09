using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Hangman
{
    public class HangmanModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private static readonly string[] Words =
        {
            "PROGRAMMIERUNG", "ALGORITHMUS", "VARIABLE", "SCHLEIFE", "METHODE",
            "VERERBUNG", "INTERFACE", "ABSTRAKT", "COMPILER", "DEBUGGER",
            "EXCEPTION", "GENERICS", "POLYMORPHIE", "KAPSELUNG", "REFERENZ",
            "NAMESPACE", "KONSTRUKTOR", "DESTRUKTOR", "ITERATOR", "DELEGATE"
        };

        private static readonly Random Rng = new Random();

        public const int MaxWrong = 7;

        private string _targetWord = string.Empty;
        private HashSet<char> _guessed = new HashSet<char>();
        private int _wrongCount;

        public string TargetWord
        {
            get => _targetWord;
            private set { _targetWord = value; OnPropertyChanged(); }
        }

        public HashSet<char> Guessed => _guessed;

        public int WrongCount
        {
            get => _wrongCount;
            private set { _wrongCount = value; OnPropertyChanged(); }
        }

        public bool GameOver => Won || WrongCount >= MaxWrong;

        public bool Won => !string.IsNullOrEmpty(TargetWord) &&
                           TargetWord.All(c => _guessed.Contains(c));

        public string MaskedWord
        {
            get
            {
                if (string.IsNullOrEmpty(TargetWord))
                    return string.Empty;

                char[] masked = new char[TargetWord.Length];
                for (int i = 0; i < TargetWord.Length; i++)
                {
                    masked[i] = _guessed.Contains(TargetWord[i]) ? TargetWord[i] : '_';
                }
                return new string(masked);
            }
        }

        public string DisplayWord
        {
            get
            {
                string masked = MaskedWord;
                if (string.IsNullOrEmpty(masked))
                    return string.Empty;
                return string.Join(" ", masked.ToCharArray());
            }
        }

        public string StatusText
        {
            get
            {
                if (Won)
                    return "Gewonnen! Das Wort war: " + TargetWord;
                if (WrongCount >= MaxWrong)
                    return "Verloren! Das Wort war: " + TargetWord;
                return "Fehlversuche: " + WrongCount + " / " + MaxWrong;
            }
        }

        public string AsciiArt
        {
            get
            {
                return _wrongCount switch
                {
                    0 => "  ┌───┐\n  │   \n  │   \n  │   \n  │   \n──┴──",
                    1 => "  ┌───┐\n  │   O\n  │   \n  │   \n  │   \n──┴──",
                    2 => "  ┌───┐\n  │   O\n  │   │\n  │   \n  │   \n──┴──",
                    3 => "  ┌───┐\n  │   O\n  │  /│\n  │   \n  │   \n──┴──",
                    4 => "  ┌───┐\n  │   O\n  │  /│\\\n  │   \n  │   \n──┴──",
                    5 => "  ┌───┐\n  │   O\n  │  /│\\\n  │  / \n  │   \n──┴──",
                    6 => "  ┌───┐\n  │   O\n  │  /│\\\n  │  / \\\n  │   \n──┴──",
                    _ => "  ┌───┐\n  │   O\n  │  /│\\\n  │  / \\\n  │   \n──┴──",
                };
            }
        }

        public HangmanModel()
        {
            NewGame();
        }

        public void NewGame()
        {
            _guessed = new HashSet<char>();
            WrongCount = 0;
            TargetWord = Words[Rng.Next(Words.Length)];
            NotifyAll();
        }

        public void Guess(char letter)
        {
            letter = char.ToUpper(letter);

            if (GameOver || _guessed.Contains(letter))
                return;

            _guessed.Add(letter);

            if (!TargetWord.Contains(letter))
            {
                WrongCount++;
            }

            NotifyAll();
        }

        private void NotifyAll()
        {
            OnPropertyChanged(nameof(MaskedWord));
            OnPropertyChanged(nameof(DisplayWord));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(AsciiArt));
            OnPropertyChanged(nameof(GameOver));
            OnPropertyChanged(nameof(Won));
            OnPropertyChanged(nameof(Guessed));
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
