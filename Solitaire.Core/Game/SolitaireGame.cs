using Nclan.Ac.RobertS.Solitaire.Core.Enums;
using Nclan.Ac.RobertS.Solitaire.Core.Models;
using Nclan.Ac.RobertS.Solitaire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nclan.Ac.RobertS.Solitaire.Core.Game
{
    /// <summary>
    /// Represents a game of Solitaire, managing the state, piles, and gameplay actions according to the selected
    /// difficulty level.
    /// </summary>
    /// <remarks>The SolitaireGame class provides the core structure and operations for a standard Solitaire
    /// game, including card movement, drawing from stock, and win condition checking. It exposes collections for
    /// tableau columns, foundations, stock, and waste piles, allowing interaction with the game's state. The difficulty
    /// level determines how many cards are drawn from the stock at a time. This class is not thread-safe; concurrent
    /// access should be externally synchronized if needed.</remarks>
    public class SolitaireGame
    {
        public List<Pile> Columns { get; }
        public Pile Stock { get; }
        public Pile Waste { get; }
        public List<Pile> Foundations { get; }

        public DifficultyLevel Difficulty { get; }

        private Deck _deck = null;

        /// <summary>
        /// Initializes a new instance of the SolitaireGame class with the specified difficulty level.
        /// </summary>
        /// <remarks>The constructor sets up the initial state of the game, including creating piles for
        /// columns, foundations, stock, and waste. The difficulty level influences how the game is initialized and
        /// played.</remarks>
        /// <param name="difficulty">The difficulty setting for the game. Determines the complexity and rules applied during gameplay.</param>
        public SolitaireGame(DifficultyLevel difficulty)
        {
            Difficulty = difficulty;
            Columns = new List<Pile>();
            Foundations = new List<Pile>();
            Stock = new Pile();
            Waste = new Pile();
            for (int i = 0; i < 4; i++)
                Foundations.Add(new Pile());

            InitialiseGame();
        }

        /// <summary>
        /// Initializes the game by setting up the deck, shuffling it, and distributing cards to columns and the stock
        /// pile.
        /// </summary>
        /// <remarks>Call this method at the start of a new game to ensure all piles are properly
        /// configured. This method resets the deck and columns, and prepares the stock for gameplay. It should not be
        /// called during an active game, as it will overwrite the current state.</remarks>
        private void InitialiseGame()
        {
            _deck = new Deck();
            _deck.Shuffle();

            for (int i = 0; i < 7; i++)
            {
                Pile column = new Pile();

                for (int j = 0; j <= i; j++)
                {
                    Card card = _deck.Draw();
                    if (j == i)
                        card.IsFaceUp = true;

                    column.AddCard(card);
                }

                Columns.Add(column);
            }

            Card remaining;
            while ((remaining = _deck.Draw()) != null)
            {
                Stock.AddCard(remaining);
            }
        }

        /// <summary>
        /// Draws a number of cards from the stock pile to the waste pile, turning each card face up. The number of
        /// cards drawn is determined by the current difficulty setting.
        /// </summary>
        /// <remarks>If the stock pile contains fewer cards than the number specified by the difficulty,
        /// only the available cards will be drawn. Each drawn card is added to the waste pile and is set to face up.
        /// This method does not throw exceptions if the stock pile is empty.</remarks>
        public void DrawFromStock()
        {
            int drawCount = (int)Difficulty;

            for (int i = 0; i < drawCount; i++)
            {
                Card card = Stock.RemoveTopCard();
                if (card == null)
                    break;

                card.IsFaceUp = true;
                Waste.AddCard(card);
            }
        }

        /// <summary>
        /// Attempts to move the top card from one pile to another according to game rules.
        /// </summary>
        /// <remarks>If the move is successful, the top card of the source pile is turned face up if it
        /// exists. The validity of the move is determined by the game rules for foundation and tableau piles.</remarks>
        /// <param name="from">The pile from which the top card will be moved. Must contain at least one card.</param>
        /// <param name="to">The pile to which the card will be moved. The move must be valid according to game rules for the target
        /// pile.</param>
        /// <returns>true if the card was successfully moved; otherwise, false.</returns>
        public bool MoveCard(Pile from, Pile to)
        {
            Card moving = from.TopCard;
            Card target = to.TopCard;

            bool validMove = false;

            // moving to a foundation pile
            if (Foundations.Contains(to))
                validMove = GameRules.CanPlaceOnFoundation(moving, target);
            else
                validMove = GameRules.CanPlaceOnTableau(moving, target);

            if (!validMove)
                return false;

            to.AddCard(from.RemoveTopCard());

            if (from.TopCard != null)
                from.TopCard.IsFaceUp = true;

            return true;
        }

        /// <summary>
        /// Determines whether the player has successfully completed the game by verifying all foundations are complete.
        /// </summary>
        /// <remarks>This method is typically used to check for a win condition in solitaire-style card
        /// games. It assumes that a foundation is considered complete when its top card is a King. If any foundation is
        /// incomplete or empty, the method returns false.</remarks>
        /// <returns>true if all foundation piles contain a King as their top card; otherwise, false.</returns>
        public bool HasPlayerWon()
        {
            foreach (var foundation in Foundations)
            {
                if (foundation.TopCard == null)
                    return false;

                if (foundation.TopCard.Value != 13)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Attempts to move a stack of cards from the specified pile to another pile, starting at the given index.
        /// Returns a value indicating whether the move was successful.
        /// </summary>
        /// <remarks>The move is only performed if the stack can legally be placed on the target pile
        /// according to game rules. If the move is successful, the top card of the source pile is turned face up if it
        /// exists.</remarks>
        /// <param name="from">The pile from which the stack of cards will be moved. Must contain enough cards to allow the move starting
        /// at the specified index.</param>
        /// <param name="to">The pile to which the stack of cards will be moved. The move is subject to game rules for placing cards on
        /// this pile.</param>
        /// <param name="startIndex">The zero-based index in the source pile at which the stack begins. Must be greater than or equal to 0 and
        /// less than the number of cards in the source pile.</param>
        /// <returns>true if the stack was moved successfully according to game rules; otherwise, false.</returns>
        public bool MoveStack(Pile from, Pile to, int startIndex)
        {
            if (startIndex < 0 || startIndex >= from.Cards.Count)
                return false;

            Card movingCard = from.Cards[startIndex];
            Card target = to.TopCard;

            if (!GameRules.CanPlaceOnTableau(movingCard, target))
                return false;

            var stack = from.Cards.GetRange(startIndex, from.Cards.Count - startIndex);
            from.Cards.RemoveRange(startIndex, stack.Count);

            foreach (var card in stack)
                to.AddCard(card);

            if (from.TopCard != null)
                from.TopCard.IsFaceUp = true;

            return true;
        }

        /// <summary>
        /// Moves all cards from the waste pile back to the stock pile, resetting their face orientation.
        /// </summary>
        /// <remarks>After calling this method, the waste pile will be empty and all cards previously in
        /// the waste will be added to the stock pile face down. This operation is typically used to recycle cards
        /// during gameplay. The method does not affect cards in other piles.</remarks>
        public void ResetStockFromWaste()
        {
            while (Waste.Cards.Count > 0)
            {
                Card card = Waste.RemoveTopCard();
                card.IsFaceUp = false;
                Stock.AddCard(card);
            }
        }
    }
}
