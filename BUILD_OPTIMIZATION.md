# Build Optimization - Distribution Size Reduction

## Issues Fixed

### 1. **Dual Output Folders**
- **Before**: Build created both `publish/` and `dist/` folders
- **After**: Only `dist/` folder remains (publish folder is automatically cleaned up)

### 2. **Large Distribution Size**
- **Before**: Alhusary audio folder (~6243 files) was included in builds
- **After**: Excluded from distribution completely

### 3. **Unnecessary Files**
- **Before**: QuranText_jpg folder could be included
- **After**: Explicitly excluded from all builds

## Changes Made

### build.py
1. **Removed** `ensure_extra_files()` function that copied Alhusary folder
2. **Added** `cleanup_publish_folder()` function to remove temporary publish folder after archiving
3. **Updated** build process to clean up after creating archives

### QuranSearchApp.csproj
Added explicit exclusions for large folders:
```xml
<Compile Remove="Alhusary\**" />
<Compile Remove="QuranText_jpg\**" />
<EmbeddedResource Remove="Alhusary\**" />
<EmbeddedResource Remove="QuranText_jpg\**" />
<None Remove="Alhusary\**" />
<None Remove="QuranText_jpg\**" />
<Content Remove="Alhusary\**" />
<Content Remove="QuranText_jpg\**" />
```

## Distribution Contents

The final distribution packages now include **only**:
- ✅ QuranSearchApp executable (single file)
- ✅ quran-uthmani.txt (Quran text)
- ✅ rules.json (Tajweed rules)
- ✅ Localization/*.json (Language files)
- ❌ ~~Alhusary audio files~~ (excluded)
- ❌ ~~QuranText_jpg images~~ (excluded)

## Expected Size Reduction

- **Before**: ~100+ MB per platform (with audio files)
- **After**: ~5-10 MB per platform (executables only)

**Size reduction: ~90-95%**

## Build Process

The build process now:
1. Creates desktop platform builds in `publish/{platform}/` (temporary)
2. Archives them to `dist/` folder
3. Builds Android APK and copies to `dist/` folder
4. **Cleans up** the `publish/` folder automatically
5. Leaves only final archives in `dist/`

## Distribution Platforms

The script now builds for **5 platforms**:
- ✅ Windows (x64)
- ✅ Linux (x64)
- ✅ macOS (Intel x64)
- ✅ macOS (Apple Silicon ARM64)
- ✅ **Android APK** (NEW!)

## Usage

```bash
# Run the build script as before
python build.py

# Output will be in dist/ folder only
ls dist/
# Quraan-win-x64-0.6.0.zip
# Quraan-linux-x64-0.6.0.tar.gz
# Quraan-macOS-x64-0.6.0.tar.gz
# Quraan-macOS-AppleSilicon-0.6.0.tar.gz
# Quraan-android-0.6.0.apk  ← NEW!
```

## Note for Users

Users who need audio files should:
1. Download the distribution package
2. Separately download or copy the Alhusary folder
3. Place it next to the executable

This keeps distribution sizes manageable while allowing users to optionally add audio support.
