@echo off
REM Quran Search App - Multi-Platform Publishing Script for Windows
REM This script builds the application for Windows, Linux, and macOS

setlocal enabledelayedexpansion

echo ==========================================
echo Quran Search App - Multi-Platform Build
echo ==========================================
echo.

REM Clean previous builds
echo Cleaning previous builds...
if exist publish rmdir /s /q publish
if exist dist rmdir /s /q dist
mkdir dist

REM Build for Windows x64
echo ==========================================
echo Building for Windows (win-x64)...
echo ==========================================
dotnet publish QuranSearchApp.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false -p:EnableCompressionInSingleFile=true -o publish\win-x64
if errorlevel 1 goto :error

echo Copying audio files...
if exist Alhusary xcopy /E /I /Y Alhusary publish\win-x64\Alhusary

echo Creating distribution package...
cd publish\win-x64
powershell -command "Compress-Archive -Path * -DestinationPath ..\..\dist\Quraan-Windows-x64.zip -Force"
cd ..\..
echo Done: dist\Quraan-Windows-x64.zip
echo.

REM Build for Linux x64
echo ==========================================
echo Building for Linux (linux-x64)...
echo ==========================================
dotnet publish QuranSearchApp.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false -p:EnableCompressionInSingleFile=true -o publish\linux-x64
if errorlevel 1 goto :error

echo Copying audio files...
if exist Alhusary xcopy /E /I /Y Alhusary publish\linux-x64\Alhusary

echo Creating distribution package...
cd publish\linux-x64
tar -czf ..\..\dist\Quraan-Linux-x64.tar.gz *
cd ..\..
echo Done: dist\Quraan-Linux-x64.tar.gz
echo.

REM Build for macOS x64
echo ==========================================
echo Building for macOS Intel (osx-x64)...
echo ==========================================
dotnet publish QuranSearchApp.csproj -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false -p:EnableCompressionInSingleFile=true -o publish\osx-x64
if errorlevel 1 goto :error

echo Copying audio files...
if exist Alhusary xcopy /E /I /Y Alhusary publish\osx-x64\Alhusary

echo Creating distribution package...
cd publish\osx-x64
tar -czf ..\..\dist\Quraan-macOS-Intel.tar.gz *
cd ..\..
echo Done: dist\Quraan-macOS-Intel.tar.gz
echo.

REM Build for macOS ARM64
echo ==========================================
echo Building for macOS ARM (osx-arm64)...
echo ==========================================
dotnet publish QuranSearchApp.csproj -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false -p:EnableCompressionInSingleFile=true -o publish\osx-arm64
if errorlevel 1 goto :error

echo Copying audio files...
if exist Alhusary xcopy /E /I /Y Alhusary publish\osx-arm64\Alhusary

echo Creating distribution package...
cd publish\osx-arm64
tar -czf ..\..\dist\Quraan-macOS-ARM.tar.gz *
cd ..\..
echo Done: dist\Quraan-macOS-ARM.tar.gz
echo.

REM Summary
echo ==========================================
echo Build Summary
echo ==========================================
echo All builds completed successfully!
echo.
echo Distribution packages created in 'dist' directory:
dir /B dist
echo.
echo To run the application:
echo   Windows: Extract ZIP and run QuranSearchApp.exe
echo   Linux:   Extract tar.gz and run ./QuranSearchApp
echo   macOS:   Extract tar.gz and run ./QuranSearchApp
echo.
goto :end

:error
echo.
echo ==========================================
echo Build FAILED!
echo ==========================================
exit /b 1

:end
endlocal
