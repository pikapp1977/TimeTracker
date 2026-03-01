# Time Tracker Build and Package Script
# Run this from PowerShell in the project directory

Write-Host "Building Time Tracker..." -ForegroundColor Cyan

# Clean previous builds
Write-Host "`nCleaning previous builds..." -ForegroundColor Yellow
if (Test-Path ".\publish") {
    Remove-Item ".\publish" -Recurse -Force
}
if (Test-Path ".\bin") {
    Remove-Item ".\bin" -Recurse -Force
}
if (Test-Path ".\obj") {
    Remove-Item ".\obj" -Recurse -Force
}

# Build and publish
Write-Host "`nPublishing application..." -ForegroundColor Yellow
dotnet publish TimeTracker.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o publish

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nBuild failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`nBuild completed successfully!" -ForegroundColor Green
Write-Host "Output: .\publish\TimeTracker.exe" -ForegroundColor Green

# Check if Inno Setup is installed
$innoSetupPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if (Test-Path $innoSetupPath) {
    Write-Host "`nCreating installer..." -ForegroundColor Yellow
    
    # Create installer directory if it doesn't exist
    if (-not (Test-Path ".\installer")) {
        New-Item -ItemType Directory -Path ".\installer" | Out-Null
    }
    
    # Run Inno Setup
    & $innoSetupPath setup.iss
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`nInstaller created successfully!" -ForegroundColor Green
        Write-Host "Output: .\installer\TimeTrackerSetup.exe" -ForegroundColor Green
    } else {
        Write-Host "`nInstaller creation failed!" -ForegroundColor Red
    }
} else {
    Write-Host "`nInno Setup not found at: $innoSetupPath" -ForegroundColor Yellow
    Write-Host "Skipping installer creation." -ForegroundColor Yellow
    Write-Host "Download Inno Setup from: https://jrsoftware.org/isdl.php" -ForegroundColor Cyan
}

Write-Host "`nDone!" -ForegroundColor Cyan
