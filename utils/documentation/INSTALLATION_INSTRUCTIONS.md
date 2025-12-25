# Kuttab - Installation Instructions

Version: 0.8

## 📦 Distribution Packages

This release includes builds for multiple platforms:

- **Windows (x64)**: `Kuttab-win-x64-0.8.zip`
- **Linux (x64)**: `Kuttab-linux-x64-0.8.tar.gz`
- **macOS (Intel)**: `Kuttab-macOS-x64-0.8.tar.gz`
- **macOS (Apple Silicon)**: `Kuttab-macOS-arm64-0.8.tar.gz`
- **Android**: `Kuttab-android-0.8.apk` (for direct installation)
- **Android Bundle**: `Kuttab-android-0.8.aab` (for Google Play Store)

---

## 🪟 Windows Installation

### Requirements
- Windows 10 or later (64-bit)

### Installation Steps

1. **Download** the file: `Kuttab-win-x64-0.8.zip`

2. **Extract** the ZIP file:
   - Right-click on the ZIP file
   - Select "Extract All..."
   - Choose a destination folder (e.g., `C:\Program Files\Kuttab`)

3. **Run** the application:
   - Navigate to the extracted folder
   - Double-click `Kuttab.exe`

4. **Optional - Create Desktop Shortcut**:
   - Right-click on `Kuttab.exe`
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

1. **Download** the file: `Kuttab-linux-x64-0.8.tar.gz`

2. **Extract** the archive:
   ```bash
   tar -xzf Kuttab-linux-x64-0.8.tar.gz
   ```

3. **Move** to installation directory (optional):
   ```bash
   sudo mv Kuttab-linux-x64-0.8 /opt/kuttab
   ```
   Or keep it in your home directory:
   ```bash
   mv Kuttab-linux-x64-0.8 ~/kuttab
   ```

4. **Make executable** (if not already):
   ```bash
   chmod +x ~/kuttab/Kuttab
   # or
   chmod +x /opt/kuttab/Kuttab
   ```

5. **Run** the application:
   ```bash
   ~/kuttab/Kuttab
   # or
   /opt/kuttab/Kuttab
   ```

### Optional - Create Desktop Entry

Create file `~/.local/share/applications/kuttab.desktop`:

```desktop
[Desktop Entry]
Name=Kuttab
Comment=Quran Search and Tajweed Application
Exec=/path/to/kuttab/Kuttab
Icon=/path/to/kuttab/icon.png
Terminal=false
Type=Application
Categories=Education;Literature;
```

Replace `/path/to/kuttab/` with your actual installation path.

---

## 🍎 macOS Installation

### Requirements
- macOS 10.15 (Catalina) or later

### Choose Your Version
- **Intel Macs**: Use `Kuttab-macOS-x64-0.8.tar.gz`
- **Apple Silicon (M1/M2/M3)**: Use `Kuttab-macOS-arm64-0.8.tar.gz`

### Installation Steps

1. **Download** the appropriate file for your Mac

2. **Extract** the archive:
   - Double-click the `.tar.gz` file in Finder
   - Or use Terminal:
     ```bash
     tar -xzf Kuttab-macOS-x64-0.8.tar.gz
     # or for Apple Silicon:
     tar -xzf Kuttab-macOS-arm64-0.8.tar.gz
     ```

3. **Move** to Applications folder (optional):
   ```bash
   mv Kuttab /Applications/
   ```
   Or keep it anywhere you prefer

4. **First Run**:
   - Navigate to the application in Finder
   - Right-click on `Kuttab`
   - Select "Open"
   - Click "Open" in the security dialog

### Troubleshooting macOS

If you see **"Kuttab cannot be opened because the developer cannot be verified"**:

**Option 1 - Using Finder:**
1. Right-click (or Control+click) on the app
2. Select "Open"
3. Click "Open" in the dialog

**Option 2 - Using Terminal:**
```bash
xattr -cr /path/to/Kuttab
```

**Option 3 - System Settings:**
1. Go to System Settings → Privacy & Security
2. Scroll down to the Security section
3. Click "Open Anyway" next to the Kuttab message

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

2. **Download** the file: `Kuttab-android-0.8.apk`
   - Transfer to your device if downloaded on computer
   - Note: For Google Play Store distribution, use `Kuttab-android-0.8.aab`

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

- **Kuttab** - Main executable
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
- Ensure the file is executable: `chmod +x Kuttab`
- Check you have required display server (X11/Wayland)
- Try running from terminal to see error messages

**macOS:**
- Remove quarantine attribute: `xattr -cr /path/to/Kuttab`
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

Thank you for using the Kuttab Application. May it be beneficial in your study of the Holy Quran.

جزاكم الله خيراً
