#!/bin/bash
# Build Release AAB for Google Play Store
# Usage: ./build-release-aab.sh (run from anywhere)

set -e

echo "=========================================="
echo "Building Kuttab Release AAB"
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

echo "Building AAB for Play Store..."
dotnet publish -c Release -f net8.0-android -p:AndroidPackageFormat=aab Kuttab.Android.csproj

# Find the signed AAB
AAB_PATH="bin/Release/net8.0-android/publish/com.kuttab.app-Signed.aab"

if [ -f "$AAB_PATH" ]; then
    echo "✅ Build successful!"
    echo "AAB location: $AAB_PATH"
    
    # Copy to publishing folder
    cp "$AAB_PATH" "play-store-publishing/com.kuttab.app-v0.81.aab"
    echo "✅ Copied to: play-store-publishing/com.kuttab.app-v0.81.aab"
    
    # Show file size
    SIZE=$(du -h "$AAB_PATH" | cut -f1)
    echo "File size: $SIZE"
    
    echo ""
    echo "Next steps:"
    echo "1. Test the AAB on a device"
    echo "2. Upload to Google Play Console"
else
    echo "❌ Build failed - AAB not found"
    exit 1
fi
