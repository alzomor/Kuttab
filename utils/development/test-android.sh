#!/bin/bash

# Android APK Testing Script for Kuttab Quran Search App

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}=========================================="
echo "Kuttab Android APK Testing Script"
echo -e "==========================================${NC}"
echo ""

# Set paths
APK_PATH="QuranSearch.Android/bin/Release/net8.0-android/publish/com.quransearch.android-Signed.apk"
PACKAGE_NAME="com.quransearch.android"
ADB="$HOME/Android/Sdk/platform-tools/adb"

# Check if APK exists
if [ ! -f "$APK_PATH" ]; then
    echo -e "${RED}✗ APK not found at: $APK_PATH${NC}"
    echo "Please build the APK first using: ./build-android.sh"
    exit 1
fi

echo -e "${GREEN}✓ APK found: $APK_PATH${NC}"
echo -e "  Size: $(du -h "$APK_PATH" | cut -f1)"
echo ""

# Check if ADB is available
if [ ! -f "$ADB" ]; then
    echo -e "${YELLOW}⚠ ADB not found at expected location${NC}"
    echo "Trying system ADB..."
    ADB="adb"
    if ! command -v adb &> /dev/null; then
        echo -e "${RED}✗ ADB not found${NC}"
        echo "Please install Android SDK platform-tools"
        exit 1
    fi
fi

echo -e "${GREEN}✓ ADB found${NC}"
echo ""

# Check for connected devices
echo "Checking for connected devices..."
DEVICES=$($ADB devices | grep -v "List of devices" | grep "device$" | wc -l)

if [ "$DEVICES" -eq 0 ]; then
    echo -e "${YELLOW}⚠ No devices connected${NC}"
    echo ""
    echo "Please choose an option:"
    echo "  1. Connect a physical Android device via USB"
    echo "  2. Start an Android emulator"
    echo "  3. Install Android Studio to create an emulator"
    echo ""
    echo "For physical device:"
    echo "  - Enable Developer Options (tap Build Number 7 times)"
    echo "  - Enable USB Debugging"
    echo "  - Connect via USB"
    echo ""
    echo "For emulator:"
    echo "  - Install Android Studio: sudo snap install android-studio --classic"
    echo "  - Create an AVD (Virtual Device)"
    echo "  - Start it from Android Studio or command line"
    echo ""
    exit 1
fi

echo -e "${GREEN}✓ Found $DEVICES device(s) connected${NC}"
echo ""

# List devices
echo "Connected devices:"
$ADB devices | grep -v "List of devices"
echo ""

# Ask user to confirm
echo "Do you want to install the APK on the connected device? (y/n)"
read -r response

if [[ ! "$response" =~ ^([yY][eE][sS]|[yY])$ ]]; then
    echo "Installation cancelled."
    exit 0
fi

echo ""
echo -e "${BLUE}Installing APK...${NC}"

# Check if app is already installed
if $ADB shell pm list packages | grep -q "$PACKAGE_NAME"; then
    echo -e "${YELLOW}App is already installed. Uninstalling old version...${NC}"
    $ADB uninstall "$PACKAGE_NAME" 2>/dev/null || true
fi

# Install the APK
if $ADB install -r "$APK_PATH"; then
    echo ""
    echo -e "${GREEN}=========================================="
    echo "✓ Installation Successful!"
    echo -e "==========================================${NC}"
    echo ""
    
    # Ask if user wants to launch the app
    echo "Do you want to launch the app now? (y/n)"
    read -r launch_response
    
    if [[ "$launch_response" =~ ^([yY][eE][sS]|[yY])$ ]]; then
        echo ""
        echo -e "${BLUE}Launching app...${NC}"
        $ADB shell am start -n "$PACKAGE_NAME/crc64e1fb321c08285b90.MainActivity" || \
        $ADB shell am start -n "$PACKAGE_NAME/.MainActivity" || \
        $ADB shell monkey -p "$PACKAGE_NAME" -c android.intent.category.LAUNCHER 1
        
        echo ""
        echo -e "${GREEN}App launched!${NC}"
        echo ""
        echo "To view logs in real-time, run:"
        echo "  $ADB logcat | grep -i quran"
        echo ""
    fi
    
    echo ""
    echo "Testing Options:"
    echo "  1. View logs: $ADB logcat | grep com.quransearch"
    echo "  2. Uninstall: $ADB uninstall $PACKAGE_NAME"
    echo "  3. Reinstall: $ADB install -r $APK_PATH"
    echo ""
    echo "App Features to Test:"
    echo "  ✓ Search for Tajweed rules"
    echo "  ✓ View search results"
    echo "  ✓ Play audio recitation"
    echo "  ✓ View Quranic text images"
    echo "  ✓ Change language settings"
    echo ""
    
else
    echo ""
    echo -e "${RED}=========================================="
    echo "✗ Installation Failed"
    echo -e "==========================================${NC}"
    echo ""
    echo "Troubleshooting:"
    echo "  1. Check USB debugging is enabled"
    echo "  2. Check device is authorized (check phone for prompt)"
    echo "  3. Try: $ADB kill-server && $ADB start-server"
    echo "  4. View detailed error: $ADB install -r $APK_PATH"
    exit 1
fi
