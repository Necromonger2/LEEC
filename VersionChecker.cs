using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Windows;

namespace LEEC
{
    public class VersionChecker
    {
        private readonly string _jsonUrl;
        private readonly string _updateUrl;

        // Property to enable or disable debug messages
        public bool IsDebugEnabled { get; set; }

        public VersionChecker(string jsonUrl, string updateUrl)
        {
            _jsonUrl = jsonUrl;
            _updateUrl = updateUrl;
            IsDebugEnabled = false; // Default to false
        }

        public async Task<bool> CheckVersionAsync()
        {
            try
            {
                // Get the local assembly version
                Version localVersion = Assembly.GetExecutingAssembly().GetName().Version;
                UpdateStatus($"Current Local Version: {localVersion}");

                // Fetch the remote version.json
                string jsonContent = await FetchJsonContent(_jsonUrl);
                UpdateStatus("Fetched remote JSON content.");

                // Parse the JSON to get the remote version
                JObject json = JObject.Parse(jsonContent);
                string remoteVersionStr = json["version"].ToString().Trim(); // Trim whitespace
                Version remoteVersion = new Version(remoteVersionStr);
                UpdateStatus($"Remote Version: {remoteVersion}");

                // Compare versions
                if (localVersion == remoteVersion)
                {
                    UpdateStatus("No updates available.");
                    return false;
                }
                else if (remoteVersion > localVersion)
                {
                    UpdateStatus("An update is available. Downloading...");
                    await DownloadUpdateAsync();
                    return true;
                }
                else
                {
                    UpdateStatus("Local version is newer than the remote version.");
                    return false;
                }
            }
            catch (HttpRequestException httpEx)
            {
                UpdateStatus($"Network error: {httpEx.Message}");
                return false; // Return false if there's a network error
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error: {ex.Message}");
                return false; // Return false if there's a general error
            }
        }

        public async Task<Version> GetRemoteVersionAsync()
        {
            try
            {
                // Fetch the remote version.json
                string jsonContent = await FetchJsonContent(_jsonUrl);
                JObject json = JObject.Parse(jsonContent);
                string remoteVersionStr = json["version"].ToString().Trim(); // Trim whitespace
                return new Version(remoteVersionStr);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error retrieving remote version: {ex.Message}");
                return null; // Return null if there's an error
            }
        }

        private async Task<string> FetchJsonContent(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                // Simply make the request without custom headers
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        private async Task DownloadUpdateAsync()
        {
            try
            {
                // Download the installer.exe file from the new URL
                using (HttpClient client = new HttpClient())
                {
                    byte[] fileBytes = await client.GetByteArrayAsync(_updateUrl);

                    // Save the file to the local directory
                    string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Installer.exe");
                    await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

                    UpdateStatus("Update downloaded successfully. Please run Installer.exe to install the new version.");
                }
            }
            catch (Exception ex)
            {
                // Throw an exception if the download fails
                throw new Exception($"Failed to download the update: {ex.Message}");
            }
        }

        private void UpdateStatus(string message)
        {
            // This method can be used to update the UI or log messages
            if (IsDebugEnabled)
            {
                // Show debug messages
                MessageBox.Show(message); // Replace with your UI update logic
            }
        }
    }
}