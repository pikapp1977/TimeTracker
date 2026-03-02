# Download Installer from GitHub - Setup Guide

## Quick Start

1. **Right-click** `download-installer.ps1`
2. Select **"Run with PowerShell"**
3. Follow the prompts

The script will automatically download your installer!

---

## First Time Setup

### Install GitHub CLI (One-Time, 2 minutes)

**Option 1: Using winget (Windows 11)**
```powershell
winget install --id GitHub.cli
```

**Option 2: Using Chocolatey**
```powershell
choco install gh
```

**Option 3: Manual Download**
1. Go to: https://cli.github.com/
2. Download and install
3. Restart your terminal/PowerShell

### Authenticate with GitHub (One-Time)

After installing GitHub CLI, authenticate:

```powershell
gh auth login
```

Follow the prompts:
1. Choose: **GitHub.com**
2. Choose: **HTTPS**
3. Choose: **Login with a web browser**
4. Copy the code shown
5. Press Enter to open browser
6. Paste the code and authorize

**That's it!** You only need to do this once.

---

## How to Use

### Automatic Download (Easy)

```powershell
.\download-installer.ps1
```

Or just **double-click** `download-installer.ps1` in File Explorer.

### What It Does

1. ✓ Checks for GitHub CLI
2. ✓ Checks authentication
3. ✓ Finds the latest successful build
4. ✓ Lists available artifacts
5. ✓ Downloads to `.\downloads\` folder
6. ✓ Opens the folder automatically

---

## Manual Download (No Setup Required)

If you don't want to install GitHub CLI:

1. Go to: https://github.com/pikapp1977/TimeTracker/actions
2. Click the latest successful workflow (green checkmark ✓)
3. Scroll down to **"Artifacts"**
4. Click to download
5. Extract if it's in a ZIP

---

## What Gets Downloaded

After running the script, you'll find:

```
downloads/
├── TimeTracker-Installer/
│   └── TimeTrackerSetup_v1.0.41.exe
```

Or similar, depending on how your GitHub Action names the artifact.

---

## Troubleshooting

### "gh is not recognized"
- GitHub CLI is not installed
- Install using one of the options above
- Restart PowerShell after installing

### "Not authenticated"
- Run: `gh auth login`
- Follow the authentication steps

### "No successful runs found"
- Your GitHub Action hasn't completed successfully yet
- Check: https://github.com/pikapp1977/TimeTracker/actions
- Wait for the build to complete (green checkmark)

### "Execution Policy" Error
If you get an execution policy error:

```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

Then run the script again.

---

## Script Features

The `download-installer.ps1` script:

✅ **Smart Detection**
- Checks if GitHub CLI is installed
- Verifies authentication
- Finds the latest successful build automatically

✅ **User-Friendly**
- Shows recent workflow runs
- Color-coded status indicators
- Clear error messages with solutions

✅ **Automatic**
- Downloads to `downloads` folder
- Opens the folder when done
- Handles multiple artifacts

---

## Quick Reference

| Task | Command |
|------|---------|
| Download installer | `.\download-installer.ps1` |
| Install GitHub CLI | `winget install --id GitHub.cli` |
| Authenticate | `gh auth login` |
| List workflow runs | `gh run list` |
| Manual download | Visit GitHub Actions page |

---

## Links

- **Your Repository**: https://github.com/pikapp1977/TimeTracker
- **Actions Tab**: https://github.com/pikapp1977/TimeTracker/actions
- **GitHub CLI**: https://cli.github.com/

---

## Summary

**First Time:**
1. Install GitHub CLI (2 minutes)
2. Run `gh auth login` (1 minute)

**Every Time After:**
1. Double-click `download-installer.ps1`
2. Get your installer!

**That's it!** 🚀
