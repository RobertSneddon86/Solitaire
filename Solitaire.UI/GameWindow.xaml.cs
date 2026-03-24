using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Nclan.Ac.RobertS.Solitaire.Core.Enums;
using Nclan.Ac.RobertS.Solitaire.Core.Game;
using Nclan.Ac.RobertS.Solitaire.Core.Models;
using Nclan.Ac.RobertS.Solitaire.Core.Persistence;

namespace Nclan.Ac.RobertS.Solitaire.UI
{
    public partial class GameWindow : Window
    {
        private Image _selectedImage = null;
        private Player _player;
        private SolitaireGame _game;
        private Pile _selectedPile = null;
        private Card _selectedCard = null;
        private int _selectedIndex = -1;
        private DispatcherTimer _timer;
        private int _secondsElapsed = 0;
        private int _moves = 0;
        private int _score = 0;
        private bool _vegasEntryPaid = false;
        private bool _hasSaved = false;
        private DifficultyLevel _pendingDifficulty;
        private bool _pendingVegasMode;
        private bool _isVegasModeActive = false;

        public GameWindow(Player player, DifficultyLevel difficulty)
        {
            InitializeComponent();

            _player = player;
            _game = new SolitaireGame(difficulty);
            _player.GamesPlayed++;
            VegasModeCheck.IsChecked = false;
            VegasModeCheck.IsEnabled = true;
            _pendingDifficulty = difficulty;
            _pendingVegasMode = false;

            if (_player.Username == "Guest" || _player.Age < 18)
            {
                VegasModeCheck.IsEnabled = false;
            }

            WelcomeTextBlock.Text = $"Welcome, {_player.Username}! ({difficulty})";
            UpdateBalanceUI();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            RenderTableau();
            RenderStockAndWaste();
            RenderFoundations();
        }

        /// <summary>
        /// Generates the resource path for the image representing the specified playing card.
        /// </summary>
        /// <remarks>The returned path follows the format
        /// 'pack://application:,,,/Resources/Images/CardDeck/{Value}.{Suit}.png', where {Value} is the card's face name
        /// (e.g., Ace, Jack, Queen, King) or numeric value, and {Suit} is the card's suit. Ensure that the
        /// corresponding image files exist at the specified location for correct display.</remarks>
        /// <param name="card">The card for which to generate the image path. Must not be null.</param>
        /// <returns>A string containing the URI path to the card's image resource.</returns>
        private string GetCardImagePath(Card card)
        {
            string valueName = card.Value switch
            {
                1 => "Ace",
                11 => "Jack",
                12 => "Queen",
                13 => "King",
                _ => card.Value.ToString()
            };

            return $"pack://application:,,,/Resources/Images/CardDeck/{valueName}.{card.Suit}.png";
        }

        /// <summary>
        /// Handles the timer's Tick event to update the displayed elapsed time.
        /// </summary>
        /// <remarks>This method increments the elapsed time counter and updates the time display in
        /// minutes and seconds. It is intended to be used as an event handler for a timer control.</remarks>
        /// <param name="sender">The source of the event, typically the timer instance that triggered the Tick event.</param>
        /// <param name="e">The event data associated with the Tick event. This parameter is not used.</param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            _secondsElapsed++;

            int minutes = _secondsElapsed / 60;
            int seconds = _secondsElapsed % 60;

            TimeTextBlock.Text = $"{minutes}:{seconds:D2}";
        }

        /// <summary>
        /// Renders the current state of the tableau by displaying all columns and their cards in the user interface.
        /// </summary>
        /// <remarks>This method updates the visual layout of the tableau grid, clearing any existing
        /// elements and recreating the columns and cards based on the game's current state. It should be called
        /// whenever the tableau needs to be refreshed, such as after a move or game event. The method is not
        /// thread-safe and should be invoked from the UI thread.</remarks>
        private void RenderTableau()
        {
            TableauGrid.Children.Clear();

            foreach (var column in _game.Columns)
            {
                Canvas columnPanel = new Canvas
                {
                    Width = 80,
                    Height = 520,
                    Margin = new Thickness(6, 0, 6, 0),
                    Background = Brushes.Transparent
                };

                columnPanel.MouseDown += EmptyColumn_Click;
                columnPanel.Tag = column;

                int offset = 0;
                int index = 0;

                foreach (var card in column.Cards)
                {
                    Image cardImage = new Image
                    {
                        Width = 80,
                        Height = 120
                    };

                    cardImage.MouseDown += Card_Click;

                    // Store pile + index
                    cardImage.Tag = new Tuple<Pile, int>(column, index);

                    string imagePath;

                    if (card.IsFaceUp)
                        imagePath = GetCardImagePath(card);
                    else
                        imagePath = "pack://application:,,,/Resources/Images/CardDeck/back.png";

                    cardImage.Source = new BitmapImage(new Uri(imagePath));

                    Canvas.SetTop(cardImage, offset);

                    offset += card.IsFaceUp ? 18 : 8;

                    columnPanel.Children.Add(cardImage);

                    index++;
                }

                TableauGrid.Children.Add(columnPanel);
            }
        }

        /// <summary>
        /// Handles the mouse click event for an empty column in the tableau, allowing the selected card stack to be
        /// moved if the column is empty.
        /// </summary>
        /// <remarks>This method only allows moves to columns that are currently empty. If the move is
        /// successful, the game state and UI are updated accordingly. The method is intended to be used as an event
        /// handler for mouse clicks on empty columns in a card game interface.</remarks>
        /// <param name="sender">The source of the event, expected to be a Canvas representing the target column.</param>
        /// <param name="e">The mouse button event data associated with the click.</param>
        private void EmptyColumn_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_selectedCard == null)
                return;

            Canvas columnPanel = sender as Canvas;
            Pile targetPile = columnPanel.Tag as Pile;

            // Only allow move if column is empty
            if (targetPile.Cards.Count != 0)
                return;

            bool moved = _game.MoveStack(_selectedPile, targetPile, _selectedIndex);

            if (moved)
            {
                _moves++;
                _score += 5;

                if (_isVegasModeActive)
                {
                    _player.Balance += 5;
                    UpdateBalanceUI();
                }

                MovesTextBlock.Text = _moves.ToString();
                ScoreTextBlock.Text = _score.ToString();

                RenderTableau();
                RenderStockAndWaste();
                RenderFoundations();
                CheckForWin();
            }

            _selectedCard = null;
            _selectedPile = null;
            _selectedImage = null;
            _selectedIndex = -1;
        }

        /// <summary>
        /// Updates the visual representation of the stock and waste piles, including their card images and counts.
        /// </summary>
        /// <remarks>This method should be called whenever the state of the stock or waste piles changes
        /// to ensure the UI reflects the current game state. The stock pile displays a card back image if it contains
        /// cards, while the waste pile displays the top card's image and enables interaction. Card counts are updated
        /// for both piles.</remarks>
        private void RenderStockAndWaste()
        {
            StockBorder.Child = null;
            WasteBorder.Child = null;

            if (_game.Stock.TopCard != null)
            {
                Image backImage = new Image
                {
                    Width = 80,
                    Height = 120,
                    Source = new BitmapImage(
                        new Uri("pack://application:,,,/Resources/Images/CardDeck/back.png"))
                };

                StockBorder.Child = backImage;
            }

            if (_game.Waste.TopCard != null)
            {
                Card card = _game.Waste.TopCard;

                Image cardImage = new Image
                {
                    Width = 80,
                    Height = 120,
                    Source = new BitmapImage(new Uri(GetCardImagePath(card)))
                };

                cardImage.MouseDown += WasteCard_Click;
                cardImage.Tag = card;

                WasteBorder.Child = cardImage;
            }

            StockCountText.Text = _game.Stock.Cards.Count.ToString();
            WasteCountText.Text = _game.Waste.Cards.Count.ToString();
        }

        /// <summary>
        /// Handles the mouse click event on the stock pile, drawing a card from the stock or resetting the stock from
        /// the waste as appropriate.
        /// </summary>
        /// <remarks>This method updates the stock and waste piles in response to user interaction. It
        /// should be connected to the stock border's mouse click event handler in the UI. The method ensures the stock
        /// pile is replenished from the waste when empty, or draws a card otherwise.</remarks>
        /// <param name="sender">The source of the event, typically the stock border UI element.</param>
        /// <param name="e">The event data associated with the mouse button click.</param>
        private void StockBorder_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_game.Stock.Cards.Count == 0)
            {
                _game.ResetStockFromWaste();
            }
            else
            {
                _game.DrawFromStock();
            }

            RenderStockAndWaste();
        }

        /// <summary>
        /// Handles the click event for a card image in the game tableau, managing card selection and move attempts
        /// based on user interaction.
        /// </summary>
        /// <remarks>This method supports both card selection and move operations. On the first click, it
        /// selects a face-up card; on the second click, it attempts to move the selected stack to the target pile.
        /// Clicking the same card again will deselect it. The method updates game state, score, and UI elements as
        /// appropriate. Only face-up cards can be selected.</remarks>
        /// <param name="sender">The source of the event, expected to be an Image representing a card in the tableau.</param>
        /// <param name="e">The event data associated with the mouse button click.</param>
        private void Card_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Image clickedImage = sender as Image;
            if (clickedImage == null)
                return;

            var data = (Tuple<Pile, int>)clickedImage.Tag;

            Pile clickedPile = data.Item1;
            int cardIndex = data.Item2;
            Card clickedCard = clickedPile.Cards[cardIndex];

            // Clicking same card = deselect
            if (_selectedCard == clickedCard)
            {
                ClearSelection();
                return;
            }

            // FIRST CLICK (select card)
            if (_selectedCard == null)
            {
                if (!clickedCard.IsFaceUp)
                    return;

                ClearSelection();

                _selectedPile = clickedPile;
                _selectedCard = clickedCard;
                _selectedIndex = cardIndex;
                _selectedImage = clickedImage;

                clickedImage.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = System.Windows.Media.Colors.Gold,
                    BlurRadius = 20,
                    ShadowDepth = 0
                };

                return;
            }

            // SECOND CLICK (attempt move)
            bool moved = _game.MoveStack(_selectedPile, clickedPile, _selectedIndex);

            if (moved)
            {
                _moves++;
                _score += 5;

                if (_isVegasModeActive)
                {
                    _player.Balance += 5;
                    UpdateBalanceUI();
                }

                MovesTextBlock.Text = _moves.ToString();
                ScoreTextBlock.Text = _score.ToString();

                RenderTableau();
                RenderStockAndWaste();
                RenderFoundations();
                CheckForWin();
            }

            ClearSelection();
        }

        private void WasteCard_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Image clickedImage = sender as Image;
            Card wasteCard = (Card)clickedImage.Tag;

            if (_selectedCard == wasteCard)
            {
                if (_selectedImage != null)
                    _selectedImage.Effect = null;

                _selectedCard = null;
                _selectedPile = null;
                _selectedImage = null;

                return;
            }

            if (_selectedCard == null)
            {
                _selectedCard = wasteCard;
                _selectedPile = _game.Waste;
                _selectedIndex = _game.Waste.Cards.Count - 1;
                _selectedImage = clickedImage;

                clickedImage.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = System.Windows.Media.Colors.Gold,
                    BlurRadius = 20,
                    ShadowDepth = 0
                };
            }
        }

        /// <summary>
        /// Updates the visual representation of the foundation piles by displaying the top card of each foundation, if
        /// present.
        /// </summary>
        /// <remarks>Call this method after changes to the foundation piles to ensure the UI reflects the
        /// current game state. This method clears and redraws each foundation's display based on the top card. It is
        /// intended for internal use within the UI rendering logic.</remarks>
        private void RenderFoundations()
        {
            Border[] borders =
            {
                Foundation0,
                Foundation1,
                Foundation2,
                Foundation3
            };

            for (int i = 0; i < _game.Foundations.Count; i++)
            {
                borders[i].Child = null;

                Card top = _game.Foundations[i].TopCard;

                if (top != null)
                {
                    Image img = new Image
                    {
                        Width = 80,
                        Height = 120,
                        Source = new BitmapImage(new Uri(GetCardImagePath(top)))
                    };

                    borders[i].Child = img;
                }
            }
        }

        /// <summary>
        /// Handles the mouse click event on a foundation pile, attempting to move the selected card to the clicked
        /// foundation and updating game state accordingly.
        /// </summary>
        /// <remarks>If the move is successful, the method updates the move count, score, and player
        /// balance (in Vegas mode), and refreshes relevant UI elements. The method also resets the selection state
        /// after processing the move.</remarks>
        /// <param name="sender">The source of the event, typically the foundation pile's UI element that was clicked.</param>
        /// <param name="e">The event data associated with the mouse button click.</param>
        private void Foundation_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_selectedCard == null)
                return;

            Border clickedBorder = sender as Border;

            int index = 0;

            if (clickedBorder == Foundation1) index = 1;
            if (clickedBorder == Foundation2) index = 2;
            if (clickedBorder == Foundation3) index = 3;

            Pile foundationPile = _game.Foundations[index];

            bool moved = _game.MoveCard(_selectedPile, foundationPile);

            if (moved)
            {
                _moves++;
                _score += 10;

                if (_isVegasModeActive)
                {
                    _player.Balance += 5;
                    UpdateBalanceUI();
                }

                MovesTextBlock.Text = _moves.ToString();
                ScoreTextBlock.Text = _score.ToString();

                RenderTableau();
                RenderStockAndWaste();
                RenderFoundations();
                CheckForWin();
            }

            if (_selectedImage != null)
                _selectedImage.Effect = null;

            _selectedCard = null;
            _selectedPile = null;
            _selectedImage = null;
            _selectedIndex = -1;
        }

        /// <summary>
        /// Checks whether the player has won the current game and updates the game state, player statistics, and user
        /// interface accordingly.
        /// </summary>
        /// <remarks>This method should be called after a move to determine if the win condition has been
        /// met. If the player wins, their statistics are updated, rewards are applied in Vegas mode, and the game is
        /// reset for a new round. The method also updates relevant UI elements to reflect the new game state. This
        /// method is not thread-safe and should be invoked on the UI thread.</remarks>
        private void CheckForWin()
        {
            if (_game.HasPlayerWon())
            {
                _vegasEntryPaid = false;
                {
                    _timer.Stop();

                    _player.GamesWon++;
                    _player.TotalMoves += _moves;
                    if (_isVegasModeActive)
                    {
                        _player.Balance += 100;
                        UpdateBalanceUI();
                    }

                    if (_score > _player.BestScore)
                        _player.BestScore = _score;

                    if (_player.BestTime == 0 || _secondsElapsed < _player.BestTime)
                        _player.BestTime = _secondsElapsed;

                    if (_player.Username != "Guest")
                    {
                        PlayerRepository repo = new PlayerRepository();
                        repo.Save(_player);
                        _hasSaved = true;
                    }

                    MessageBox.Show("Congratulations! You won the game! 🎉");

                    DifficultyLevel difficulty = _pendingDifficulty;

                    _game = new SolitaireGame(difficulty);

                    _secondsElapsed = 0;
                    TimeTextBlock.Text = "0:00";
                    _timer.Start();

                    WelcomeTextBlock.Text = $"Welcome, {_player.Username}! ({difficulty})";

                    RenderTableau();
                    RenderStockAndWaste();
                    RenderFoundations();
                }
            }
        }

        /// <summary>
        /// Starts a new Solitaire game session and resets the game state. Applies Vegas mode entry fee if enabled and
        /// updates player statistics from the previous game.
        /// </summary>
        /// <remarks>If Vegas mode is enabled and the entry fee has not been paid, the player's balance is
        /// deducted and Vegas mode is locked for the round. Player statistics are updated only if a previous game was
        /// played. The difficulty level is determined by the selected option in the UI.</remarks>
        /// <param name="sender">The source of the event, typically the button that was clicked to start a new game.</param>
        /// <param name="e">The event data associated with the click action.</param>
        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            // Apply pending settings to active game
            _isVegasModeActive = _pendingVegasMode;

            // Apply Vegas entry fee ONLY when game starts
            if (_isVegasModeActive && !_vegasEntryPaid)
            {
                _player.Balance -= 52;
                _vegasEntryPaid = true;

                VegasModeCheck.IsEnabled = false; // lock during game
            }
            else
            {
                _vegasEntryPaid = false;
                VegasModeCheck.IsEnabled = true;
            }

            UpdateBalanceUI();

            // Handle previous game stats
            if (_moves > 0)
            {
                _player.GamesLost++;
                _player.TotalMoves += _moves;

                if (_player.Username != "Guest")
                {
                    PlayerRepository repo = new PlayerRepository();
                    repo.Save(_player);
                    _hasSaved = true;
                }
            }

            // Apply difficulty
            DifficultyLevel difficulty = _pendingDifficulty;

            // Create new game
            _game = new SolitaireGame(difficulty);

            // Reset game state
            _hasSaved = false;
            _secondsElapsed = 0;
            _moves = 0;
            _score = 0;

            TimeTextBlock.Text = "0:00";
            MovesTextBlock.Text = "0";
            ScoreTextBlock.Text = "0";

            WelcomeTextBlock.Text = $"Welcome, {_player.Username}! ({difficulty})";

            RenderTableau();
            RenderStockAndWaste();
            RenderFoundations();
            ClearSelection();
        }

        /// <summary>
        /// Clears the current selection, resetting all related selection state to its default values.
        /// </summary>
        /// <remarks>Call this method to remove any active selection and restore the selection state. This
        /// is typically used when a new selection is required or when the selection should be cleared due to user
        /// interaction.</remarks>
        private void ClearSelection()
        {
            if (_selectedImage != null)
                _selectedImage.Effect = null;

            _selectedCard = null;
            _selectedPile = null;
            _selectedImage = null;
            _selectedIndex = -1;
        }

        /// <summary>
        /// Handles the logout action when the logout button is clicked, saving the current player data if applicable
        /// and returning to the login window.
        /// </summary>
        /// <remarks>This method closes the current window and displays the login window. If the current
        /// player is not a guest, their data is saved before logging out.</remarks>
        /// <param name="sender">The source of the event, typically the logout button control.</param>
        /// <param name="e">The event data associated with the button click.</param>
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (_player.Username != "Guest")
            {
                PlayerRepository repo = new PlayerRepository();
                repo.Save(_player);
                _hasSaved = true;
            }

            LoginWindow login = new LoginWindow();
            login.Show();

            this.Close();
        }

        /// <summary>
        /// Handles the click event for the Statistics button, opening the statistics window for the current player and
        /// hiding the main window.
        /// </summary>
        /// <remarks>After the statistics window is shown, the main window is hidden. To return to the
        /// main window, the user must close the statistics window.</remarks>
        /// <param name="sender">The source of the event, typically the Statistics button control.</param>
        /// <param name="e">The event data associated with the button click.</param>
        private void Statistics_Click(object sender, RoutedEventArgs e)
        {
            StatsWindow stats = new StatsWindow(_player, this);
            stats.Show();

            this.Hide();
        }

        /// <summary>
        /// Handles the actions to perform when the window is closed.
        /// </summary>
        /// <remarks>If the player is not a guest, their data is saved when the window is closed. The base
        /// class's OnClosed method is called after custom actions are performed.</remarks>
        /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            if (!_hasSaved && _player.Username != "Guest")
            {
                PlayerRepository repo = new PlayerRepository();
                repo.Save(_player);
            }

            base.OnClosed(e);
        }

        /// <summary>
        /// Updates the balance display in the user interface to reflect the current player balance and visual state
        /// based on Vegas mode.
        /// </summary>
        /// <remarks>When Vegas mode is active, the balance display uses color and opacity to indicate
        /// whether the balance is positive or negative. When Vegas mode is inactive, the display is grayed out and
        /// partially transparent. This method should be called whenever the player's balance or Vegas mode state
        /// changes to ensure the UI remains accurate.</remarks>
        private void UpdateBalanceUI()
        {
            BalanceTextBlock.Text = $"£{_player.Balance}";

            if (_isVegasModeActive)
            {
                // Vegas mode active
                BalanceTextBlock.Foreground = _player.Balance >= 0 ? Brushes.Green : Brushes.Red; // If balance 0 or above then box becomes green else the box is red
                BalanceTextBlock.Opacity = 1.0;
            }
            else
            {
                // Vegas mode inactive (greyed out)
                BalanceTextBlock.Foreground = Brushes.Gray;
                BalanceTextBlock.Opacity = 0.5;
            }
        }

        /// <summary>
        /// Handles the event when the difficulty selection is changed, updating the pending difficulty level based on the
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DifficultyBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _pendingDifficulty = DifficultyBox.SelectedIndex == 0
                ? DifficultyLevel.Easy
                : DifficultyLevel.Medium;
        }

        /// <summary>
        /// Handles the event when the Vegas mode checkbox is checked, setting the pending Vegas mode state to true.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VegasModeCheck_Checked(object sender, RoutedEventArgs e)
        {
            _pendingVegasMode = true;
            UpdateBalanceUI();
        }

        /// <summary>
        /// Handles the event when the Vegas mode checkbox is unchecked.
        /// </summary>
        /// <param name="sender">The source of the event, typically the Vegas mode checkbox control.</param>
        /// <param name="e">The event data associated with the unchecking action.</param>
        private void VegasModeCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            _pendingVegasMode = false;
            UpdateBalanceUI();
        }

        /// <summary>
        /// Shutsdown the application when the Quit button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to quit?",
                "Exit Game",
                MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
