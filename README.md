# Quran Pattern Search Application

An Avalonia-based GUI application for searching Arabic text patterns in the Quran with multimedia support.

## Features

- **Pattern Search**: Search for predefined Arabic patterns in the Quran text
- **Dropdown Selection**: Choose from predefined rules/patterns stored in `rules.txt`
- **Search Results**: Display all matching Ayahs (verses) with full text
- **Multiple Occurrences**: If a pattern appears multiple times in one Aya, it's displayed multiple times
- **Audio Controls**: 
  - Play single Aya
  - Repeat selected Aya multiple times
  - Play all found Ayahs in sequence
- **Picture Display**: Area for displaying images related to selected Aya
- **RTL Support**: Right-to-left text display for Arabic content

## Files Structure

- `QuranSearchApp.csproj` - Project configuration
- `quran-uthmani.txt` - Quranic text in Uthmani script
- `rules.txt` - Predefined search patterns
- `Models/QuranAya.cs` - Data model for Quranic verses
- `Services/QuranSearchService.cs` - Core search functionality
- `ViewModels/MainWindowViewModel.cs` - UI logic and data binding
- `Views/MainWindow.axaml` - Main window UI layout
- `Program.cs` - Application entry point

## How to Run

### Prerequisites
- .NET 8.0 SDK
- Linux desktop environment with X11 or Wayland

### Build and Run
```bash
# Build the application
dotnet build QuranSearchApp.csproj

# Run the application
dotnet run --project QuranSearchApp.csproj
```

### Usage
1. Select a pattern from the dropdown menu
2. Click "Search" to find all occurrences
3. Browse results in the list box
4. Select an Aya to view details
5. Use audio controls to play selected or all Ayahs
6. Images will be displayed in the picture box (when implemented)

## Technical Details

- **Framework**: .NET 8.0
- **UI Framework**: Avalonia UI 11.0.10
- **Architecture**: MVVM pattern with ReactiveUI
- **Text Format**: Pipe-separated format (Surah|Aya|Text)
- **Search Algorithm**: Case-insensitive string matching with occurrence counting

## Future Enhancements

- Audio file integration and playback
- Image loading and display
- Custom pattern creation
- Export/import functionality
- Multi-language support

## Troubleshooting

If you encounter display issues:
- Ensure you have a GUI environment running
- Try running with `DISPLAY=:0 dotnet run --project QuranSearchApp.csproj`
- For headless systems, consider using X11 forwarding or VNC
