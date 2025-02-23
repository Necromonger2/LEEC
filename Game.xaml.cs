using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LEEC
{
    public partial class Game : Window
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string TitleId = "BEEE4"; // Your PlayFab Title ID

        // Add properties to store PlayFabId and SessionTicket
        public string PlayFabId { get; private set; }
        public string SessionTicket { get; private set; }

        public Game()
        {
            InitializeComponent();
            this.Closing += Game_Closing; // Handle the Closing event
        }

        // Handle the Closing event to prevent the application from closing when the window is hidden
        private void Game_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // If the window is being hidden, cancel the close operation
            if (this.Visibility == Visibility.Hidden)
            {
                e.Cancel = true;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Application.Current.Shutdown();
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text; // Username input
            var password = PasswordBox.Password; // Get password from PasswordBox

            var request = new
            {
                Email = username, // Assuming the username is an email
                Password = password,
                TitleId = TitleId
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            try
            {
                // Correct endpoint for LoginWithEmailAddress
                var response = await httpClient.PostAsync($"https://{TitleId}.playfabapi.com/Client/LoginWithEmailAddress", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Parse the response to get the PlayFab ID and SessionTicket
                    var jsonResponse = JObject.Parse(responseString);
                    PlayFabId = jsonResponse["data"]?["PlayFabId"]?.ToString();
                    SessionTicket = jsonResponse["data"]?["SessionTicket"]?.ToString();

                    // Update the UI with a success message
                    MessageTextBlock.Text = "Login successful!";
                    MessageTextBlock.Foreground = System.Windows.Media.Brushes.Green;

                    // Open the MainGameMenu form
                    OpenMainGameMenu();
                }
                else
                {
                    // Parse the error response
                    var jsonResponse = JObject.Parse(responseString);
                    var errorCode = jsonResponse["errorCode"]?.ToString();
                    var errorMessage = jsonResponse["errorMessage"]?.ToString();

                    // Check if the error is due to a banned account
                    if (errorCode == "1002" && errorMessage.Contains("banned"))
                    {
                        // Show a MessageBox with ban details
                        MessageBox.Show(
                            "Your account is banned. Please contact support for more information.",
                            "Banned",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );

                        // Close the application
                        Application.Current.Shutdown();
                    }
                    else
                    {
                        // Update the UI with an error message
                        MessageTextBlock.Text = $"Login failed: {errorMessage}";
                        MessageTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                    }
                }
            }
            catch (Exception ex)
            {
                // Update the UI with an exception message
                MessageTextBlock.Text = $"Error: {ex.Message}";
                MessageTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private async Task<BanInfo> CheckIfUserIsBanned(string playFabId)
        {
            try
            {
                // Create the request object for GetUserReadOnlyData
                var request = new
                {
                    PlayFabId = playFabId,
                    Keys = new[] { "BanStatus", "BanDuration", "BanEndTime", "BanReason" } // Keys for ban-related data
                };

                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

                // Add the SessionTicket to the headers
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("X-Authorization", SessionTicket);

                // Endpoint for GetUserReadOnlyData
                var response = await httpClient.PostAsync($"https://{TitleId}.playfabapi.com/Client/GetUserReadOnlyData", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Parse the response
                    var jsonResponse = JObject.Parse(responseString);
                    var data = jsonResponse["data"]?["Data"];

                    if (data != null)
                    {
                        // Check if the user is banned
                        var banStatus = data["BanStatus"]?["Value"]?.ToString();
                        var banDuration = data["BanDuration"]?["Value"]?.ToString();
                        var banEndTime = data["BanEndTime"]?["Value"]?.ToString();
                        var banReason = data["BanReason"]?["Value"]?.ToString();

                        if (banStatus == "true")
                        {
                            return new BanInfo
                            {
                                IsBanned = true,
                                Reason = banReason ?? "Violation of terms of service", // Use custom reason if available
                                BanEndTime = banEndTime ?? "Unknown" // Use custom end time if available
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking ban status: {ex.Message}");
            }

            return new BanInfo { IsBanned = false }; // User is not banned
        }

        // Helper method to open the MainGameMenu form
        public void OpenMainGameMenu()
        {
            // Hide the current window
            this.Hide();

            // Create and show the MainGameMenu, passing a reference to the current window
            var mainGameMenu = new MainGameMenu(this);
            mainGameMenu.Show();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var email = UsernameTextBox.Text; // Email input
            var password = PasswordBox.Password; // Get password from PasswordBox

            // Generate a valid username from the email
            var username = GenerateValidUsername(email);

            // Create the request object for registration
            var request = new
            {
                Email = email, // Email for registration
                Password = password, // Password for registration
                Username = username, // Generated username
                TitleId = TitleId // Your PlayFab Title ID
            };

            // Serialize the request object to JSON
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            try
            {
                // Endpoint for registering a user
                var response = await httpClient.PostAsync($"https://{TitleId}.playfabapi.com/Client/RegisterPlayFabUser", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Update the UI with a success message
                    MessageTextBlock.Text = "Registration successful! Logging you in...";
                    MessageTextBlock.Foreground = System.Windows.Media.Brushes.Green;

                    // Automatically log in the user after registration
                    await LoginAfterRegistration(email, password);
                }
                else
                {
                    // Update the UI with an error message
                    MessageTextBlock.Text = $"Registration failed: {responseString}";
                    MessageTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                // Update the UI with an exception message
                MessageTextBlock.Text = $"Error: {ex.Message}";
                MessageTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        // Helper method to log in the user after registration
        private async Task LoginAfterRegistration(string email, string password)
        {
            var request = new
            {
                Email = email,
                Password = password,
                TitleId = TitleId
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            try
            {
                // Endpoint for LoginWithEmailAddress
                var response = await httpClient.PostAsync($"https://{TitleId}.playfabapi.com/Client/LoginWithEmailAddress", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Update the UI with a success message
                    MessageTextBlock.Text = "Login successful!";
                    MessageTextBlock.Foreground = System.Windows.Media.Brushes.Green;

                    // Open the MainGameMenu form
                    OpenMainGameMenu();
                }
                else
                {
                    // Update the UI with an error message
                    MessageTextBlock.Text = $"Login failed: {responseString}";
                    MessageTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                // Update the UI with an exception message
                MessageTextBlock.Text = $"Error: {ex.Message}";
                MessageTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        // Helper method to open the UserDetailsForm
        public void OpenUserDetailsForm()
        {
            // Hide the current window
            this.Hide();

            // Create and show the UserDetailsForm, passing a reference to the current window
            var userDetailsForm = new UserDetailsForm(this);
            userDetailsForm.Show();
        }

        // Event handler for the Forgot Password button
        private async void ForgotPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            var email = UsernameTextBox.Text; // Get the email from the UsernameTextBox

            if (string.IsNullOrEmpty(email))
            {
                MessageTextBlock.Text = "Please enter your email address.";
                MessageTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            var request = new
            {
                Email = email,
                TitleId = TitleId
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            try
            {
                // Endpoint for sending a password reset email
                var response = await httpClient.PostAsync($"https://{TitleId}.playfabapi.com/Client/SendAccountRecoveryEmail", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Update the UI with a success message
                    MessageTextBlock.Text = "Password reset email sent. Please check your inbox.";
                    MessageTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    // Update the UI with an error message
                    MessageTextBlock.Text = $"Failed to send password reset email: {responseString}";
                    MessageTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                // Update the UI with an exception message
                MessageTextBlock.Text = $"Error: {ex.Message}";
                MessageTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        // Helper method to generate a valid username from an email
        private string GenerateValidUsername(string email)
        {
            // Remove invalid characters
            var validUsername = new StringBuilder();
            foreach (var c in email)
            {
                if (char.IsLetterOrDigit(c) || c == '_' || c == '-')
                {
                    validUsername.Append(c);
                }
            }

            // Ensure the username is within the allowed length
            return validUsername.ToString().Substring(0, Math.Min(validUsername.Length, 20));
        }

        // Event handler for when the UsernameTextBox gains focus
        private void UsernameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Hide the placeholder text when the UsernameTextBox gains focus
            UsernamePlaceholder.Visibility = Visibility.Collapsed;
        }

        // Event handler for when the UsernameTextBox loses focus
        private void UsernameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Show the placeholder text if the UsernameTextBox is empty and loses focus
            if (string.IsNullOrEmpty(UsernameTextBox.Text))
            {
                UsernamePlaceholder.Visibility = Visibility.Visible;
            }
        }

        // Event handler for when the PasswordBox gains focus
        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Hide the placeholder text when the PasswordBox gains focus
            PasswordPlaceholder.Visibility = Visibility.Collapsed;
        }

        // Event handler for when the PasswordBox loses focus
        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Show the placeholder text if the PasswordBox is empty and loses focus
            if (string.IsNullOrEmpty(PasswordBox.Password))
            {
                PasswordPlaceholder.Visibility = Visibility.Visible;
            }
        }
    }
}