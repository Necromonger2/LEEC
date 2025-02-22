param (
    [string]$ProjectFilePath
)

# Check if required parameters are provided
if ([string]::IsNullOrEmpty($ProjectFilePath)) {
    Write-Host "Error: Project file path is not provided."
    exit 1
}

# Function to get the version from the .csproj file
function Get-VersionFromProjectFile {
    param (
        [string]$ProjectFilePath
    )

    if (Test-Path $ProjectFilePath) {
        # Load the project file as XML
        $xml = [xml](Get-Content $ProjectFilePath)

        # Find the <Version> element
        $versionElement = $xml.Project.PropertyGroup.Version
        if ($versionElement) {
            return $versionElement
        } else {
            Write-Host "Version element not found in the project file."
            return $null
        }
    } else {
        Write-Host "Project file not found: $ProjectFilePath"
        return $null
    }
}

# Get the version from the .csproj file
$version = Get-VersionFromProjectFile -ProjectFilePath $ProjectFilePath

if ($version) {
    # Get the project directory
    $projectDir = Split-Path -Parent $ProjectFilePath

    # Create a JSON object with the version
    $versionJson = @{
        version = $version
    } | ConvertTo-Json

    # Write the JSON to a file in the project directory
    $versionFilePath = Join-Path $projectDir "version.json"
    Set-Content -Path $versionFilePath -Value $versionJson

    Write-Host "Generated version file: $versionFilePath"
} else {
    Write-Host "Failed to retrieve version from the project file."
}