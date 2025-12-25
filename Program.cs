using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Kuttab.ViewModels;
using Kuttab.Views;
using System;
using System.Text;

namespace Kuttab;

public partial class App : Application
{
    public override void Initialize()
    {
        // Load XAML resources
        AvaloniaXamlLoader.Load(this);
        
        // Set up theme resources
        if (Current != null)
        {
            // Set light theme by default
            RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;
            
            // Add theme resources if not already present
            if (!Current.Resources.TryGetResource("ThemeBackgroundBrush", null, out _))
            {
                Current.Resources.Add("ThemeBackgroundBrush", new SolidColorBrush(Color.Parse("#F5F5DC")));
                Current.Resources.Add("CardBackground", new SolidColorBrush(Colors.White));
                Current.Resources.Add("TextColor", new SolidColorBrush(Color.Parse("#333333")));
                Current.Resources.Add("SecondaryTextColor", new SolidColorBrush(Color.Parse("#555555")));
            }
        }
        
        // Run Unicode test on startup (commented out for production)
        // TestUnicodePreservation();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    private void TestUnicodePreservation()
    {
        try
        {
            // Test the specific characters you mentioned
            var testChars = new[] { 'ۘ', 'ۛ' };
            var allStopSigns = new[] { 'ۚ', 'ۖ', 'ۗ', 'ۙ', 'ۘ', 'ۛ' };
            
            Console.WriteLine("=== Unicode Character Test ===");
            
            // Test individual characters
            foreach (var ch in testChars)
            {
                Console.WriteLine($"Character: {ch}");
                Console.WriteLine($"Unicode Code Point: U+{((int)ch):X4}");
                Console.WriteLine($"UTF-16 Encoding: {Encoding.Unicode.GetBytes(ch.ToString()).Length} bytes");
                Console.WriteLine($"UTF-8 Encoding: {Encoding.UTF8.GetBytes(ch.ToString()).Length} bytes");
                Console.WriteLine();
            }
            
            // Test string operations that happen in highlighting
            string testText = "بِسْمِ اللَّهِ الرَّحْمَٰنِ الرَّحِيمِۘ الْحَمْدُ لِلَّهِ رَبِّ الْعَالَمِينَۛ";
            Console.WriteLine($"Original text: {testText}");
            Console.WriteLine($"Text length: {testText.Length}");
            
            // Find positions of the characters
            foreach (var stopSign in allStopSigns)
            {
                int index = testText.IndexOf(stopSign);
                if (index >= 0)
                {
                    Console.WriteLine($"\nFound '{stopSign}' at position {index}");
                    
                    // Test substring operations like in the highlighting code
                    int start = Math.Max(0, index - 1);
                    int length = Math.Min(3, testText.Length - start);
                    
                    string extracted = testText.Substring(start, length);
                    Console.WriteLine($"Substring({start}, {length}): '{extracted}'");
                    
                    // Verify the character is still there
                    bool stillContains = extracted.Contains(stopSign);
                    Console.WriteLine($"Character preserved: {stillContains}");
                    
                    // Check each character in the substring
                    Console.Write("Characters in substring: ");
                    foreach (char c in extracted)
                    {
                        Console.Write($"'{c}' (U+{((int)c):X4}) ");
                    }
                    Console.WriteLine();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unicode test error: {ex.Message}");
        }
    }
}

class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .AfterSetup(builder =>
            {
                // Ensure default theme is set after setup
                if (builder.Instance is App app)
                {
                    app.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;
                }
            });
}
