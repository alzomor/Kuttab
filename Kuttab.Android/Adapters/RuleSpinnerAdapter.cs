using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;

namespace Kuttab.Android.Adapters
{
    public class RuleSpinnerAdapter : ArrayAdapter<string>
    {
        private readonly HashSet<string> _groupHeaders;
        
        public RuleSpinnerAdapter(Context context, int resource, List<string> items, HashSet<string> groupHeaders) 
            : base(context, resource, items)
        {
            _groupHeaders = groupHeaders;
        }

        public override View GetView(int position, View? convertView, ViewGroup parent)
        {
            var view = base.GetView(position, convertView, parent);
            var textView = view.FindViewById<TextView>(global::Android.Resource.Id.Text1);
            
            if (textView != null)
            {
                var item = GetItem(position);
                if (item != null && _groupHeaders.Contains(item))
                {
                    textView.SetTypeface(Typeface.DefaultBold, TypefaceStyle.Bold);
                    textView.SetTextColor(global::Android.Graphics.Color.DarkGray);
                }
                else
                {
                    textView.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                    textView.SetTextColor(global::Android.Graphics.Color.Black);
                }
            }
            
            return view;
        }

        public override View GetDropDownView(int position, View? convertView, ViewGroup parent)
        {
            var view = base.GetDropDownView(position, convertView, parent);
            var textView = view.FindViewById<TextView>(global::Android.Resource.Id.Text1);
            
            if (textView != null)
            {
                var item = GetItem(position);
                if (item != null && _groupHeaders.Contains(item))
                {
                    // Make group headers bold and non-selectable looking
                    textView.SetTypeface(Typeface.DefaultBold, TypefaceStyle.Bold);
                    textView.SetTextColor(global::Android.Graphics.Color.DarkGray);
                    textView.Enabled = false;
                }
                else
                {
                    textView.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                    textView.SetTextColor(global::Android.Graphics.Color.Black);
                    textView.Enabled = true;
                }
            }
            
            return view;
        }

        public bool IsGroupHeader(int position)
        {
            var item = GetItem(position);
            return item != null && _groupHeaders.Contains(item);
        }
    }
}
