using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LEEC
{
    public partial class UserDetailsForm : Window
    {
        private readonly Game _gameWindow; // Reference to the Game window
        private static readonly HttpClient httpClient = new HttpClient();
        private const string TitleId = "BEEE4"; // Your PlayFab Title ID

        public UserDetailsForm(Game gameWindow)
        {
            InitializeComponent();
            _gameWindow = gameWindow; // Store the reference to the Game window
            LoadUserProfile(); // Load the user's profile when the form is opened
        }

        // Fetch and display the user's profile data
        private async void LoadUserProfile()
        {
            try
            {
                // Get the PlayFab ID and SessionTicket from the Game class
                var playFabId = _gameWindow.PlayFabId;
                var sessionTicket = _gameWindow.SessionTicket;

                if (string.IsNullOrEmpty(playFabId) || string.IsNullOrEmpty(sessionTicket))
                {
                    MessageBox.Show("User not logged in.");
                    return;
                }

                // Create the request object for GetPlayerProfile
                var request = new
                {
                    PlayFabId = playFabId,
                    ProfileConstraints = new
                    {
                        ShowDisplayName = true,
                        ShowCreated = true
                    }
                };

                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

                // Add the SessionTicket to the headers
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("X-Authorization", sessionTicket);

                // Endpoint for GetPlayerProfile
                var response = await httpClient.PostAsync($"https://{TitleId}.playfabapi.com/Client/GetPlayerProfile", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Parse the response
                    var jsonResponse = JObject.Parse(responseString);
                    var profile = jsonResponse["data"]?["PlayerProfile"];

                    if (profile != null)
                    {
                        // Populate the display name field
                        DisplayNameTextBox.Text = profile["DisplayName"]?.ToString();
                    }
                    else
                    {
                        MessageBox.Show("Failed to load profile data.");
                    }
                }
                else
                {
                    MessageBox.Show($"Failed to fetch profile: {responseString}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        // Save Changes Button Click Event
        private async void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Clear the status message
                StatusMessageText.Text = "";
                StatusMessageText.Foreground = System.Windows.Media.Brushes.White;

                // Get the new display name from the input field
                var newDisplayName = DisplayNameTextBox.Text;

                // Update the display name
                if (!string.IsNullOrEmpty(newDisplayName))
                {
                    await UpdateDisplayName(newDisplayName);
                    StatusMessageText.Text = "Display name updated successfully!";
                    StatusMessageText.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    StatusMessageText.Text = "Display name cannot be empty.";
                    StatusMessageText.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                // Set the status message color to red for errors
                StatusMessageText.Text = $"Error: {ex.Message}";
                StatusMessageText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        // Helper method to update the display name
        private async Task UpdateDisplayName(string newDisplayName)
        {
            var request = new
            {
                DisplayName = newDisplayName
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            // Add the SessionTicket to the headers
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("X-Authorization", _gameWindow.SessionTicket);

            // Endpoint for UpdateUserTitleDisplayName
            var response = await httpClient.PostAsync($"https://{TitleId}.playfabapi.com/Client/UpdateUserTitleDisplayName", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to update display name: {responseString}");
            }
        }

        private void BackToMainMenuButton_Click(object sender, RoutedEventArgs e)
        {
            // Show the MainGameMenu and close this window
            _gameWindow.OpenMainGameMenu(); // Call a method in Game to show the MainGameMenu
            this.Close();
        }
    }
}