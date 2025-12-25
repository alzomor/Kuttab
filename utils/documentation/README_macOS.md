# Quran Search App - macOS Installation

## Running the Unsigned App

Since this is a test build without an Apple Developer certificate, macOS will block it by default. Here's how to run it:

### Option 1: Right-Click Method (Recommended)
1. Download and extract the app
2. **Right-click** (or Control+click) on `QuranSearchApp.app`
3. Select **Open** from the menu
4. Click **Open** in the security dialog
5. The app will now run (you only need to do this once)

### Option 2: Terminal Method
Open Terminal and run:
```bash
# Navigate to where you extracted the app
cd /path/to/app/folder

# Remove quarantine flag
xattr -cr QuranSearchApp.app

# Run the app
open QuranSearchApp.app
```

## Troubleshooting

If you see "App is damaged and can't be opened":
```bash
xattr -cr QuranSearchApp.app
```

If you see security warnings:
- Go to **System Preferences → Security & Privacy**
- Click **Open Anyway** for QuranSearchApp

## Note
This is a test build for development purposes. Future releases will be properly signed for easier installation.
