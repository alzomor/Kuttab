# Testing Kuttab Android APK

## Build Status: ✅ SUCCESS

Your Android APK has been built successfully!

**APK Location**: `QuranSearch.Android/bin/Release/net8.0-android/publish/com.quransearch.android-Signed.apk`
**APK Size**: 96 MB
**Package Name**: com.quransearch.android
**Target Android**: API 34 (Android 14)
**Minimum Android**: API 21 (Android 5.0)

---

## Testing Options

### Option 1: Test on Physical Android Device (Recommended)

#### Step 1: Enable USB Debugging on Your Phone
1. Go to **Settings** → **About Phone**
2. Tap **Build Number** 7 times to enable Developer Options
3. Go back to **Settings** → **Developer Options**
4. Enable **USB Debugging**

#### Step 2: Connect Your Device
```bash
# Connect your phone via USB cable
# Check if device is detected
~/Android/Sdk/platform-tools/adb devices
```

You should see your device listed. If it shows "unauthorized", check your phone for a permission prompt.

#### Step 3: Install the APK
```bash
cd /home/hossam-alzomor/Work/Kuttab
~/Android/Sdk/platform-tools/adb install -r QuranSearch.Android/bin/Release/net8.0-android/publish/com.quransearch.android-Signed.apk
```

#### Step 4: Launch the App
The app will appear in your app drawer as "Kuttab" or "Quran Search". You can also launch it via ADB:
```bash
~/Android/Sdk/platform-tools/adb shell am start -n com.quransearch.android/com.quransearch.android.MainActivity
```

#### Step 5: View Logs (for debugging)
```bash
# View real-time logs
~/Android/Sdk/platform-tools/adb logcat | grep -i quran

# Or filter by package
~/Android/Sdk/platform-tools/adb logcat | grep com.quransearch
```

---

### Option 2: Install Android Studio and Use Emulator

#### Step 1: Install Android Studio
```bash
# Option A: Using snap (easiest)
sudo snap install android-studio --classic

# Option B: Download from website
# Visit: https://developer.android.com/studio
# Download and extract to ~/android-studio
```

#### Step 2: Launch Android Studio
```bash
# If installed via snap
android-studio

# If installed manually
~/android-studio/bin/studio.sh
```

#### Step 3: Create an Emulator (AVD)
1. Open Android Studio
2. Click **More Actions** → **Virtual Device Manager**
3. Click **Create Device**
4. Select a device (e.g., Pixel 6)
5. Select a system image (e.g., Android 14 - API 34)
6. Click **Finish**

#### Step 4: Start the Emulator
```bash
# List available emulators
~/Android/Sdk/emulator/emulator -list-avds

# Start an emulator (replace with your AVD name)
~/Android/Sdk/emulator/emulator -avd Pixel_6_API_34 &
```

#### Step 5: Install APK on Emulator
```bash
# Wait for emulator to boot (check with adb devices)
~/Android/Sdk/platform-tools/adb devices

# Install the APK
~/Android/Sdk/platform-tools/adb install -r QuranSearch.Android/bin/Release/net8.0-android/publish/com.quransearch.android-Signed.apk
```

---

### Option 3: Manual Installation (No Computer Required)

1. **Copy APK to Phone**:
   - Copy `com.quransearch.android-Signed.apk` to your phone via USB, email, or cloud storage
   
2. **Enable Unknown Sources**:
   - Go to **Settings** → **Security** → Enable **Install Unknown Apps**
   - Select the file manager app you'll use to install

3. **Install**:
   - Open the APK file on your phone
   - Tap **Install**
   - Tap **Open** to launch the app

---

## Testing Checklist

Once the app is installed, test the following features:

### Basic Functionality
- [ ] App launches without crashing
- [ ] Main screen displays correctly
- [ ] Arabic text displays properly (RTL)
- [ ] Rule dropdown/spinner loads Tajweed rules

### Search Features
- [ ] Select a Tajweed rule from dropdown
- [ ] Click search button
- [ ] Results display correctly
- [ ] Multiple occurrences show properly
- [ ] Highlighting works on matched text

### Audio Features
- [ ] Play single Aya audio
- [ ] Repeat single Aya
- [ ] Play all found Ayahs in sequence
- [ ] Stop audio playback
- [ ] Audio controls respond correctly

### Picture Features
- [ ] Quranic text images load
- [ ] Images display correctly
- [ ] Zoom/pan works (if implemented)

### Settings
- [ ] Language switching works
- [ ] Settings persist after app restart
- [ ] Donate button/screen works

### Performance
- [ ] App is responsive
- [ ] No lag during search
- [ ] Smooth scrolling through results
- [ ] Memory usage is acceptable

---

## Troubleshooting

### Device Not Detected
```bash
# Restart ADB server
~/Android/Sdk/platform-tools/adb kill-server
~/Android/Sdk/platform-tools/adb start-server

# Check USB connection mode (should be File Transfer/MTP)
```

### Installation Failed
```bash
# Uninstall old version first
~/Android/Sdk/platform-tools/adb uninstall com.quransearch.android

# Try installing again
~/Android/Sdk/platform-tools/adb install -r QuranSearch.Android/bin/Release/net8.0-android/publish/com.quransearch.android-Signed.apk
```

### App Crashes on Launch
```bash
# View crash logs
~/Android/Sdk/platform-tools/adb logcat -d | grep -A 50 "FATAL EXCEPTION"

# Or use logcat with filters
~/Android/Sdk/platform-tools/adb logcat AndroidRuntime:E *:S
```

### Permissions Issues
- Check that the app has necessary permissions in Settings → Apps → Kuttab → Permissions
- Required permissions: Storage (for Quran text/audio files), Network (for downloading images)

---

## Quick Test Script

I've created a test script for you. Run it to automatically test the installation:

```bash
cd /home/hossam-alzomor/Work/Kuttab
chmod +x test-android.sh
./test-android.sh
```

---

## Next Steps After Testing

### If Everything Works:
1. Consider building a signed release APK for distribution
2. Test on multiple Android versions (API 21-34)
3. Optimize APK size if needed
4. Prepare for Google Play Store submission

### If Issues Found:
1. Check logcat output for errors
2. Review crash reports
3. Fix issues in the code
4. Rebuild and test again

---

## Build Information

- **Built with**: .NET 8.0.416 with Android workload
- **Android SDK**: API 34
- **Build Tools**: 34.0.0
- **Build Date**: December 24, 2025
- **Build Type**: Release (optimized)

---

## Additional Resources

- [Android Debug Bridge (ADB) Documentation](https://developer.android.com/studio/command-line/adb)
- [Android Emulator Guide](https://developer.android.com/studio/run/emulator)
- [.NET Android Documentation](https://learn.microsoft.com/en-us/dotnet/android/)
