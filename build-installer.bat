@echo off
echo ========================================
echo TimeTracker Build and Installer Script
echo ========================================
echo.

REM Step 0: Sync version across all files
echo [0/5] Synchronizing version...
powershell -ExecutionPolicy Bypass -File update-version.ps1
if errorlevel 1 goto error

REM Step 1: Clean previous builds
echo.
echo [1/5] Cleaning previous builds...
dotnet clean
if errorlevel 1 goto error

REM Step 2: Build Release
echo.
echo [2/5] Building Release configuration...
dotnet publish TimeTracker.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
if errorlevel 1 goto error

REM Step 3: Check for Inno Setup
echo.
echo [3/5] Checking for Inno Setup...
if exist "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" (
    echo Inno Setup found!
    goto build_installer
)
if exist "C:\Program Files\Inno Setup 6\ISCC.exe" (
    echo Inno Setup found!
    set ISCC="C:\Program Files\Inno Setup 6\ISCC.exe"
    goto build_installer
)

REM Inno Setup not found
echo.
echo WARNING: Inno Setup not found!
echo.
echo To create a professional installer, download Inno Setup from:
echo https://jrsoftware.org/isdown.php
echo.
echo Your application has been built successfully at:
echo %CD%\bin\Release\net8.0-windows\win-x64\publish\TimeTracker.exe
echo.
goto end

:build_installer
REM Step 4: Build installer with Inno Setup
echo.
echo [4/5] Building installer...
if defined ISCC (
    %ISCC% TimeTrackerSetup.iss
) else (
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" TimeTrackerSetup.iss
)
if errorlevel 1 goto error

REM Read version from Version.props
for /f "tokens=2 delims=<>" %%a in ('findstr /C:"<VersionPrefix>" Version.props') do set VERSION=%%a

echo.
echo ========================================
echo BUILD SUCCESSFUL!
echo ========================================
echo.
echo Release executable: bin\Release\net8.0-windows\win-x64\publish\TimeTracker.exe
echo Installer created: TimeTrackerSetup_v%VERSION%.exe
echo.
goto end

:error
echo.
echo ========================================
echo BUILD FAILED!
echo ========================================
echo.
exit /b 1

:end
echo Press any key to exit...
pause >nul
