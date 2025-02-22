using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace LEEC
{
    public class VersionChecker
    {
        private readonly string _jsonUrl;
        private readonly string _updateUrl;

        public VersionChecker(string jsonUrl, string updateUrl)
        {
            _jsonUrl = jsonUrl;
            _updateUrl = updateUrl;
        }

        public async Task<bool> CheckVersionAsync()
        {
            try
            {
                // Get the local assembly version
                Version localVersion = Assembly.GetExecutingAssembly().GetName().Version;

                // Fetch the remote version.json
                string jsonContent = await FetchJsonContent(_jsonUrl);

                // Parse the JSON to get the remote version
                JObject json = JObject.Parse(jsonContent);
                string remoteVersionStr = json["version"].ToString();
                Version remoteVersion = new Version(remoteVersionStr);

                // Compare versions
                if (localVersion == remoteVersion)
                {
                    // No update required
                    return false;
                }
                else if (remoteVersion > localVersion)
                {
                    // Notify the user that an update is being downloaded
                    await DownloadUpdateAsync();
                    return true;
                }
                else
                {
                    // Local version is newer
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Log the error or handle it as needed
                MessageBox.Show($"Error: {ex.Message}", "Version Check Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
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
                // Download the update.exe file
                using (HttpClient client = new HttpClient())
                {
                    byte[] fileBytes = await client.GetByteArrayAsync(_updateUrl);

                    // Save the file to the local directory
                    string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update.exe");
                    await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

                    MessageBox.Show("Update downloaded successfully. Please run update.exe to install the new version.", "Download Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error:  {ex.Message}" +Environment.NewLine + "FATAL ERROR CONTACT VENDOR", "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}