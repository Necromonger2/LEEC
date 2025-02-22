// version.js

// Function to fetch and display the version
function displayVersion() {
    fetch('version.json')
        .then(response => response.json())
        .then(data => {
            // Update the HTML element with the version
            const versionElement = document.getElementById('version');
            if (versionElement) {
                versionElement.innerText = data.version;
            }
        })
        .catch(error => {
            console.error('Error fetching version:', error);
        });
}

// Run the function when the page loads
window.onload = displayVersion;