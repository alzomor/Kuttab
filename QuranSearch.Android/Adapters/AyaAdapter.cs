using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using QuranSearch.Core.Models;
using System;
using System.Collections.Generic;

namespace QuranSearch.Android.Adapters;

public class AyaAdapter : RecyclerView.Adapter
{
    private List<QuranAya> _items = new();
    private int _selectedPosition = -1;
    public event EventHandler<int>? ItemClick;
    
    public void UpdateData(List<QuranAya> items)
    {
        _items = items;
        NotifyDataSetChanged();
    }
    
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
        // Set reference
        _ayaReference.Text = $"{aya.SurahNumber}:{aya.AyaNumber}";
        
        // Set Arabic text with highlighting
        if (aya.MatchPositions != null && aya.MatchPositions.Count > 0)
        {
            var spannableString = new SpannableString(aya.Text);
            
            foreach (var match in aya.MatchPositions)
            {
                // Orange background for matched text
                var backgroundSpan = new BackgroundColorSpan(Color.Orange);
                spannableString.SetSpan(
                    backgroundSpan, 
                    match.Start, 
                    match.Start + match.Length, 
                    SpanTypes.ExclusiveExclusive);
                
                // Bold text for matched parts
                var boldSpan = new StyleSpan(TypefaceStyle.Bold);
                spannableString.SetSpan(
                    boldSpan, 
                    match.Start, 
                    match.Start + match.Length, 
                    SpanTypes.ExclusiveExclusive);
            }
            
            _arabicText.TextFormatted = spannableString;
        }
        else
        {
            _arabicText.Text = aya.Text;
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
        
        // Set matched part info
        if (aya.MatchPositions != null && aya.MatchPositions.Count > 0)
        {
            var firstMatch = aya.MatchPositions[0];
            var matchedText = aya.Text.Substring(firstMatch.Start, Math.Min(firstMatch.Length, 30));
            if (firstMatch.Length > 30)
                matchedText += "...";
            _matchedPart.Text = $"Matched: {matchedText}";
            _matchedPart.Visibility = ViewStates.Visible;
        }
        else
        {
            _matchedPart.Visibility = ViewStates.Gone;
        }
    }
}
