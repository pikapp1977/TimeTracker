# Time Tracker - Build and Installer Guide

## ✅ Release Build Completed Successfully

**Version**: 1.0.41  
**Build Date**: March 1, 2026  
**Build Configuration**: Release, Self-Contained, Single File

---

## Published Files Location

```
C:\users\admin\documents\timetracker\bin\Release\net8.0-windows\win-x64\publish\
```

**Contents:**
- `TimeTracker.exe` - Single-file executable (~181 MB)
- `LatoFont\` - Font directory for PDF generation
- `TimeTracker.pdb` - Debug symbols (optional, can be removed for distribution)

---

## Distribution Options

### Option 1: Portable ZIP (Simplest)

**Manual Creation:**
1. Navigate to the publish folder
2. Select all files (TimeTracker.exe and LatoFont folder)
3. Right-click → "Send to" → "Compressed (zipped) folder"
4. Rename to: `TimeTracker_v1.0.41_Portable.zip`

**PowerShell Command:**
```powershell
cd C:\users\admin\documents\timetracker
Compress-Archive -Path "bin\Release\net8.0-windows\win-x64\publish\*" `
                 -DestinationPath "TimeTracker_v1.0.41_Portable.zip" -Force
```

**Distribution:**
- Users extract the ZIP anywhere
- Run `TimeTracker.exe` directly
- No installation required
- Database stored in `Documents\timetracker.db`

---

### Option 2: Professional Installer (Inno Setup)

**Prerequisites:**
Download and install Inno Setup: https://jrsoftware.org/isdown.php

**Build Installer:**

1. **Using Inno Setup IDE:**
   - Open `TimeTrackerSetup.iss` in Inno Setup
   - Click "Compile" button (or press Ctrl+F9)
   - Installer will be created in `Installer\` folder

2. **Using Command Line:**
   ```batch
   "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" TimeTrackerSetup.iss
   ```

**Installer Features:**
- Professional Windows installer
- Start Menu shortcuts
- Optional desktop icon
- Clean uninstallation
- Windows version check (requires Windows 10+)
- Ultra compression (LZMA2)
- Output: `TimeTrackerSetup_1.0.41.exe` (~100-120 MB)

**Installer Script (`TimeTrackerSetup.iss`) Includes:**
- ✅ Automatic program files installation
- ✅ Start menu shortcut
- ✅ Desktop icon (optional)
- ✅ Uninstaller
- ✅ Windows 10 version check
- ✅ Launch option after install

---

### Option 3: ClickOnce Deployment (Network Install)

For deployment over network or web:

```batch
dotnet publish -c Release -r win-x64 --self-contained true ^
    /p:PublishSingleFile=true ^
    /p:PublishReadyToRun=true
```

Then use Visual Studio to configure ClickOnce deployment.

---

## Build Commands Reference

### Clean Build
```batch
dotnet clean
dotnet build -c Release
```

### Publish Single File
```batch
dotnet publish TimeTracker.csproj ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=true
```

### Publish with ReadyToRun (Faster Startup)
```batch
dotnet publish TimeTracker.csproj ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:PublishReadyToRun=true
```

### Publish with Trimming (Smaller Size)
```batch
dotnet publish TimeTracker.csproj ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:PublishTrimmed=true
```

**Warning**: Trimming may break reflection-based features. Test thoroughly!

---

## Version Management

To update version number, edit `TimeTracker.csproj`:

```xml
<Version>1.0.41</Version>
```

Also update in `TimeTrackerSetup.iss`:

```
#define MyAppVersion "1.0.41"
```

---

## Distribution Checklist

Before distributing:

- [ ] Update version number in both `.csproj` and `.iss`
- [ ] Build in Release configuration
- [ ] Test the executable on a clean machine
- [ ] Verify database creation works
- [ ] Test invoice generation (Excel and PDF)
- [ ] Check all UI tabs function correctly
- [ ] Remove `.pdb` file if not needed
- [ ] Create installer or ZIP package
- [ ] Document any dependencies
- [ ] Create release notes

---

## File Sizes

| Package Type | Size | Notes |
|--------------|------|-------|
| Raw EXE | ~181 MB | Self-contained with .NET runtime |
| Portable ZIP | ~95 MB | Compressed |
| Inno Setup Installer | ~100 MB | LZMA2 compressed |

**Why so large?**
- Includes entire .NET 8 runtime
- Self-contained (no dependencies)
- QuestPDF library with fonts
- ClosedXML library
- SQLite library

**Benefits:**
- ✅ No .NET installation required
- ✅ Works on any Windows 10+ machine
- ✅ No DLL hell
- ✅ Truly portable

---

## Quick Distribution (Current Build)

**The executable is ready to use right now!**

Simply copy this file to distribute:
```
C:\users\admin\documents\timetracker\bin\Release\net8.0-windows\win-x64\publish\TimeTracker.exe
```

Also include:
```
C:\users\admin\documents\timetracker\bin\Release\net8.0-windows\win-x64\publish\LatoFont\
```

**That's it!** No installer needed for basic distribution.

---

## System Requirements

**Minimum:**
- Windows 10 (64-bit)
- 200 MB free disk space
- 512 MB RAM
- Display: 1024x768

**Recommended:**
- Windows 10/11 (64-bit)
- 500 MB free disk space
- 2 GB RAM
- Display: 1920x1080

**Dependencies:**
- None (self-contained)

---

## Troubleshooting Build Issues

### Issue: "Unable to find package"
**Solution:**
```batch
dotnet restore
dotnet build -c Release
```

### Issue: "Publish failed"
**Solution:**
```batch
dotnet clean
rmdir /s /q bin obj
dotnet publish -c Release
```

### Issue: "Out of memory during build"
**Solution:**
Add to `.csproj`:
```xml
<PropertyGroup>
  <PublishTrimmed>false</PublishTrimmed>
  <PublishReadyToRun>false</PublishReadyToRun>
</PropertyGroup>
```

---

## Creating Release on GitHub

1. Build the release
2. Create ZIP or installer
3. Tag the release:
   ```bash
   git tag -a v1.0.41 -m "Version 1.0.41 - Added database indexes"
   git push origin v1.0.41
   ```
4. Create GitHub Release
5. Upload ZIP/installer as asset
6. Add release notes

---

## Next Steps

1. **Test the build** on a different computer
2. **Choose distribution method**:
   - Portable ZIP (easiest)
   - Inno Setup installer (professional)
   - Both (recommended)
3. **Create release notes**
4. **Distribute**

---

## Contact & Support

For build issues or questions, check:
- Project README.md
- GitHub Issues
- Documentation in `/docs`

**The release build is complete and ready to distribute!**
