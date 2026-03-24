using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nclan.Ac.RobertS.Solitaire.Core.Models;

namespace Nclan.Ac.RobertS.Solitaire.Core.Persistence
{
    /// <summary>
    /// Provides methods for storing and retrieving player data using a CSV file as the backing store.
    /// </summary>
    /// <remarks>The PlayerRepository manages player records by reading from and writing to a local CSV file.
    /// It allows updating existing player information or adding new players, and retrieving player details by username.
    /// This class is not thread-safe; concurrent access may result in data loss or corruption. All player data is
    /// persisted in the file named "players.csv" located in the application's working directory.</remarks>
    public class PlayerRepository
    {
        private readonly string _filePath = "players.csv";

        /// <summary>
        /// Saves the specified player's data to the persistent storage. Updates the existing record if the player
        /// already exists; otherwise, adds a new record.
        /// </summary>
        /// <remarks>If a record for the player already exists, it will be overwritten with the new data.
        /// Otherwise, a new record will be created. The data is stored in a comma-separated format. This method is not
        /// thread-safe; concurrent calls may result in data loss or corruption.</remarks>
        /// <param name="player">The player whose data is to be saved. Cannot be null. The player's properties are used to populate the
        /// record.</param>
        public void Save(Player player)
        {
            List<string> lines = new List<string>();

            if (File.Exists(_filePath))
                lines = File.ReadAllLines(_filePath).ToList();

            bool updated = false;

            for (int i = 0; i < lines.Count; i++)
            {
                var parts = lines[i].Split(',');

                if (parts[0] == player.Username)
                {
                    lines[i] =
                        $"{player.Username},{player.Password},{player.Age},{player.Balance},{player.GamesPlayed},{player.GamesWon},{player.GamesLost},{player.TotalMoves},{player.BestScore},{player.BestTime}";
                    updated = true;
                    break;
                }
            }

            if (!updated)
            {
                lines.Add(
                    $"{player.Username},{player.Password},{player.Age},{player.Balance},{player.GamesPlayed},{player.GamesWon},{player.GamesLost},{player.TotalMoves},{player.BestScore},{player.BestTime}");
            }

            File.WriteAllLines(_filePath, lines);
        }

        /// <summary>
        /// Retrieves the player information associated with the specified username.
        /// </summary>
        /// <remarks>If the underlying data file does not exist or no player with the specified username
        /// is found, the method returns <see langword="null"/>. The search is case-sensitive and only the first
        /// matching entry is returned.</remarks>
        /// <param name="username">The username of the player to retrieve. Cannot be null or empty.</param>
        /// <returns>A <see cref="Player"/> object containing the player's details if a matching username is found; otherwise,
        /// <see langword="null"/>.</returns>
        public Player? GetByUsername(string username)
        {
            if (!File.Exists(_filePath))
                return null;

            var lines = File.ReadAllLines(_filePath);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(',');

                if (parts[0] == username)
                {
                    return new Player
                    {
                        Username = parts[0],
                        Password = parts[1],
                        Age = int.Parse(parts[2]),
                        Balance = int.Parse(parts[3]),
                        GamesPlayed = int.Parse(parts[4]),
                        GamesWon = int.Parse(parts[5]),
                        GamesLost = int.Parse(parts[6]),
                        TotalMoves = int.Parse(parts[7]),
                        BestScore = int.Parse(parts[8]),
                        BestTime = int.Parse(parts[9])
                    };
                }
            }

            return null;
        }
    }
}