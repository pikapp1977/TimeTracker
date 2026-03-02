# How to Create the Installer

## Current Status

✅ **Release build complete:** `bin\Release\net8.0-windows\win-x64\publish\TimeTracker.exe`
✅ **Installer script created:** `TimeTrackerSetup.iss`

## Option 1: Quick Distribution (No Installation)

**The executable is ready to use right now!**

Simply share these files:
```
bin\Release\net8.0-windows\win-x64\publish\TimeTracker.exe
bin\Release\net8.0-windows\win-x64\publish\LatoFont\
```

Users just run `TimeTracker.exe` - no installation needed!

## Option 2: Create ZIP Package

**Open PowerShell and run:**
```powershell
cd C:\users\admin\documents\timetracker
Compress-Archive -Path "bin\Release\net8.0-windows\win-x64\publish\*" -DestinationPath "TimeTracker_v1.0.41.zip" -Force
```

**Result:** `TimeTracker_v1.0.41.zip` (~95 MB compressed)

## Option 3: Create Professional Installer

### Step 1: Install Inno Setup
Download from: https://jrsoftware.org/isdown.php

### Step 2: Build Installer

**Method A: Using Inno Setup IDE**
1. Open `TimeTrackerSetup.iss` in Inno Setup
2. Press F9 or click "Compile"
3. Installer will be created: `TimeTrackerSetup_v1.0.41.exe`

**Method B: Using Command Line**
```batch
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" TimeTrackerSetup.iss
```

### Result
✅ Professional Windows installer: `TimeTrackerSetup_v1.0.41.exe` (~100 MB)

## Files Location Summary

| What | Where |
|------|-------|
| **Executable (ready now)** | `bin\Release\net8.0-windows\win-x64\publish\TimeTracker.exe` |
| **Installer Script** | `TimeTrackerSetup.iss` |
| **ZIP (after creating)** | `TimeTracker_v1.0.41.zip` |
| **Installer (after building)** | `TimeTrackerSetup_v1.0.41.exe` |

## Recommendation

For quick sharing: **Just copy the EXE and LatoFont folder**
For distribution: **Create the ZIP** (easy, no extra software needed)
For professional release: **Build the installer** (requires Inno Setup)
