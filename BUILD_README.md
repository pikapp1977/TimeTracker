# TimeTracker Build Process

## Quick Start

### One-Step Build (Recommended)

Simply double-click `build-installer.bat` - it will:
1. Clean previous builds
2. Build the Release version
3. Check for Inno Setup
4. Create the installer (if Inno Setup is installed)

**Output:**
- Executable: `bin\Release\net8.0-windows\win-x64\publish\TimeTracker.exe`
- Installer: `TimeTrackerSetup_v1.0.41.exe` (if Inno Setup is installed)

---

## Prerequisites

### Required
- .NET 8 SDK (you already have this)
- Windows 10 or later

### Optional (for installer creation)
- Inno Setup 6: https://jrsoftware.org/isdown.php
  - Download the installer
  - Run with default settings
  - That's it!

---

## Build Options

### Option 1: Automated Build (Best)

```batch
build-installer.bat
```

This script handles everything automatically.

### Option 2: Manual Build

**Step 1: Build the application**
```batch
dotnet publish TimeTracker.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

**Step 2: Create installer** (requires Inno Setup)
```batch
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" TimeTrackerSetup.iss
```

### Option 3: Visual Studio

1. Open `TimeTracker.sln`
2. Set configuration to **Release**
3. Right-click project → **Publish**
4. Choose **Folder** profile
5. Click **Publish**

---

## What Gets Created

### Without Inno Setup
- `TimeTracker.exe` (181 MB, self-contained)
- Ready to distribute as-is - no installer needed!

### With Inno Setup
- `TimeTrackerSetup_v1.0.41.exe` (~100 MB, compressed installer)
- Professional Windows installer with:
  - Start Menu shortcut
  - Optional desktop icon
  - Clean uninstallation
  - Windows version check

---

## Installing Inno Setup (One-Time Setup)

1. **Download**: https://jrsoftware.org/isdl.php
   - Click "Download Inno Setup 6"
   - File: `innosetup-6.x.x.exe` (~5 MB)

2. **Install**
   - Run the downloaded file
   - Click Next → Next → Next → Install
   - Default settings are perfect

3. **Build**
   - Run `build-installer.bat` again
   - Installer will be created automatically

**Total time: ~2 minutes**

---

## Troubleshooting

### "dotnet not found"
- Install .NET 8 SDK: https://dotnet.microsoft.com/download

### "ISCC.exe not found"
- Install Inno Setup (see above)
- Or build without installer (executable works standalone)

### Build fails with errors
```batch
dotnet clean
rmdir /s /q bin obj
build-installer.bat
```

### Installer not created but build succeeded
- Inno Setup is not installed
- The executable at `bin\Release\...\TimeTracker.exe` works without an installer
- Users can just run the EXE directly

---

## Updating Version Number

Edit these files:

1. **TimeTracker.csproj**
   ```xml
   <Version>1.0.41</Version>
   ```

2. **TimeTrackerSetup.iss**
   ```
   #define MyAppVersion "1.0.41"
   ```

Then run `build-installer.bat`

---

## Distribution

### For End Users (Simple)
Give them either:
- **Installer**: `TimeTrackerSetup_v1.0.41.exe` - double-click to install
- **Portable**: `TimeTracker.exe` - double-click to run (no install)

Both work identically. Installer is more professional; portable is simpler.

### System Requirements
- Windows 10 or later (64-bit)
- 200 MB free disk space
- No other requirements (everything is included)

---

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Build Installer

on:
  push:
    tags:
      - 'v*'

jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v2
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '8.0.x'
      
      - name: Build
        run: dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
      
      - name: Setup Inno Setup
        run: choco install innosetup
      
      - name: Create Installer
        run: iscc TimeTrackerSetup.iss
      
      - name: Upload Release
        uses: actions/upload-artifact@v2
        with:
          name: TimeTracker-Installer
          path: TimeTrackerSetup_v*.exe
```

---

## Build Artifacts

After running `build-installer.bat`:

```
timetracker/
├── bin/
│   └── Release/
│       └── net8.0-windows/
│           └── win-x64/
│               └── publish/
│                   ├── TimeTracker.exe (executable)
│                   └── LatoFont/ (fonts)
├── TimeTrackerSetup_v1.0.41.exe (installer)
└── build-installer.bat (build script)
```

---

## Quick Reference

| Task | Command |
|------|---------|
| Build everything | `build-installer.bat` |
| Build without installer | `dotnet publish -c Release` |
| Clean build | `dotnet clean` |
| Test build | `dotnet build -c Debug` |
| Run tests | `dotnet test` |

---

## Support

- Build issues? Check the Troubleshooting section above
- Need help? See documentation in project README.md
- Inno Setup docs: https://jrsoftware.org/ishelp/

**The build process is now fully automated!** Just run `build-installer.bat` 🚀
