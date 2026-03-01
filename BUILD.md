# Building Time Tracker

## Prerequisites

1. **.NET 8.0 SDK** - Download from https://dotnet.microsoft.com/download/dotnet/8.0
2. **Inno Setup 6** (optional, for creating installer) - Download from https://jrsoftware.org/isdl.php

## Quick Build

### Option 1: PowerShell (Recommended)
```powershell
.\build-and-package.ps1
```

### Option 2: Command Prompt
```cmd
build-and-package.bat
```

### Option 3: Manual Build
```powershell
# Clean
dotnet clean

# Publish
dotnet publish TimeTracker.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish

# Create installer (if Inno Setup is installed)
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" setup.iss
```

## Output

- **Application**: `.\publish\TimeTracker.exe` (~180 MB, self-contained)
- **Installer**: `.\installer\TimeTrackerSetup.exe` (if Inno Setup is available)

## Version History

### v1.0.41 (Current)
- Added time entry locking feature
- Lock/unlock button to protect entries from deletion
- "Clear All Unlocked" now preserves locked entries
- Visual indicator (🔒 and yellow highlight) for locked entries
- Updated "Delete Selected" to prevent deletion of locked entries

### v1.0.40
- Previous stable release

## Notes

- The application is built as a **self-contained single-file executable**
- No .NET runtime installation required on target machines
- Database stored in: `%LOCALAPPDATA%\TimeTracker\timetracker.db`
- Build output is approximately 180-190 MB due to included runtime
