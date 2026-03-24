using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nclan.Ac.RobertS.Solitaire.Core.Models
{
    /// <summary>
    /// Represents a collection of playing cards arranged in a stack, where cards can be added or removed from the top.
    /// </summary>
    /// <remarks>A pile maintains the order of cards, with the most recently added card considered the top
    /// card. This class is commonly used in card games to model stacks such as discard piles or draw piles.</remarks>
    public class Pile
    {
        public List<Card> Cards { get; }

        public Pile()
        {
            Cards = new List<Card>();
        }

        public Card TopCard =>
            Cards.Count > 0 ? Cards[Cards.Count - 1] : null;

        public void AddCard(Card card)
        {
            Cards.Add(card);
        }

        public Card RemoveTopCard()
        {
            if (Cards.Count == 0)
                return null;

            Card card = TopCard;
            Cards.RemoveAt(Cards.Count - 1);
            return card;
        }
    }
}
