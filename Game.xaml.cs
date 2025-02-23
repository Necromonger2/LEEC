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
                    MessageBox.Show("Login successful!");
                    // Proceed to the next part of your application
                }
                else
                {
                    MessageBox.Show($"Login failed: {responseString}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
}