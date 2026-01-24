#!/bin/bash

echo "=========================================="
echo "Creating New Release Keystore"
echo "=========================================="

# Navigate to project root
cd "$(dirname "$0")/../.."

# Check if keystore already exists
if [ -f "kuttab-release.keystore" ]; then
    echo "⚠️  Keystore already exists!"
    echo "   Existing: kuttab-release.keystore"
    echo ""
    echo "Choose an option:"
    echo "1. Keep existing keystore (need to remember password)"
    echo "2. Create new keystore (will overwrite existing)"
    echo ""
    read -p "Enter choice (1 or 2): " choice
    
    if [ "$choice" != "2" ]; then
        echo "Keeping existing keystore."
        echo "You need to remember your keystore password!"
        exit 0
    fi
    
    echo "Backing up existing keystore..."
    mv kuttab-release.keystore kuttab-release.keystore.backup
fi

echo "Creating new keystore..."
echo "Please provide the following information:"
echo ""

# Get keystore password
read -s -p "Enter keystore password (min 6 chars): " keystore_password
echo ""
if [ ${#keystore_password} -lt 6 ]; then
    echo "❌ Password must be at least 6 characters!"
    exit 1
fi

# Get alias password
read -s -p "Enter alias password (can be same as keystore): " alias_password
echo ""

# Get Distinguished Name information
echo ""
echo "Enter certificate information (or press Enter for defaults):"
read -p "First and Last Name (Hossam Alzomor): " name
read -p "Organizational Unit (Al-manar): " ou
read -p "Organization (Al-manar): " org
read -p "City (Freiburg): " city
read -p "State (Baden Wurtemberg): " state
read -p "Country Code (DE): " country

# Set defaults if empty
name=${name:-"Hossam Alzomor"}
ou=${ou:-"Al-manar"}
org=${org:-"Al-manar"}
city=${city:-"Freiburg"}
state=${state:-"Baden Wurtemberg"}
country=${country:-"DE"}

# Create keystore
keytool -genkey -v \
    -keystore kuttab-release.keystore \
    -alias kuttab \
    -keyalg RSA \
    -keysize 2048 \
    -validity 10000 \
    -storepass "$keystore_password" \
    -keypass "$alias_password" \
    -dname "CN=$name, OU=$ou, O=$org, L=$city, S=$state, C=$country"

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Keystore created successfully!"
    echo ""
    echo "📝 IMPORTANT: Save these passwords securely!"
    echo "   Keystore Password: $keystore_password"
    echo "   Alias Password: $alias_password"
    echo ""
    echo "🔧 To build with this keystore:"
    echo "   export KEYSTORE_PASSWORD='$keystore_password'"
    echo "   export KEYSTORE_ALIAS_PASSWORD='$alias_password'"
    echo "   ./play-store-publishing/build-scripts/build-release-aab.sh"
    echo ""
    echo "⚠️  BACKUP THIS FILE: kuttab-release.keystore"
    echo "   Without it, you cannot update your app!"
    
    # Set environment variables for immediate use
    export KEYSTORE_PASSWORD="$keystore_password"
    export KEYSTORE_ALIAS_PASSWORD="$alias_password"
    
    echo ""
    echo "🚀 Ready to build release AAB!"
else
    echo "❌ Failed to create keystore!"
    exit 1
fi
