using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Kuttab.Core.Models;
using Kuttab.Core.Services;
using System;
using System.Collections.Generic;

namespace Kuttab.Android.Adapters;

public class AyaAdapter : RecyclerView.Adapter
{
    private List<QuranAya> _items = new();
    private int _selectedPosition = -1;
    private int _playingPosition = -1;
    public event EventHandler<int>? ItemClick;
    public event EventHandler<int>? ItemLongClick;
    private LocalizationService? _localization;
    private bool _isRtl = false;
    private int _fontSize = 18;
    
    public void UpdateData(List<QuranAya> items)
    {
        _items = items;
        // Reset previous selection/highlight on new dataset
        _selectedPosition = -1;
        NotifyDataSetChanged();
    }
    
    public void SetLocalization(LocalizationService localization, bool isRtl)
    {
        _localization = localization;
        _isRtl = isRtl;
        NotifyDataSetChanged();
    }
    
    public void SetFontSize(int fontSize)
    {
        if (fontSize >= 12 && fontSize <= 32)
        {
            _fontSize = fontSize;
            NotifyDataSetChanged();
        }
    }
    
    public int GetFontSize() => _fontSize;
    
    public string GetMatchedLabel()
    {
        // Fallback to English if localization not available
        return _localization?.GetString("Matched") ?? "Matched:";
    }
    
    public bool IsRtl => _isRtl;
    
    public QuranAya? GetItem(int position)
    {
        if (position >= 0 && position < _items.Count)
            return _items[position];
        return null;
    }
    
    public int GetSelectedPosition() => _selectedPosition;
    
    public int GetPlayingPosition() => _playingPosition;
    
    public void SetPlayingPosition(int position)
    {
        var previousPosition = _playingPosition;
        _playingPosition = position;
        
        // Update previous playing item
        if (previousPosition != -1)
        {
            NotifyItemChanged(previousPosition);
        }
        
        // Update new playing item
        if (position != -1)
        {
            NotifyItemChanged(position);
        }
    }
    
    public void ClearPlayingPosition()
    {
        var previousPosition = _playingPosition;
        _playingPosition = -1;
        
        if (previousPosition != -1)
        {
            NotifyItemChanged(previousPosition);
        }
    }
    
    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
    {
        var view = LayoutInflater.From(parent.Context)?
            .Inflate(Resource.Layout.item_aya, parent, false);
        if (view == null)
            throw new InvalidOperationException("Failed to inflate item_aya layout");
        return new AyaViewHolder(view, OnItemClick, OnItemLongClick, this);
    }
    
    public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
    {
        if (holder is AyaViewHolder ayaHolder && position < _items.Count)
        {
            ayaHolder.Bind(_items[position]);
        }
    }
    
    public override int ItemCount => _items.Count;
    
    private void OnItemClick(int position)
    {
        var previousPosition = _selectedPosition;
        _selectedPosition = position;
        
        // Update previous selection
        if (previousPosition != -1)
        {
            NotifyItemChanged(previousPosition);
        }
        
        // Update new selection
        NotifyItemChanged(position);
        
        ItemClick?.Invoke(this, position);
    }
    
    private void OnItemLongClick(int position)
    {
        ItemLongClick?.Invoke(this, position);
    }
}

public class AyaViewHolder : RecyclerView.ViewHolder
{
    private readonly TextView _ayaReference;
    private readonly TextView _arabicText;
    private readonly TextView _matchedPart;
    private readonly View _itemView;
    private readonly AyaAdapter _adapter;
    
    public AyaViewHolder(View itemView, Action<int> clickListener, Action<int> longClickListener, AyaAdapter adapter) : base(itemView)
    {
        _itemView = itemView;
        _adapter = adapter;
        _ayaReference = itemView.FindViewById<TextView>(Resource.Id.ayaReference) 
            ?? throw new InvalidOperationException("ayaReference not found");
        _arabicText = itemView.FindViewById<TextView>(Resource.Id.arabicText) 
            ?? throw new InvalidOperationException("arabicText not found");
        _matchedPart = itemView.FindViewById<TextView>(Resource.Id.matchedPart) 
            ?? throw new InvalidOperationException("matchedPart not found");
        
        itemView.Click += (s, e) => clickListener(AdapterPosition);
        itemView.LongClick += (s, e) => longClickListener(AdapterPosition);
    }
    
    public void Bind(QuranAya aya)
    {
        // Set reference with Surah name
        var surahName = SurahInfo.GetSurahName(aya.SurahNumber);
        if (!string.IsNullOrEmpty(surahName))
        {
            _ayaReference.Text = $"{aya.SurahNumber} ({surahName}): {aya.AyaNumber}";
        }
        else
        {
            _ayaReference.Text = $"{aya.SurahNumber}:{aya.AyaNumber}";
        }
        
        // Set Arabic text with highlighting
        if (aya.MatchPositions != null && aya.MatchPositions.Count > 0)
        {
            const char ZWJ = '\u200D';
            var textBuilder = new System.Text.StringBuilder(aya.Text);
            var offsetAdjustment = 0;
            
            // Store extended positions for stop signs (maps match index to adjusted start/length)
            var extendedPositions = new Dictionary<int, (int start, int length)>();

            bool IsDiacritic(char c)
            {
                return (c >= '\u064B' && c <= '\u065F') ||  // Arabic diacritics (ً ٌ ٍ َ ُ ِ ّ ْ)
                       (c >= '\u0670' && c <= '\u06DC') ||  // Extended Arabic marks (includes ۖ ۗ ۘ ۙ ۚ ۛ ۜ)
                       c == '\u0640' ||                      // Tatweel (ـ)
                       c == '\u06DD' ||                      // Arabic end of ayah
                       c == '\u06DE' ||                      // ۞ Start of rub el hizb
                       c == '\u06E9';                        // ۩ Place of sajdah
            }

            bool IsArabicLetter(char c)
            {
                return (c >= '\u0621' && c <= '\u064A') || c == '\u0671';
            }

            bool IsConnector(char c)
            {
                if (!IsArabicLetter(c)) return false;
                return c != '\u0627' && c != '\u0622' && c != '\u0623' && c != '\u0625' && c != '\u0671' &&
                       c != '\u062F' && c != '\u0630' && c != '\u0631' && c != '\u0632' && c != '\u0648' && c != '\u0621';
            }

            // Check if character can receive connection from the right
            // All Arabic letters except hamza (ء) can receive connections
            bool CanReceiveFromRight(char c)
            {
                if (!IsArabicLetter(c)) return false;
                return c != '\u0621'; // Hamza is the only letter that cannot receive connection
            }

            int FindPrevBaseIndex(string s, int start)
            {
                int i = start;
                while (i >= 0 && (IsDiacritic(s[i]) || s[i] == ZWJ)) i--;
                return i;
            }

            int FindNextBaseIndex(string s, int start)
            {
                int i = start;
                while (i < s.Length && (IsDiacritic(s[i]) || s[i] == ZWJ)) i++;
                return i < s.Length ? i : -1;
            }

            var sortedMatches = aya.MatchPositions.OrderBy(m => m.Start).ToList();

            for (int matchIndex = 0; matchIndex < sortedMatches.Count; matchIndex++)
            {
                var match = sortedMatches[matchIndex];
                
                // Check if match contains only diacritics/stop signs (no base characters)
                int adjustedLength = match.Length;
                int adjustedStart = match.Start;
                int matchEnd = match.Start + match.Length;
                bool spanHasBaseChar = false;
                
                for (int i = match.Start; i < matchEnd && i < aya.Text.Length; i++)
                {
                    if (!IsDiacritic(aya.Text[i]))
                    {
                        spanHasBaseChar = true;
                        break;
                    }
                }

                // For stop signs and non-spacing marks, extend to include surrounding context
                if (!spanHasBaseChar)
                {
                    bool extendedBefore = false;
                    bool extendedAfter = false;
                    
                    // Try to include following whitespace
                    if (matchEnd < aya.Text.Length && char.IsWhiteSpace(aya.Text[matchEnd]))
                    {
                        adjustedLength += 1;
                        extendedAfter = true;
                    }
                    
                    // Try to include preceding whitespace
                    if (match.Start > 0 && char.IsWhiteSpace(aya.Text[match.Start - 1]))
                    {
                        adjustedStart -= 1;
                        adjustedLength += 1;
                        extendedBefore = true;
                    }

                    // If no whitespace before, include previous base character
                    if (!extendedBefore)
                    {
                        int prev = match.Start - 1;
                        while (prev >= 0 && IsDiacritic(aya.Text[prev])) prev--;
                        if (prev >= 0)
                        {
                            adjustedLength += (match.Start - prev);
                            adjustedStart = prev;
                        }
                    }
                    
                    // If no whitespace after, include next base character
                    if (!extendedAfter)
                    {
                        int next = matchEnd;
                        while (next < aya.Text.Length && IsDiacritic(aya.Text[next])) next++;
                        if (next < aya.Text.Length)
                        {
                            adjustedLength = (next - adjustedStart) + 1;
                        }
                    }
                    
                    // Store the extended position for use in highlighting loop
                    extendedPositions[matchIndex] = (adjustedStart, adjustedLength);
                }

                adjustedStart += offsetAdjustment;
                var added = 0;

                var prevIndex = FindPrevBaseIndex(textBuilder.ToString(), adjustedStart - 1);
                var nextIndex = FindNextBaseIndex(textBuilder.ToString(), adjustedStart + adjustedLength);

                char firstChar = adjustedStart < textBuilder.Length ? textBuilder[adjustedStart] : '\0';
                char lastChar = (adjustedStart + adjustedLength - 1) < textBuilder.Length && adjustedLength > 0
                    ? textBuilder[adjustedStart + adjustedLength - 1]
                    : '\0';

                bool connectPrevToFirst = prevIndex >= 0 && IsConnector(textBuilder[prevIndex]) && IsConnector(firstChar);
                bool connectLastToNext = nextIndex >= 0 && IsConnector(lastChar) && CanReceiveFromRight(textBuilder[nextIndex]);

                // Helper: end of previous cluster (after its diacritics)
                int PrevClusterEnd()
                {
                    if (prevIndex < 0) return -1;
                    int pos = prevIndex + 1;
                    while (pos < textBuilder.Length && IsDiacritic(textBuilder[pos])) pos++;
                    return pos;
                }

                // Helper: position immediately after last base of match (before its diacritics)
                int MatchBaseEnd()
                {
                    return adjustedStart + adjustedLength;
                }

                // Helper: position after last base + its diacritics (between clusters)
                int MatchClusterEnd()
                {
                    int pos = adjustedStart + adjustedLength + added;
                    while (pos < textBuilder.Length && IsDiacritic(textBuilder[pos])) pos++;
                    return pos;
                }

                // BEFORE boundary: external ZWJ after previous cluster, internal ZWJ at start of match
                if (connectPrevToFirst)
                {
                    // External ZWJ after previous cluster
                    int extPos = PrevClusterEnd();
                    if (extPos >= 0 && !(extPos < textBuilder.Length && textBuilder[extPos] == ZWJ))
                    {
                        textBuilder.Insert(extPos, ZWJ);
                        // If the insert position is before adjustedStart, it shifts adjustedStart by +1
                        if (extPos <= adjustedStart) adjustedStart++;
                        added++;
                    }

                    // Internal ZWJ at the very start of the match
                    if (!(adjustedStart < textBuilder.Length && textBuilder[adjustedStart] == ZWJ))
                    {
                        textBuilder.Insert(adjustedStart, ZWJ);
                        added++;
                        // Match content shifted by +1 inside
                    }
                }

                // AFTER boundary: internal ZWJ right after last base, external ZWJ after trailing diacritics
                if (connectLastToNext)
                {
                    // Internal ZWJ immediately after the last base (before its diacritics)
                    int intAfterPos = MatchBaseEnd() + (connectPrevToFirst ? 1 : 0); // account for internal before ZWJ
                    if (!(intAfterPos < textBuilder.Length && textBuilder[intAfterPos] == ZWJ))
                    {
                        textBuilder.Insert(intAfterPos, ZWJ);
                        added++;
                    }

                    // External ZWJ after last base + its diacritics (between clusters)
                    int extAfterPos = MatchClusterEnd();
                    if (!(extAfterPos < textBuilder.Length && textBuilder[extAfterPos] == ZWJ))
                    {
                        textBuilder.Insert(extAfterPos, ZWJ);
                        added++;
                    }
                }

                offsetAdjustment += added;
            }

            var adjustedText = textBuilder.ToString();
            var spannableString = new SpannableString(adjustedText);

            for (int matchIndex = 0; matchIndex < sortedMatches.Count; matchIndex++)
            {
                var match = sortedMatches[matchIndex];
                
                // Use extended position if this is a stop sign match
                int matchStart = match.Start;
                int matchLength = match.Length;
                if (extendedPositions.TryGetValue(matchIndex, out var extended))
                {
                    matchStart = extended.start;
                    matchLength = extended.length;
                }
                
                // Count ALL ZWJs that appear BEFORE or AT this match's original position
                // This gives us the correct offset in the adjusted text
                // We need to find the position in adjustedText that corresponds to matchStart
                int zwjsBefore = 0;
                int originalPos = 0;
                int adjustedStart = 0;
                for (int i = 0; i < adjustedText.Length; i++)
                {
                    if (originalPos == matchStart)
                    {
                        // Found the position, but we need to skip any ZWJs at this exact position
                        // to find where the actual match content starts
                        adjustedStart = i;
                        // Skip leading ZWJs that were inserted at the match boundary
                        while (adjustedStart < adjustedText.Length && adjustedText[adjustedStart] == ZWJ)
                        {
                            adjustedStart++;
                        }
                        break;
                    }
                    if (adjustedText[i] == ZWJ)
                    {
                        zwjsBefore++;
                    }
                    else
                    {
                        originalPos++;
                    }
                }
                var beforeIndex = adjustedStart - 1;
                var afterIndex = adjustedStart + matchLength;
                
                // Include internal ZWJs (if any) but exclude external ZWJs
                int leftHasZWJ = (beforeIndex >= 0 && beforeIndex < adjustedText.Length && adjustedText[beforeIndex] == ZWJ) ? 1 : 0;
                int rightHasZWJ = (afterIndex >= 0 && afterIndex < adjustedText.Length && adjustedText[afterIndex] == ZWJ) ? 1 : 0;

                // Span starts at adjustedStart (may begin with internal ZWJ)
                var spanStart = adjustedStart;
                // Compute end: base match end
                var spanEnd = afterIndex;
                // If an internal ZWJ was inserted right after last base, it will be at 'afterIndex'
                if (spanEnd < adjustedText.Length && adjustedText[spanEnd] == ZWJ)
                    spanEnd++;
                // Include trailing diacritics after last base
                while (spanEnd < adjustedText.Length && IsDiacritic(adjustedText[spanEnd]))
                    spanEnd++;
                // Exclude external ZWJ after cluster if present
                if (spanEnd < adjustedText.Length && adjustedText[spanEnd] == ZWJ)
                {
                    // Do not extend span to cover this external ZWJ
                }

                var backgroundSpan = new BackgroundColorSpan(Color.Orange);
                spannableString.SetSpan(
                    backgroundSpan,
                    spanStart,
                    spanEnd,
                    SpanTypes.ExclusiveExclusive);

                // Bold styling can break Arabic joining across span boundaries; avoid bold for RTL Arabic
                if (!_adapter.IsRtl)
                {
                    var boldSpan = new StyleSpan(TypefaceStyle.Bold);
                    spannableString.SetSpan(
                        boldSpan,
                        spanStart,
                        spanEnd,
                        SpanTypes.ExclusiveExclusive);
                }
            }

            _arabicText.TextFormatted = spannableString;
            // Always use RTL and right alignment for Arabic Quranic text (use absolute Right, not relative End)
            _arabicText.TextDirection = global::Android.Views.TextDirection.Rtl;
            _arabicText.Gravity = global::Android.Views.GravityFlags.Right;
            _arabicText.TextAlignment = global::Android.Views.TextAlignment.ViewEnd;
            _arabicText.SetTextSize(global::Android.Util.ComplexUnitType.Sp, _adapter.GetFontSize());
        }
        else
        {
            _arabicText.Text = aya.Text;
            // Always use RTL and right alignment for Arabic Quranic text (use absolute Right, not relative End)
            _arabicText.TextDirection = global::Android.Views.TextDirection.Rtl;
            _arabicText.Gravity = global::Android.Views.GravityFlags.Right;
            _arabicText.TextAlignment = global::Android.Views.TextAlignment.ViewEnd;
            _arabicText.SetTextSize(global::Android.Util.ComplexUnitType.Sp, _adapter.GetFontSize());
        }
        
        // Set selection/playing background
        if (AdapterPosition == _adapter.GetPlayingPosition())
        {
            _itemView.SetBackgroundColor(Color.ParseColor("#C8E6C9")); // Light green for currently playing
        }
        else if (AdapterPosition == _adapter.GetSelectedPosition())
        {
            _itemView.SetBackgroundColor(Color.ParseColor("#E3F2FD")); // Light blue for selected
        }
        else
        {
            _itemView.SetBackgroundColor(Color.Transparent);
        }
        
        // Set matched part info (all matches, localized label, and alignment by language)
        if (!string.IsNullOrWhiteSpace(aya.MatchedText))
        {
            var label = _adapter.GetMatchedLabel();
            _matchedPart.Text = $"{label} {aya.MatchedText}";
            _matchedPart.Visibility = ViewStates.Visible;
            _matchedPart.TextDirection = _adapter.IsRtl 
                ? global::Android.Views.TextDirection.Rtl 
                : global::Android.Views.TextDirection.Ltr;
            _matchedPart.Gravity = _adapter.IsRtl 
                ? global::Android.Views.GravityFlags.End 
                : global::Android.Views.GravityFlags.Start;
            _matchedPart.TextAlignment = _adapter.IsRtl 
                ? global::Android.Views.TextAlignment.ViewEnd 
                : global::Android.Views.TextAlignment.ViewStart;
        }
        else
        {
            _matchedPart.Visibility = ViewStates.Gone;
        }
    }
}
