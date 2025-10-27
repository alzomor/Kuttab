# Highlighting Issue - Technical Details

## Problem Statement

The `HighlightedTextBlock` custom UserControl does NOT instantiate when used in a ListBox ItemTemplate in Avalonia, despite working perfectly in MAUI.

## Symptoms

1. **No constructor execution** - `Console.WriteLine` in constructor never appears
2. **No property setters called** - Text and MatchPositions setters never fire
3. **No visual tree attachment** - `AttachedToVisualTree` event never triggers
4. **Silent failure** - No errors, no warnings, no exceptions
5. **Binding appears correct** - XAML structure looks valid

## Code Location

### Custom Control
- **XAML**: `/home/hossam/Work/Quraan/Views/HighlightedTextBlock.axaml`
- **Code**: `/home/hossam/Work/Quraan/Views/HighlightedTextBlock.axaml.cs`

### Usage
- **File**: `/home/hossam/Work/Quraan/Views/MainWindow.axaml`
- **Lines**: 229-270 (ListBox ItemTemplate)

## XAML Structure

### Control Definition (HighlightedTextBlock.axaml)
```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="QuranSearchApp.Views.HighlightedTextBlock">
    <TextBlock Name="MainTextBlock" 
               FontSize="20" 
               FontFamily="Scheherazade New"
               TextWrapping="Wrap"
               FlowDirection="RightToLeft"/>
</UserControl>
```

### Usage in DataTemplate
```xml
<ListBox.ItemTemplate>
    <DataTemplate DataType="models:QuranAya" x:CompileBindings="False">
        <Border ...>
            <StackPanel>
                <!-- Header -->
                <Border>...</Border>
                
                <!-- This control is NOT being created -->
                <views:HighlightedTextBlock 
                    Text="{Binding Text}"
                    MatchPositions="{Binding MatchPositions}"/>
            </StackPanel>
        </Border>
    </DataTemplate>
</ListBox.ItemTemplate>
```

## Property System Implementation

### StyledProperty Definitions
```csharp
public static readonly StyledProperty<string?> TextProperty =
    AvaloniaProperty.Register<HighlightedTextBlock, string?>(
        nameof(Text), 
        string.Empty,
        coerce: (obj, value) =>
        {
            if (obj is HighlightedTextBlock control)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(
                    () => control.UpdateHighlightedText(), 
                    Avalonia.Threading.DispatcherPriority.Normal);
            }
            return value;
        });

public static readonly StyledProperty<List<MatchPosition>?> MatchPositionsProperty =
    AvaloniaProperty.Register<HighlightedTextBlock, List<MatchPosition>?>(
        nameof(MatchPositions), 
        defaultValue: null,
        coerce: (obj, value) =>
        {
            if (obj is HighlightedTextBlock control)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(
                    () => control.UpdateHighlightedText(), 
                    Avalonia.Threading.DispatcherPriority.Normal);
            }
            return value;
        });
```

### Constructor with Debug Logging
```csharp
public HighlightedTextBlock()
{
    Console.WriteLine("[HighlightedTextBlock] *** CONSTRUCTOR CALLED ***");
    InitializeComponent();
    
    this.AttachedToVisualTree += (s, e) =>
    {
        Console.WriteLine("[HighlightedTextBlock] *** ATTACHED TO VISUAL TREE ***");
        _mainTextBlock = this.FindControl<TextBlock>("MainTextBlock");
        UpdateHighlightedText();
    };
}
```

**Result**: None of these messages ever appear in console.

## Attempted Fixes

### 1. Property System Fix
**Issue**: MAUI allows `new List<>()` as default, Avalonia doesn't  
**Fix Applied**: Changed to `defaultValue: null`  
**Result**: ❌ Did not resolve instantiation issue

### 2. Change Callbacks
**Issue**: No property change notifications  
**Fix Applied**: Added `coerce` callbacks (Avalonia's approach)  
**Result**: ❌ Callbacks never fire (control not created)

### 3. Compiled Bindings
**Issue**: Avalonia's compiled bindings might block custom controls  
**Fix Applied**: Added `x:CompileBindings="False"` to DataTemplate  
**Result**: ❌ Did not resolve issue

### 4. XAML Simplification
**Issue**: Complex font references might cause parse errors  
**Fix Applied**: Simplified to basic TextBlock with simple font name  
**Result**: ❌ Did not resolve issue

### 5. Namespace Declaration
**Issue**: Wrong namespace might prevent instantiation  
**Fix Applied**: Verified `xmlns:views="using:QuranSearchApp.Views"`  
**Result**: ✅ Namespace is correct

### 6. Clean Rebuild
**Issue**: Stale build artifacts  
**Fix Applied**: `rm -rf bin/ obj/` and full rebuild  
**Result**: ❌ Did not resolve issue

### 7. Debug Visibility
**Issue**: Hard to diagnose without visual feedback  
**Fix Applied**: Added red debug text and yellow background  
**Result**: ❌ Nothing appeared (confirms no instantiation)

## Comparison: MAUI vs Avalonia

| Aspect | MAUI | Avalonia | Issue |
|--------|------|----------|-------|
| Property default | `new List<>()` OK | Must be `null` | ✅ Fixed |
| Binding system | Simpler | Compiled bindings | Tried fix |
| DataTemplate | Works seamlessly | More complex | ? |
| UserControl in template | Works | Silent failure | ❌ Problem |
| Font references | `avares://` works | Might fail silently | Tried fix |

## What Works

1. ✅ Plain `TextBlock` in same DataTemplate works fine
2. ✅ Data binding to `Text` property works (when using plain TextBlock)
3. ✅ Data binding to `MatchPositions` works (data is there)
4. ✅ Other controls in DataTemplate work (Border, StackPanel, etc.)
5. ✅ The custom control compiles without errors
6. ✅ The control is in the DLL (verified with `strings` command)

## What Doesn't Work

1. ❌ Custom UserControl instantiation in DataTemplate
2. ❌ Constructor never executes
3. ❌ Property setters never called
4. ❌ Visual tree attachment never occurs
5. ❌ No error messages or warnings

## Possible Root Causes

### Theory 1: DataTemplate Instantiation Issue
Avalonia might handle custom controls in DataTemplates differently than MAUI. The control might need special registration or a different approach.

### Theory 2: XAML Parsing Silent Failure
The AXAML file might have a subtle issue that causes Avalonia to skip it without error. The parser might fall back to not creating the control.

### Theory 3: Compiled Bindings Interference
Despite disabling with `x:CompileBindings="False"`, the globally enabled compiled bindings might still interfere with custom control creation.

### Theory 4: Resource Loading Issue
Fonts or other resources might fail to load, causing the control creation to fail silently without propagating the error.

### Theory 5: Initialization Order
The UserControl might require initialization that isn't happening in the DataTemplate context, causing instantiation to abort.

## Workaround

Replace custom control with plain TextBlock:

```xml
<TextBlock Text="{Binding Text}"
           FontSize="20"
           FontFamily="Scheherazade New"
           TextWrapping="Wrap"
           FlowDirection="RightToLeft"/>
```

**Impact**:
- ✅ Text displays correctly
- ❌ No highlighting on matched portions
- ✅ All other functionality works

## Potential Solutions to Try

### Option 1: Inline Rendering (Recommended)
Implement highlighting directly in the DataTemplate without custom control:

```xml
<ItemsControl ItemsSource="{Binding TextSegments}">
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Text}" 
                       Background="{Binding IsHighlighted, 
                           Converter={StaticResource BoolToColorConverter}}"
                       Foreground="Black"/>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <WrapPanel Orientation="Horizontal"/>
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
</ItemsControl>
```

This would require preparing text segments in ViewModel.

### Option 2: Behavior Pattern
Use Avalonia Behaviors instead of UserControl:

```csharp
public class TextHighlightBehavior : Behavior<TextBlock>
{
    // Implement highlighting logic as behavior
}
```

### Option 3: Value Converter
Create converter that returns formatted text:

```csharp
public class HighlightConverter : IMultiValueConverter
{
    public object Convert(IList<object> values, ...)
    {
        var text = values[0] as string;
        var matches = values[1] as List<MatchPosition>;
        // Return formatted Inlines
    }
}
```

### Option 4: Code-Behind Approach
Programmatically create and format text in code-behind when template applies.

### Option 5: Community Help
Post minimal reproducible example to:
- Avalonia GitHub Issues
- Avalonia Discord #help channel
- Stack Overflow with `avaloniaui` tag

## Android Approach (Phase 2)

For Android, use native `SpannableString` instead:

```csharp
var spannable = new SpannableString(text);
foreach (var match in matchPositions)
{
    spannable.SetSpan(
        new BackgroundColorSpan(Color.Orange),
        match.Start,
        match.Start + match.Length,
        SpanTypes.ExclusiveExclusive);
}
textView.TextFormatted = spannable;
```

This is the standard Android way and should work perfectly.

## Conclusion

This is an **Avalonia-specific issue** with custom UserControl instantiation in DataTemplates. It worked in MAUI, which suggests the code logic is correct. The issue appears to be with Avalonia's template system or XAML parser.

**Recommendation**: Proceed with Phase 2 (Android) using native Android highlighting. Revisit this issue later with fresh perspective or community input.

---

**Documented**: 2025-10-27 01:15 UTC+01:00  
**Status**: Unresolved, Workaround Applied  
**Blocks**: No (UI polish only)
