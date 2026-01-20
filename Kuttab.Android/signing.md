# Android App Signing Guide for Play Store

## Creating a Release Keystore

Before publishing to Google Play Store, you need to create a release keystore for signing your APK/AAB.

### Step 1: Generate Keystore

Run the following command to create a keystore:

```bash
keytool -genkey -v -keystore kuttab-release.keystore -alias kuttab -keyalg RSA -keysize 2048 -validity 10000
```

You will be prompted for:
- Keystore password (remember this!)
- Key password (remember this!)
- Your name, organization, city, state, country

**IMPORTANT**: Keep this keystore file and passwords secure. You'll need them for all future updates.

### Step 2: Store Keystore Securely

1. Move the keystore to a secure location (NOT in the git repository)
2. Create a backup of the keystore file
3. Document the passwords securely (password manager recommended)

### Step 3: Configure Signing in Project

Add the following to your `Kuttab.Android.csproj` file inside the `<PropertyGroup>` section:

```xml
<!-- Release signing configuration -->
<AndroidKeyStore>true</AndroidKeyStore>
<AndroidSigningKeyStore>path/to/kuttab-release.keystore</AndroidSigningKeyStore>
<AndroidSigningKeyAlias>kuttab</AndroidSigningKeyAlias>
<AndroidSigningKeyPass>your-key-password</AndroidSigningKeyPass>
<AndroidSigningStorePass>your-store-password</AndroidSigningStorePass>
```

**SECURITY NOTE**: Never commit passwords to git. Use environment variables or secure build systems.

### Step 4: Build Signed Release

For APK:
```bash
dotnet publish -c Release -f net8.0-android Kuttab.Android.csproj
```

For AAB (recommended for Play Store):
```bash
dotnet publish -c Release -f net8.0-android -p:AndroidPackageFormat=aab Kuttab.Android.csproj
```

## Google Play App Signing

Google Play offers app signing service. When you upload your first release:
1. Google will manage your app signing key
2. You upload an "upload key" (the keystore you created)
3. Google re-signs with their managed key

This is the recommended approach for new apps.

## Verification

After building, verify your signed APK/AAB:

```bash
# For APK
jarsigner -verify -verbose -certs path/to/your-app.apk

# For AAB
bundletool validate --bundle=path/to/your-app.aab
```

## Important Notes

- **Never lose your keystore**: Without it, you cannot update your app
- **Use strong passwords**: Minimum 6 characters
- **Backup everything**: Store keystore and passwords in multiple secure locations
- **Use AAB format**: Google Play requires AAB for new apps (since August 2021)
