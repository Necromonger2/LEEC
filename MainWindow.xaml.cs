using System;
using System.Windows;
using System.Threading.Tasks;

namespace LEEC
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Hide the main window initially
            this.Visibility = Visibility.Hidden;

            // Call the version check logic when the window is initialized
            CheckVersionAsync();
        }

        private async void CheckVersionAsync()
        {
            try
            {
                // Initialize the VersionChecker with the appropriate URLs
                VersionChecker versionChecker = new VersionChecker(
                    "https://necromonger2.github.io/LEEC/version.json",
                    "https://necromonger2.github.io/LEEC/update.exe"
                );

                // Check for updates
                bool updateRequired = await versionChecker.CheckVersionAsync();

                if (updateRequired)
                {
                    // Close the application to allow the update to be installed
                    Application.Current.Shutdown();
                }
                else
                {
                    // No update required, show the main window
                    this.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                // Show the main window even if there's an error
                this.Visibility = Visibility.Hidden;
                MessageBox.Show($"Error: {ex.Message}", "Version Check Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }
    }
}