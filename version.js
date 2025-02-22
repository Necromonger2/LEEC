fetch('./version.json')
    .then(response => response.json())
    .then(data => {
        // Update the version in the title and the paragraph
        document.title = `Download LEEC The Game - Version: ${data.version}`;
        document.getElementById('version').innerText = data.version;
    })
    .catch(error => {
        console.error('Error fetching version:', error);
        document.getElementById('version').innerText = 'Unknown Version';
    });