param (
    [string]$version,
    [string]$commitComment
)

# Path to the README.md file
$readmePath = "README.md"

# New update information
$newUpdate = @"
# New update
File version: $version
Git comment: $commitComment

# Previous updates
"@

# Read the existing content of README.md
$existingContent = Get-Content -Path $readmePath -Raw

# Insert the new update information at the top
$updatedContent = $newUpdate + "`n`n" + $existingContent

# Write the updated content back to README.md
Set-Content -Path $readmePath -Value $updatedContent