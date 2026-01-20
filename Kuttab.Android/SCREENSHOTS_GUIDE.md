# Screenshots and Graphics Guide for Play Store

## Required Assets for Google Play Store Submission

### 1. Feature Graphic (REQUIRED)
**Dimensions**: 1024 x 500 pixels  
**Format**: PNG or JPG (24-bit, no alpha)  
**File Size**: Max 1MB

**Purpose**: Displayed at the top of your store listing

**Design Tips**:
- Showcase your app name "Kuttab - Tajweed Trainer"
- Include app icon
- Use Arabic calligraphy or Quranic motifs
- Keep text minimal and readable
- Use your brand colors
- Avoid clutter

**Tools to Create**:
- Canva (free templates available)
- Adobe Photoshop
- GIMP (free)
- Figma (free)

---

### 2. App Icon (COMPLETED ✅)
**Dimensions**: 512 x 512 pixels  
**Format**: PNG (32-bit with alpha)  
**Status**: Already available in your project

Your app icons are already created in all required densities:
- mdpi, hdpi, xhdpi, xxhdpi, xxxhdpi

---

### 3. Screenshots (REQUIRED - Minimum 2, Recommended 4-8)

**Phone Screenshots**:
- **Minimum**: 2 screenshots
- **Recommended**: 4-8 screenshots
- **Aspect Ratio**: 16:9 or 9:16
- **Minimum Dimension**: 320px on shortest side
- **Maximum Dimension**: 3840px on longest side
- **Format**: PNG or JPG (24-bit, no alpha)

**Recommended Screenshots to Capture**:

1. **Main Search Screen** (Primary)
   - Show the Tajweed rule selection dropdown
   - Display search results with highlighted text
   - Include Arabic text clearly visible

2. **Search Results with Highlighting**
   - Show multiple search results
   - Highlight the matched Tajweed pattern
   - Display Surah and Aya numbers

3. **Audio Recitation Screen**
   - Show the recitation interface
   - Display reciter selection
   - Show playback controls

4. **Settings Screen**
   - Show language options
   - Display reciter choices
   - Show search domain settings

5. **Recitation Mode**
   - Show the dedicated recitation tab
   - Display Surah selection
   - Show repeat options

6. **Info/Credits Screen** (Optional)
   - Show app information
   - Display sources and credits

7. **Multi-language Support** (Optional)
   - Screenshot in Arabic interface
   - Screenshot in English interface

8. **Offline Mode** (Optional)
   - Show offline capabilities

---

## How to Capture Screenshots

### Method 1: Using Android Emulator (Recommended)
```bash
# Start emulator
emulator -avd <your_avd_name>

# Run your app
dotnet build -c Release -f net8.0-android Kuttab.Android/Kuttab.Android.csproj
adb install path/to/your.apk

# Take screenshot (from emulator menu)
# Or use: adb shell screencap -p /sdcard/screenshot.png
# Then: adb pull /sdcard/screenshot.png
```

### Method 2: Using Physical Device
1. Connect your Android device via USB
2. Enable USB debugging
3. Install and run your app
4. Take screenshots using device buttons (Power + Volume Down)
5. Transfer screenshots to computer

### Method 3: Using Android Studio
1. Open Android Studio
2. Run app on emulator or device
3. Use Camera icon in Device toolbar
4. Screenshots saved automatically

---

## Screenshot Best Practices

### Do's:
✅ Use high-resolution device screenshots (1080p or higher)  
✅ Show actual app content (no mockups)  
✅ Display key features prominently  
✅ Use consistent device frame (optional)  
✅ Show Arabic text clearly  
✅ Include captions/annotations (optional but helpful)  
✅ Arrange in logical order (user journey)  
✅ Remove status bar clutter (optional)  

### Don'ts:
❌ Don't use blurry or low-quality images  
❌ Don't show outdated UI  
❌ Don't include personal information  
❌ Don't use misleading content  
❌ Don't violate copyright (use your own content)  
❌ Don't include prices or promotions in screenshots  

---

## Screenshot Dimensions Reference

### Common Android Phone Resolutions:
- **1080 x 1920** (Full HD, 16:9) - Most common
- **1440 x 2560** (QHD, 16:9)
- **1080 x 2340** (19.5:9)
- **1440 x 3040** (19:9)

**Recommendation**: Use 1080 x 1920 or higher for best quality

---

## Tablet Screenshots (Optional but Recommended)

If you want to support tablets:
- **7-inch Tablet**: Minimum 1024 x 600
- **10-inch Tablet**: Minimum 1280 x 800
- **Aspect Ratio**: 16:9 or 16:10

---

## Tools for Screenshot Enhancement

### Adding Device Frames:
- **Device Art Generator**: https://developer.android.com/distribute/marketing-tools/device-art-generator
- **Mockuphone**: https://mockuphone.com/
- **Screely**: https://www.screely.com/

### Adding Captions/Text:
- **Canva**: Easy drag-and-drop
- **Figma**: Professional design tool
- **Adobe Spark**: Quick graphics

### Screenshot Optimization:
- **TinyPNG**: Compress images without quality loss
- **ImageOptim**: Mac app for optimization
- **Squoosh**: Web-based image optimizer

---

## Localized Screenshots (Optional)

For better conversion in different markets, consider creating localized screenshots:
- Arabic screenshots with Arabic UI
- English screenshots with English UI
- German screenshots with German UI

Google Play allows you to upload different screenshots for each language.

---

## Video Preview (Optional but Highly Recommended)

**YouTube Video**: 30 seconds to 2 minutes
- Show app launch
- Demonstrate key features
- Show Tajweed search in action
- Display audio playback
- Highlight multi-language support
- End with call-to-action

**Video Tips**:
- Keep it short and engaging (60-90 seconds ideal)
- Add background music (royalty-free)
- Include captions for accessibility
- Show real app usage
- Highlight unique features

---

## Checklist Before Upload

- [ ] Feature Graphic (1024x500) created
- [ ] Minimum 2 phone screenshots captured
- [ ] Screenshots are high quality (1080p+)
- [ ] Screenshots show key features
- [ ] No personal/sensitive information visible
- [ ] Images are properly sized
- [ ] File sizes are under limits
- [ ] Screenshots are in correct order
- [ ] Optional: Tablet screenshots added
- [ ] Optional: Promotional video created
- [ ] Optional: Localized screenshots for other languages

---

## Quick Screenshot Checklist

**Must Have** (Minimum 2):
1. ✅ Main search screen with Tajweed rules
2. ✅ Search results with highlighted text

**Should Have** (4-6 total):
3. ✅ Audio recitation interface
4. ✅ Settings screen
5. ✅ Recitation mode
6. ✅ Multi-language support

**Nice to Have** (7-8 total):
7. ✅ Info/credits screen
8. ✅ Offline mode demonstration

---

## Need Help?

If you need assistance creating graphics:
- Hire a designer on Fiverr or Upwork
- Use Canva templates (search "app store screenshots")
- Ask in Android developer communities
- Use automated screenshot tools

Remember: Quality screenshots significantly impact download rates!
