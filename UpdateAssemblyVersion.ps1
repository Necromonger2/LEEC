param (
    [string]$ProjectFilePath,
    [string]$RevisionFilePath = "revision.txt"  # Default path for the revision file
)

Write-Host "Starting script..."
Write-Host "ProjectFilePath: $ProjectFilePath"
Write-Host "RevisionFilePath: $RevisionFilePath"

# Check if required parameters are provided
if ([string]::IsNullOrEmpty($ProjectFilePath)) {
    Write-Host "Error: Project file path is not provided."
    exit 1
}

# Initialize or read the revision number
if (Test-Path $RevisionFilePath) {
    $revision = Get-Content $RevisionFilePath
    if (-not [int]::TryParse($revision, [ref]$revision)) {
        Write-Host "Error: Invalid revision number in $RevisionFilePath."
        exit 1
    }
} else {
    $revision = 1500  # Start with revision 1500 if the file does not exist
}

# Load the project file as XML
Write-Host "Loading project file..."
$xml = [xml](Get-Content $ProjectFilePath)

# Find the first <PropertyGroup> element
Write-Host "Finding <PropertyGroup>..."
$propertyGroup = $xml.Project.PropertyGroup | Select-Object -First 1
if (-not $propertyGroup) {
    Write-Host "Error: No <PropertyGroup> found in the project file."
    exit 1
}

# Get the current AssemblyVersion and FileVersion
$currentVersion = $propertyGroup.FileVersion
Write-Host "Current FileVersion: $currentVersion"

# Extract the revision number from the current version
$versionParts = $currentVersion -split '\.'
if ($versionParts.Length -ne 4) {
    Write-Host "Error: Invalid version format in the project file."
    exit 1
}

# Increment the revision number
$revision = [int]$versionParts[3] + 1  # Increment the last part (revision)

# Generate the new version number in the format 0.YYYY.MMDD.Revision
$year = Get-Date -Format "yyyy"  # Current year
$month = Get-Date -Format "MM"    # Current month
$day = Get-Date -Format "dd"      # Current day
$newVersion = "0.$year.$month$day.$revision"

Write-Host "Generated new version: $newVersion"

# Update the <AssemblyVersion> and <FileVersion> elements
Write-Host "Updating AssemblyVersion and FileVersion..."
$propertyGroup.AssemblyVersion = $newVersion
$propertyGroup.FileVersion = $newVersion

# Save the updated XML back to the project file
Write-Host "Saving project file..."
$xml.Save($ProjectFilePath)

Write-Host "Updated AssemblyVersion and FileVersion to $newVersion in $ProjectFilePath"

# Create a JSON object with the version and required properties
Write-Host "Creating version.json..."
$versionJson = @{
    version = "0.$year.$month$day.$([int]$revision - 1)"  # Write revision - 1 to JSON
    inherit = "some_value"  # Replace with the appropriate value for your use case
} | ConvertTo-Json -Depth 3  # Increase depth if necessary for nested objects

# Write the JSON to a file
$versionFilePath = Join-Path (Split-Path -Parent $ProjectFilePath) "version.json"
Write-Host "Writing version file to: $versionFilePath"
Set-Content -Path $versionFilePath -Value $versionJson

Write-Host "Generated version file: $versionFilePath"
Write-Host "Script completed successfully."