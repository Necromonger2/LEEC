using System;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;

namespace LEEC
{
    public partial class Game : Window
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string TitleId = "BEEE4"; // Your PlayFab Title ID

        public Game()
        {
            InitializeComponent();
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
                    // Update the UI with a success message
                    MessageTextBlock.Text = "Login successful!";
                    MessageTextBlock.Foreground = System.Windows.Media.Brushes.Green;
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
                    MessageTextBlock.Text = "Registration successful!";
                    MessageTextBlock.Foreground = System.Windows.Media.Brushes.Green;
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