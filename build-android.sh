#!/bin/bash

# Android Build Script for Kuttab Quran Search App
# This script helps set up the environment and build the Android APK

set -e

echo "=========================================="
echo "Kuttab Android Build Script"
echo "=========================================="
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if .NET SDK is installed
echo "Checking .NET SDK..."
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}Error: .NET SDK is not installed${NC}"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo -e "${GREEN}✓ .NET SDK version: $DOTNET_VERSION${NC}"
echo ""

# Check for Android workload
echo "Checking for Android workload..."
if dotnet workload list | grep -q "android"; then
    echo -e "${GREEN}✓ Android workload is installed${NC}"
else
    echo -e "${YELLOW}⚠ Android workload is not installed${NC}"
    echo ""
    echo "To build Android apps, you need to install the Android workload."
    echo "This requires:"
    echo "  1. Installing .NET SDK from Microsoft (not from Ubuntu repos)"
    echo "  2. Installing Android SDK and build tools"
    echo ""
    echo "Would you like to install the required components? (y/n)"
    read -r response
    
    if [[ "$response" =~ ^([yY][eE][sS]|[yY])$ ]]; then
        echo ""
        echo "Installing Android workload..."
        echo "This may take several minutes and requires ~5GB of disk space..."
        
        # Try to install Android workload
        if sudo dotnet workload install android; then
            echo -e "${GREEN}✓ Android workload installed successfully${NC}"
        else
            echo -e "${RED}✗ Failed to install Android workload${NC}"
            echo ""
            echo "Manual installation required. Please follow these steps:"
            echo ""
            echo "1. Install .NET SDK from Microsoft:"
            echo "   wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh"
            echo "   chmod +x dotnet-install.sh"
            echo "   ./dotnet-install.sh --channel 8.0"
            echo ""
            echo "2. Add to PATH (add to ~/.bashrc):"
            echo "   export DOTNET_ROOT=\$HOME/.dotnet"
            echo "   export PATH=\$PATH:\$DOTNET_ROOT:\$DOTNET_ROOT/tools"
            echo ""
            echo "3. Install Android workload:"
            echo "   dotnet workload install android"
            echo ""
            exit 1
        fi
    else
        echo "Build cancelled. Please install Android workload manually."
        exit 1
    fi
fi

echo ""

# Check for Java
echo "Checking for Java..."
if ! command -v java &> /dev/null; then
    echo -e "${RED}✗ Java is not installed${NC}"
    echo "Installing OpenJDK 17..."
    sudo apt-get update
    sudo apt-get install -y openjdk-17-jdk
else
    JAVA_VERSION=$(java -version 2>&1 | head -n 1)
    echo -e "${GREEN}✓ Java is installed: $JAVA_VERSION${NC}"
fi

echo ""

# Set Android SDK path if not set
if [ -z "$ANDROID_HOME" ]; then
    echo -e "${YELLOW}⚠ ANDROID_HOME is not set${NC}"
    
    # Check common Android SDK locations
    POSSIBLE_PATHS=(
        "$HOME/Android/Sdk"
        "$HOME/.android/sdk"
        "/usr/lib/android-sdk"
        "/opt/android-sdk"
    )
    
    for path in "${POSSIBLE_PATHS[@]}"; do
        if [ -d "$path" ]; then
            echo -e "${GREEN}✓ Found Android SDK at: $path${NC}"
            export ANDROID_HOME="$path"
            export PATH="$PATH:$ANDROID_HOME/tools:$ANDROID_HOME/platform-tools"
            break
        fi
    done
    
    if [ -z "$ANDROID_HOME" ]; then
        echo -e "${YELLOW}Android SDK not found. You may need to install Android Studio or Android SDK command-line tools.${NC}"
        echo ""
        echo "To install Android SDK command-line tools:"
        echo "1. Download from: https://developer.android.com/studio#command-tools"
        echo "2. Extract to ~/Android/Sdk/cmdline-tools/latest"
        echo "3. Set ANDROID_HOME environment variable"
        echo ""
        echo "Or install Android Studio which includes the SDK."
        echo ""
    fi
else
    echo -e "${GREEN}✓ ANDROID_HOME is set: $ANDROID_HOME${NC}"
fi

echo ""
echo "=========================================="
echo "Building Android APK"
echo "=========================================="
echo ""

# Navigate to Android project directory
cd "$(dirname "$0")/QuranSearch.Android"

# Clean previous builds
echo "Cleaning previous builds..."
rm -rf bin obj
dotnet clean

echo ""

# Restore packages
echo "Restoring NuGet packages..."
dotnet restore

echo ""

# Build the APK
echo "Building APK (Debug configuration)..."
dotnet build -c Debug -f net8.0-android

if [ $? -eq 0 ]; then
    echo ""
    echo -e "${GREEN}=========================================="
    echo "✓ Build Successful!"
    echo "==========================================${NC}"
    echo ""
    
    # Find the APK
    APK_PATH=$(find bin/Debug -name "*.apk" | head -n 1)
    
    if [ -n "$APK_PATH" ]; then
        echo -e "${GREEN}APK Location:${NC}"
        echo "  $APK_PATH"
        echo ""
        echo "File size: $(du -h "$APK_PATH" | cut -f1)"
        echo ""
        echo "To install on device:"
        echo "  adb install \"$APK_PATH\""
        echo ""
        echo "Or copy the APK to your Android device and install manually."
    else
        echo -e "${YELLOW}APK file not found in expected location.${NC}"
        echo "Check the bin/Debug directory for the APK file."
    fi
else
    echo ""
    echo -e "${RED}=========================================="
    echo "✗ Build Failed"
    echo "==========================================${NC}"
    echo ""
    echo "Please check the error messages above."
    exit 1
fi
