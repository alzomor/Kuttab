# Kuttab v0.92 - Release Notes

## What's New

### 🔄 **Runtime Data Updates**
- **Donation amounts now update automatically** from GitHub without app reinstall
- **Push notifications reduced to 1-hour check** for time-sensitive alerts (previously 6 hours)
- **Improved cache system** for faster loading and offline support

### 🎯 **Enhanced Speech Recognition**
- **ASR alternative results support** - tries up to 3 speech recognition alternatives for better accuracy
- **Smart skip/recovery logic** - handles missed or wrong ASR words by looking ahead 2-3 words
- **Cumulative partial results handling** - prevents re-processing already matched words

### 🌙 **Ramadan Features**
- **Ramadan end notification** (Mar 13-20, 2026) with thawab message
- **Updated donation progress**: 850,000 EUR raised, 150,000 EUR remaining
- **Encourages sharing app for thawab with family and friends**

### 🐛 **Bug Fixes**
- Fixed "Speech recognition not available" dialog appearing after session completion
- Improved handling of repeated words (e.g., "الرحمن الرحيم") to prevent false skips
- Better error recovery from ASR inaccuracies

## Technical Details
- Version: 0.92
- Build: Optimized for all Android architectures (ARM32, ARM64, x86, x64)
- Size: 44 MB (AAB), 43 MB (APK)
- Minimum Android: 5.0 (API 21)
- Target Android: 15 (API 35)

## Benefits for Users
- **More accurate Quran recitation recognition**
- **Real-time donation progress updates**
- **Timely Ramadan notifications**
- **Smoother Hifz mode experience**
- **Better handling of speech recognition errors**
