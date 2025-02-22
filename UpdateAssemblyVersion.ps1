param (
    [string]$ProjectFilePath
)

Write-Host "Starting script..."
Write-Host "ProjectFilePath: $ProjectFilePath"

# Check if required parameters are provided
if ([string]::IsNullOrEmpty($ProjectFilePath)) {
    Write-Host "Error: Project file path is not provided."
    exit 1
}

# Generate the version number in the format 0.YYYY.MMDD.HHMM
$year = Get-Date -Format "yyyy"  # YYYY (e.g., 2023)
$monthDay = Get-Date -Format "MMdd"  # MMDD (e.g., 1025 for October 25)
$hourMinute = Get-Date -Format "HHmm"  # HHMM (e.g., 1234 for 12:34)

# Combine into version format 0.YYYY.MMDD.HHMM
$version = "0.$year.$monthDay.$hourMinute"

Write-Host "Generated version: $version"

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

# Update or create the <AssemblyVersion> and <FileVersion> elements
Write-Host "Updating AssemblyVersion and FileVersion..."
$propertyGroup.AssemblyVersion = $version
$propertyGroup.FileVersion = $version

# Save the updated XML back to the project file
Write-Host "Saving project file..."
$xml.Save($ProjectFilePath)

Write-Host "Updated AssemblyVersion and FileVersion to $version in $ProjectFilePath"

# Determine the output directory (project root directory)
$projectDir = Split-Path -Parent $ProjectFilePath
$outputDir = $projectDir  # Use the project root directory

Write-Host "Output directory: $outputDir"

# Create the output directory if it doesn't exist
if (-not (Test-Path $outputDir)) {
    Write-Host "Creating output directory..."
    New-Item -ItemType Directory -Path $outputDir
}

# Create a JSON object with the version
Write-Host "Creating version.json..."
$versionJson = @{
    version = $version
} | ConvertTo-Json

# Write the JSON to a file
$versionFilePath = Join-Path $outputDir "version.json"
Write-Host "Writing version file to: $versionFilePath"
Set-Content -Path $versionFilePath -Value $versionJson

Write-Host "Generated version file: $versionFilePath"
Write-Host "Script completed successfully."