# TimeTracker Installer Downloader
# Uses GitHub CLI to download the latest build artifact

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "TimeTracker Installer Downloader" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if GitHub CLI is installed
Write-Host "Checking for GitHub CLI (gh)..." -ForegroundColor Yellow
$ghInstalled = Get-Command gh -ErrorAction SilentlyContinue

if (-not $ghInstalled) {
    Write-Host ""
    Write-Host "ERROR: GitHub CLI is not installed!" -ForegroundColor Red
    Write-Host ""
    Write-Host "To install GitHub CLI:" -ForegroundColor Yellow
    Write-Host "  Option 1: winget install --id GitHub.cli" -ForegroundColor White
    Write-Host "  Option 2: choco install gh" -ForegroundColor White
    Write-Host "  Option 3: Download from https://cli.github.com/" -ForegroundColor White
    Write-Host ""
    Write-Host "After installing, run this script again." -ForegroundColor Yellow
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "✓ GitHub CLI found!" -ForegroundColor Green
Write-Host ""

# Check if authenticated
Write-Host "Checking GitHub authentication..." -ForegroundColor Yellow
$authStatus = gh auth status 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERROR: Not authenticated with GitHub!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Run this command to authenticate:" -ForegroundColor Yellow
    Write-Host "  gh auth login" -ForegroundColor White
    Write-Host ""
    Write-Host "Then run this script again." -ForegroundColor Yellow
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "✓ Authenticated!" -ForegroundColor Green
Write-Host ""

# Get the latest workflow run
Write-Host "Fetching latest workflow runs..." -ForegroundColor Yellow
$runs = gh run list --limit 5 --json databaseId,status,conclusion,displayTitle,createdAt | ConvertFrom-Json

if ($runs.Count -eq 0) {
    Write-Host ""
    Write-Host "ERROR: No workflow runs found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Make sure your GitHub Action has run at least once." -ForegroundColor Yellow
    Write-Host "Check: https://github.com/pikapp1977/TimeTracker/actions" -ForegroundColor White
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host ""
Write-Host "Recent workflow runs:" -ForegroundColor Cyan
Write-Host "----------------------------------------" -ForegroundColor Cyan
for ($i = 0; $i -lt $runs.Count; $i++) {
    $run = $runs[$i]
    $status = $run.status
    $conclusion = $run.conclusion
    $title = $run.displayTitle
    $date = $run.createdAt
    
    $statusIcon = switch ($status) {
        "completed" { 
            switch ($conclusion) {
                "success" { "✓" }
                "failure" { "✗" }
                default { "○" }
            }
        }
        "in_progress" { "⏳" }
        default { "○" }
    }
    
    $statusColor = switch ($status) {
        "completed" { 
            switch ($conclusion) {
                "success" { "Green" }
                "failure" { "Red" }
                default { "Yellow" }
            }
        }
        "in_progress" { "Yellow" }
        default { "Gray" }
    }
    
    Write-Host "$($i + 1). $statusIcon " -ForegroundColor $statusColor -NoNewline
    Write-Host "$title" -ForegroundColor White -NoNewline
    Write-Host " ($status)" -ForegroundColor Gray
}
Write-Host "----------------------------------------" -ForegroundColor Cyan
Write-Host ""

# Find the first successful run
$successfulRun = $runs | Where-Object { $_.conclusion -eq "success" } | Select-Object -First 1

if (-not $successfulRun) {
    Write-Host "ERROR: No successful workflow runs found!" -ForegroundColor Red
    Write-Host ""
    
    $inProgress = $runs | Where-Object { $_.status -eq "in_progress" } | Select-Object -First 1
    if ($inProgress) {
        Write-Host "There is a build currently in progress..." -ForegroundColor Yellow
        Write-Host "Please wait for it to complete and run this script again." -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Check progress at: https://github.com/pikapp1977/TimeTracker/actions" -ForegroundColor White
    } else {
        Write-Host "The latest builds have failed." -ForegroundColor Yellow
        Write-Host "Check the logs at: https://github.com/pikapp1977/TimeTracker/actions" -ForegroundColor White
    }
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit 1
}

$runId = $successfulRun.databaseId
Write-Host "Found successful build: Run #$runId" -ForegroundColor Green
Write-Host ""

# List artifacts for this run
Write-Host "Checking for artifacts..." -ForegroundColor Yellow
$artifacts = gh run view $runId --json artifacts | ConvertFrom-Json

if ($artifacts.artifacts.Count -eq 0) {
    Write-Host ""
    Write-Host "ERROR: No artifacts found for this run!" -ForegroundColor Red
    Write-Host ""
    Write-Host "The workflow may not be configured to upload artifacts." -ForegroundColor Yellow
    Write-Host "Check the workflow file: .github/workflows/*.yml" -ForegroundColor White
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "✓ Found $($artifacts.artifacts.Count) artifact(s)!" -ForegroundColor Green
Write-Host ""

foreach ($artifact in $artifacts.artifacts) {
    Write-Host "  - $($artifact.name)" -ForegroundColor White
}
Write-Host ""

# Download artifacts
Write-Host "Downloading installer..." -ForegroundColor Yellow
Write-Host ""

# Create downloads folder
$downloadPath = ".\downloads"
if (-not (Test-Path $downloadPath)) {
    New-Item -ItemType Directory -Path $downloadPath | Out-Null
}

# Download all artifacts from the run
gh run download $runId --dir $downloadPath

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "✓ DOWNLOAD SUCCESSFUL!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Installer downloaded to:" -ForegroundColor Cyan
    Write-Host "  $((Get-Item $downloadPath).FullName)" -ForegroundColor White
    Write-Host ""
    
    # List downloaded files
    Write-Host "Downloaded files:" -ForegroundColor Cyan
    Get-ChildItem -Path $downloadPath -Recurse -File | ForEach-Object {
        $size = "{0:N2} MB" -f ($_.Length / 1MB)
        Write-Host "  - $($_.Name) ($size)" -ForegroundColor White
    }
    Write-Host ""
    
    # Open the downloads folder
    Write-Host "Opening downloads folder..." -ForegroundColor Yellow
    Start-Process explorer.exe -ArgumentList $downloadPath
    
} else {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "✗ DOWNLOAD FAILED!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please try downloading manually from:" -ForegroundColor Yellow
    Write-Host "  https://github.com/pikapp1977/TimeTracker/actions/runs/$runId" -ForegroundColor White
    Write-Host ""
}

Write-Host ""
Read-Host "Press Enter to exit"
