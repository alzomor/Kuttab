# Audio & Picture Caching System

## Overview
The application now uses a **smart caching system** for both audio and picture files:
- **Default mode**: Remote audio/pictures (downloads and caches automatically)
- Files are downloaded from the internet **once** and cached locally
- Subsequent requests use the **cached files** (no re-download)
- Works **offline** after files are cached
- Cache persists across sessions
- Cache location: `AL Husary/` and `QuranText_jpg/` folders in the executable directory
  - During development: `bin/Debug/net8.0/AL Husary/` and `bin/Debug/net8.0/QuranText_jpg/`
  - After publishing: Same directory as the executable

## How It Works
1. When you request an audio/picture file:
   - App checks the local cache folder first
   - If found: uses the cached file (instant load)
   - If not found: downloads and saves to cache (one-time download)
2. Next time you request the same file: instant load from cache

## Common Issues

### 1. No Audio Players Available (Linux)
**Symptom**: Audio files are downloaded but don't play on Linux machines.

**Solution**: Install at least one audio player:
```bash
# Ubuntu/Debian
sudo apt-get install mpg123
# or
sudo apt-get install pulseaudio-utils  # for paplay
# or
sudo apt-get install ffmpeg  # for ffplay

# Fedora/RHEL
sudo dnf install mpg123
# or
sudo dnf install pulseaudio-utils
# or
sudo dnf install ffmpeg

# Arch
sudo pacman -S mpg123
# or
sudo pacman -S pulseaudio
# or
sudo pacman -S ffmpeg
```

**Verification**: Check which players are available:
```bash
which mpg123 paplay ffplay aplay
```

### 2. Network/Firewall Issues
**Symptom**: Audio downloads fail or timeout.

**Solution**: 
- Check internet connection
- Verify firewall allows outbound HTTPS connections
- Test manual download:
```bash
curl -o test.mp3 https://everyayah.com/data/Husary_128kbps/001001.mp3
```

### 3. Windows PowerShell Restrictions
**Symptom**: Audio doesn't play on Windows.

**Solution**: Ensure PowerShell is not restricted:
```powershell
# Check execution policy
Get-ExecutionPolicy

# If Restricted, run as Administrator:
Set-ExecutionPolicy RemoteSigned
```

## Debugging Steps

### Step 1: Enable Console Logging
Run the application from terminal/command prompt to see detailed logs:

**Linux/macOS**:
```bash
cd /path/to/QuranSearchApp
dotnet run --project QuranSearchApp.csproj
```

**Windows**:
```cmd
cd C:\path\to\QuranSearchApp
dotnet run --project QuranSearchApp.csproj
```

### Step 2: Check Console Output
Look for these log messages:

**Successful Download**:
```
[AudioService] Downloading from: https://everyayah.com/data/Husary_128kbps/001001.mp3
[AudioService] Temp file: /tmp/001001_xxxxx.mp3
[AudioService] Response status: OK
[AudioService] Downloaded file size: 82164 bytes
```

**Successful Playback Start (with caching)**:
```
[AudioService] Using cached file: /path/to/AL Husary/001001.mp3
[AudioService] Using audio player: mpg123
[AudioService] Starting playback: mpg123 -q "/path/to/AL Husary/001001.mp3"
[AudioService] File path: /path/to/AL Husary/001001.mp3
[AudioService] File exists: True
```

**Error Examples**:
```
[AudioService] No audio player found on Linux. Tried: mpg123, paplay, ffplay, aplay
[AudioService] Download failed: The request timed out
[AudioService] Exception in StartAudioPlaybackAsync: Cannot find the specified file
```

### Step 3: Test Manual Playback
Download a test file and try playing it manually:

**Linux**:
```bash
# Download
curl -o /tmp/test.mp3 https://everyayah.com/data/Husary_128kbps/001001.mp3

# Test playback with each player
mpg123 /tmp/test.mp3
paplay /tmp/test.mp3
ffplay -nodisp -autoexit /tmp/test.mp3
aplay /tmp/test.mp3  # Note: aplay only works with WAV files, not MP3
```

**Windows**:
```powershell
# Download
Invoke-WebRequest -Uri "https://everyayah.com/data/Husary_128kbps/001001.mp3" -OutFile "$env:TEMP\test.mp3"

# Test playback
$player = New-Object System.Windows.Media.MediaPlayer
$player.Open("$env:TEMP\test.mp3")
$player.Play()
Start-Sleep -Seconds 10
```

## Audio Player Priority (Linux)

The application tries these players in order:
1. **mpg123** (recommended) - lightweight, reliable MP3 player
2. **paplay** - PulseAudio player (requires PulseAudio)
3. **ffplay** - FFmpeg player (powerful but heavier)
4. **aplay** - ALSA player (doesn't support MP3 natively)

## Recommended Setup

### For Best Compatibility (Linux)
Install `mpg123`:
```bash
sudo apt-get install mpg123  # Ubuntu/Debian
sudo dnf install mpg123      # Fedora
sudo pacman -S mpg123        # Arch
```

### For Windows
No installation needed - uses built-in PowerShell with Windows Media Foundation (MediaPlayer class for MP3 support).

### For macOS
No installation needed - uses built-in `afplay`.

## Playback Controls

### Audio Control Buttons
- **Play Single**: Plays the selected Aya once
- **Play Repeat**: Plays the selected Aya continuously until stopped
- **Play All**: Plays all search results in sequence
- **Stop**: Stops playback immediately and resets UI state

### UI Indicators
- **Red dot + "Playing" text**: Indicates active playback
- **Repeat indicator**: Shows when repeat mode is active
- **Stop button (red)**: Active only during playback

### Expected Behavior
- When playback finishes naturally, the UI automatically resets (stop button becomes inactive, play buttons become active)
- Pressing Stop immediately stops playback and resets all UI states in a single press
- All UI updates are dispatched on the UI thread for immediate visual feedback

## Cache Management

### View Cache Location
The cache folders are located in the same directory as the application executable:
- **Audio cache**: `<executable-directory>/AL Husary/`
- **Picture cache**: `<executable-directory>/QuranText_jpg/`

### Cache Size
- Each audio file: ~80-350 KB (depending on verse length)
- Each picture file: varies based on image content
- Total size depends on how many files you've accessed

### Clear Cache
To clear the cache and force re-download:
```bash
# Linux/macOS
rm -rf "AL Husary"/*
rm -rf "QuranText_jpg"/*

# Windows
del /Q "AL Husary\*.*"
del /Q "QuranText_jpg\*.*"
```

### Pre-populate Cache
To download all files in advance (for offline use):
1. Enable "Use Remote Audio" and "Use Remote Images" options
2. Search for a rule that returns many results
3. Click "Play All" - this will download all audio files
4. Select each result to view - this will download all pictures
5. After first download, all files are cached for offline use

## Still Having Issues?

1. **Check application logs** - Run from terminal to see console output
2. **Verify audio works locally** - Switch to local audio files to isolate the issue
3. **Test network connectivity** - Try downloading files manually
4. **Check system requirements** - Ensure .NET 8.0 runtime is installed
5. **File permissions** - Ensure cache directories are writable
6. **Check disk space** - Ensure sufficient space for cache

## Contact
If issues persist, provide the console log output when reporting the problem.
