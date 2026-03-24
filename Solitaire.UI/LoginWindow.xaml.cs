using Nclan.Ac.RobertS.Solitaire.Core.Enums;
using Nclan.Ac.RobertS.Solitaire.Core.Models;
using Nclan.Ac.RobertS.Solitaire.Core.Persistence;
using System.Windows;
using System.Windows.Media;

namespace Nclan.Ac.RobertS.Solitaire.UI
{
    public partial class LoginWindow : Window
    {
        private enum AuthMode
        {
            Login,
            SignUp
        }

        private AuthMode _currentMode = AuthMode.Login;
        private readonly PlayerRepository _repository = new PlayerRepository();

        public LoginWindow()
        {
            InitializeComponent();
            SetLoginMode();
        }

        /// <summary>
        /// Handles the click event for the login toggle control, switching the application's login mode.
        /// </summary>
        /// <param name="sender">The source of the event, typically the control that was clicked.</param>
        /// <param name="e">The event data associated with the click action.</param>
        private void LoginToggle_Click(object sender, RoutedEventArgs e)
        {
            SetLoginMode();
        }

        /// <summary>
        /// Handles the click event for the sign-up toggle control, switching the interface to sign-up mode.
        /// </summary>
        /// <param name="sender">The source of the event, typically the sign-up toggle control.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void SignUpToggle_Click(object sender, RoutedEventArgs e)
        {
            SetSignUpMode();
        }

        /// <summary>
        /// Configures the user interface to display the login mode, updating relevant controls to reflect sign-in
        /// options.
        /// </summary>
        private void SetLoginMode()
        {
            _currentMode = AuthMode.Login;

            AgeSection.Visibility = Visibility.Collapsed;
            MainActionButton.Content = "Sign In";

            LoginToggleButton.Background = Brushes.White;
            SignUpToggleButton.Background = Brushes.Transparent;
        }

        /// <summary>
        /// Configures the authentication interface for sign-up mode.
        /// </summary>
        /// <remarks>This method updates the UI to display sign-up specific elements, such as making the
        /// age input section visible and changing button labels. It should be called when the user initiates account
        /// creation. Calling this method will override any previous authentication mode settings.</remarks>
        private void SetSignUpMode()
        {
            _currentMode = AuthMode.SignUp;

            AgeSection.Visibility = Visibility.Visible;
            MainActionButton.Content = "Create Account";

            SignUpToggleButton.Background = Brushes.White;
            LoginToggleButton.Background = Brushes.Transparent;
        }

        /// <summary>
        /// Handles the click event for the main action button, performing either a login or sign-up operation based on
        /// the current authentication mode.
        /// </summary>
        /// <remarks>The action performed depends on the current authentication mode. If the mode is set
        /// to login, a login operation is initiated; otherwise, a sign-up operation is performed.</remarks>
        /// <param name="sender">The source of the event, typically the main action button that was clicked.</param>
        /// <param name="e">The event data associated with the button click.</param>
        private void MainActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMode == AuthMode.Login)
                HandleLogin();
            else
                HandleSignUp();
        }

        /// <summary>
        /// Handles the user login process by validating credentials and opening the game window upon successful
        /// authentication.
        /// </summary>
        /// <remarks>This method displays error messages if the username or password is missing, or if the
        /// credentials are invalid. Upon successful login, it closes the current window and opens the game window with
        /// a medium difficulty level. This method is intended to be called in response to a login action, such as a
        /// button click.</remarks>
        private void HandleLogin()
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter username and password.");
                return;
            }

            Player player = _repository.GetByUsername(username);

            if (player == null || player.Password != password)
            {
                MessageBox.Show("Invalid login details.");
                return;
            }

            GameWindow gameWindow = new GameWindow(player, DifficultyLevel.Medium);
            gameWindow.Show();
            this.Close();
        }

        /// <summary>
        /// Handles the user sign-up process by validating input fields, creating a new player account, and launching
        /// the game window for the registered user.
        /// </summary>
        /// <remarks>This method verifies that all required fields are provided, checks for realistic age
        /// values, and ensures the username is unique. If the user is under 18, certain features such as Vegas mode
        /// will be disabled. Upon successful registration, the method initializes the player's account and starts the
        /// game. This method displays error messages for invalid input and closes the current window after launching
        /// the game.</remarks>
        private void HandleSignUp()
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (AgeTextBox.SelectedDate == null)
            {
                MessageBox.Show("Please select your date of birth.");
                return;
            }

            DateTime dob = AgeTextBox.SelectedDate.Value;

            // Calculate age from DOB
            int age = DateTime.Today.Year - dob.Year;

            if (dob.Date > DateTime.Today.AddYears(-age))
            {
                age--;
            }

            if (age < 0 || age > 120)
            {
                MessageBox.Show("Please enter a realistic age.");
                return;
            }

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("All fields are required.");
                return;
            }

            Player existing = _repository.GetByUsername(username);
            if (existing != null)
            {
                MessageBox.Show("Username already exists.");
                return;
            }

            if (age < 18)
            {
                MessageBox.Show("You can create an account, but Vegas mode will be disabled for under 18s.");
            }

            Player newPlayer = new Player
            {
                Username = username,
                Password = password,
                Age = age,
                Balance = 0,
                GamesPlayed = 0,
                GamesWon = 0,
                GamesLost = 0,
                TotalMoves = 0,
                BestScore = 0,
                BestTime = 0
            };

            _repository.Save(newPlayer);

            GameWindow gameWindow = new GameWindow(newPlayer, DifficultyLevel.Medium);
            gameWindow.Show();
            this.Close();

        }

        /// <summary>
        /// Handles the click event for the Guest button, launching the game window with a guest player profile.
        /// </summary>
        /// <remarks>This method initializes a new guest player with default values and starts the game at
        /// medium difficulty. The current window is closed after the game window is shown.</remarks>
        /// <param name="sender">The source of the event, typically the Guest button control.</param>
        /// <param name="e">The event data associated with the button click.</param>
        private void Guest_Click(object sender, RoutedEventArgs e)
        {
            Player guest = new Player
            {
                Username = "Guest",
                Age = 0,
                Balance = 0
            };

            GameWindow gameWindow = new GameWindow(guest, DifficultyLevel.Medium);
            gameWindow.Show();
            this.Close();
        }

        /// <summary>
        /// When exit is pressed will shutdown the program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}