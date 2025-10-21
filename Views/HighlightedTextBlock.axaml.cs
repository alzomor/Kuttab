using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using System.Collections.Generic;
using System.Linq;
using QuranSearchApp.Models;

namespace QuranSearchApp.Views;

public partial class HighlightedTextBlock : UserControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<HighlightedTextBlock, string>(nameof(Text), string.Empty);

    public static readonly StyledProperty<List<MatchPosition>> MatchPositionsProperty =
        AvaloniaProperty.Register<HighlightedTextBlock, List<MatchPosition>>(nameof(MatchPositions), new List<MatchPosition>());

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public List<MatchPosition> MatchPositions
    {
        get => GetValue(MatchPositionsProperty);
        set => SetValue(MatchPositionsProperty, value);
    }

    public HighlightedTextBlock()
    {
        InitializeComponent();
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == TextProperty || e.Property == MatchPositionsProperty)
        {
            UpdateHighlightedText();
        }
    }

    private void UpdateHighlightedText()
    {
        var textBlock = this.FindControl<TextBlock>("MainTextBlock");  // find MainTextBlock  in the XAML
        if (textBlock == null) return;                                                  //return if not found

        textBlock.Inlines?.Clear();                                                    // Clear existing text and formatting

        if (string.IsNullOrEmpty(Text))                                             //if no text ,then no need for highlighting
            return;

        if (MatchPositions == null || !MatchPositions.Any())                     //if no matches, just display the text as is   
        {
            textBlock.Inlines?.Add(new Run { Text = Text });
            return;
        }

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
        
        // Helper to check if character is a diacritic
        bool IsDiacritic(char c)
        {
            return (c >= '\u064B' && c <= '\u065F') ||  // Arabic diacritics
                   (c >= '\u0670' && c <= '\u06DC') ||  // Extended Arabic marks
                   c == '\u0640';                        // Tatweel
        }
        
        var sortedPositions = MatchPositions.OrderBy(m => m.Start).ToList();
        int currentIndex = 0;

        foreach (var match in sortedPositions)
        {
            // Determine word boundaries for smart ZWJ insertion - skip diacritics first
            bool isAtWordStart = match.Start == 0;
            if (!isAtWordStart && match.Start > 0)
            {
                // Skip diacritics backward to check for actual word boundary
                int checkIndex = match.Start - 1;
                while (checkIndex >= 0 && IsDiacritic(Text[checkIndex]))
                {
                    checkIndex--;
                }
                isAtWordStart = checkIndex < 0 || char.IsWhiteSpace(Text[checkIndex]);
            }
            
            bool isAtWordEnd = match.Start + match.Length >= Text.Length;
            if (!isAtWordEnd)
            {
                // Skip diacritics forward to check for actual word boundary
                int checkIndex = match.Start + match.Length;
                while (checkIndex < Text.Length && IsDiacritic(Text[checkIndex]))
                {
                    checkIndex++;
                }
                isAtWordEnd = checkIndex >= Text.Length || char.IsWhiteSpace(Text[checkIndex]);
            }
            
            // Check if characters actually connect in Arabic
            bool prevCharConnects = false;
            bool nextCharConnects = false;
            
            if (!isAtWordStart && match.Start > 0)
            {
                // Skip diacritics to find the actual letter before the match
                int prevIndex = match.Start - 1;
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
            else if (match.Start + match.Length < Text.Length)
            {
                // Skip diacritics backwards from end of match to find last actual letter
                int lastCharIndex = match.Start + match.Length - 1;
                while (lastCharIndex >= match.Start && IsDiacritic(Text[lastCharIndex]))
                {
                    lastCharIndex--;
                }
                
                if (lastCharIndex >= match.Start)
                {
                    char highlightLastChar = Text[lastCharIndex];
                    
                    // Skip diacritics FORWARD from end of match to find next actual character
                    int nextCharIndex = match.Start + match.Length;
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
            if (match.Start > currentIndex)
            {
                var beforeText = Text.Substring(currentIndex, match.Start - currentIndex);
                // Add ZWJ at end only if previous character actually connects
                if (prevCharConnects)
                {
                    beforeText += ZWJ;
                }
                textBlock.Inlines?.Add(new Run { Text = beforeText });
            }

            // Add highlighted match with orange background
            var matchText = Text.Substring(match.Start, match.Length);
            
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

            currentIndex = match.Start + match.Length;
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
