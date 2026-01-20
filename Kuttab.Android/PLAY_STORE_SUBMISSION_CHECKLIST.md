# Google Play Store Submission Checklist

Complete this checklist before submitting your app to Google Play Store.

---

## 📋 Pre-Submission Requirements

### 1. App Signing & Build ✅/❌

- [ ] **Release keystore created** (see `signing.md`)
  - Keystore file: `kuttab-release.keystore`
  - Passwords documented securely
  - Backup created and stored safely
  
- [ ] **Signing configured in project**
  - `AndroidKeyStore` set to `true`
  - Keystore path configured
  - Passwords set via environment variables
  
- [ ] **Release build successful**
  - AAB built for Play Store (recommended)
  - Or APK built for alternative distribution
  - Build verified with `jarsigner`
  
- [ ] **Version numbers updated**
  - `ApplicationVersion` incremented (currently: 81)
  - `ApplicationDisplayVersion` updated (currently: 0.81)
  - Version code must be unique and higher than previous

### 2. App Configuration ✅

- [x] **Package name verified**: `com.kuttab.app`
- [x] **App name set**: "Kuttab - Tajweed Trainer"
- [x] **AndroidManifest.xml updated** with proper permissions
- [x] **App icons present** in all densities (mdpi to xxxhdpi)
- [ ] **Minimum SDK version**: API 21 (Android 5.0) - Verify compatibility
- [ ] **Target SDK version**: Latest stable (API 33/34 recommended)

### 3. Privacy & Legal ❗ REQUIRED

- [x] **Privacy policy created** (`PRIVACY_POLICY.md`)
- [ ] **Privacy policy hosted publicly**
  - URL: ________________________________
  - Accessible without login
  - Matches app's actual data practices
  
- [ ] **Data safety form completed** in Play Console
  - Declared: No data collection
  - Declared: No data sharing
  - Declared: No third-party data usage
  
- [ ] **Content rating questionnaire completed**
  - IARC rating obtained
  - Appropriate for all ages

### 4. Store Listing Assets ❗ REQUIRED

- [ ] **Feature Graphic** (1024x500 PNG/JPG)
  - Created and optimized
  - File size under 1MB
  - Showcases app branding
  
- [ ] **App Icon** (512x512 PNG)
  - ✅ Already available in project
  - High-resolution version ready
  
- [ ] **Screenshots** (Minimum 2, Recommended 4-8)
  - [ ] Screenshot 1: Main search screen
  - [ ] Screenshot 2: Search results with highlighting
  - [ ] Screenshot 3: Audio recitation interface
  - [ ] Screenshot 4: Settings screen
  - [ ] Screenshot 5: Recitation mode
  - [ ] Screenshot 6: Multi-language support
  - [ ] Screenshot 7: Info/credits (optional)
  - [ ] Screenshot 8: Offline mode (optional)
  - All screenshots are 1080p or higher
  - No personal information visible
  
- [ ] **Promotional Video** (Optional but recommended)
  - YouTube URL: ________________________________
  - 30-90 seconds duration
  - Shows key features

### 5. Store Listing Text ✅

- [x] **Short description** (80 chars max)
  - "Learn Quranic Tajweed rules with interactive search and audio recitations"
  
- [x] **Full description** (4000 chars max)
  - See `PLAY_STORE_LISTING.md`
  - Highlights key features
  - Includes disclaimer about educational purpose
  
- [x] **What's New** (500 chars max)
  - Release notes prepared
  - Version 0.81 changes documented
  
- [x] **Keywords/Tags** identified
  - Quran, Tajweed, Islam, Education, etc.

### 6. App Testing ❗ CRITICAL

- [ ] **Tested on multiple devices**
  - [ ] Phone (small screen)
  - [ ] Phone (large screen)
  - [ ] Tablet (optional)
  
- [ ] **Tested on different Android versions**
  - [ ] Android 5.0 (API 21) - Minimum
  - [ ] Android 8.0 (API 26)
  - [ ] Android 10 (API 29)
  - [ ] Android 12+ (API 31+) - Latest
  
- [ ] **All features tested**
  - [ ] Tajweed rule search works
  - [ ] Audio playback (online mode)
  - [ ] Audio playback (offline mode)
  - [ ] Recitation mode with repeat
  - [ ] Language switching (Arabic, English, German)
  - [ ] Settings save and persist
  - [ ] Donation links open correctly
  - [ ] Info page displays properly
  
- [ ] **No crashes or ANRs**
  - App launches successfully
  - No force closes during normal use
  - No "Application Not Responding" dialogs
  
- [ ] **Performance verified**
  - App loads in reasonable time
  - Smooth scrolling
  - Audio plays without stuttering
  - No memory leaks

### 7. Localization (If Supporting Multiple Languages)

- [x] **English** - Default
- [x] **Arabic** - Supported
- [x] **German** - Supported
- [ ] **Store listing translated** (optional)
  - English listing
  - Arabic listing
  - German listing

### 8. Google Play Console Setup

- [ ] **Developer account created**
  - One-time $25 registration fee paid
  - Account verified
  
- [ ] **App created in Console**
  - Package name: `com.kuttab.app`
  - App title: "Kuttab - Tajweed Trainer"
  
- [ ] **Store presence configured**
  - App category: Education
  - Tags added
  - Contact email: almanar.backup@gmail.com
  
- [ ] **Content rating completed**
  - IARC questionnaire filled
  - Rating certificate obtained
  
- [ ] **Pricing & distribution set**
  - Free app
  - Countries selected (worldwide recommended)
  - Distribution consent given

### 9. App Content Declaration

- [ ] **Ads declaration**: No ads
- [ ] **In-app purchases**: None
- [ ] **Target audience**: Ages 13+ (or All ages)
- [ ] **App access**: Full access, no restrictions
- [ ] **COVID-19 contact tracing**: No
- [ ] **Data safety**: No data collected

### 10. Release Track Selection

Choose your initial release track:

- [ ] **Internal Testing** (Up to 100 testers)
  - Best for initial testing
  - Quick review process
  
- [ ] **Closed Testing** (Invite-only)
  - Controlled beta testing
  - Gather feedback before public release
  
- [ ] **Open Testing** (Public beta)
  - Anyone can join
  - Wider testing audience
  
- [ ] **Production** (Full release)
  - Public release
  - Available to all users

**Recommendation**: Start with Internal or Closed Testing

---

## 🚀 Submission Steps

### Step 1: Prepare Release Build

```bash
# Set keystore password
export KEYSTORE_PASSWORD="your-password"

# Build AAB for Play Store
cd Kuttab.Android
dotnet publish -c Release -f net8.0-android -p:AndroidPackageFormat=aab

# Verify signing
jarsigner -verify -verbose -certs bin/Release/net8.0-android/publish/com.kuttab.app-Signed.aab
```

### Step 2: Upload to Play Console

1. Log in to [Google Play Console](https://play.google.com/console)
2. Select your app
3. Navigate to desired release track
4. Click "Create new release"
5. Upload AAB file
6. Review warnings (if any)
7. Add release notes
8. Save and review

### Step 3: Complete Store Listing

1. Go to "Store presence" → "Main store listing"
2. Upload all graphics (icon, feature graphic, screenshots)
3. Enter app description
4. Add contact details
5. Save

### Step 4: Content Rating

1. Go to "Policy" → "App content"
2. Complete "Content rating" questionnaire
3. Submit for rating
4. Wait for IARC certificate

### Step 5: Pricing & Distribution

1. Go to "Policy" → "Pricing & distribution"
2. Select countries
3. Confirm pricing (Free)
4. Accept terms and conditions
5. Save

### Step 6: Data Safety

1. Go to "Policy" → "App content" → "Data safety"
2. Declare: No data collected
3. Declare: No data shared
4. Submit

### Step 7: Review & Publish

1. Review all sections for completeness
2. Check for any warnings or errors
3. Click "Review release"
4. Submit for review

### Step 8: Wait for Review

- **Initial review**: 1-7 days (typically 1-3 days)
- **Updates**: Usually faster (hours to 1 day)
- Monitor email for review status
- Respond promptly to any review feedback

---

## 📊 Post-Submission

### After Approval

- [ ] **App goes live** on Play Store
- [ ] **Monitor reviews** and ratings
- [ ] **Respond to user feedback**
- [ ] **Track crashes** in Play Console
- [ ] **Monitor performance** metrics
- [ ] **Plan updates** based on feedback

### Ongoing Maintenance

- [ ] **Regular updates** (bug fixes, features)
- [ ] **Version management** (increment version codes)
- [ ] **User support** (respond to reviews and emails)
- [ ] **Performance monitoring** (crashes, ANRs)
- [ ] **Security updates** (keep dependencies updated)

---

## ⚠️ Common Rejection Reasons

Be aware of these common issues that cause app rejection:

1. **Privacy Policy Issues**
   - Missing privacy policy URL
   - Privacy policy not accessible
   - Privacy policy doesn't match app behavior

2. **Permissions Issues**
   - Requesting unnecessary permissions
   - Not explaining permission usage
   - Dangerous permissions without justification

3. **Content Issues**
   - Misleading description or screenshots
   - Inappropriate content
   - Copyright violations

4. **Technical Issues**
   - App crashes on launch
   - Critical bugs
   - Doesn't work as described

5. **Policy Violations**
   - Violates Google Play policies
   - Spam or misleading behavior
   - Inappropriate monetization

---

## 📞 Support & Resources

### Google Play Resources
- [Play Console Help](https://support.google.com/googleplay/android-developer)
- [Launch Checklist](https://developer.android.com/distribute/best-practices/launch/launch-checklist)
- [Play Policy Center](https://play.google.com/about/developer-content-policy/)

### Your App Documentation
- `signing.md` - App signing guide
- `PRIVACY_POLICY.md` - Privacy policy
- `PLAY_STORE_LISTING.md` - Store listing content
- `SCREENSHOTS_GUIDE.md` - Graphics requirements
- `BUILD_RELEASE.md` - Build instructions

### Contact
- **Email**: almanar.backup@gmail.com
- **Subject**: Kuttab Play Store Submission

---

## ✅ Final Pre-Submission Checklist

Before clicking "Submit for review":

- [ ] All required assets uploaded
- [ ] Privacy policy URL added and accessible
- [ ] Content rating obtained
- [ ] Data safety form completed
- [ ] App tested thoroughly
- [ ] No crashes or critical bugs
- [ ] Version numbers correct
- [ ] Release notes written
- [ ] All Play Console sections complete (no warnings)
- [ ] Terms and conditions accepted
- [ ] Keystore backed up securely

---

## 🎉 Ready to Submit?

If all items above are checked, you're ready to submit your app to Google Play Store!

**Good luck with your submission! 🚀**

---

**Last Updated**: January 14, 2025  
**App Version**: 0.81  
**Package**: com.kuttab.app
