using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Kuttab.Core.Services;
using Kuttab.Android.Utils;

namespace Kuttab.Android;

[Activity(Label = "Tajweed Rule", Theme = "@style/AppTheme")]
public class TajweedRuleActivity : AppCompatActivity
{
    private TextView? _ruleTitle;
    private TextView? _ruleNameText;
    private TextView? _ruleExplanationText;
    private Button? _closeButton;
    private Button? _closeButtonBottom;
    
    private LocalizationService? _localizationService;
    private string _arabicRuleName = string.Empty;
    private string _languageCode = "en";

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_tajweed_rule);

        InitializeServices();
        InitializeViews();
        LoadRuleData();
        SetupEventHandlers();
        UpdateLayoutDirection();
    }

    private void InitializeServices()
    {
        var fileService = new Services.AndroidFileService(this);
        _localizationService = new LocalizationService(fileService);
        
        // Load saved language from SharedPreferences with system language detection
        _languageCode = LanguageHelper.GetLanguageFromPreferences(this);
        _localizationService.CurrentLanguage = _languageCode;
        
        // Get rule name from intent
        _arabicRuleName = Intent?.GetStringExtra("ArabicRuleName") ?? string.Empty;
    }

    private void InitializeViews()
    {
        _ruleTitle = FindViewById<TextView>(Resource.Id.ruleTitle);
        _ruleNameText = FindViewById<TextView>(Resource.Id.ruleNameText);
        _ruleExplanationText = FindViewById<TextView>(Resource.Id.ruleExplanationText);
        _closeButton = FindViewById<Button>(Resource.Id.closeButton);
        _closeButtonBottom = FindViewById<Button>(Resource.Id.closeButtonBottom);
    }

    private void LoadRuleData()
    {
        if (string.IsNullOrEmpty(_arabicRuleName))
        {
            ShowError(GetString(Resource.String.no_rule_selected));
            Finish();
            return;
        }

        // Get localized rule name
        var localizedRuleName = RuleNameTranslator.GetLocalizedName(_arabicRuleName, _languageCode);
        
        // Get rule explanation
        var explanation = TajweedRulesExplanation.GetExplanation(_arabicRuleName, _languageCode);
        
        if (string.IsNullOrEmpty(explanation))
        {
            explanation = GetString(Resource.String.no_explanation_available);
        }

        // Update UI
        if (_ruleTitle != null)
        {
            _ruleTitle.Text = GetString(Resource.String.tajweed_rule_title);
        }
        
        if (_ruleNameText != null)
        {
            // Show both Arabic name and localized name if different
            if (_languageCode == "ar")
            {
                _ruleNameText.Text = _arabicRuleName;
            }
            else
            {
                _ruleNameText.Text = $"{localizedRuleName}\n{_arabicRuleName}";
            }
        }
        
        if (_ruleExplanationText != null)
        {
            _ruleExplanationText.Text = explanation;
        }
    }

    private void SetupEventHandlers()
    {
        if (_closeButton != null)
        {
            _closeButton.Click += (s, e) => Finish();
        }
        
        if (_closeButtonBottom != null)
        {
            _closeButtonBottom.Click += (s, e) => Finish();
        }
    }

    private void UpdateLayoutDirection()
    {
        if (_localizationService == null) return;
        
        var isRtl = _localizationService.IsRightToLeft;
        var layoutDirection = isRtl ? LayoutDirection.Rtl : LayoutDirection.Ltr;
        
        if (Window?.DecorView != null)
        {
            Window.DecorView.LayoutDirection = layoutDirection;
        }
    }

    private void ShowError(string message)
    {
        Toast.MakeText(this, message, ToastLength.Long)?.Show();
    }

    public override void OnBackPressed()
    {
        base.OnBackPressed();
        Finish();
    }
}
