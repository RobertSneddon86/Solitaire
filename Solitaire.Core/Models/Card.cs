using Nclan.Ac.RobertS.Solitaire.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nclan.Ac.RobertS.Solitaire.Core.Models
{
    /// <summary>
    /// Represents a playing card with a suit, value, color, and face orientation.
    /// </summary>
    /// <remarks>A Card instance models a standard playing card, supporting both face-up and face-down states.
    /// The card's color is determined by its suit, and the value typically ranges from 1 (Ace) to 13 (King). The class
    /// provides properties to access the card's suit, value, color, and image path for UI display. Card objects are
    /// immutable except for the IsFaceUp property, which can be toggled to reflect the card's orientation.</remarks>
    public class Card
    {
        public Suit Suit { get; }
        public int Value { get; }
        public bool IsFaceUp { get; set; }

        public CardColor Color =>
            (Suit == Suit.Hearts || Suit == Suit.Diamonds)
            ? CardColor.Red
            : CardColor.Black;
        public string ImagePath
        {
            get
            {
                if (!IsFaceUp)
                    return "pack://application:,,,/Resources/Images/CardDeck/card back red.png";

                string valueName = Value switch
                {
                    1 => "Ace",
                    11 => "Jack",
                    12 => "Queen",
                    13 => "King",
                    _ => Value.ToString()
                };

                return $"pack://application:,,,/Resources/Images/CardDeck/{valueName}.{Suit}.png";
            }
        }

        /// <summary>
        /// Initializes a new instance of the Card class with the specified suit and value, and sets the card face down.
        /// </summary>
        /// <remarks>The card is created face down by default. To reveal the card, set the IsFaceUp
        /// property to <see langword="true"/>.</remarks>
        /// <param name="suit">The suit of the card. Must be a valid value from the Suit enumeration.</param>
        /// <param name="value">The numeric value of the card. Typically ranges from 1 (Ace) to 13 (King), depending on the game rules.</param>
        public Card(Suit suit, int value)
        {
            Suit = suit;
            Value = value;
            IsFaceUp = false;
        }
    }
}
