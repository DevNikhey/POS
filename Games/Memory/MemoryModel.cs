using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Memory
{
    public class MemoryModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private int _moves;
        private int _matchedPairs;
        private Card? _firstCard;
        private bool _checking;
        private readonly Random _random = new Random();

        public ObservableCollection<Card> Cards { get; set; } = new ObservableCollection<Card>();

        public int Columns => 4;

        public int Moves
        {
            get => _moves;
            set { _moves = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); }
        }

        public int MatchedPairs
        {
            get => _matchedPairs;
            set { _matchedPairs = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); OnPropertyChanged(nameof(IsWon)); }
        }

        public bool IsWon => MatchedPairs == 8;

        public string StatusText => IsWon
            ? $"You won in {Moves} moves!"
            : $"Moves: {Moves}  |  Pairs found: {MatchedPairs} / 8";

        public MemoryModel()
        {
            NewGame();
        }

        public void NewGame()
        {
            Cards.Clear();
            _firstCard = null;
            _checking = false;
            Moves = 0;
            MatchedPairs = 0;

            string[] symbols = { "A", "B", "C", "D", "E", "F", "G", "H" };
            List<Card> deck = new List<Card>();

            for (int i = 0; i < symbols.Length; i++)
            {
                deck.Add(new Card { PairId = i, Symbol = symbols[i] });
                deck.Add(new Card { PairId = i, Symbol = symbols[i] });
            }

            // Fisher-Yates shuffle
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                Card temp = deck[i];
                deck[i] = deck[j];
                deck[j] = temp;
            }

            foreach (Card card in deck)
            {
                Cards.Add(card);
            }
        }

        public async Task FlipCard(Card card)
        {
            if (_checking) return;
            if (card.FaceUp || card.Matched) return;

            card.FaceUp = true;

            if (_firstCard == null)
            {
                _firstCard = card;
                return;
            }

            // Second card flipped
            Moves++;
            Card first = _firstCard;
            _firstCard = null;

            if (first.PairId == card.PairId)
            {
                first.Matched = true;
                card.Matched = true;
                MatchedPairs++;
            }
            else
            {
                _checking = true;
                await Task.Delay(800);
                first.FaceUp = false;
                card.FaceUp = false;
                _checking = false;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
