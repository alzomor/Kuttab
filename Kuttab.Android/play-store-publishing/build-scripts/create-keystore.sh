#!/bin/bash
# Create Release Keystore for App Signing
# Usage: ./create-keystore.sh (run from anywhere)

set -e

echo "=========================================="
echo "Creating Kuttab Release Keystore"
echo "=========================================="

# Get the script directory and navigate to project root
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_DIR="$( cd "$SCRIPT_DIR/../.." && pwd )"

echo "Project directory: $PROJECT_DIR"
cd "$PROJECT_DIR"

KEYSTORE_FILE="kuttab-release.keystore"

# Check if keystore already exists
if [ -f "$KEYSTORE_FILE" ]; then
    echo "⚠️  WARNING: Keystore already exists at: $KEYSTORE_FILE"
    echo "Do you want to overwrite it? (yes/no)"
    read -r response
    if [ "$response" != "yes" ]; then
        echo "Aborted. Keeping existing keystore."
        exit 0
    fi
    echo "Backing up existing keystore..."
    cp "$KEYSTORE_FILE" "${KEYSTORE_FILE}.backup.$(date +%Y%m%d_%H%M%S)"
fi

echo ""
echo "Creating new keystore..."
echo "You will be prompted for:"
echo "  - Keystore password (remember this!)"
echo "  - Key password (can be same as keystore password)"
echo "  - Your name and organization details"
echo ""

keytool -genkey -v \
    -keystore "$KEYSTORE_FILE" \
    -alias kuttab \
    -keyalg RSA \
    -keysize 2048 \
    -validity 10000

if [ -f "$KEYSTORE_FILE" ]; then
    echo ""
    echo "✅ Keystore created successfully!"
    echo "Location: $KEYSTORE_FILE"
    echo ""
    echo "⚠️  IMPORTANT:"
    echo "1. Backup this keystore file securely"
    echo "2. Store passwords in a password manager"
    echo "3. Never commit this file to git"
    echo "4. You'll need this keystore for all future app updates"
    echo ""
    echo "Next steps:"
    echo "1. Set environment variable: export KEYSTORE_PASSWORD='your-password'"
    echo "2. Run: ./build-release-aab.sh"
else
    echo "❌ Failed to create keystore"
    exit 1
fi
