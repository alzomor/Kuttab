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

        var sortedPositions = MatchPositions.OrderBy(m => m.Start).ToList();
        int currentIndex = 0;

        foreach (var match in sortedPositions)
        {
            // Add text before the match
            if (match.Start > currentIndex)
            {
                var beforeText = Text.Substring(currentIndex, match.Start - currentIndex);
                textBlock.Inlines?.Add(new Run { Text = beforeText });
            }

            // Add highlighted match with yellow background
            if (match.Length == 1)
            {
                match.Length = 3; // to make it more visible, highlight 2 characters if only one character is matched
                if (match.Start > 0)
                    match.Start--;
            }
            var matchText = Text.Substring(match.Start, match.Length);
            var highlightedRun = new Run 
            { 
                Text = matchText,
                Background = Brushes.Yellow,
                Foreground = Brushes.DarkBlue
            };
            textBlock.Inlines?.Add(highlightedRun);

            currentIndex = match.Start + match.Length;
        }

        // Add remaining text after the last match
        if (currentIndex < Text.Length)
        {
            var remainingText = Text.Substring(currentIndex);
            textBlock.Inlines?.Add(new Run { Text = remainingText });
        }
    }
}
