using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Kuttab.Core.Services;
using Kuttab.Android.Utils;
using System;
using System.IO;
using System.Text.Json;

namespace Kuttab.Android;

[Activity(Label = "Donate", Theme = "@style/AppTheme")]
public class DonateActivity : AppCompatActivity
{
    private const string PayPalUrl = "https://www.paypal.com/donate/?cmd=_s-xclick&hosted_button_id=ESTNXJLMMQQQS&ssrt=1765056010634";
    private const string AssociationUrl = "https://bbfverein.de/";
    private const string Iban = "DE11 6805 0101 0014 3501 24";
    private const string PaymentReference = "Spende BBF-Bauprojekt | über die App Kuttab";
    private const string AccountHolderName = "Bildungs- und Begegnungsverein Freiburg e.V.";

    private LocalizationService? _localizationService;

    // Donation data (loaded from donation.json)
    private long _totalCost = 1000000;
    private long _currentDonations = 700000;

    private Button? _paypalButton;
    private Button? _copyIbanButton;
    private Button? _copyReferenceButton;
    private Button? _copyAccountHolderButton;
    private Button? _shareDonationButton;
    private TextView? _associationLink;

    // Text views for localization
    private TextView? _donateTitle;
    private TextView? _donateSubtitle;
    private TextView? _paypalTitle;
    private TextView? _paypalDescription;
    private TextView? _bankTransferTitle;
    private TextView? _accountHolderLabel;
    private TextView? _paymentReferenceLabel;
    private TextView? _bankTransferNote;
    private TextView? _aboutAssociationTitle;
    private TextView? _associationDescription;
    private TextView? _taxDeductionNote;
    private TextView? _barakahMessage;

    protected override void OnCreate(Bundle? savedInstanceState)
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
        
        SetContentView(Resource.Layout.activity_donate);

        // Enable back button in action bar
        if (SupportActionBar != null)
        {
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.Title = GetString(Resource.String.donate_title);
        }

        InitializeServices();
        LoadDonationData();
        InitializeViews();
        SetupEventHandlers();
        UpdateLocalizedText();
    }

    private void LoadDonationData()
    {
        try
        {
            using var stream = Assets?.Open("donation.json");
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                
                if (root.TryGetProperty("totalCost", out var totalCostEl))
                    _totalCost = totalCostEl.GetInt64();
                if (root.TryGetProperty("currentDonations", out var currentEl))
                    _currentDonations = currentEl.GetInt64();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading donation data: {ex.Message}");
        }
    }

    private void InitializeServices()
    {
        var fileService = new Services.AndroidFileService(this);
        _localizationService = new LocalizationService(fileService);
        
        // Load saved language from SharedPreferences with system language detection
        var language = LanguageHelper.GetLanguageFromPreferences(this);
        _localizationService.CurrentLanguage = language;
    }

    private void InitializeViews()
    {
        _paypalButton = FindViewById<Button>(Resource.Id.paypalButton);
        _copyIbanButton = FindViewById<Button>(Resource.Id.copyIbanButton);
        _copyReferenceButton = FindViewById<Button>(Resource.Id.copyReferenceButton);
        _copyAccountHolderButton = FindViewById<Button>(Resource.Id.copyAccountHolderButton);
        _shareDonationButton = FindViewById<Button>(Resource.Id.shareDonationInfoButton);
        _associationLink = FindViewById<TextView>(Resource.Id.associationLink);

        // Localized text views
        _donateTitle = FindViewById<TextView>(Resource.Id.donateTitle);
        _donateSubtitle = FindViewById<TextView>(Resource.Id.donateSubtitle);
        _paypalTitle = FindViewById<TextView>(Resource.Id.paypalTitle);
        _paypalDescription = FindViewById<TextView>(Resource.Id.paypalDescription);
        _bankTransferTitle = FindViewById<TextView>(Resource.Id.bankTransferTitle);
        _accountHolderLabel = FindViewById<TextView>(Resource.Id.accountHolderLabel);
        _paymentReferenceLabel = FindViewById<TextView>(Resource.Id.paymentReferenceLabel);
        _bankTransferNote = FindViewById<TextView>(Resource.Id.bankTransferNote);
        _aboutAssociationTitle = FindViewById<TextView>(Resource.Id.aboutAssociationTitle);
        _associationDescription = FindViewById<TextView>(Resource.Id.associationDescription);
        _taxDeductionNote = FindViewById<TextView>(Resource.Id.taxDeductionNote);
        _barakahMessage = FindViewById<TextView>(Resource.Id.barakahMessage);
    }

    private void SetupEventHandlers()
    {
        if (_paypalButton != null)
            _paypalButton.Click += OnPayPalClick;

        if (_copyIbanButton != null)
            _copyIbanButton.Click += OnCopyIbanClick;

        if (_copyReferenceButton != null)
            _copyReferenceButton.Click += OnCopyReferenceClick;

        if (_associationLink != null)
            _associationLink.Click += OnAssociationLinkClick;
        
        if (_copyAccountHolderButton != null)
            _copyAccountHolderButton.Click += (s, e) =>
            {
                CopyToClipboard("AccountHolder", AccountHolderName);
                ShowToast(GetString(Resource.String.copied_to_clipboard));
            };
        
        if (_shareDonationButton != null)
            _shareDonationButton.Click += (s, e) => ShareDonationInfo();
    }

    private void UpdateLocalizedText()
    {
        if (_localizationService == null) return;

        // Set RTL layout direction for Arabic
        UpdateLayoutDirection();

        if (_donateTitle != null)
            _donateTitle.Text = _localizationService.DonateTitle;

        if (_donateSubtitle != null)
        {
            var remaining = _totalCost - _currentDonations;
            var totalStr = _totalCost.ToString("N0");
            var currentStr = _currentDonations.ToString("N0");
            var remainingStr = remaining.ToString("N0");
            _donateSubtitle.Text = string.Format(_localizationService.DonateSubtitle, totalStr, currentStr, remainingStr);
        }

        if (_paypalTitle != null)
            _paypalTitle.Text = _localizationService.DonateViaPayPal;

        if (_paypalDescription != null)
            _paypalDescription.Text = _localizationService.PayPalDescription;

        if (_bankTransferTitle != null)
            _bankTransferTitle.Text = _localizationService.DonateViaBankTransfer;

        if (_accountHolderLabel != null)
            _accountHolderLabel.Text = _localizationService.AccountHolder;

        if (_paymentReferenceLabel != null)
            _paymentReferenceLabel.Text = _localizationService.PaymentReference;

        if (_bankTransferNote != null)
            _bankTransferNote.Text = _localizationService.BankTransferNote;

        if (_aboutAssociationTitle != null)
            _aboutAssociationTitle.Text = _localizationService.AboutAssociation;

        if (_associationDescription != null)
            _associationDescription.Text = _localizationService.AssociationDescription;

        if (_taxDeductionNote != null)
            _taxDeductionNote.Text = _localizationService.TaxDeductionNote;

        if (_barakahMessage != null)
            _barakahMessage.Text = _localizationService.BarakahMessage;

        if (_copyIbanButton != null)
            _copyIbanButton.Text = _localizationService.CopyButton;

        if (_copyReferenceButton != null)
            _copyReferenceButton.Text = _localizationService.CopyButton;
        
        if (_copyAccountHolderButton != null)
            _copyAccountHolderButton.Text = _localizationService.CopyButton;
        
        if (_shareDonationButton != null)
            _shareDonationButton.Text = _localizationService.ShareDonationInfo;

        // Update action bar title
        if (SupportActionBar != null)
            SupportActionBar.Title = _localizationService.DonateTitle;
    }

    private void UpdateLayoutDirection()
    {
        if (_localizationService == null) return;
        
        var isRtl = _localizationService.IsRightToLeft;
        var layoutDirection = isRtl ? LayoutDirection.Rtl : LayoutDirection.Ltr;
        
        // Set layout direction on the window's decor view for full RTL support
        if (Window?.DecorView != null)
            Window.DecorView.LayoutDirection = layoutDirection;
        
        // Set text alignment for section titles based on RTL
        var textAlignment = isRtl ? TextAlignment.ViewEnd : TextAlignment.ViewStart;
        var gravity = isRtl ? GravityFlags.End : GravityFlags.Start;
        
        if (_paypalTitle != null)
        {
            _paypalTitle.TextAlignment = textAlignment;
            _paypalTitle.Gravity = gravity;
        }
        if (_bankTransferTitle != null)
        {
            _bankTransferTitle.TextAlignment = textAlignment;
            _bankTransferTitle.Gravity = gravity;
        }
        if (_aboutAssociationTitle != null)
        {
            _aboutAssociationTitle.TextAlignment = textAlignment;
            _aboutAssociationTitle.Gravity = gravity;
        }
        if (_paypalDescription != null)
        {
            _paypalDescription.TextAlignment = textAlignment;
            _paypalDescription.Gravity = gravity;
        }
        if (_associationDescription != null)
        {
            _associationDescription.TextAlignment = textAlignment;
            _associationDescription.Gravity = gravity;
        }
        if (_bankTransferNote != null)
        {
            _bankTransferNote.TextAlignment = textAlignment;
            _bankTransferNote.Gravity = gravity;
        }
        if (_accountHolderLabel != null)
        {
            _accountHolderLabel.TextAlignment = textAlignment;
            _accountHolderLabel.Gravity = gravity;
        }
        if (_paymentReferenceLabel != null)
        {
            _paymentReferenceLabel.TextAlignment = textAlignment;
            _paymentReferenceLabel.Gravity = gravity;
        }
        if (_taxDeductionNote != null)
        {
            _taxDeductionNote.TextAlignment = textAlignment;
            _taxDeductionNote.Gravity = gravity;
        }
    }

    private void ShareDonationInfo()
    {
        try
        {
            var loc = _localizationService;
            var remaining = _totalCost - _currentDonations;
            
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("💚 " + (loc?.DonateTitle ?? "Support Kuttab"));
            sb.AppendLine();
            sb.AppendLine(string.Format(
                loc?.DonateSubtitle ?? "Total: {0} EUR | Collected: {1} EUR | Needed: {2} EUR",
                _totalCost.ToString("N0"), _currentDonations.ToString("N0"), remaining.ToString("N0")));
            sb.AppendLine();
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine();
            sb.AppendLine("🏦 " + (loc?.DonateViaBankTransfer ?? "Bank Transfer"));
            sb.AppendLine($"{loc?.AccountHolder ?? "Account Holder:"} {AccountHolderName}");
            sb.AppendLine($"IBAN: {Iban}");
            sb.AppendLine($"{loc?.PaymentReference ?? "Payment Reference:"} {PaymentReference}");
            sb.AppendLine();
            sb.AppendLine(loc?.DonateViaPayPal ?? "PayPal");
            sb.AppendLine(PayPalUrl);
            sb.AppendLine();
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine();
            sb.AppendLine(loc?.TaxDeductionNote ?? "");
            sb.AppendLine();
            sb.AppendLine("📱 Kuttab - Quran Tajweed Trainer");
            sb.AppendLine("https://play.google.com/store/apps/details?id=com.kuttab.app");
            
            var shareIntent = new Intent(Intent.ActionSend);
            shareIntent.SetType("text/plain");
            shareIntent.PutExtra(Intent.ExtraText, sb.ToString());
            shareIntent.PutExtra(Intent.ExtraSubject, loc?.DonateTitle ?? "Support Kuttab");
            
            var chooserTitle = loc?.ShareDonationChooser ?? "Share donation info via";
            var chooserIntent = Intent.CreateChooser(shareIntent, chooserTitle);
            if (chooserIntent != null)
                StartActivity(chooserIntent);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error sharing donation info: {ex.Message}");
        }
    }

    private void OnPayPalClick(object? sender, EventArgs e)
    {
        OpenUrl(PayPalUrl);
    }

    private void OnCopyIbanClick(object? sender, EventArgs e)
    {
        CopyToClipboard("IBAN", Iban);
        ShowToast(GetString(Resource.String.copied_to_clipboard));
    }

    private void OnCopyReferenceClick(object? sender, EventArgs e)
    {
        CopyToClipboard("Verwendungszweck", PaymentReference);
        ShowToast(GetString(Resource.String.copied_to_clipboard));
    }

    private void OnAssociationLinkClick(object? sender, EventArgs e)
    {
        OpenUrl(AssociationUrl);
    }

    private void OpenUrl(string url)
    {
        try
        {
            var intent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse(url));
            StartActivity(intent);
        }
        catch (Exception ex)
        {
            ShowToast($"Could not open link: {ex.Message}");
        }
    }

    private void CopyToClipboard(string label, string text)
    {
        try
        {
            var clipboard = (ClipboardManager?)GetSystemService(ClipboardService);
            if (clipboard != null)
            {
                var clip = ClipData.NewPlainText(label, text);
                clipboard.PrimaryClip = clip;
            }
        }
        catch (Exception ex)
        {
            ShowToast($"Could not copy: {ex.Message}");
        }
    }

    private void ShowToast(string message)
    {
        Toast.MakeText(this, message, ToastLength.Short)?.Show();
    }

    public override bool OnOptionsItemSelected(IMenuItem item)
    {
        if (item.ItemId == global::Android.Resource.Id.Home)
        {
            Finish();
            return true;
        }
        return base.OnOptionsItemSelected(item);
    }
}
