@echo off
echo ========================================
echo Building Time Tracker
echo ========================================

echo.
echo Cleaning previous builds...
if exist publish rmdir /s /q publish
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj

echo.
echo Publishing application...
dotnet publish TimeTracker.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish

if errorlevel 1 (
    echo.
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo Build completed successfully!
echo Output: .\publish\TimeTracker.exe

echo.
echo Creating installer...
if not exist installer mkdir installer

set INNO_PATH=C:\Program Files (x86)\Inno Setup 6\ISCC.exe
if exist "%INNO_PATH%" (
    "%INNO_PATH%" setup.iss
    if errorlevel 1 (
        echo.
        echo Installer creation failed!
    ) else (
        echo.
        echo Installer created successfully!
        echo Output: .\installer\TimeTrackerSetup.exe
    )
) else (
    echo Inno Setup not found at: %INNO_PATH%
    echo Skipping installer creation.
    echo Download Inno Setup from: https://jrsoftware.org/isdl.php
)

echo.
echo Done!
pause
