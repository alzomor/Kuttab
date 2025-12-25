# Publishing Quran Search App

This document describes how to build and publish the Quran Search App for multiple platforms.

## Quick Start

### On Linux/macOS:
```bash
./publish.sh
```

### On Windows:
```cmd
publish.bat
```

## Manual Publishing

If you prefer to build manually for specific platforms:

### Windows (x64)
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/win-x64
```

### Linux (x64)
```bash
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o publish/linux-x64
```

### macOS (Intel)
```bash
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -o publish/osx-x64
```

### macOS (Apple Silicon)
```bash
dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -o publish/osx-arm64
```

## Output

After running the publish script, you'll find:

- **publish/** - Contains the built applications for each platform
- **dist/** - Contains packaged distributions ready for distribution
  - `Quraan-Windows-x64.zip` - Windows package
  - `Quraan-Linux-x64.tar.gz` - Linux package
  - `Quraan-macOS-Intel.tar.gz` - macOS Intel package
  - `Quraan-macOS-ARM.tar.gz` - macOS Apple Silicon package

## Distribution

### Windows
1. Extract the ZIP file
2. Run `QuranSearchApp.exe`
3. Ensure the `Alhusary` folder is in the same directory for audio playback

### Linux
1. Extract the tar.gz file: `tar -xzf Quraan-Linux-x64.tar.gz`
2. Make executable: `chmod +x QuranSearchApp`
3. Run: `./QuranSearchApp`
4. Ensure the `Alhusary` folder is in the same directory for audio playback

### macOS
1. Extract the tar.gz file: `tar -xzf Quraan-macOS-*.tar.gz`
2. Make executable: `chmod +x QuranSearchApp`
3. Run: `./QuranSearchApp`
4. You may need to allow the app in System Preferences > Security & Privacy
5. Ensure the `Alhusary` folder is in the same directory for audio playback

## Build Options Explained

- **-c Release**: Build in Release configuration (optimized)
- **-r [runtime]**: Target runtime identifier (win-x64, linux-x64, osx-x64, osx-arm64)
- **--self-contained true**: Include .NET runtime (no need to install .NET separately)
- **-p:PublishSingleFile=true**: Bundle everything into a single executable
- **-p:PublishTrimmed=false**: Don't trim unused assemblies (safer for Avalonia apps)
- **-p:IncludeNativeLibrariesForSelfExtract=true**: Include native libraries

## Requirements

- .NET 8.0 SDK installed
- For creating tar.gz on Windows: WSL or tar utility installed

## Troubleshooting

### Audio Not Working
- Ensure the `Alhusary` folder with MP3 files is in the same directory as the executable
- On Linux: Install `mpv`, `mpg123`, or `paplay`
- On macOS: `afplay` is built-in
- On Windows: PowerShell MediaPlayer is used (built-in)

### Permission Denied (Linux/macOS)
```bash
chmod +x QuranSearchApp
```

### macOS Security Warning
Right-click the app and select "Open" the first time, or go to System Preferences > Security & Privacy and allow the app.

## Version Information

The version is automatically read from `QuranSearchApp.csproj`:
```xml
<Version>0.6.0</Version>
```

Update this version number before publishing new releases.
