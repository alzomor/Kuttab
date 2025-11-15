using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using QuranSearch.Core.Models;
using QuranSearch.Core.Services;
using System;
using System.Collections.Generic;

namespace QuranSearch.Android.Adapters;

public class AyaAdapter : RecyclerView.Adapter
{
    private List<QuranAya> _items = new();
    private int _selectedPosition = -1;
    public event EventHandler<int>? ItemClick;
    private LocalizationService? _localization;
    private bool _isRtl = false;
    
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
    
    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
    {
        var view = LayoutInflater.From(parent.Context)?
            .Inflate(Resource.Layout.item_aya, parent, false);
        if (view == null)
            throw new InvalidOperationException("Failed to inflate item_aya layout");
        return new AyaViewHolder(view, OnItemClick, this);
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
}

public class AyaViewHolder : RecyclerView.ViewHolder
{
    private readonly TextView _ayaReference;
    private readonly TextView _arabicText;
    private readonly TextView _matchedPart;
    private readonly View _itemView;
    private readonly AyaAdapter _adapter;
    
    public AyaViewHolder(View itemView, Action<int> clickListener, AyaAdapter adapter) : base(itemView)
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
            // Add Zero-Width Joiner (ZWJ) before and after highlighted parts
            // to preserve Arabic contextual letter forms
            const char ZWJ = '\u200D';
            var textBuilder = new System.Text.StringBuilder(aya.Text);
            var offsetAdjustment = 0;
            
            // Sort matches by start position
            var sortedMatches = aya.MatchPositions.OrderBy(m => m.Start).ToList();
            
            foreach (var match in sortedMatches)
            {
                var adjustedStart = match.Start + offsetAdjustment;
                
                // Insert ZWJ before the match
                textBuilder.Insert(adjustedStart, ZWJ);
                offsetAdjustment++;
                
                // Insert ZWJ after the match
                textBuilder.Insert(adjustedStart + match.Length + 1, ZWJ);
                offsetAdjustment++;
            }
            
            var adjustedText = textBuilder.ToString();
            var spannableString = new SpannableString(adjustedText);
            offsetAdjustment = 0;
            
            foreach (var match in sortedMatches)
            {
                var adjustedStart = match.Start + offsetAdjustment;
                var adjustedLength = match.Length + 2; // Include both ZWJs
                
                // Orange background for matched text
                var backgroundSpan = new BackgroundColorSpan(Color.Orange);
                spannableString.SetSpan(
                    backgroundSpan, 
                    adjustedStart, 
                    adjustedStart + adjustedLength, 
                    SpanTypes.ExclusiveExclusive);
                
                // Bold text for matched parts
                var boldSpan = new StyleSpan(TypefaceStyle.Bold);
                spannableString.SetSpan(
                    boldSpan, 
                    adjustedStart, 
                    adjustedStart + adjustedLength, 
                    SpanTypes.ExclusiveExclusive);
                
                offsetAdjustment += 2; // Account for the two ZWJs added
            }
            
            _arabicText.TextFormatted = spannableString;
            // Ensure RTL and right alignment are preserved after setting formatted text
            _arabicText.TextDirection = _adapter.IsRtl 
                ? global::Android.Views.TextDirection.Rtl 
                : global::Android.Views.TextDirection.Ltr;
            _arabicText.Gravity = _adapter.IsRtl 
                ? global::Android.Views.GravityFlags.End 
                : global::Android.Views.GravityFlags.Start;
            _arabicText.TextAlignment = _adapter.IsRtl 
                ? global::Android.Views.TextAlignment.ViewEnd 
                : global::Android.Views.TextAlignment.ViewStart;
        }
        else
        {
            _arabicText.Text = aya.Text;
            // Ensure RTL and right alignment
            _arabicText.TextDirection = _adapter.IsRtl 
                ? global::Android.Views.TextDirection.Rtl 
                : global::Android.Views.TextDirection.Ltr;
            _arabicText.Gravity = _adapter.IsRtl 
                ? global::Android.Views.GravityFlags.End 
                : global::Android.Views.GravityFlags.Start;
        }
        
        // Set selection background
        if (AdapterPosition == _adapter.GetSelectedPosition())
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
