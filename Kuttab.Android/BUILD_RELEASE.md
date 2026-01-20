# Building Release APK/AAB for Google Play Store

## Prerequisites

Before building for release, ensure you have:

1. ✅ Created a release keystore (see `signing.md`)
2. ✅ Updated version number in `Kuttab.Android.csproj`
3. ✅ Tested the app thoroughly
4. ✅ Reviewed and updated `AndroidManifest.xml`
5. ✅ Privacy policy hosted at a public URL

---

## Build Configuration

### Current Configuration

Your `Kuttab.Android.csproj` is already configured for release builds:

```xml
<ApplicationId>com.kuttab.app</ApplicationId>
<ApplicationVersion>81</ApplicationVersion>
<ApplicationDisplayVersion>0.81</ApplicationDisplayVersion>
```

**Before each release:**
1. Increment `ApplicationVersion` (e.g., 81 → 82)
2. Update `ApplicationDisplayVersion` if needed (e.g., 0.81 → 0.82)

---

## Option 1: Build AAB (Recommended for Play Store)

Google Play requires AAB (Android App Bundle) format for new apps.

### Step 1: Configure Signing

Add to `Kuttab.Android.csproj` inside `<PropertyGroup>` (for Release configuration only):

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
  <AndroidKeyStore>true</AndroidKeyStore>
  <AndroidSigningKeyStore>$(MSBuildThisFileDirectory)kuttab-release.keystore</AndroidSigningKeyStore>
  <AndroidSigningKeyAlias>kuttab</AndroidSigningKeyAlias>
  <AndroidSigningKeyPass>env:KEYSTORE_PASSWORD</AndroidSigningKeyPass>
  <AndroidSigningStorePass>env:KEYSTORE_PASSWORD</AndroidSigningStorePass>
  <AndroidPackageFormat>aab</AndroidPackageFormat>
</PropertyGroup>
```

**Security Note**: Use environment variables for passwords, never commit them to git!

### Step 2: Set Environment Variables

```bash
export KEYSTORE_PASSWORD="your-keystore-password"
```

Or for Windows PowerShell:
```powershell
$env:KEYSTORE_PASSWORD="your-keystore-password"
```

### Step 3: Build AAB

```bash
cd Kuttab.Android
dotnet publish -c Release -f net8.0-android
```

The signed AAB will be in:
```
Kuttab.Android/bin/Release/net8.0-android/publish/com.kuttab.app-Signed.aab
```

---

## Option 2: Build APK (For Testing or Alternative Stores)

### Build Signed APK

```bash
cd Kuttab.Android
dotnet publish -c Release -f net8.0-android -p:AndroidPackageFormat=apk
```

The signed APK will be in:
```
Kuttab.Android/bin/Release/net8.0-android/publish/com.kuttab.app-Signed.apk
```

---

## Build Script (Automated)

You can use the existing `build.py` script which already includes Android build support:

```bash
python3 build.py
```

This will:
- Build the Android APK
- Copy it to the `dist/` folder
- Name it: `Kuttab-android-0.81.apk`

**Note**: You'll need to modify `build.py` to build AAB instead of APK for Play Store submission.

---

## Verify Your Build

### 1. Verify Signing

```bash
# For APK
jarsigner -verify -verbose -certs path/to/com.kuttab.app-Signed.apk

# For AAB (requires bundletool)
bundletool validate --bundle=path/to/com.kuttab.app-Signed.aab
```

### 2. Check App Info

```bash
# For APK
aapt dump badging path/to/com.kuttab.app-Signed.apk | grep -E "package|version"

# Should show:
# package: name='com.kuttab.app' versionCode='81' versionName='0.81'
```

### 3. Install and Test

```bash
# Install on connected device
adb install -r path/to/com.kuttab.app-Signed.apk

# Or for AAB (convert to APK first using bundletool)
bundletool build-apks --bundle=app.aab --output=app.apks --mode=universal
unzip app.apks universal.apk
adb install universal.apk
```

---

## Build Optimization

Your project already has good optimization settings:

```xml
<AndroidEnableAssemblyCompression>true</AndroidEnableAssemblyCompression>
<AndroidUseFastDeployment>false</AndroidUseFastDeployment>
<RunAOTCompilation>false</RunAOTCompilation>
<PublishTrimmed>false</PublishTrimmed>
<AndroidLinkMode>None</AndroidLinkMode>
<DebugSymbols>false</DebugSymbols>
<DebugType>none</DebugType>
```

### Optional: Further Reduce APK Size

If you want to reduce the APK size further, you can enable:

```xml
<!-- Enable R8 code shrinking -->
<AndroidEnableProguard>false</AndroidEnableProguard>
<AndroidLinkMode>SdkOnly</AndroidLinkMode>

<!-- Enable multi-dex if needed -->
<AndroidDexTool>d8</AndroidDexTool>
```

**Warning**: Test thoroughly after enabling linking, as it may break reflection-based code.

---

## Build for Different ABIs (Advanced)

By default, your project builds a universal APK/AAB with all ABIs. For smaller downloads, you can build separate APKs:

```bash
# Build for specific ABI
dotnet publish -c Release -f net8.0-android -r android-arm64
dotnet publish -c Release -f net8.0-android -r android-arm
dotnet publish -c Release -f net8.0-android -r android-x64
dotnet publish -c Release -f net8.0-android -r android-x86
```

**Note**: AAB format handles this automatically, so this is only needed for APK distribution.

---

## Pre-Release Checklist

Before building your release:

- [ ] Version numbers updated in `Kuttab.Android.csproj`
- [ ] All features tested on multiple devices
- [ ] No debug code or logging left in
- [ ] Privacy policy URL ready
- [ ] App icons verified (all densities)
- [ ] Permissions in `AndroidManifest.xml` reviewed
- [ ] Keystore and passwords secured
- [ ] Release notes prepared
- [ ] Screenshots and graphics ready
- [ ] Tested on minimum API level (Android 5.0 / API 21)
- [ ] Tested on latest Android version
- [ ] Tested in different languages
- [ ] Audio playback tested (online and offline)
- [ ] No crashes or ANRs (Application Not Responding)

---

## Common Build Issues

### Issue: "Keystore not found"
**Solution**: Ensure keystore path is correct in `.csproj` file

### Issue: "Invalid password"
**Solution**: Check environment variable is set correctly

### Issue: "APK not signed"
**Solution**: Verify `AndroidKeyStore` is set to `true`

### Issue: "Build fails with SDK errors"
**Solution**: Ensure Android SDK is properly installed and `ANDROID_HOME` is set

### Issue: "AAB too large"
**Solution**: 
- Remove unused resources
- Enable ProGuard/R8
- Check for large assets in `Assets/` folder

---

## File Size Guidelines

**Google Play Limits**:
- APK: Max 100 MB (150 MB with expansion files)
- AAB: Max 150 MB (2 GB with expansion files)

**Your Current Build**:
- Check size after building
- If over 100 MB, consider:
  - Moving audio files to on-demand download
  - Compressing images
  - Removing unused resources

---

## Upload to Play Console

### Using AAB (Recommended):

1. Go to Google Play Console
2. Select your app
3. Navigate to "Release" → "Production" (or Testing track)
4. Click "Create new release"
5. Upload your AAB file
6. Fill in release notes
7. Review and rollout

### Using APK:

1. Same steps as AAB
2. Upload APK instead
3. Note: New apps must use AAB

---

## Version Management

### Semantic Versioning Recommendation:

- **Major.Minor.Patch** (e.g., 1.0.0)
- **Major**: Breaking changes
- **Minor**: New features
- **Patch**: Bug fixes

### Version Code Rules:

- Must be an integer
- Must increase with each release
- Cannot be reused
- Suggested format: YYMMDDNN (e.g., 25011401 for Jan 14, 2025, build 01)

---

## Post-Build Steps

After successful build:

1. ✅ Test the signed APK/AAB on real devices
2. ✅ Run through all app features
3. ✅ Check app size
4. ✅ Verify signing certificate
5. ✅ Create release notes
6. ✅ Tag the release in git
7. ✅ Backup the keystore
8. ✅ Upload to Play Console

---

## Continuous Integration (Optional)

For automated builds, consider setting up:
- GitHub Actions
- GitLab CI/CD
- Azure DevOps
- Jenkins

Example GitHub Actions workflow available upon request.

---

## Support

If you encounter build issues:
1. Check .NET MAUI documentation
2. Review Android developer docs
3. Search Stack Overflow
4. Check GitHub issues for similar problems

---

## Quick Build Commands Reference

```bash
# Build AAB (Play Store)
dotnet publish -c Release -f net8.0-android -p:AndroidPackageFormat=aab

# Build APK (Testing)
dotnet publish -c Release -f net8.0-android -p:AndroidPackageFormat=apk

# Build with Python script
python3 build.py

# Verify signing
jarsigner -verify -verbose -certs your-app.apk

# Install on device
adb install -r your-app.apk

# Check version
aapt dump badging your-app.apk | grep version
```

---

Good luck with your release! 🚀
