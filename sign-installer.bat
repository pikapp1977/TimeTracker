@echo off
REM Code Signing Script for TimeTracker Installer
REM 
REM SETUP REQUIRED:
REM 1. Purchase a code signing certificate
REM 2. Save certificate as: certificates\codesign.pfx
REM 3. Set CERT_PASSWORD environment variable
REM 4. Update SIGNTOOL_PATH if needed

echo ========================================
echo TimeTracker Installer Signing Script
echo ========================================
echo.

REM Configuration
set CERT_PATH=certificates\codesign.pfx
set CERT_PASSWORD=%CODE_SIGN_PASSWORD%
set TIMESTAMP_SERVER=http://timestamp.sectigo.com
set SIGNTOOL_PATH=C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe

REM Alternative SignTool paths (uncomment if needed)
REM set SIGNTOOL_PATH=C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\signtool.exe
REM set SIGNTOOL_PATH=C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x64\signtool.exe

REM Read version from Version.props
for /f "tokens=2 delims=<>" %%a in ('findstr /C:"<VersionPrefix>" Version.props') do set VERSION=%%a
set INSTALLER_NAME=TimeTrackerSetup_v%VERSION%.exe

echo Configuration:
echo   Installer: %INSTALLER_NAME%
echo   Certificate: %CERT_PATH%
echo   Timestamp: %TIMESTAMP_SERVER%
echo.

REM Check if installer exists
if not exist "%INSTALLER_NAME%" (
    echo ERROR: Installer not found: %INSTALLER_NAME%
    echo.
    echo Please build the installer first using: build-installer.bat
    echo.
    goto end
)

REM Check if certificate exists
if not exist "%CERT_PATH%" (
    echo ERROR: Certificate not found: %CERT_PATH%
    echo.
    echo Please ensure you have:
    echo   1. Purchased a code signing certificate
    echo   2. Saved it as: %CERT_PATH%
    echo.
    echo See CODE_SIGNING_GUIDE.md for details
    echo.
    goto end
)

REM Check if SignTool exists
if not exist "%SIGNTOOL_PATH%" (
    echo ERROR: SignTool not found at: %SIGNTOOL_PATH%
    echo.
    echo Please install Windows SDK or update SIGNTOOL_PATH in this script
    echo Download: https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/
    echo.
    goto end
)

REM Check if password is set
if "%CERT_PASSWORD%"=="" (
    echo ERROR: CODE_SIGN_PASSWORD environment variable not set
    echo.
    echo Please set it with:
    echo   set CODE_SIGN_PASSWORD=YourPassword
    echo.
    echo Or for permanent setting, add to system environment variables
    echo.
    goto end
)

REM Sign the installer
echo Signing installer...
"%SIGNTOOL_PATH%" sign /f "%CERT_PATH%" /p "%CERT_PASSWORD%" /t "%TIMESTAMP_SERVER%" /fd sha256 /v "%INSTALLER_NAME%"

if errorlevel 1 (
    echo.
    echo ========================================
    echo SIGNING FAILED!
    echo ========================================
    echo.
    echo Possible causes:
    echo   - Incorrect certificate password
    echo   - Expired certificate
    echo   - Timestamp server unavailable
    echo   - Certificate format issue
    echo.
    goto end
)

echo.
echo Verifying signature...
"%SIGNTOOL_PATH%" verify /pa /v "%INSTALLER_NAME%"

if errorlevel 1 (
    echo.
    echo WARNING: Signature verification failed!
    echo.
    goto end
)

echo.
echo ========================================
echo SIGNING SUCCESSFUL!
echo ========================================
echo.
echo Signed installer: %INSTALLER_NAME%
echo.
echo You can now distribute this installer.
echo Windows SmartScreen warning should not appear (or be reduced).
echo.

:end
pause
