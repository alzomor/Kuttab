# Quran Pattern Search Application

A cross-platform application suite for searching Arabic text patterns in the Quran with multimedia support. Available as Avalonia desktop app, MAUI mobile app, and web application.

## Features

- **Pattern Search**: Search for predefined Arabic patterns in the Quran text using Tajweed rules
- **Dropdown Selection**: Choose from predefined rules/patterns stored in `rules.json`
- **Search Results**: Display all matching Ayahs (verses) with full text and highlighting
- **Multiple Occurrences**: If a pattern appears multiple times in one Aya, it's displayed multiple times
- **Audio Controls**: 
  - Play single Aya with Al-Husary recitation
  - Repeat selected Aya continuously
  - Play all found Ayahs in sequence
  - Cross-platform audio support (Linux/macOS/Windows)
- **Picture Display**: Display Quranic text images for selected Ayahs
- **RTL Support**: Right-to-left text display for Arabic content
- **Cross-Platform**: Desktop (Avalonia), Mobile (MAUI), and Web versions

## Project Structure

### Core Components
- `QuranSearchApp.csproj` - Avalonia desktop application
- `QuranSearch.Core/` - Shared business logic and models
- `QuranSearch.MAUI/` - Cross-platform mobile application
- `QuranSearch.Web/` - ASP.NET Core web application
- `QuranSearchWebApi/` - Web API backend

### Data Files
- `quran-uthmani.txt` - Quranic text in Uthmani script
- `rules.json` - Tajweed rules and search patterns with metadata
- `AL Husary/` - Audio files for verse-by-verse recitation
- `QuranText_jpg/` - Quranic text images

### Shared Components
- `Models/` - Data models (QuranAya, TajweedRule)
- `Services/` - Core services (QuranSearchService, AudioService, PictureService)
- `ViewModels/` - MVVM view models for desktop app
- `Views/` - Avalonia UI views and controls

## How to Run

### Prerequisites
- .NET 8.0 SDK
- For desktop: Linux/Windows/macOS desktop environment
- For mobile: Android SDK or iOS development tools
- For web: Any modern web browser

### Avalonia Desktop Application
```bash
# Clean and run the desktop app
rm -rf obj bin
dotnet run --project QuranSearchApp.csproj
```

### MAUI Mobile Application
```bash
# For Android
dotnet build QuranSearch.MAUI/QuranSearch.MAUI.csproj -f net8.0-android
dotnet run --project QuranSearch.MAUI/QuranSearch.MAUI.csproj -f net8.0-android

# For iOS (macOS only)
dotnet build QuranSearch.MAUI/QuranSearch.MAUI.csproj -f net8.0-ios
```

### Web Application
```bash
# Run the web version
dotnet run --project QuranSearch.Web/QuranSearch.Web.csproj
# Access at http://localhost:5000
```

### Usage
1. Select a pattern from the dropdown menu
2. Click "Search" to find all occurrences
3. Browse results in the list box
4. Select an Aya to view details
5. Use audio controls to play selected or all Ayahs
6. Images will be displayed in the picture box (when implemented)

## Technical Details

### Avalonia Desktop App
- **Framework**: .NET 8.0
- **UI Framework**: Avalonia UI 11.0.10
- **Architecture**: MVVM pattern with ReactiveUI
- **Audio**: Cross-platform system command integration
- **Text Rendering**: RTL support for Arabic text

### MAUI Mobile App
- **Framework**: .NET 8.0 MAUI
- **Platforms**: Android, iOS
- **Shared Logic**: QuranSearch.Core library
- **UI**: Native platform controls

### Web Application
- **Framework**: ASP.NET Core 8.0
- **Frontend**: Blazor Server/WebAssembly
- **API**: RESTful web services
- **Deployment**: Cross-platform web hosting

### Data Format
- **Text Format**: Pipe-separated format (Surah|Aya|Text)
- **Rules Format**: JSON with pattern metadata and descriptions
- **Audio Format**: MP3 files with 6-digit naming (SSS+AAA)
- **Images**: JPG format for Quranic text visualization

### Search Algorithm
- **Pattern Matching**: Regex-based with Tajweed rule support
- **Highlighting**: Multi-occurrence text highlighting
- **Performance**: Optimized for large text corpus search

## Audio Integration

The application includes full audio playback capabilities:
- **Reciter**: Sheikh Mahmoud Khalil Al-Husary
- **Format**: Verse-by-verse MP3 files
- **Controls**: Play, repeat, sequence playback, stop
- **Cross-platform**: Linux (paplay), macOS (afplay), Windows (PowerShell)

## Troubleshooting

### Desktop Application
- Ensure GUI environment is running
- For headless systems: `DISPLAY=:0 dotnet run --project QuranSearchApp.csproj`
- Clean build artifacts if encountering errors: `rm -rf obj bin`

### MAUI Application
- Install required workloads: `dotnet workload install maui`
- For Android: Ensure Android SDK is properly configured
- For iOS: Requires macOS with Xcode

### Web Application
- Check port availability (default: 5000)
- Ensure firewall allows web traffic
- For production: Configure proper hosting environment
