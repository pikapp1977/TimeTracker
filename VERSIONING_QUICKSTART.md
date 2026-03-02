# TimeTracker Versioning - Quick Start Guide

## ✅ What Was Implemented

Your TimeTracker application now has a professional versioning system with:

1. **Centralized version control** in `Version.props`
2. **Help → About menu** that displays the current version
3. **Automatic version synchronization** across all build files
4. **Version-tagged installer** files

## 🚀 How to Use

### View the Current Version

**In the application:**
1. Run TimeTracker.exe
2. Click **Help** in the menu bar
3. Click **About**
4. You'll see a dialog showing:
   - Application name
   - Current version number
   - Description
   - Copyright info

### Update the Version

**Quick method:**
```powershell
.\update-version.ps1 -NewVersion "1.0.42"
```

**Or manually:**
1. Open `Version.props`
2. Change `<VersionPrefix>1.0.41</VersionPrefix>` to your new version
3. Save the file
4. Run `.\update-version.ps1` (without parameters) to sync

### Build with New Version

Simply run your normal build:
```batch
build-installer.bat
```

This will:
- Auto-sync the version (Step 0)
- Build the application with the new version
- Create installer named `TimeTrackerSetup_v1.0.42.exe`

## 📁 Files Created/Modified

### New Files:
- `Version.props` - Master version source
- `update-version.ps1` - Version sync script
- `VERSIONING.md` - Detailed documentation
- `VERSIONING_QUICKSTART.md` - This file

### Modified Files:
- `TimeTracker.csproj` - Now imports Version.props
- `MainForm.cs` - Added Help menu with About dialog
- `build-installer.bat` - Added version sync step

## 🔍 How It Works

```
Version.props (1.0.41)
    ↓
    ├─→ TimeTracker.csproj (reads via import)
    │   ↓
    │   └─→ TimeTracker.exe (assembly version 1.0.41)
    │       ↓
    │       └─→ About Dialog (displays via reflection)
    │
    └─→ update-version.ps1 (syncs to)
        ↓
        └─→ TimeTrackerSetup.iss
            ↓
            └─→ TimeTrackerSetup_v1.0.41.exe
```

## 🎯 Next Steps

1. **Test the About dialog:**
   - Build and run: `dotnet run`
   - Check Help → About

2. **Try updating version:**
   ```powershell
   .\update-version.ps1 -NewVersion "1.0.42"
   ```

3. **Build new installer:**
   ```batch
   build-installer.bat
   ```

4. **Verify installer name:**
   - Should be `TimeTrackerSetup_v1.0.42.exe`

## 💡 Tips

- **Before releases:** Always bump the version
- **Version format:** Use semantic versioning (MAJOR.MINOR.PATCH)
- **Stay consistent:** Use the sync script to avoid mismatches
- **Commit Version.props:** Keep it in source control

## ❓ Troubleshooting

**Q: About menu doesn't show?**
A: Rebuild the solution: `dotnet build -c Release`

**Q: Version shows 0.0.0?**
A: Make sure Version.props is in the root directory and check TimeTracker.csproj has the Import line

**Q: Installer has wrong version in filename?**
A: Run `.\update-version.ps1` before building

## 📞 Version History Example

```
1.0.41 - Current version (2026-03-01)
1.0.42 - Bug fixes (upcoming)
1.1.0  - New feature: Export to CSV (planned)
2.0.0  - Major UI overhaul (future)
```
