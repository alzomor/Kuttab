using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Kuttab.Core.Services;
using Kuttab.Android.Utils;
using Kuttab.Android.Services;

namespace Kuttab.Android;

[Activity(Label = "@string/info_title", Theme = "@style/AppTheme")]
public class InfoActivity : AppCompatActivity
{
    private LocalizationService? _localizationService;
    
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // Handle edge-to-edge on Android 15+ (SDK 35)
        if (Window != null)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Window.SetStatusBarColor(global::Android.Graphics.Color.ParseColor("#0D3F13"));
                Window.SetNavigationBarColor(global::Android.Graphics.Color.ParseColor("#1B5E20"));
            }
        }
        
        // Initialize localization service and apply language setting
        var fileService = new AndroidFileService(this);
        _localizationService = new LocalizationService(fileService);
        LoadSettings();
        ApplyLanguageSettings();
        
        SetContentView(Resource.Layout.activity_info);
        
        // Apply localized text to all translatable views
        ApplyLocalizedContent();
        
        // Set version dynamically from package info
        var versionTextView = FindViewById<TextView>(Resource.Id.appVersionTextView);
        if (versionTextView != null)
        {
            try
            {
                var packageInfo = PackageManager?.GetPackageInfo(PackageName ?? "", 0);
                var versionName = packageInfo?.VersionName ?? "0.0";
                versionTextView.Text = $"v{versionName}";
            }
            catch
            {
                // Keep default from layout
            }
        }
        
        // Set up action bar
        if (SupportActionBar != null)
        {
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
        }
        
        // Share button
        var shareButton = FindViewById<Button>(Resource.Id.shareAppInfoButton);
        if (shareButton != null)
        {
            shareButton.Text = _localizationService?.ShareAppInfo ?? "\uD83D\uDCE4 Share App";
            shareButton.Click += (s, e) => ShareAppInfo();
        }
    }
    
    private void ApplyLocalizedContent()
    {
        if (_localizationService == null) return;
        var loc = _localizationService;

        SetText(Resource.Id.infoSubtitleText, loc["AppSubtitle"]);
        SetText(Resource.Id.citationsTitleText, loc["CitationsTitle"]);
        SetText(Resource.Id.quranTextTitleText, loc["QuranTextCitation"]);
        SetText(Resource.Id.quranTextContentText, loc["QuranTextCitationText"]);
        SetText(Resource.Id.audioTitleText, loc["AudioFilesCitation"]);
        SetText(Resource.Id.audioContentText, loc["AudioFilesCitationText"]);
        SetText(Resource.Id.picturesTitleText, loc["PictureFilesCitation"]);
        SetText(Resource.Id.picturesContentText, loc["PictureFilesCitationText"]);
        SetText(Resource.Id.usulAiTitleText, loc["UsulAiCitation"]);
        SetText(Resource.Id.usulAiContentText, loc["UsulAiCitationText"]);
        SetText(Resource.Id.disclaimerTitleText, loc["DisclaimerTitle"]);
        SetText(Resource.Id.trainingDisclaimerTitleText, loc["TrainingDisclaimer"]);
        SetText(Resource.Id.trainingDisclaimerContentText, loc["TrainingDisclaimerText"]);
        SetText(Resource.Id.generalDisclaimerText, loc["DisclaimerText"]);
        SetText(Resource.Id.contactEmailTitleText, loc["ContactEmail"]);
        SetText(Resource.Id.contactNoteText, loc["ContactNote"]);
    }

    private void SetText(int viewId, string text)
    {
        var view = FindViewById<TextView>(viewId);
        if (view != null && !string.IsNullOrEmpty(text) && !text.StartsWith("["))
            view.Text = text;
    }

    private void ShareAppInfo()
    {
        try
        {
            var loc = _localizationService;
            var versionName = "0.93";
            try
            {
                var packageInfo = PackageManager?.GetPackageInfo(PackageName ?? "", 0);
                versionName = packageInfo?.VersionName ?? "0.93";
            }
            catch { }
            
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"📖 Kuttab v{versionName}");
            sb.AppendLine("Kuttab Tajweed Trainer");
            sb.AppendLine();
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine();
            sb.AppendLine("✨ Features:");
            sb.AppendLine("• Show Tajweed rule explanations");
            sb.AppendLine("• Find all matching Ayat to Tajweed rules in selected range");
            sb.AppendLine("• Select and listen to Ayah/Ayat from list of reciters");
            sb.AppendLine("• Listen to single Ayah, repeat it, or play all matching Ayat");
            sb.AppendLine("• Record Ayah with your voice and compare to selected reciter");
            sb.AppendLine("• Listen to Quran with color-highlighted Tajweed rules");
            sb.AppendLine("• View explanation for each rule and matching letters");
            sb.AppendLine("• Hifz mode with speech recognition for memorization practice");
            sb.AppendLine();
            sb.AppendLine("🕌 Support:");
            sb.AppendLine("• Donate to BBF Islamic Center building project in Freiburg im Breisgau");
            sb.AppendLine();
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine();
            sb.AppendLine("📧 Contact: almanar.backup@gmail.com");
            sb.AppendLine("📱 https://play.google.com/store/apps/details?id=com.kuttab.app");
            
            var shareIntent = new Intent(Intent.ActionSend);
            shareIntent.SetType("text/plain");
            shareIntent.PutExtra(Intent.ExtraText, sb.ToString());
            shareIntent.PutExtra(Intent.ExtraSubject, $"Kuttab v{versionName}");
            
            var chooserTitle = loc?.ShareAppInfoChooser ?? "Share app via";
            var chooserIntent = Intent.CreateChooser(shareIntent, chooserTitle);
            if (chooserIntent != null)
                StartActivity(chooserIntent);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error sharing app info: {ex.Message}");
        }
    }
    
    private void LoadSettings()
    {
        var prefs = GetSharedPreferences("QuranSearchSettings", FileCreationMode.Private);
        
        // Load language with system language detection
        var language = LanguageHelper.GetLanguageFromPreferences(this);
        if (_localizationService != null)
        {
            _localizationService.CurrentLanguage = language;
        }
    }
    
    private void ApplyLanguageSettings()
    {
        if (_localizationService == null) return;
        
        try
        {
            var languageCode = _localizationService.CurrentLanguage;
            
            // Set locale for the activity
            var locale = new Java.Util.Locale(languageCode);
            Java.Util.Locale.Default = locale;
            
            var config = new Configuration();
            config.SetLocale(locale);
            
            // Apply configuration to resources
            BaseContext.Resources.UpdateConfiguration(config, BaseContext.Resources.DisplayMetrics);
            
            // Update layout direction
            var layoutDirection = languageCode == "ar" ? LayoutDirection.Rtl : LayoutDirection.Ltr;
            Window.DecorView.LayoutDirection = layoutDirection;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error applying language settings: {ex.Message}");
        }
    }
}
