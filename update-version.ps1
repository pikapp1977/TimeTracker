# PowerShell script to synchronize version across all files
param(
    [string]$NewVersion = ""
)

$ErrorActionPreference = "Stop"

# Read current version from Version.props
$versionPropsPath = "Version.props"
if (Test-Path $versionPropsPath) {
    [xml]$versionProps = Get-Content $versionPropsPath
    $currentVersion = $versionProps.Project.PropertyGroup.VersionPrefix
    Write-Host "Current version in Version.props: $currentVersion" -ForegroundColor Cyan
} else {
    Write-Host "Error: Version.props not found!" -ForegroundColor Red
    exit 1
}

# If new version provided, update Version.props
if ($NewVersion -ne "") {
    Write-Host "Updating version to: $NewVersion" -ForegroundColor Yellow
    $versionProps.Project.PropertyGroup.VersionPrefix = $NewVersion
    $versionProps.Save((Resolve-Path $versionPropsPath))
    $currentVersion = $NewVersion
    Write-Host "✓ Updated Version.props" -ForegroundColor Green
}

# Update TimeTrackerSetup.iss
$issPath = "TimeTrackerSetup.iss"
if (Test-Path $issPath) {
    $issContent = Get-Content $issPath -Raw
    $issContent = $issContent -replace '#define MyAppVersion ".*"', "#define MyAppVersion `"$currentVersion`""
    $issContent | Set-Content $issPath -NoNewline
    Write-Host "✓ Updated TimeTrackerSetup.iss" -ForegroundColor Green
} else {
    Write-Host "Warning: TimeTrackerSetup.iss not found!" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Version synchronization complete!" -ForegroundColor Green
Write-Host "All files are now using version: $currentVersion" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor White
Write-Host "  1. Build the application: dotnet build" -ForegroundColor Gray
Write-Host "  2. Create installer: build-installer.bat" -ForegroundColor Gray
