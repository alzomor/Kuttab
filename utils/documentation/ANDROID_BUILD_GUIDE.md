# Android Build Guide for Kuttab Quran Search App

## Current Status

Your Android project is ready to build, but the .NET SDK on your system doesn't include the Android workload needed to compile Android applications.

## Prerequisites

To build Android applications with .NET, you need:

1. **.NET SDK with Android workload** (currently missing)
2. **Java Development Kit (JDK)** ✓ (already installed)
3. **Android SDK and build tools**

## Installation Options

### Option 1: Install Android Workload (Quickest)

Run the provided build script which will attempt to install the Android workload:

```bash
cd /home/hossam-alzomor/Work/Kuttab
chmod +x build-android.sh
./build-android.sh
```

The script will:
- Check your current .NET installation
- Attempt to install the Android workload
- Guide you through any missing dependencies
- Build the APK if everything is set up correctly

### Option 2: Manual Installation

If the automated script doesn't work, follow these steps:

#### Step 1: Install Android SDK Command-Line Tools

```bash
# Create Android SDK directory
mkdir -p ~/Android/Sdk/cmdline-tools

# Download Android command-line tools
cd ~/Downloads
wget https://dl.google.com/android/repository/commandlinetools-linux-11076708_latest.zip

# Extract to SDK directory
unzip commandlinetools-linux-11076708_latest.zip -d ~/Android/Sdk/cmdline-tools
mv ~/Android/Sdk/cmdline-tools/cmdline-tools ~/Android/Sdk/cmdline-tools/latest

# Set environment variables (add to ~/.bashrc)
echo 'export ANDROID_HOME=$HOME/Android/Sdk' >> ~/.bashrc
echo 'export PATH=$PATH:$ANDROID_HOME/cmdline-tools/latest/bin:$ANDROID_HOME/platform-tools' >> ~/.bashrc
source ~/.bashrc
```

#### Step 2: Install Android SDK Components

```bash
# Accept licenses
yes | sdkmanager --licenses

# Install required SDK components
sdkmanager "platform-tools" "platforms;android-34" "build-tools;34.0.0"
```

#### Step 3: Install .NET Android Workload

Try installing with sudo (may require system .NET SDK):

```bash
sudo dotnet workload install android
```

If that doesn't work, install .NET SDK from Microsoft (not Ubuntu repos):

```bash
# Download and run .NET installer
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0

# Add to PATH (add to ~/.bashrc)
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools

# Reload shell
source ~/.bashrc

# Now install Android workload
dotnet workload install android
```

### Option 3: Use Android Studio (Easiest for Beginners)

1. **Download Android Studio**:
   ```bash
   # Download from: https://developer.android.com/studio
   # Or use snap:
   sudo snap install android-studio --classic
   ```

2. **Open Android Studio** and let it install the Android SDK

3. **Set environment variables**:
   ```bash
   echo 'export ANDROID_HOME=$HOME/Android/Sdk' >> ~/.bashrc
   echo 'export PATH=$PATH:$ANDROID_HOME/platform-tools' >> ~/.bashrc
   source ~/.bashrc
   ```

4. **Install .NET Android workload**:
   ```bash
   sudo dotnet workload install android
   ```

## Building the APK

Once the Android workload is installed:

### Using the Build Script

```bash
cd /home/hossam-alzomor/Work/Kuttab
./build-android.sh
```

### Manual Build Commands

```bash
cd /home/hossam-alzomor/Work/Kuttab/QuranSearch.Android

# Clean previous builds
dotnet clean
rm -rf bin obj

# Restore packages
dotnet restore

# Build Debug APK
dotnet build -c Debug -f net8.0-android

# Build Release APK (optimized, smaller size)
dotnet publish -c Release -f net8.0-android
```

### Build Output

The APK will be located at:
- **Debug**: `QuranSearch.Android/bin/Debug/net8.0-android/com.quransearch.android-Signed.apk`
- **Release**: `QuranSearch.Android/bin/Release/net8.0-android/publish/com.quransearch.android-Signed.apk`

## Installing on Android Device

### Via USB (ADB)

1. **Enable Developer Options** on your Android device:
   - Go to Settings → About Phone
   - Tap "Build Number" 7 times
   - Go back to Settings → Developer Options
   - Enable "USB Debugging"

2. **Connect device and install**:
   ```bash
   # Check device is connected
   adb devices
   
   # Install APK
   adb install QuranSearch.Android/bin/Debug/net8.0-android/com.quransearch.android-Signed.apk
   ```

### Manual Installation

1. Copy the APK file to your Android device
2. Open the APK file on your device
3. Allow installation from unknown sources if prompted
4. Install the app

## Troubleshooting

### Error: "Android workload not found"

The .NET SDK from Ubuntu repositories doesn't include mobile workloads. Install .NET from Microsoft directly (see Option 2, Step 3 above).

### Error: "ANDROID_HOME not set"

Set the environment variable:
```bash
export ANDROID_HOME=$HOME/Android/Sdk
export PATH=$PATH:$ANDROID_HOME/platform-tools
```

### Error: "Java not found"

Install OpenJDK:
```bash
sudo apt-get update
sudo apt-get install openjdk-17-jdk
```

### Build succeeds but APK not found

Check these locations:
```bash
find QuranSearch.Android/bin -name "*.apk"
```

### App crashes on startup

Check logcat for errors:
```bash
adb logcat | grep "quransearch"
```

## Project Information

- **Package Name**: `com.quransearch.android`
- **Target Framework**: net8.0-android
- **Minimum Android Version**: API 21 (Android 5.0)
- **Target Android Version**: API 34 (Android 14)

## Next Steps After Building

1. Test the APK on a physical device or emulator
2. For release builds, consider:
   - Signing with a release keystore
   - Enabling ProGuard/R8 for code optimization
   - Testing on multiple Android versions
   - Publishing to Google Play Store

## Additional Resources

- [.NET Android Documentation](https://learn.microsoft.com/en-us/dotnet/android/)
- [Android Developer Guide](https://developer.android.com/guide)
- [.NET Workloads](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-workload-install)
