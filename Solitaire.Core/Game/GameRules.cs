using Nclan.Ac.RobertS.Solitaire.Core.Enums;
using Nclan.Ac.RobertS.Solitaire.Core.Models;

namespace Nclan.Ac.RobertS.Solitaire.Core.Game
{
    /// <summary>
    /// Provides static methods for evaluating card placement rules in a solitaire game.
    /// </summary>
    /// <remarks>The GameRules class defines logic for determining whether a card can be placed on a
    /// foundation or tableau pile according to standard solitaire rules. All methods are thread-safe and do not modify
    /// game state.</remarks>
    public static class GameRules
    {
        /// <summary>
        /// Determines whether the specified card can be placed onto the target foundation card according to standard
        /// solitaire rules.
        /// </summary>
        /// <remarks>A card can be placed on an empty foundation only if it is an Ace. For a non-empty
        /// foundation, the moving card must be of the same suit and have a value exactly one higher than the target
        /// card.</remarks>
        /// <param name="moving">The card to be moved onto the foundation. Must not be null.</param>
        /// <param name="target">The current top card of the foundation, or null if the foundation is empty.</param>
        /// <returns>true if the moving card can be placed onto the foundation; otherwise, false.</returns>
        public static bool CanPlaceOnFoundation(Card moving, Card target)
        {
            // Empty foundation → only Ace allowed
            if (target == null)
                return moving.Value == 1;

            // Same suit ascending
            return moving.Suit == target.Suit &&
                   moving.Value == target.Value + 1;
        }

        public static bool CanPlaceOnTableau(Card moving, Card target)
        {
            // Empty column → only King allowed
            if (target == null)
                return moving.Value == 13;

            bool oppositeColor =
                moving.Color != target.Color;

            bool correctOrder =
                moving.Value == target.Value - 1;

            return oppositeColor && correctOrder;
        }
    }
}