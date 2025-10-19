#!/bin/bash

# Quran Search App - Multi-Platform Publishing Script
# This script builds the application for Windows, Linux, and macOS

set -e  # Exit on error

echo "=========================================="
echo "Quran Search App - Multi-Platform Build"
echo "=========================================="
echo ""

# Get version from project file
VERSION=$(grep -oP '<Version>\K[^<]+' QuranSearchApp.csproj)
echo "Building version: $VERSION"
echo ""

# Clean previous builds
echo "Cleaning previous builds..."
rm -rf publish/
rm -rf dist/
mkdir -p dist

# Build configurations
declare -a PLATFORMS=(
    "win-x64:Windows"
    "linux-x64:Linux"
    "osx-x64:macOS-Intel"
    "osx-arm64:macOS-ARM"
)

# Function to publish for a specific platform
publish_platform() {
    local RID=$1
    local PLATFORM_NAME=$2
    
    echo "=========================================="
    echo "Building for $PLATFORM_NAME ($RID)..."
    echo "=========================================="
    
    # Publish the application
    dotnet publish QuranSearchApp.csproj \
        -c Release \
        -r $RID \
        --self-contained true \
        -p:PublishSingleFile=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -p:DebugType=None \
        -p:DebugSymbols=false \
        -p:EnableCompressionInSingleFile=true \
        -o "publish/$RID"
    
    if [ $? -eq 0 ]; then
        echo "✓ Build successful for $PLATFORM_NAME"
        
        # Copy audio files if they exist
        if [ -d "Alhusary" ]; then
            echo "  Copying audio files..."
            cp -r Alhusary "publish/$RID/"
        fi
        
        # Create distribution package
        echo "  Creating distribution package..."
        cd publish/$RID
        
        if [[ $RID == win-* ]]; then
            # Windows: Create ZIP
            zip -r "../../dist/Quraan-$PLATFORM_NAME-$VERSION.zip" . > /dev/null
            echo "  ✓ Created: dist/Quraan-$PLATFORM_NAME-$VERSION.zip"
        else
            # Linux/macOS: Create tar.gz
            tar -czf "../../dist/Quraan-$PLATFORM_NAME-$VERSION.tar.gz" . > /dev/null
            echo "  ✓ Created: dist/Quraan-$PLATFORM_NAME-$VERSION.tar.gz"
        fi
        
        cd ../..
        echo ""
    else
        echo "✗ Build failed for $PLATFORM_NAME"
        exit 1
    fi
}

# Build for all platforms
for platform in "${PLATFORMS[@]}"; do
    IFS=':' read -r RID PLATFORM_NAME <<< "$platform"
    publish_platform "$RID" "$PLATFORM_NAME"
done

# Summary
echo "=========================================="
echo "Build Summary"
echo "=========================================="
echo "All builds completed successfully!"
echo ""
echo "Distribution packages created in 'dist/' directory:"
ls -lh dist/
echo ""
echo "To run the application:"
echo "  Windows: Extract ZIP and run QuranSearchApp.exe"
echo "  Linux:   Extract tar.gz and run ./QuranSearchApp"
echo "  macOS:   Extract tar.gz and run ./QuranSearchApp"
echo ""
