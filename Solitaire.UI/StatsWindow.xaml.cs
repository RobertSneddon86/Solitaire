using System;
using System.Windows;
using System.Windows.Media;
using Nclan.Ac.RobertS.Solitaire.Core.Models;

namespace Nclan.Ac.RobertS.Solitaire.UI
{
    public partial class StatsWindow : Window
    {
        private Player _player;
        private GameWindow _gameWindow;

        public StatsWindow(Player player, GameWindow gameWindow)
        {
            InitializeComponent();

            _player = player;
            _gameWindow = gameWindow;

            PlayerPerformanceText.Text = $"{_player.Username}'s Performance";

            // Hide achievements for guest players
            if (_player.Username == "Guest")
            {
                AchievementsSection.Visibility = Visibility.Collapsed;
            }

            LoadStats();
            LockAchievements();
            UpdateAchievements();
        }

        /// <summary>
        /// Handles the Back button click event by displaying the game window and closing the current window.
        /// </summary>
        /// <param name="sender">The source of the event, typically the Back button control.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _gameWindow.Show();
            this.Close();
        }

        /// <summary>
        /// Updates the user interface with the current statistics and performance metrics for the player.
        /// </summary>
        /// <remarks>This method displays values such as games played, games won, games lost, total moves,
        /// balance, best score, best time, win rate, loss rate, and average moves. If the player is under 18 years old
        /// or has the username "Guest", the balance display is visually subdued to indicate restricted access. The
        /// method should be called whenever player statistics need to be refreshed in the UI.</remarks>
        private void LoadStats()
        {
            GamesPlayedText.Text = _player.GamesPlayed.ToString();
            GamesWonText.Text = _player.GamesWon.ToString();
            GamesLostText.Text = _player.GamesLost.ToString();
            TotalMovesText.Text = _player.TotalMoves.ToString();
            BalanceText.Text = $"£{_player.Balance}";
            BestScoreText.Text = _player.BestScore.ToString();

            if (_player.Age < 18 || _player.Username == "Guest")
            {
                BalanceText.Foreground = Brushes.Gray;
                BalanceText.Opacity = 0.5;
            }

            // Convert seconds to mm:ss
            BestTimeText.Text = TimeSpan
                .FromSeconds(_player.BestTime)
                .ToString(@"m\:ss");

            double winRate = 0;

            if (_player.GamesPlayed > 0)
                winRate = (double)_player.GamesWon / _player.GamesPlayed * 100;

            WinRateText.Text = $"{winRate:0.0}%";
            LossRateText.Text = $"{100 - winRate:0.0}%";

            WinRateBar.Value = winRate;

            AverageMovesText.Text = _player.GamesPlayed > 0
                ? ((double)_player.TotalMoves / _player.GamesPlayed).ToString("0.0")
                : "0";
        }

        /// <summary>
        /// Updates the achievement indicators based on the player's game and win statistics.
        /// </summary>
        /// <remarks>This method visually updates achievement borders to reflect milestones such as first
        /// game played, first win, ten wins, and achieving a win rate of at least 50 percent. It should be called after
        /// player statistics are modified to ensure the UI accurately represents the player's progress.</remarks>
        private void UpdateAchievements()
        {
            if (_player.GamesPlayed > 0)
            {
                FirstGameBorder.Opacity = 1;
                FirstGameBorder.Background = Brushes.LightGreen;
            }

            if (_player.GamesWon > 0)
            {
                FirstWinBorder.Opacity = 1;
                FirstWinBorder.Background = Brushes.Gold;
            }

            if (_player.GamesWon >= 10)
            {
                TenWinsBorder.Opacity = 1;
                TenWinsBorder.Background = Brushes.Gold;
            }

            if (_player.GamesPlayed > 0)
            {
                double winRate = (double)_player.GamesWon / _player.GamesPlayed * 100;

                if (winRate >= 50)
                {
                    FiftyWinBorder.Opacity = 1;
                    FiftyWinBorder.Background = Brushes.LightBlue;
                }
            }
        }

        /// <summary>
        /// Locks all achievement borders by setting their opacity to indicate that the achievements are unavailable.
        /// </summary>
        /// <remarks>Use this method to visually disable achievement indicators when achievements have not
        /// been earned or are inaccessible. This affects the appearance of the achievement borders, making them appear
        /// dimmed to the user.</remarks>
        private void LockAchievements()
        {
            FirstGameBorder.Opacity = 0.4;
            FirstWinBorder.Opacity = 0.4;
            TenWinsBorder.Opacity = 0.4;
            FiftyWinBorder.Opacity = 0.4;
        }
    }
}