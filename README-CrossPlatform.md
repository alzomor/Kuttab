# Quran Search - Cross-Platform Application

A comprehensive Quran search application with Tajweed rules highlighting, built for **Web, Windows, Android, and Linux** platforms using .NET MAUI and Blazor.

## 🏗️ Architecture

```
📁 QuranSearch.Core/          # Shared business logic library
├── Models/                   # Data models (QuranAya, TajweedRule)
├── Services/                 # Core services (QuranSearchService, IAudioService)
└── QuranSearch.Core.csproj

📁 QuranSearch.MAUI/          # Cross-platform native app
├── Components/               # Blazor UI components
├── Services/                 # Platform-specific implementations
├── Platforms/                # Platform-specific configurations
└── QuranSearch.MAUI.csproj   # Targets: Windows, Android, Linux

📁 QuranSearch.Web/           # Web application
├── Pages/                    # Blazor WebAssembly pages
├── Services/                 # Web-specific services
└── QuranSearch.Web.csproj    # Blazor WebAssembly

📁 Data Files/
├── quran-uthmani.txt         # Quran text in Uthmani script
└── rules.json                # Tajweed rules configuration
```

## 🚀 Features

### ✅ **Core Functionality**
- **Tajweed Rules Search**: Search by specific Tajweed rules (مد العوض, إخفاء, etc.)
- **Text Highlighting**: Visual highlighting of matched patterns
- **Audio Playback**: Play individual Ayahs, repeat mode, and sequence playback
- **Cross-Platform**: Single codebase for all platforms

### ✅ **Platform Support**
- **🌐 Web**: Blazor WebAssembly (runs in any browser)
- **🖥️ Windows**: Native Windows application
- **📱 Android**: Native Android app
- **🐧 Linux**: Native Linux application

### ✅ **Audio Features**
- Play single Aya
- Repeat mode for memorization
- Play all search results in sequence
- Platform-specific audio implementations

## 🛠️ Build Instructions

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- For Android: Android SDK
- For Linux: PulseAudio (paplay command)

### Building the Web Application
```bash
cd QuranSearch.Web
dotnet build
dotnet run
# Navigate to https://localhost:5001
```

### Building the MAUI Application
```bash
cd QuranSearch.MAUI

# For Windows
dotnet build -f net8.0-windows10.0.19041.0

# For Android (requires Android SDK)
dotnet build -f net8.0-android

# For Linux (experimental)
dotnet build -f net8.0
```

### Building All Projects
```bash
# From root directory
dotnet build QuranSearch.sln
```

## 📦 Deployment

### Web Deployment
```bash
cd QuranSearch.Web
dotnet publish -c Release
# Deploy wwwroot folder to web server
```

### Windows Deployment
```bash
cd QuranSearch.MAUI
dotnet publish -f net8.0-windows10.0.19041.0 -c Release
```

### Android Deployment
```bash
cd QuranSearch.MAUI
dotnet publish -f net8.0-android -c Release
# Generates APK file for distribution
```

## 🎯 Development Workflow

### Single Codebase Benefits
1. **Modify UI once** → applies to all platforms
2. **Update business logic** → shared across all apps
3. **Add new Tajweed rules** → automatically available everywhere
4. **Fix bugs** → fixed on all platforms simultaneously

### Platform-Specific Customizations
- **Audio Services**: Each platform has optimized audio implementation
- **File Access**: Platform-appropriate file system access
- **UI Adaptations**: Responsive design for different screen sizes

## 🔧 Configuration

### Adding New Tajweed Rules
Edit `rules.json`:
```json
{
  "rules": [
    {
      "name": "New Rule Name",
      "cases": [
        {
          "description": "Rule description",
          "regex": "regex pattern"
        }
      ]
    }
  ]
}
```

### Audio Files
Place MP3 files in `AL Husary/` directory with format: `SSSAAA.mp3`
- SSS: 3-digit Surah number (001-114)
- AAA: 3-digit Aya number (001-286)

## 🎨 UI Customization

### Shared Styles
- **CSS**: `wwwroot/css/app.css` (applies to both MAUI and Web)
- **Colors**: Modify `Resources/Styles/Colors.xaml` for MAUI themes
- **Bootstrap**: Web version uses Bootstrap 5 for responsive design

### Arabic Text Support
- **Font**: Amiri font for proper Arabic rendering
- **RTL Support**: Right-to-left text direction
- **Highlighting**: Yellow highlighting for matched Tajweed patterns

## 🚀 Next Steps

### Immediate Improvements
1. **Enhanced Audio**: Add JavaScript-based web audio for better web experience
2. **Offline Support**: PWA capabilities for web version
3. **Search Filters**: Filter by Surah, Juz, or Hizb
4. **Bookmarks**: Save favorite Ayahs

### Platform Enhancements
1. **iOS Support**: Add iOS target to MAUI project
2. **macOS Support**: Add macOS target
3. **Desktop Linux**: Improve Linux desktop integration
4. **Mobile UI**: Optimize mobile layouts

## 📱 Platform-Specific Notes

### Windows
- Uses PowerShell for audio playback
- Full desktop experience with native controls

### Android
- Uses Android MediaPlayer for audio
- Touch-optimized interface
- Supports Android 7.0+ (API 24)

### Linux
- Uses `paplay` command for audio
- Requires PulseAudio installation
- GTK-based native interface

### Web
- Progressive Web App (PWA) ready
- Works offline after initial load
- Cross-browser compatibility

## 🔄 Migration from Original Desktop App

The original Avalonia desktop application has been successfully migrated to this cross-platform architecture:

- ✅ **All functionality preserved**
- ✅ **Same Tajweed rules engine**
- ✅ **Same audio playback features**
- ✅ **Enhanced UI with modern design**
- ✅ **Added web deployment capability**

## 🤝 Contributing

1. Modify shared logic in `QuranSearch.Core`
2. Update UI in Blazor components (shared across platforms)
3. Add platform-specific features in respective platform folders
4. Test on all target platforms before committing

---

**Built with ❤️ using .NET MAUI and Blazor**
