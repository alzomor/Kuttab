#!/bin/bash
# Build Release APK for Testing
# Usage: ./build-release-apk.sh (run from anywhere)

set -e

echo "=========================================="
echo "Building Kuttab Release APK"
echo "=========================================="

# Get the script directory and navigate to project root
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_DIR="$( cd "$SCRIPT_DIR/../.." && pwd )"

echo "Project directory: $PROJECT_DIR"
cd "$PROJECT_DIR"

# Check if keystore password is set
if [ -z "$KEYSTORE_PASSWORD" ]; then
    echo "ERROR: KEYSTORE_PASSWORD environment variable is not set"
    echo "Please set it with: export KEYSTORE_PASSWORD='your-password'"
    exit 1
fi

# Check if keystore exists
if [ ! -f "kuttab-release.keystore" ]; then
    echo "ERROR: kuttab-release.keystore not found in $PROJECT_DIR"
    echo "Please create it first using the instructions in signing.md"
    exit 1
fi

echo "Building APK for testing..."
# Use the .NET with Android workload
ANDROID_HOME=~/Android/Sdk ~/.dotnet-android/dotnet publish -c Release -f net8.0-android -p:AndroidPackageFormat=apk Kuttab.Android.csproj

# Find the signed APK
APK_PATH="bin/Release/net8.0-android/publish/com.kuttab.app-Signed.apk"

if [ -f "$APK_PATH" ]; then
    echo "✅ Build successful!"
    echo "APK location: $APK_PATH"
    
    # Copy to publishing folder
    cp "$APK_PATH" "play-store-publishing/com.kuttab.app-v0.82.apk"
    echo "✅ Copied to: play-store-publishing/com.kuttab.app-v0.82.apk"
    
    # Show file size
    SIZE=$(du -h "$APK_PATH" | cut -f1)
    echo "File size: $SIZE"
    
    echo ""
    echo "Next steps:"
    echo "1. Install on device: adb install -r play-store-publishing/com.kuttab.app-v0.82.apk"
    echo "2. Test all features"
else
    echo "❌ Build failed - APK not found"
    exit 1
fi
