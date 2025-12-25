# TODO - Quran Search Application

## 🔴 KNOWN ISSUES (Phase 1 - Desktop)

### ❌ Text Highlighting Not Working in Avalonia

**Status**: UNRESOLVED  
**Priority**: Medium (Workaround applied)  
**Severity**: UI Polish Issue

#### Problem Description:
The `HighlightedTextBlock` custom control that worked perfectly in MAUI is NOT being instantiated in Avalonia. The control's constructor is never called, meaning no highlighting is applied to matched Tajweed rule text.

#### Symptoms:
- No `[HighlightedTextBlock]` debug messages appear in console
- No orange background highlighting on matched text
- Control constructor never executes
- No errors or warnings - **silent failure**

#### Root Cause Analysis:
1. **DataTemplate instantiation failure** - The custom UserControl is not being created within the ListBox ItemTemplate
2. **Avalonia vs MAUI differences**:
   - MAUI: Custom controls in DataTemplates work seamlessly
   - Avalonia: Different binding/templating system causing silent failure
3. **Attempted fixes that didn't work**:
   - ✅ Fixed property system (switched from `new List<>()` to `null` default)
   - ✅ Added proper `coerce` callbacks for property changes
   - ✅ Disabled compiled bindings with `x:CompileBindings="False"`
   - ✅ Simplified XAML (removed complex font references)
   - ✅ Added extensive debug logging
   - ❌ **None of these resolved the instantiation issue**

#### Current Workaround:
Plain `TextBlock` displays text without highlighting. All functionality works except visual highlighting of matched portions.

#### Files Involved:
- `/home/hossam/Work/Quraan/Views/HighlightedTextBlock.axaml` - Custom control XAML
- `/home/hossam/Work/Quraan/Views/HighlightedTextBlock.axaml.cs` - Custom control code-behind
- `/home/hossam/Work/Quraan/Views/MainWindow.axaml` (line 230-270) - ItemTemplate using the control

#### Potential Solutions to Try Later:
1. **Inline rendering** - Implement highlighting directly in ItemTemplate without custom control
2. **Behavior pattern** - Use Avalonia Behaviors instead of UserControl
3. **Value converter** - Create a converter that returns formatted text with `TextBlock.Inlines`
4. **Code-behind approach** - Programmatically create text runs in ViewModel
5. **Community help** - Post on Avalonia GitHub/Discord with minimal repro

#### Impact Assessment:
- ✅ Does NOT block Phase 2 (Android development)
- ✅ Core functionality works (search, audio, images, localization)
- ✅ Can be fixed in parallel with Phase 2
- ⚠️ Reduces user experience (no visual highlighting)

---

## 🚀 PHASE 2: Android Migration

### Overview
Create Android version of the Quran Search application using the shared `QuranSearch.Core` library.

### Prerequisites - ✅ COMPLETE
- ✅ Core library extracted and tested
- ✅ Desktop app restructured and working
- ✅ Build system functional (0 errors)
- ✅ All services properly abstracted
- ✅ Platform-independent models and logic

---

### Step 1: Project Setup

#### 1.1 Create Android Project
```bash
cd /home/hossam/Work/Quraan
dotnet new android -n QuranSearch.Android
cd QuranSearch.Android
```

#### 1.2 Add Reference to Core Library
Edit `QuranSearch.Android.csproj`:
```xml
<ItemGroup>
  <ProjectReference Include="../QuranSearch.Core/QuranSearch.Core.csproj" />
</ItemGroup>
```

#### 1.3 Install Required NuGet Packages
```bash
dotnet add package Xamarin.AndroidX.AppCompat
dotnet add package Xamarin.AndroidX.RecyclerView
dotnet add package Xamarin.Google.Android.Material
```

---

### Step 2: Implement Platform Services

#### 2.1 AndroidFileService
**File**: `Services/AndroidFileService.cs`

```csharp
using Android.Content;
using QuranSearch.Core.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace QuranSearch.Android.Services;

public class AndroidFileService : IFileService
{
    private readonly Context _context;
    
    public AndroidFileService(Context context)
    {
        _context = context;
    }
    
    public string GetAppDataDirectory()
    {
        return _context.FilesDir.AbsolutePath;
    }
    
    public bool FileExists(string path)
    {
        // Check assets first
        try
        {
            using var stream = _context.Assets.Open(path);
            return true;
        }
        catch
        {
            // Check internal storage
            var fullPath = Path.Combine(GetAppDataDirectory(), path);
            return File.Exists(fullPath);
        }
    }
    
    public async Task<string> ReadAllTextAsync(string path)
    {
        try
        {
            // Try reading from assets
            using var stream = _context.Assets.Open(path);
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
        catch
        {
            // Read from internal storage
            var fullPath = Path.Combine(GetAppDataDirectory(), path);
            return await File.ReadAllTextAsync(fullPath);
        }
    }
    
    public string ReadAllText(string path)
    {
        try
        {
            using var stream = _context.Assets.Open(path);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        catch
        {
            var fullPath = Path.Combine(GetAppDataDirectory(), path);
            return File.ReadAllText(fullPath);
        }
    }
    
    public Stream GetFileStream(string path)
    {
        try
        {
            return _context.Assets.Open(path);
        }
        catch
        {
            var fullPath = Path.Combine(GetAppDataDirectory(), path);
            return File.OpenRead(fullPath);
        }
    }
}
```

#### 2.2 AndroidAudioService
**File**: `Services/AndroidAudioService.cs`

```csharp
using Android.Media;
using QuranSearch.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace QuranSearch.Android.Services;

public class AndroidAudioService : IAudioService
{
    private MediaPlayer? _mediaPlayer;
    
    public bool UseRemoteSource { get; set; } = true;
    
    public event EventHandler? PlaybackStateChanged;
    public event EventHandler<string>? PlaybackError;
    public event EventHandler? SequencePlaybackEnded;
    
    public async Task PlayAudioAsync(string audioPath)
    {
        StopPlayback();
        
        _mediaPlayer = new MediaPlayer();
        
        if (UseRemoteSource)
        {
            // Use online URL
            _mediaPlayer.SetDataSource(audioPath);
        }
        else
        {
            // Use local file
            _mediaPlayer.SetDataSource(audioPath);
        }
        
        _mediaPlayer.Completion += (s, e) =>
        {
            PlaybackStateChanged?.Invoke(this, EventArgs.Empty);
        };
        
        await _mediaPlayer.PrepareAsync();
        _mediaPlayer.Start();
        
        PlaybackStateChanged?.Invoke(this, EventArgs.Empty);
    }
    
    public void StopPlayback()
    {
        if (_mediaPlayer != null)
        {
            _mediaPlayer.Stop();
            _mediaPlayer.Release();
            _mediaPlayer = null;
            PlaybackStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    
    // Implement other IAudioService methods...
}
```

#### 2.3 AndroidPictureService
**File**: `Services/AndroidPictureService.cs`

```csharp
using Android.Content;
using Android.Graphics;
using QuranSearch.Core.Interfaces;
using System.Threading.Tasks;

namespace QuranSearch.Android.Services;

public class AndroidPictureService : IPictureService
{
    private readonly Context _context;
    
    public AndroidPictureService(Context context)
    {
        _context = context;
    }
    
    public bool PictureExists(int surahNumber, int ayaNumber)
    {
        var filename = $"QuranText_jpg/{surahNumber}_{ayaNumber}.jpg";
        try
        {
            using var stream = _context.Assets.Open(filename);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public string GetPicturePath(int surahNumber, int ayaNumber)
    {
        return $"QuranText_jpg/{surahNumber}_{ayaNumber}.jpg";
    }
    
    public async Task<object?> LoadBitmapAsync(string path)
    {
        try
        {
            using var stream = _context.Assets.Open(path);
            return await BitmapFactory.DecodeStreamAsync(stream);
        }
        catch
        {
            return null;
        }
    }
}
```

---

### Step 3: Android UI Implementation

#### 3.1 Main Activity
**File**: `MainActivity.cs`

```csharp
using Android.App;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;
using QuranSearch.Core.Services;
using QuranSearch.Android.Services;

namespace QuranSearch.Android;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : AppCompatActivity
{
    private QuranSearchService _searchService;
    private RecyclerView _recyclerView;
    private AyaAdapter _adapter;
    
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_main);
        
        // Initialize services
        var fileService = new AndroidFileService(this);
        _searchService = new QuranSearchService(fileService);
        
        // Setup UI
        _recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
        _recyclerView.SetLayoutManager(new LinearLayoutManager(this));
        _adapter = new AyaAdapter();
        _recyclerView.SetAdapter(_adapter);
        
        // Load data
        LoadDataAsync();
    }
    
    private async void LoadDataAsync()
    {
        await _searchService.LoadQuranTextAsync();
        await _searchService.LoadRulesAsync();
        
        // Update UI with loaded rules
        var rules = _searchService.GetRuleNames();
        // Populate spinner/dropdown with rules
    }
}
```

#### 3.2 RecyclerView Adapter
**File**: `Adapters/AyaAdapter.cs`

```csharp
using Android.Views;
using AndroidX.RecyclerView.Widget;
using QuranSearch.Core.Models;
using System.Collections.Generic;

namespace QuranSearch.Android.Adapters;

public class AyaAdapter : RecyclerView.Adapter
{
    private List<QuranAya> _items = new();
    
    public void UpdateData(List<QuranAya> items)
    {
        _items = items;
        NotifyDataSetChanged();
    }
    
    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
    {
        var view = LayoutInflater.From(parent.Context)
            .Inflate(Resource.Layout.item_aya, parent, false);
        return new AyaViewHolder(view);
    }
    
    public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
    {
        if (holder is AyaViewHolder ayaHolder)
        {
            ayaHolder.Bind(_items[position]);
        }
    }
    
    public override int ItemCount => _items.Count;
}
```

---

### Step 4: Assets and Resources

#### 4.1 Copy Data Files to Assets
```bash
mkdir -p QuranSearch.Android/Assets
cp quran-uthmani.txt QuranSearch.Android/Assets/
cp rules.json QuranSearch.Android/Assets/
cp -r Localization QuranSearch.Android/Assets/
```

#### 4.2 Update Android Project File
```xml
<ItemGroup>
  <AndroidAsset Include="Assets\**" />
</ItemGroup>
```

---

### Step 5: Build and Test

#### 5.1 Build Android APK
```bash
cd QuranSearch.Android
dotnet build
```

#### 5.2 Deploy to Emulator/Device
```bash
dotnet publish -f net8.0-android -c Release
adb install bin/Release/net8.0-android/QuranSearch.Android-Signed.apk
```

---

## 📋 Phase 2 Checklist

### Core Functionality
- [x] Create Android project
- [x] Reference Core library
- [x] Implement AndroidFileService
- [x] Implement AndroidAudioService  
- [x] Implement AndroidPictureService
- [x] Copy assets (quran text, rules, localization)

### UI Implementation
- [x] Create MainActivity
- [x] Implement rule selection (Spinner/Dropdown)
- [x] Implement search button and logic
- [x] Create RecyclerView for results
- [x] Implement ViewHolder for Aya items
- [x] Add Arabic text display (RTL support)
- [x] Implement text highlighting (SpannableString with ZWJ)

### Features
- [x] Audio playback integration (with proper MediaPlayer lifecycle)
- [x] Image viewing integration (everyayah.com CDN)
- [x] Multi-language support (AR/EN/DE)
- [x] RTL/LTR layout switching
- [x] Play single/repeat/sequence functionality

### Testing
- [x] Test on physical device
- [x] Test all Tajweed rules
- [x] Test audio playback
- [x] Test image display
- [x] Test language switching

### Deployment
- [x] Configure AndroidManifest.xml
- [x] Set permissions (INTERNET, ACCESS_NETWORK_STATE)
- [x] Test debug builds on device
- [x] Create app theme and splash screen (Islamic green/gold theme)
- [ ] Generate signed APK for production
- [ ] Publish to Play Store (future)

---

## 📁 Expected File Structure After Phase 2

```
/home/hossam/Work/Quraan/
├── QuranSearch.Core/           # ✅ Shared library (DONE)
│   ├── Models/
│   ├── Services/
│   ├── Interfaces/
│   └── ViewModels/
│
├── QuranSearchApp/             # ✅ Desktop (Avalonia) (DONE)
│   ├── Services/
│   ├── ViewModels/
│   └── Views/
│
└── QuranSearch.Android/        # 🚀 Android (TO DO)
    ├── Services/
    │   ├── AndroidFileService.cs
    │   ├── AndroidAudioService.cs
    │   └── AndroidPictureService.cs
    ├── Activities/
    │   └── MainActivity.cs
    ├── Adapters/
    │   └── AyaAdapter.cs
    ├── Resources/
    │   ├── layout/
    │   ├── values/
    │   └── drawable/
    └── Assets/
        ├── quran-uthmani.txt
        ├── rules.json
        └── Localization/
```

---

## 🎯 Success Criteria

### Phase 1 (Desktop) - ✅ COMPLETE (with 1 known issue)
- ✅ Core library created and functional
- ✅ Desktop app restructured
- ✅ Build succeeds (0 errors)
- ✅ Search functionality works
- ✅ Audio playback works
- ✅ Image display works
- ✅ Multi-language works
- ⚠️ Highlighting not working (workaround applied)

### Phase 2 (Android) - ✅ COMPLETE
- [x] Android app builds successfully
- [x] All Tajweed search features work
- [x] Audio playback works on Android
- [x] Images display correctly
- [x] RTL text renders properly
- [x] App installable on devices
- [x] 70%+ code shared with desktop
- [x] Zero-Width Joiner (ZWJ) preserves Arabic contextual forms
- [x] Multilingual UI (Arabic, English, German)
- [x] Local-first storage with on-demand downloads

---

## 💡 Notes

- Desktop highlighting issue does NOT block Android development
- Android can implement highlighting using SpannableString (different approach)
- All business logic is ready in Core library
- Android development can proceed independently

---

**Last Updated**: 2025-11-10 02:00 UTC+01:00  
**Status**: ✅ Phase 1 Complete (Desktop - Avalonia), ✅ Phase 2 Complete (Android)

## 🎉 Phase 2 Completion Summary

### Major Achievements:
1. **Audio Playback Fixed**
   - Resolved MediaPlayer error -38 (lifecycle issue)
   - Configured audio routing to device speakers
   - Set volume to maximum for audibility

2. **Multilingual Support**
   - Arabic, English, and German translations
   - Real-time language switching
   - Consistent icons across all languages

3. **Arabic Text Rendering**
   - Proper RTL alignment in RecyclerView
   - Zero-Width Joiner (ZWJ) preserves contextual letter forms
   - Forced text direction in adapter after setting SpannableString

4. **Image Display**
   - everyayah.com CDN integration
   - Full-width display below search results
   - Local-first caching strategy

5. **Network Management**
   - Local storage first approach
   - On-demand file downloads
   - Offline error handling

6. **Professional UI Theme**
   - Islamic/Quran theme with green and gold colors
   - Custom splash screen on app launch
   - Smooth theme transition to main app
   - Consistent branding throughout

### Commits Created:
- Audio playback fixes and local-first file management
- Language switching, contextual forms, and image improvements
- Arabic text right-alignment fixes
- Multilingual button translations with consistent icons
- Final Arabic text alignment in adapter
- App theme and splash screen with Islamic colors

### Files Modified/Created:
- `QuranSearch.Android/MainActivity.cs`
- `QuranSearch.Android/Services/AndroidAudioService.cs`
- `QuranSearch.Android/Services/AndroidPictureService.cs`
- `QuranSearch.Android/Services/AndroidFileService.cs`
- `QuranSearch.Android/Adapters/AyaAdapter.cs`
- `QuranSearch.Android/Resources/layout/activity_main.xml`
- `QuranSearch.Android/Resources/layout/item_aya.xml`
- `QuranSearch.Android/Resources/values/colors.xml`
- `QuranSearch.Android/Resources/values/styles.xml`
- `QuranSearch.Android/Resources/drawable/splash_screen.xml`
- `QuranSearch.Android/Assets/Localization/*.json`
- `QuranSearch.Android/AndroidManifest.xml`

### Testing Status:
- ✅ Tested on physical Android device (R58R12688GY)
- ✅ All features verified working
- ✅ Audio, images, language switching, RTL alignment confirmed
