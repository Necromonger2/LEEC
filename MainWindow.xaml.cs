using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace LEEC
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Call the version check logic when the window is initialized
            CheckVersionAsync();
        }

        private async void CheckVersionAsync()
        {
            try
            {
                // Get the local assembly version
                Version localVersion = Assembly.GetExecutingAssembly().GetName().Version;

                // Fetch the remote version.json
                string jsonUrl = "https://raw.githubusercontent.com/Necromonger2/LEEC/master/version.json";
                string jsonContent = await FetchJsonContent(jsonUrl);

                // Parse the JSON to get the remote version
                JObject json = JObject.Parse(jsonContent);
                string remoteVersionStr = json["version"].ToString();
                Version remoteVersion = new Version(remoteVersionStr);

                // Compare versions
                if (localVersion == remoteVersion)
                {
                    MessageBox.Show("You are using the latest version.", "Version Check", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (remoteVersion > localVersion)
                {
                    // Prompt the user to download the update
                    MessageBoxResult result = MessageBox.Show(
                        $"A newer version ({remoteVersion}) is available. Do you want to close the application and download the update?",
                        "Update Available",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Close the application and download the update
                        await DownloadUpdateAsync();
                        Application.Current.Shutdown();
                    }
                }
                else
                {
                    MessageBox.Show($"Your version ({localVersion}) is newer than the remote version ({remoteVersion}).", "Version Check", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Version Check Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<string> FetchJsonContent(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                // GitHub requires a user-agent header
                client.DefaultRequestHeaders.Add("User-Agent", "LEEC");

                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        private async Task DownloadUpdateAsync()
        {
            try
            {
                // URL to the update.exe file (replace with the actual URL)
                string updateUrl = "https://github.com/Necromonger2/LEEC/releases/latest/download/update.exe";

                // Download the update.exe file
                using (HttpClient client = new HttpClient())
                {
                    byte[] fileBytes = await client.GetByteArrayAsync(updateUrl);

                    // Save the file to the local directory
                    string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update.exe");
                    await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

                    MessageBox.Show("Update downloaded successfully. Please run update.exe to install the new version.", "Download Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to download update: {ex.Message}", "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}