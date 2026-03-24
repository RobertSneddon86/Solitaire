using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nclan.Ac.RobertS.Solitaire.Core.Models
{
    /// <summary>
    /// Represents a player in the game, including account information and gameplay statistics.
    /// </summary>
    /// <remarks>The Player class stores user credentials and tracks various metrics related to gameplay, such
    /// as the number of games played, won, and lost, as well as performance statistics like best score and best time.
    /// This class can be used to manage player profiles and monitor progress within the game.</remarks>
    public class Player
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int Age { get; set; }

        public int Balance { get; set; }

        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public int GamesLost { get; set; }

        public int TotalMoves { get; set; }
        public int BestScore { get; set; }
        public int BestTime { get; set; }
    }
}
