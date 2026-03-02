# TimeTracker Versioning System

## Overview

The TimeTracker application uses a centralized versioning system where the version number is defined in one place and automatically synchronized across all components.

## Version Location

**Primary Source:** `Version.props`
```xml
<VersionPrefix>1.0.41</VersionPrefix>
```

This version is automatically used by:
- The application executable (assembly version)
- The About dialog in the Help menu
- The Inno Setup installer script
- All build outputs

## How to Update the Version

### Option 1: Using the PowerShell Script (Recommended)

```powershell
.\update-version.ps1 -NewVersion "1.0.42"
```

This will:
- Update `Version.props`
- Update `TimeTrackerSetup.iss`
- Show confirmation of all changes

### Option 2: Manual Update

1. Edit `Version.props` and change the `<VersionPrefix>` value
2. Run `.\update-version.ps1` (without parameters) to sync to other files
3. Or run `build-installer.bat` which automatically syncs versions

### Option 3: Edit Version.props Directly

Just change the version in `Version.props`:
```xml
<VersionPrefix>1.0.42</VersionPrefix>
```

Then build normally - the version will be picked up automatically.

## Version Display

Users can see the application version by:
1. Opening the application
2. Clicking **Help** → **About** in the menu bar
3. The version will be displayed in the About dialog

## Build Process

When you run `build-installer.bat`, it will:
1. **Step 0:** Synchronize version from `Version.props` to all files
2. **Step 1:** Clean previous builds
3. **Step 2:** Build the application with the new version
4. **Step 3:** Check for Inno Setup
5. **Step 4:** Create installer with version in filename (e.g., `TimeTrackerSetup_v1.0.42.exe`)

## Version Components

The version follows semantic versioning: `MAJOR.MINOR.PATCH`

- **MAJOR:** Increment for breaking changes or major feature releases
- **MINOR:** Increment for new features (backward compatible)
- **PATCH:** Increment for bug fixes

Current version: **1.0.41**

## Files That Use Version

| File | Purpose | Auto-Updated |
|------|---------|--------------|
| `Version.props` | Master version source | Manual |
| `TimeTracker.csproj` | Reads from Version.props | Yes (via import) |
| `TimeTrackerSetup.iss` | Installer script | Yes (via script) |
| `MainForm.cs` | About dialog | Yes (via reflection) |
| Installer output | Filename includes version | Yes (automatic) |

## Best Practices

1. **Always update version before release builds**
2. **Use the update-version.ps1 script** to ensure consistency
3. **Commit Version.props changes** to version control
4. **Test the About dialog** after version updates
5. **Document version changes** in release notes
