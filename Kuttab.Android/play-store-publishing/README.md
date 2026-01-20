# Kuttab - Play Store Publishing Package

This folder contains all the artifacts and documentation needed to publish Kuttab to the Google Play Store.

## 📁 Folder Structure

```
play-store-publishing/
├── graphics/              # App icons and graphics
│   ├── app-icon-original.png (✅ Ready)
│   └── GRAPHICS_REQUIREMENTS.txt (Instructions for missing graphics)
├── store-listing/         # Store listing text content
│   ├── short-description.txt (✅ Ready)
│   ├── full-description.txt (✅ Ready)
│   ├── whats-new.txt (✅ Ready)
│   └── keywords.txt (✅ Ready)
├── legal/                 # Legal documents
│   └── privacy-policy.html (✅ Ready - needs hosting)
├── build-scripts/         # Build automation scripts
│   ├── create-keystore.sh (✅ Ready)
│   ├── build-release-aab.sh (✅ Ready)
│   └── build-release-apk.sh (✅ Ready)
├── documentation/         # Additional documentation
│   └── APP_METADATA.txt (✅ Ready)
└── README.md             # This file
```

## ✅ What's Ready

### Store Listing Content
- ✅ Short description (80 chars)
- ✅ Full description
- ✅ What's new / Release notes
- ✅ Keywords for SEO

### Legal Documents
- ✅ Privacy policy (HTML format)
  - **Action Required**: Host at public URL

### Graphics
- ✅ App icon (512x512) extracted from project

### Build Scripts
- ✅ Keystore creation script
- ✅ AAB build script (for Play Store)
- ✅ APK build script (for testing)

### Documentation
- ✅ App metadata and configuration details

## ❌ What You Need to Create

### 1. Feature Graphic (REQUIRED)
- **Dimensions**: 1024 x 500 pixels
- **Format**: PNG or JPG
- **Location**: Save to `graphics/feature-graphic.png`
- **Tools**: Canva, Photoshop, GIMP, Figma
- **See**: `graphics/GRAPHICS_REQUIREMENTS.txt`

### 2. Screenshots (REQUIRED - Minimum 2)
- **Dimensions**: 1080 x 1920 or higher
- **Format**: PNG or JPG
- **Location**: Save to `graphics/screenshots/`
- **Recommended**: 4-8 screenshots showing key features
- **See**: `../SCREENSHOTS_GUIDE.md`

### 3. Release Keystore (REQUIRED)
- **Run**: `cd .. && ./play-store-publishing/build-scripts/create-keystore.sh`
- **Backup**: Store keystore and passwords securely
- **Never**: Commit keystore to git

### 4. Host Privacy Policy (REQUIRED)
- **File**: `legal/privacy-policy.html`
- **Action**: Upload to GitHub Pages, your website, or Google Sites
- **URL**: Add to Play Console during submission

## 🚀 Quick Start Guide

### Step 1: Create Keystore (First Time Only)

```bash
cd /home/hossam/Work/Quraan/Kuttab.Android
chmod +x play-store-publishing/build-scripts/*.sh
./play-store-publishing/build-scripts/create-keystore.sh
```

**⚠️ CRITICAL**: Backup the keystore file and remember the passwords!

### Step 2: Build Release AAB

```bash
# Set keystore password
export KEYSTORE_PASSWORD="your-password"

# Build AAB for Play Store
./play-store-publishing/build-scripts/build-release-aab.sh
```

Output will be in: `play-store-publishing/com.kuttab.app-v0.81.aab`

### Step 3: Create Graphics

1. **Feature Graphic** (1024x500):
   - Use Canva template or design tool
   - Include app name and icon
   - Save to `graphics/feature-graphic.png`

2. **Screenshots** (minimum 2):
   - Run app on emulator or device
   - Capture key screens
   - Save to `graphics/screenshots/`

### Step 4: Host Privacy Policy

Upload `legal/privacy-policy.html` to:
- GitHub Pages (recommended)
- Your website
- Google Sites

### Step 5: Submit to Play Console

1. Go to [Google Play Console](https://play.google.com/console)
2. Create new app or select existing
3. Upload AAB from `play-store-publishing/`
4. Copy text from `store-listing/*.txt` files
5. Upload graphics from `graphics/`
6. Add privacy policy URL
7. Complete all required sections
8. Submit for review

## 📋 Submission Checklist

Use the main checklist: `../PLAY_STORE_SUBMISSION_CHECKLIST.md`

Quick checklist:
- [ ] Keystore created and backed up
- [ ] AAB built successfully
- [ ] Feature graphic created (1024x500)
- [ ] Screenshots captured (minimum 2)
- [ ] Privacy policy hosted at public URL
- [ ] Google Play Developer account created ($25)
- [ ] All store listing text ready
- [ ] App tested thoroughly
- [ ] All Play Console sections completed

## 📊 File Status

| File/Folder | Status | Action Required |
|------------|--------|-----------------|
| store-listing/*.txt | ✅ Ready | Copy to Play Console |
| legal/privacy-policy.html | ✅ Ready | Host at public URL |
| graphics/app-icon-original.png | ✅ Ready | Upload to Play Console |
| graphics/feature-graphic.png | ❌ Missing | Create (1024x500) |
| graphics/screenshots/ | ❌ Missing | Capture 2-8 screenshots |
| build-scripts/*.sh | ✅ Ready | Execute to build |
| Keystore | ❌ Missing | Run create-keystore.sh |
| AAB/APK | ❌ Missing | Run build scripts |

## 🔒 Security Notes

**Never commit these files to git:**
- `*.keystore` - Your signing key
- `*.jks` - Java keystore files
- `Directory.Build.props` - Contains passwords
- `*.aab` / `*.apk` - Built binaries

These are already in `.gitignore`.

## 📞 Support

**Email**: almanar.backup@gmail.com  
**Subject**: Kuttab Play Store Submission

## 📚 Additional Documentation

- `../PLAY_STORE_SUBMISSION_CHECKLIST.md` - Complete submission guide
- `../signing.md` - Keystore and signing details
- `../BUILD_RELEASE.md` - Build instructions
- `../SCREENSHOTS_GUIDE.md` - Graphics creation guide
- `../PLAY_STORE_LISTING.md` - Detailed store listing info

## 🎯 Next Steps

1. **Read** this README completely
2. **Create** keystore using provided script
3. **Build** AAB using provided script
4. **Create** feature graphic (1024x500)
5. **Capture** screenshots (minimum 2)
6. **Host** privacy policy at public URL
7. **Submit** to Google Play Console

## ⏱️ Time Estimate

- Keystore creation: 5 minutes
- Build AAB: 5-10 minutes
- Create graphics: 1-2 hours
- Capture screenshots: 30 minutes
- Host privacy policy: 15 minutes
- Play Console submission: 1-2 hours
- **Total**: ~4-6 hours

Google review: 1-7 days (typically 1-3 days)

## 🎉 You're Almost There!

All the automated work is done. Just create the graphics, build the AAB, and submit!

---

**Last Updated**: January 14, 2025  
**Package Version**: 1.0  
**App Version**: 0.81
