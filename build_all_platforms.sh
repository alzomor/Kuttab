#!/bin/bash

# Exit on error
set -e

# Clean previous builds
rm -rf ./publish/*

# Function to publish for a specific runtime
publish_for_runtime() {
    local runtime=$1
    echo "\n🔨 Building for $runtime..."
    
    dotnet publish -c Release \
        -r $runtime \
        --self-contained true \
        -p:PublishSingleFile=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -p:DebugType=None \
        -p:DebugSymbols=false \
        -p:EnableCompressionInSingleFile=true \
        -o ./publish/$runtime
        
    echo "✅ Successfully built for $runtime"
}

# Build for Windows
publish_for_runtime "win-x64"

# Build for Linux
publish_for_runtime "linux-x64"

# Build for macOS (Intel)
publish_for_runtime "osx-x64"

# Build for macOS (Apple Silicon)
publish_for_runtime "osx-arm64"

echo "\n🎉 All builds completed successfully!"
echo "📁 Output directory: $(pwd)/publish"

# Create archives for distribution
echo "\n📦 Creating distribution packages..."

# Windows ZIP
cd ./publish/win-x64
zip -r ../../Quraan-Windows-x64.zip .
cd ../..

# Linux TAR.GZ
cd ./publish/linux-x64
tar -czf ../../Quraan-Linux-x64.tar.gz .
cd ../..

# macOS (Intel) TAR.GZ
cd ./publish/osx-x64
tar -czf ../../Quraan-macOS-Intel.tar.gz .
cd ../..

# macOS (Apple Silicon) TAR.GZ
cd ./publish/osx-arm64
tar -czf ../../Quraan-macOS-AppleSilicon.tar.gz .
cd ../..

echo "\n📦 Distribution packages created:"
ls -lh Quraan-*.{zip,tar.gz}

echo "\n🚀 All done! You can find the distribution packages in the project root directory."
