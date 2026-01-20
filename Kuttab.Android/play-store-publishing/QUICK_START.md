# Quick Start - Publishing Kuttab to Google Play Store

## 🎯 Three Simple Steps to Publish

### Step 1: Create Your Keystore (5 minutes)

```bash
cd /home/hossam/Work/Quraan/Kuttab.Android
./play-store-publishing/build-scripts/create-keystore.sh
```

**Important**: Backup the keystore file and save your passwords!

### Step 2: Build Release AAB (10 minutes)

```bash
# Set your keystore password
export KEYSTORE_PASSWORD="your-password-here"

# Build the release AAB
./play-store-publishing/build-scripts/build-release-aab.sh
```

Output: `play-store-publishing/com.kuttab.app-v0.81.aab`

### Step 3: Submit to Play Store (2 hours)

1. **Create Graphics** (1-2 hours):
   - Feature graphic: 1024x500 pixels
   - Screenshots: Minimum 2 (recommended 4-8)
   - See: `graphics/GRAPHICS_REQUIREMENTS.txt`

2. **Host Privacy Policy** (15 minutes):
   - Upload `legal/privacy-policy.html` to GitHub Pages or your website
   - Get the public URL

3. **Submit to Play Console** (1 hour):
   - Go to https://play.google.com/console
   - Create app (if first time)
   - Upload AAB from `play-store-publishing/`
   - Copy text from `store-listing/*.txt` files
   - Upload graphics
   - Add privacy policy URL
   - Submit for review

## 📋 Everything You Need is Here

```
play-store-publishing/
├── build-scripts/          ← Run these to build
├── store-listing/          ← Copy text to Play Console
├── legal/                  ← Host privacy-policy.html
├── graphics/               ← Add your graphics here
└── documentation/          ← Reference information
```

## ✅ What's Already Done

- ✅ Store listing text (short & full descriptions)
- ✅ Privacy policy (HTML ready to host)
- ✅ Build scripts (automated)
- ✅ App icon (512x512)
- ✅ Release notes
- ✅ Keywords
- ✅ App metadata

## ❌ What You Need to Do

- [ ] Create keystore (run script)
- [ ] Build AAB (run script)
- [ ] Create feature graphic (1024x500)
- [ ] Capture screenshots (2-8 images)
- [ ] Host privacy policy (get URL)
- [ ] Submit to Play Console

## 🔥 Pro Tips

1. **Start with Internal Testing** - Don't go straight to production
2. **Test the AAB** - Install and test before submitting
3. **Use Canva** - Easy way to create feature graphic
4. **Take Good Screenshots** - They significantly impact downloads
5. **Respond Quickly** - If Google requests changes

## ⏱️ Timeline

- Your work: 4-6 hours
- Google review: 1-7 days (usually 1-3 days)
- **Total**: About 1 week to go live

## 📞 Need Help?

Email: almanar.backup@gmail.com  
Subject: Kuttab Play Store

## 📚 More Details

- `README.md` - Full documentation
- `../PLAY_STORE_SUBMISSION_CHECKLIST.md` - Complete checklist
- `../SCREENSHOTS_GUIDE.md` - Graphics guide

---

**You're ready to publish! Follow these 3 steps and you'll be live soon! 🚀**
