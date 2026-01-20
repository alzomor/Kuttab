# Kuttab Android - Google Play Store Preparation

This directory contains all the documentation and configuration needed to publish Kuttab to the Google Play Store.

## 📚 Documentation Files

### Essential Guides
1. **`PLAY_STORE_SUBMISSION_CHECKLIST.md`** ⭐ START HERE
   - Complete step-by-step checklist for Play Store submission
   - Covers all requirements from build to publication
   - Track your progress through the submission process

2. **`signing.md`**
   - How to create and manage your release keystore
   - App signing configuration
   - Security best practices

3. **`BUILD_RELEASE.md`**
   - Building signed APK/AAB for release
   - Build commands and verification steps
   - Troubleshooting common build issues

4. **`PRIVACY_POLICY.md`**
   - Complete privacy policy for your app
   - Must be hosted at a public URL before submission
   - Compliant with Google Play requirements

5. **`PLAY_STORE_LISTING.md`**
   - App description, short description, and metadata
   - Keywords and tags
   - Content rating information
   - Data safety declarations

6. **`SCREENSHOTS_GUIDE.md`**
   - Requirements for screenshots and graphics
   - Feature graphic specifications
   - Tips for creating compelling store assets

### Configuration Files
- **`Directory.Build.props.example`** - Template for signing configuration
- **`.gitignore`** - Prevents committing sensitive files

## 🚀 Quick Start Guide

### Step 1: Create Keystore (First Time Only)
```bash
keytool -genkey -v -keystore kuttab-release.keystore -alias kuttab -keyalg RSA -keysize 2048 -validity 10000
```

**⚠️ CRITICAL**: Backup this keystore file and remember the passwords!

### Step 2: Configure Signing
```bash
# Copy the example configuration
cp Directory.Build.props.example Directory.Build.props

# Edit Directory.Build.props with your keystore information
# OR use environment variables (recommended)
export KEYSTORE_PASSWORD="your-password"
```

### Step 3: Build Release AAB
```bash
dotnet publish -c Release -f net8.0-android -p:AndroidPackageFormat=aab
```

Output: `bin/Release/net8.0-android/publish/com.kuttab.app-Signed.aab`

### Step 4: Create Store Assets
- Feature graphic (1024x500)
- Screenshots (minimum 2, recommended 4-8)
- See `SCREENSHOTS_GUIDE.md` for details

### Step 5: Host Privacy Policy
Upload `PRIVACY_POLICY.md` to a public URL:
- GitHub Pages
- Your website
- Google Sites

### Step 6: Submit to Play Console
Follow the complete checklist in `PLAY_STORE_SUBMISSION_CHECKLIST.md`

## 📋 Current App Configuration

- **Package Name**: `com.kuttab.app`
- **App Name**: Kuttab - Tajweed Trainer
- **Current Version**: 0.81 (Version Code: 81)
- **Minimum SDK**: API 21 (Android 5.0)
- **Target SDK**: API 33+ (Update to latest)
- **Category**: Education

## ✅ What's Already Done

- [x] App icons created (all densities)
- [x] AndroidManifest.xml configured
- [x] Privacy policy written
- [x] Store listing content prepared
- [x] Build configuration optimized
- [x] Permissions documented

## ❗ What You Need to Do

- [ ] Create release keystore
- [ ] Build signed AAB
- [ ] Create feature graphic (1024x500)
- [ ] Capture screenshots (minimum 2)
- [ ] Host privacy policy at public URL
- [ ] Create Google Play Developer account ($25 one-time fee)
- [ ] Complete Play Console setup
- [ ] Upload and submit app

## 🔒 Security Reminders

**NEVER commit these files to git:**
- `*.keystore` - Your signing key
- `*.jks` - Java keystore files
- `Directory.Build.props` - Contains passwords
- Any file with passwords or secrets

These are already in `.gitignore` for your protection.

## 📊 File Size

Current APK size: ~TBD (build to check)
- Google Play limit: 150 MB for AAB
- If over 100 MB, consider on-demand downloads for audio files

## 🌐 Supported Languages

- Arabic (العربية)
- English
- German (Deutsch)

Consider creating localized store listings for better reach.

## 📞 Support

**Email**: almanar.backup@gmail.com  
**Subject**: Kuttab Play Store

## 🎯 Next Steps

1. Read `PLAY_STORE_SUBMISSION_CHECKLIST.md` completely
2. Follow each step in order
3. Check off items as you complete them
4. Submit for review when all items are complete

## 📖 Additional Resources

- [Google Play Console](https://play.google.com/console)
- [Android Developer Docs](https://developer.android.com/distribute)
- [Play Policy Center](https://play.google.com/about/developer-content-policy/)

## 🎉 Good Luck!

You're well-prepared for Play Store submission. Follow the checklist carefully, and your app will be live soon!

---

**Last Updated**: January 14, 2025  
**Documentation Version**: 1.0
