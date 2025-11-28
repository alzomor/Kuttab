# Quraan App - Installation Instructions

Version: 0.6.0

## 📦 Distribution Packages

This release includes builds for multiple platforms:

- **Windows (x64)**: `Quraan-win-x64-0.6.0.zip`
- **Linux (x64)**: `Quraan-linux-x64-0.6.0.tar.gz`
- **macOS (Intel)**: `Quraan-macOS-x64-0.6.0.tar.gz`
- **macOS (Apple Silicon)**: `Quraan-macOS-arm64-0.6.0.tar.gz`
- **Android**: `Quraan-android-0.6.0.apk`

---

## 🪟 Windows Installation

### Requirements
- Windows 10 or later (64-bit)

### Installation Steps

1. **Download** the file: `Quraan-win-x64-0.6.0.zip`

2. **Extract** the ZIP file:
   - Right-click on the ZIP file
   - Select "Extract All..."
   - Choose a destination folder (e.g., `C:\Program Files\Quraan`)

3. **Run** the application:
   - Navigate to the extracted folder
   - Double-click `QuranSearchApp.exe`

4. **Optional - Create Desktop Shortcut**:
   - Right-click on `QuranSearchApp.exe`
   - Select "Send to" → "Desktop (create shortcut)"

### First Run
- Windows may show a security warning ("Windows protected your PC")
- Click "More info" → "Run anyway"
- This is normal for unsigned applications

---

## 🐧 Linux Installation

### Requirements
- Linux distribution with glibc 2.31+ (Ubuntu 20.04+, Fedora 33+, etc.)
- X11 or Wayland display server

### Installation Steps

1. **Download** the file: `Quraan-linux-x64-0.6.0.tar.gz`

2. **Extract** the archive:
   ```bash
   tar -xzf Quraan-linux-x64-0.6.0.tar.gz
   ```

3. **Move** to installation directory (optional):
   ```bash
   sudo mv Quraan-linux-x64-0.6.0 /opt/quraan
   ```
   Or keep it in your home directory:
   ```bash
   mv Quraan-linux-x64-0.6.0 ~/quraan
   ```

4. **Make executable** (if not already):
   ```bash
   chmod +x ~/quraan/QuranSearchApp
   # or
   chmod +x /opt/quraan/QuranSearchApp
   ```

5. **Run** the application:
   ```bash
   ~/quraan/QuranSearchApp
   # or
   /opt/quraan/QuranSearchApp
   ```

### Optional - Create Desktop Entry

Create file `~/.local/share/applications/quraan.desktop`:

```desktop
[Desktop Entry]
Name=Quraan Search
Comment=Quran Search and Tajweed Application
Exec=/path/to/quraan/QuranSearchApp
Icon=/path/to/quraan/icon.png
Terminal=false
Type=Application
Categories=Education;Literature;
```

Replace `/path/to/quraan/` with your actual installation path.

---

## 🍎 macOS Installation

### Requirements
- macOS 10.15 (Catalina) or later

### Choose Your Version
- **Intel Macs**: Use `Quraan-macOS-x64-0.6.0.tar.gz`
- **Apple Silicon (M1/M2/M3)**: Use `Quraan-macOS-arm64-0.6.0.tar.gz`

### Installation Steps

1. **Download** the appropriate file for your Mac

2. **Extract** the archive:
   - Double-click the `.tar.gz` file in Finder
   - Or use Terminal:
     ```bash
     tar -xzf Quraan-macOS-x64-0.6.0.tar.gz
     # or for Apple Silicon:
     tar -xzf Quraan-macOS-arm64-0.6.0.tar.gz
     ```

3. **Move** to Applications folder (optional):
   ```bash
   mv QuranSearchApp /Applications/
   ```
   Or keep it anywhere you prefer

4. **First Run**:
   - Navigate to the application in Finder
   - Right-click on `QuranSearchApp`
   - Select "Open"
   - Click "Open" in the security dialog

### Troubleshooting macOS

If you see **"QuranSearchApp cannot be opened because the developer cannot be verified"**:

**Option 1 - Using Finder:**
1. Right-click (or Control+click) on the app
2. Select "Open"
3. Click "Open" in the dialog

**Option 2 - Using Terminal:**
```bash
xattr -cr /path/to/QuranSearchApp
```

**Option 3 - System Settings:**
1. Go to System Settings → Privacy & Security
2. Scroll down to the Security section
3. Click "Open Anyway" next to the QuranSearchApp message

---

## 🤖 Android Installation

### Requirements
- Android 5.0 (Lollipop, API 21) or later
- ~30 MB free storage space

### Installation Steps

1. **Enable Unknown Sources** (if needed):
   - Go to Settings → Security (or Privacy)
   - Enable "Install unknown apps" or "Unknown sources"
   - Allow installation from your browser/file manager

2. **Download** the file: `Quraan-android-0.6.0.apk`
   - Transfer to your device if downloaded on computer

3. **Install** the APK:
   - Tap on the downloaded APK file
   - Tap "Install"
   - Wait for installation to complete
   - Tap "Open" or find the app in your app drawer

### Android Permissions

The app may request the following permissions:
- **Storage**: To access Quran text files and cache images
- **Network**: To download Quranic page images (optional)
- **Audio**: To play verse recitations (optional)

---

## 📁 Application Files

After installation, the application includes:

- **QuranSearchApp** - Main executable
- **quran-uthmani.txt** - Quranic text (Uthmani script)
- **rules.json** - Tajweed rules definitions
- **Localization/** - Language files (Arabic, English, German)

### Optional Audio Files

Audio files are **not included** in the distribution to keep file sizes small.

To add audio support:
1. Download the Alhusary recitation folder separately
2. Place it next to the executable with the name "Alhusary"
3. Restart the application

---

## 🌐 Language Support

The application supports:
- **العربية** (Arabic)
- **English**
- **Deutsch** (German)

Change language from the dropdown menu in the top-right corner of the application.

---

## 🔍 Features

- **Search**: Full-text search across the entire Quran
- **Tajweed Rules**: Visual highlighting of Tajweed rules
- **Multiple Languages**: UI available in Arabic, English, and German
- **Audio Playback**: Play verse recitations (requires audio files)
- **Cross-platform**: Works on Windows, Linux, macOS, and Android

---

## 🐛 Troubleshooting

### Application won't start

**Windows:**
- Make sure all files from the ZIP are extracted together
- Try running as Administrator
- Check Windows Defender didn't quarantine any files

**Linux:**
- Ensure the file is executable: `chmod +x QuranSearchApp`
- Check you have required display server (X11/Wayland)
- Try running from terminal to see error messages

**macOS:**
- Remove quarantine attribute: `xattr -cr /path/to/QuranSearchApp`
- Make sure you're using the correct version for your Mac (Intel vs Apple Silicon)

**Android:**
- Enable installation from unknown sources
- Check you have enough storage space
- Try reinstalling if the app crashes

### Missing Quran text

If the app starts but shows no text:
- Ensure `quran-uthmani.txt` is in the same folder as the executable
- Check file permissions (should be readable)

### Audio not working

- Audio files are **not included** by default
- Download Alhusary folder separately
- Place it next to the executable
- Check audio file permissions

---

## 💡 Support

For issues, questions, or feature requests:
- Check the README.md file in the source repository
- Report issues on the project's issue tracker

---

## 📄 License

This application is provided as-is for educational and religious purposes.

---

## ✨ Thank You

Thank you for using the Quraan Search Application. May it be beneficial in your study of the Holy Quran.

جزاكم الله خيراً
