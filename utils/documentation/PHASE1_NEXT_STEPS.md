# Phase 1 - Next Steps: Integrate Core with Desktop

## ✅ Completed So Far (70%)

### Core Library Structure Created:
```
QuranSearch.Core/
├── Models/                     ✅
│   ├── QuranAya.cs
│   └── TajweedRule.cs
├── Services/                   ✅
│   ├── AllahFilter.cs
│   ├── QuranSearchService.cs
│   └── LocalizationService.cs
├── Interfaces/                 ✅
│   ├── IAudioService.cs
│   ├── IPictureService.cs
│   └── IFileService.cs
├── ViewModels/                 ✅
│   ├── ViewModelBase.cs
│   └── SimpleCommand.cs
└── QuranSearch.Core.csproj     ✅
```

---

## 🎯 Remaining Tasks (30%)

### Task 1: Create DesktopFileService (15 minutes)

**File**: `/Services/DesktopFileService.cs` (NEW)

```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using QuranSearch.Core.Interfaces;

namespace QuranSearchApp.Services;

/// <summary>
/// Desktop implementation of IFileService
/// Uses standard .NET File API
/// </summary>
public class DesktopFileService : IFileService
{
    public string GetAppDataDirectory()
    {
        return AppDomain.CurrentDomain.BaseDirectory;
    }

    public bool FileExists(string path)
    {
        var fullPath = Path.IsPathRooted(path) 
            ? path 
            : Path.Combine(GetAppDataDirectory(), path);
        return File.Exists(fullPath);
    }

    public async Task<string> ReadAllTextAsync(string path)
    {
        var fullPath = Path.IsPathRooted(path) 
            ? path 
            : Path.Combine(GetAppDataDirectory(), path);
        return await File.ReadAllTextAsync(fullPath);
    }

    public Stream GetFileStream(string path)
    {
        var fullPath = Path.IsPathRooted(path) 
            ? path 
            : Path.Combine(GetAppDataDirectory(), path);
        return File.OpenRead(fullPath);
    }
}
```

---

### Task 2: Update Desktop Project Reference (5 minutes)

**File**: `QuranSearchApp.csproj`

**Add this inside `<ItemGroup>` with other PackageReferences:**
```xml
<ItemGroup>
  <!-- Add reference to Core -->
  <ProjectReference Include="QuranSearch.Core\QuranSearch.Core.csproj" />
</ItemGroup>
```

---

### Task 3: Create MainWindowViewModel in Core (20 minutes)

**Option A: I create it for you** (Recommended)
- I'll create a modified version that uses dependency injection
- Constructor will accept: `IFileService`, `IAudioService`, `IPictureService`
- All service instantiation becomes dependency injection

**Option B: You want to review first**
- I provide the changes needed
- You review and approve
- Then I implement

**Which do you prefer?**

---

### Task 4: Update Desktop MainWindowViewModel (10 minutes)

**File**: `/ViewModels/MainWindowViewModel.Desktop.cs` (NEW - Desktop-specific wrapper)

This will:
1. Inherit from Core's MainWindowViewModel
2. Add any desktop-specific properties
3. Handle Avalonia-specific types (like Bitmap)

OR

**Alternative**: Delete desktop MainWindowViewModel and use Core's directly

---

### Task 5: Update Using Statements (5 minutes)

**Files needing updates:**
- `/Views/MainWindow.axaml.cs`
- `/Program.cs`

**Change:**
```csharp
// OLD
using QuranSearchApp.Models;
using QuranSearchApp.ViewModels;

// NEW
using QuranSearch.Core.Models;
using QuranSearch.Core.ViewModels;
using QuranSearch.Core.Services;
```

---

### Task 6: Wire Up Services in Program.cs (10 minutes)

**File**: `/Program.cs`

**Update to use dependency injection:**
```csharp
public static void Main(string[] args)
{
    BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
}

public static AppBuilder BuildAvaloniaApp()
{
    // Create services
    var fileService = new DesktopFileService();
    var audioService = new AudioService(); // Your existing one
    var pictureService = new PictureService(); // Your existing one
    
    // Create ViewModel with dependencies
    var viewModel = new MainWindowViewModel(
        fileService, 
        audioService, 
        pictureService
    );
    
    return AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace()
        .UseReactiveUI();
}
```

---

### Task 7: Test Desktop App (30 minutes)

**Commands:**
```bash
cd /home/hossam/Work/Quraan
dotnet clean
dotnet build
dotnet run
```

**Test Checklist:**
- ✅ App starts without errors
- ✅ Search works
- ✅ Audio playback works
- ✅ Picture display works
- ✅ Language switching works
- ✅ All existing features work

---

## 🎯 Decision Point: MainWindowViewModel Strategy

I see **2 options** for handling MainWindowViewModel:

### Option 1: Full Core Implementation (Recommended)
**Pros:**
- ✅ Maximum code reuse
- ✅ Single ViewModel works for Desktop + Android
- ✅ Clean architecture

**Cons:**
- ⚠️ Needs Avalonia-specific types abstracted (Bitmap → interface)
- ⚠️ FlowDirection needs to be handled per-platform

**Changes Needed:**
1. Create `IImageType` interface in Core
2. MainWindowViewModel uses `IImageType` instead of `Bitmap`
3. Desktop: `Bitmap` implements `IImageType`
4. Android: Android bitmap implements `IImageType`

### Option 2: Shared Base + Platform Extensions
**Pros:**
- ✅ Easier to implement
- ✅ Platform-specific code stays in platform projects

**Cons:**
- ⚠️ Some code duplication
- ⚠️ Need to maintain both versions

**Structure:**
```
Core/ViewModels/MainViewModelBase.cs  (shared logic)
Desktop/ViewModels/MainWindowViewModel.cs (desktop-specific)
Android/ViewModels/MainViewModel.cs (android-specific)
```

---

## ❓ Your Decision Needed

Before I continue, please choose:

### **Question 1: MainWindowViewModel Strategy**
- [ ] **Option 1**: Full Core implementation (I'll abstract Bitmap/FlowDirection)
- [ ] **Option 2**: Shared base with platform-specific extensions

### **Question 2: Ready to Update Desktop?**
- [ ] **Yes**: Proceed with integrating Core into desktop
- [ ] **Review First**: Show me the changes before applying

### **Question 3: Timeline Preference**
- [ ] **Complete Phase 1 Today**: Get desktop working with Core (2-3 hours)
- [ ] **Review & Approve**: I want to review each step before proceeding

---

## 📊 Time Estimates

| Task | Time | Status |
|------|------|--------|
| Create DesktopFileService | 15 min | ⚪ Pending |
| Update Project Reference | 5 min | ⚪ Pending |
| Create Core MainWindowViewModel | 20 min | ⚪ Pending |
| Update Desktop ViewModel | 10 min | ⚪ Pending |
| Update Using Statements | 5 min | ⚪ Pending |
| Wire Up Services | 10 min | ⚪ Pending |
| Test Desktop App | 30 min | ⚪ Pending |
| **Total Remaining** | **~1.5 hours** | **30%** |

---

## 🚀 What Happens After Phase 1?

Once desktop works with Core:
1. ✅ **Zero risk** - Desktop continues working perfectly
2. ✅ **Ready for Android** - Can start Phase 2
3. ✅ **Clean architecture** - Business logic separated from UI
4. ✅ **MAUI option** - Still available if needed

---

## 📝 Current Status

**Phase 1 Progress**: 70% ✅  
**Desktop Impact**: None yet (still using old code) ✅  
**Core Library**: Fully functional ✅  
**Next Action**: Awaiting your decision on MainWindowViewModel strategy  

**Ready to proceed when you are!** 🎯
