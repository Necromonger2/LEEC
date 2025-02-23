using System.Windows;

namespace LEEC
{
    public partial class MainGameMenu : Window
    {
        private readonly Game _gameWindow; // Reference to the Game window

        public MainGameMenu(Game gameWindow)
        {
            InitializeComponent();
            _gameWindow = gameWindow; // Store the reference to the Game window
            this.Closing += MainGameMenu_Closing; // Handle the Closing event
        }

        // Handle the Closing event to prevent the application from closing when the window is hidden
        private void MainGameMenu_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // If the window is being hidden, cancel the close operation
            if (this.Visibility == Visibility.Hidden)
            {
                e.Cancel = true;
            }
        }

        // Logout Button Click Event
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Show the login window and close this window
            _gameWindow.Show();
            this.Close();
        }

        // User Details Button Click Event
        private void UserDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            // Open the UserDetailsForm
            _gameWindow.OpenUserDetailsForm();
        }
    }
}