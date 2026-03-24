using Nclan.Ac.RobertS.Solitaire.Core.Enums;
using Nclan.Ac.RobertS.Solitaire.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nclan.Ac.RobertS.Solitaire.Models
{
    /// <summary>
    /// Represents a standard deck of playing cards that can be shuffled and drawn from.
    /// </summary>
    /// <remarks>The deck is initialized with 52 cards, one for each combination of suit and value in a
    /// standard playing card set. The deck supports shuffling to randomize the order of cards and drawing cards one at
    /// a time from the top. This class is not thread-safe; concurrent access should be synchronized if used in
    /// multithreaded scenarios.</remarks>
    public class Deck
    {
        private readonly List<Card> _cards;
        private readonly Random _random;

        public Deck()
        {
            _random = new Random();
            _cards = new List<Card>();

            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                for (int value = 1; value <= 13; value++)
                {
                    _cards.Add(new Card(suit, value));
                }
            }
        }

        public void Shuffle()
        {
            _cards.Sort((a, b) => _random.Next(-1, 2));
        }

        public Card Draw()
        {
            if (_cards.Count == 0)
                return null;

            Card card = _cards[0];
            _cards.RemoveAt(0);
            return card;
        }
    }
}