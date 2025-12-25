using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using Kuttab.Core.Models;

namespace Kuttab.Views;

public partial class HighlightedTextBlock : UserControl
{
    private TextBlock? _mainTextBlock;
    
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<HighlightedTextBlock, string?>(
            nameof(Text), 
            string.Empty,
            coerce: (obj, value) =>
            {
                if (obj is HighlightedTextBlock control)
                {
                    Console.WriteLine($"[HighlightedTextBlock] Text property changed via coerce");
                    // Schedule update on next UI thread cycle
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => control.UpdateHighlightedText(), Avalonia.Threading.DispatcherPriority.Normal);
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
                    Console.WriteLine($"[HighlightedTextBlock] MatchPositions property changed via coerce with {value?.Count ?? 0} items");
                    // Schedule update on next UI thread cycle
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => control.UpdateHighlightedText(), Avalonia.Threading.DispatcherPriority.Normal);
                }
                return value;
            });

    public string? Text
    {
        get => GetValue(TextProperty);
        set
        {
            Console.WriteLine($"[HighlightedTextBlock] Text property setter called with: {value?.Substring(0, Math.Min(30, value?.Length ?? 0))}...");
            SetValue(TextProperty, value);
        }
    }

    public List<MatchPosition>? MatchPositions
    {
        get => GetValue(MatchPositionsProperty);
        set
        {
            Console.WriteLine($"[HighlightedTextBlock] MatchPositions property setter called with {value?.Count ?? 0} positions");
            SetValue(MatchPositionsProperty, value);
        }
    }

    public HighlightedTextBlock()
    {
        Console.WriteLine("[HighlightedTextBlock] *** CONSTRUCTOR CALLED ***");
        InitializeComponent();
        
        // Cache the TextBlock reference after initialization
        this.AttachedToVisualTree += (s, e) =>
        {
            Console.WriteLine("[HighlightedTextBlock] *** ATTACHED TO VISUAL TREE ***");
            _mainTextBlock = this.FindControl<TextBlock>("MainTextBlock");
            Console.WriteLine($"[HighlightedTextBlock] MainTextBlock found: {_mainTextBlock != null}");
            UpdateHighlightedText(); // Update after control is ready
        };
    }

    private void UpdateHighlightedText()
    {
        var textBlock = _mainTextBlock ?? this.FindControl<TextBlock>("MainTextBlock");
        if (textBlock == null)
        {
            Console.WriteLine("[HighlightedTextBlock] ERROR: MainTextBlock not found!");
            return;
        }

        textBlock.Inlines?.Clear();                                                    // Clear existing text and formatting

        if (string.IsNullOrEmpty(Text))                                             //if no text ,then no need for highlighting
            return;

        if (MatchPositions == null || !MatchPositions.Any())                     //if no matches, just display the text as is   
        {
            Console.WriteLine($"[HighlightedTextBlock] No match positions for text: {Text?.Substring(0, Math.Min(50, Text?.Length ?? 0))}...");
            textBlock.Inlines?.Add(new Run { Text = Text });
            return;
        }
        
        Console.WriteLine($"[HighlightedTextBlock] Highlighting {MatchPositions.Count} matches in text: {Text?.Substring(0, Math.Min(50, Text?.Length ?? 0))}...");

        // Zero-Width Joiner to maintain Arabic contextual forms when splitting text
        const string ZWJ = "\u200D";
        
        // Local function for checking non-connector letters (right-joining only)
        // Arabic letters that don't connect to the following letter: ا د ذ ر ز و
        bool IsNonConnector(char c)
        {
            return c == 'ا' || c == 'أ' || c == 'إ' || c == 'آ' || c == 'ٱ' ||  // Alif variants
                   c == 'د' || c == 'ذ' ||  // Dal, Thal
                   c == 'ر' || c == 'ز' ||  // Ra, Zay
                   c == 'و' || c == 'ؤ' ||  // Waw variants
                   c == 'ٰ' || c == 'ۥ' || c == 'ۦ';  // Small Alif, small Waw, small Ya
        }
        
        // Helper to check if character is a diacritic or Quranic annotation mark
        bool IsDiacritic(char c)
        {
            return (c >= '\u064B' && c <= '\u065F') ||  // Arabic diacritics (ً ٌ ٍ َ ُ ِ ّ ْ)
                   (c >= '\u0670' && c <= '\u06DC') ||  // Extended Arabic marks (includes ۖ ۗ ۘ ۙ ۚ ۛ ۜ)
                   c == '\u0640' ||                      // Tatweel (ـ)
                   c == '\u06DD' ||                      // Arabic end of ayah
                   c == '\u06DE' ||                      // ۞ Start of rub el hizb
                   c == '\u06E9';                        // ۩ Place of sajdah
        }
        
        var sortedPositions = MatchPositions.OrderBy(m => m.Start).ToList();
        int currentIndex = 0;

        foreach (var match in sortedPositions)
        {
            // Compute adjusted span to ensure visibility for non-spacing marks (e.g., stop signs)
            int adjustedStart = match.Start;
            int adjustedLength = match.Length;
            int matchEnd = match.Start + match.Length;
            bool spanHasBaseChar = false;
            for (int i = match.Start; i < matchEnd && i < Text.Length; i++)
            {
                if (!IsDiacritic(Text[i]))
                {
                    spanHasBaseChar = true;
                    break;
                }
            }

            if (!spanHasBaseChar)
            {
                // For non-spacing marks (like stop signs), include BOTH preceding and following context
                bool extendedBefore = false;
                bool extendedAfter = false;
                
                // Try to include following whitespace or character
                if (matchEnd < Text.Length && char.IsWhiteSpace(Text[matchEnd]))
                {
                    adjustedLength += 1; // include following space
                    extendedAfter = true;
                }
                
                // Try to include preceding whitespace or character
                if (match.Start > 0 && char.IsWhiteSpace(Text[match.Start - 1]))
                {
                    adjustedStart -= 1; // include preceding space
                    adjustedLength += 1;
                    extendedBefore = true;
                }

                // If no whitespace before, include previous base character
                if (!extendedBefore)
                {
                    int prev = match.Start - 1;
                    while (prev >= 0 && IsDiacritic(Text[prev])) prev--;
                    if (prev >= 0)
                    {
                        adjustedLength += (match.Start - prev);
                        adjustedStart = prev;
                        extendedBefore = true;
                    }
                }
                
                // If no whitespace after, include next base character
                if (!extendedAfter)
                {
                    int next = matchEnd;
                    while (next < Text.Length && IsDiacritic(Text[next])) next++;
                    if (next < Text.Length)
                    {
                        adjustedLength = (next - adjustedStart) + 1;
                        extendedAfter = true;
                    }
                }
            }

            // Determine word boundaries for smart ZWJ insertion - skip diacritics first
            bool isAtWordStart = adjustedStart == 0;
            if (!isAtWordStart && adjustedStart > 0)
            {
                // Skip diacritics backward to check for actual word boundary
                int checkIndex = adjustedStart - 1;
                while (checkIndex >= 0 && IsDiacritic(Text[checkIndex]))
                {
                    checkIndex--;
                }
                isAtWordStart = checkIndex < 0 || char.IsWhiteSpace(Text[checkIndex]);
            }
            
            bool isAtWordEnd = adjustedStart + adjustedLength >= Text.Length;
            if (!isAtWordEnd)
            {
                // Skip diacritics forward to check for actual word boundary
                int checkIndex = adjustedStart + adjustedLength;
                while (checkIndex < Text.Length && IsDiacritic(Text[checkIndex]))
                {
                    checkIndex++;
                }
                isAtWordEnd = checkIndex >= Text.Length || char.IsWhiteSpace(Text[checkIndex]);
            }
            
            // Check if characters actually connect in Arabic
            bool prevCharConnects = false;
            bool nextCharConnects = false;
            
            if (!isAtWordStart && adjustedStart > 0)
            {
                // Skip diacritics to find the actual letter before the match
                int prevIndex = adjustedStart - 1;
                while (prevIndex >= 0 && IsDiacritic(Text[prevIndex]))
                {
                    prevIndex--;
                }
                
                if (prevIndex >= 0)
                {
                    char prevChar = Text[prevIndex];
                    // Previous char connects if it's not a non-connector and not whitespace
                    prevCharConnects = !IsNonConnector(prevChar) && !char.IsWhiteSpace(prevChar);
                }
            }
            
            // At word end, never connect forward
            if (isAtWordEnd)
            {
                nextCharConnects = false;
            }
            else if (adjustedStart + adjustedLength < Text.Length)
            {
                // Skip diacritics backwards from end of match to find last actual letter
                int lastCharIndex = adjustedStart + adjustedLength - 1;
                while (lastCharIndex >= match.Start && IsDiacritic(Text[lastCharIndex]))
                {
                    lastCharIndex--;
                }
                
                if (lastCharIndex >= adjustedStart)
                {
                    char highlightLastChar = Text[lastCharIndex];
                    
                    // Skip diacritics FORWARD from end of match to find next actual character
                    int nextCharIndex = adjustedStart + adjustedLength;
                    while (nextCharIndex < Text.Length && IsDiacritic(Text[nextCharIndex]))
                    {
                        nextCharIndex++;
                    }
                    
                    // Only connect if:
                    // 1. Last highlighted char is not a non-connector
                    // 2. Next actual character exists and is not whitespace
                    nextCharConnects = !IsNonConnector(highlightLastChar) &&
                                      nextCharIndex < Text.Length &&
                                      !char.IsWhiteSpace(Text[nextCharIndex]);
                }
            }
            
            // Add text before the match
            if (adjustedStart > currentIndex)
            {
                var beforeText = Text.Substring(currentIndex, adjustedStart - currentIndex);
                // Add ZWJ at end only if previous character actually connects
                if (prevCharConnects)
                {
                    beforeText += ZWJ;
                }
                textBlock.Inlines?.Add(new Run { Text = beforeText });
            }

            // Add highlighted match with orange background
            var matchText = Text.Substring(adjustedStart, Math.Min(adjustedLength, Math.Max(0, Text.Length - adjustedStart)));
            
            // Add ZWJ only where characters actually connect in Arabic
            string prefix = prevCharConnects ? ZWJ : "";
            string suffix = nextCharConnects ? ZWJ : "";
            
            var highlightedRun = new Run 
            { 
                Text = prefix + matchText + suffix,
                Background = Brushes.Orange,  // Changed from Yellow to Orange for better visibility
                Foreground = Brushes.Black,   // Changed from DarkBlue to Black for better contrast
                FontWeight = FontWeight.Bold  // Added bold to make stop signs more prominent
            };
            textBlock.Inlines?.Add(highlightedRun);

            currentIndex = adjustedStart + adjustedLength;
        }

        // Add remaining text after the last match
        if (currentIndex < Text.Length)
        {
            var remainingText = Text.Substring(currentIndex);
            // Check if we need ZWJ at start (if last match's last char connects)
            if (sortedPositions.Any())
            {
                var lastMatch = sortedPositions.Last();
                
                // Skip diacritics backwards to find last actual letter in match
                int lastCharIndex = lastMatch.Start + lastMatch.Length - 1;
                while (lastCharIndex >= lastMatch.Start && lastCharIndex < Text.Length && 
                       IsDiacritic(Text[lastCharIndex]))
                {
                    lastCharIndex--;
                }
                
                if (lastCharIndex >= lastMatch.Start && lastCharIndex < Text.Length)
                {
                    char lastChar = Text[lastCharIndex];
                    
                    // Skip diacritics forward to find next actual character after match
                    int nextCharIndex = lastMatch.Start + lastMatch.Length;
                    while (nextCharIndex < Text.Length && IsDiacritic(Text[nextCharIndex]))
                    {
                        nextCharIndex++;
                    }
                    
                    bool lastMatchAtWordEnd = (nextCharIndex >= Text.Length) || 
                                             char.IsWhiteSpace(Text[nextCharIndex]);
                    
                    // Add ZWJ only if last char connects and next actual char is not whitespace/boundary
                    if (!lastMatchAtWordEnd && !IsNonConnector(lastChar))
                    {
                        remainingText = ZWJ + remainingText;
                    }
                }
            }
            textBlock.Inlines?.Add(new Run { Text = remainingText });
        }
    }
}
