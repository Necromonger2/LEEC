using System;
using System.Diagnostics; // Add this using directive
using System.Reflection;
using System.Windows;
using System.Threading.Tasks;

namespace LEEC
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Show the main window immediately
            this.Visibility = Visibility.Visible;

            // Display the current version
            Version localVersion = Assembly.GetExecutingAssembly().GetName().Version;
            CurrentVersionTextBlock.Text = $"Current Version: {localVersion}";

            // Initialize the VersionChecker with the new installer URL
            VersionChecker versionChecker = new VersionChecker(
                "https://necromonger2.github.io/LEEC/version.json", // Ensure this URL points to the correct version.json
                "https://github.com/Necromonger2/LEEC/raw/master/Installer.exe" // Updated installer URL
            )
            {
                IsDebugEnabled = false // Set to true to enable debug messages
            };

            // Call the version check logic when the window is initialized
            CheckVersionAsync(versionChecker);
        }

        private async void CheckVersionAsync(VersionChecker versionChecker)
        {
            try
            {
                // Show loading indicator
                StatusTextBlock.Text = "Checking for updates...";
                LoadingProgressBar.Visibility = Visibility.Visible; // Ensure the progress bar is visible

                // Check for updates
                bool updateRequired = await versionChecker.CheckVersionAsync();

                // Display the remote version directly from CheckVersionAsync
                Version remoteVersion = await versionChecker.GetRemoteVersionAsync();
                if (remoteVersion != null)
                {
                    RemoteVersionTextBlock.Text = $"Remote Version: {remoteVersion}";
                }
                else
                {
                    RemoteVersionTextBlock.Text = "Failed to retrieve remote version.";
                }

                // Check if an update is required
                if (updateRequired)
                {
                    // Optionally, you can add a delay here to let the user read the message
                    await Task.Delay(3000); // 3 seconds delay before shutdown
                    Application.Current.Shutdown();
                }
                else
                {
                    // No update required, start Game.exe
                    StatusTextBlock.Text = "No updates available. Starting the game...";
                    StartGame();
                }
            }
            catch (Exception ex)
            {
                // Show the error message on the UI
                StatusTextBlock.Text = $"Error: {ex.Message}";

                // Close the application if an update fails to download
                MessageBox.Show($"An error occurred: {ex.Message}. The application will now close.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            finally
            {
                // Hide loading indicator
                LoadingProgressBar.Visibility = Visibility.Collapsed; // Keep the progress bar visible
            }
        }

        private void StartGame()
        {
            try
            {
                // Get the path to the current directory
                string gamePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Game.exe");

                // Start the Game.exe application
                Process.Start(gamePath);
                Application.Current.Shutdown(); // Close the updater application
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start the game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }
    }
}