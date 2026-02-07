using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using Kuttab.Core.Services;
using Kuttab.Core.ViewModels;

namespace Kuttab.ViewModels;

public class DonateViewModel : ViewModelBase
{
    private readonly LocalizationService _localizationService;
    
    // Donation data (loaded from donation.json)
    private long _totalCost = 1000000;
    private long _currentDonations = 700000;
    
    private const string PayPalUrl = "https://www.paypal.com/donate/?cmd=_s-xclick&hosted_button_id=ESTNXJLMMQQQS&ssrt=1765056010634";
    private const string Iban = "DE11 6805 0101 0014 3501 24";
    private const string PaymentReference = "Spende BBF-Bauprojekt | über die App Kuttab";

    public DonateViewModel() : this(new LocalizationService(new Kuttab.Services.DesktopFileService()))
    {
    }

    public DonateViewModel(LocalizationService localizationService)
    {
        _localizationService = localizationService;
        
        LoadDonationData();
        
        // Subscribe to language changes
        _localizationService.LanguageChanged += OnLanguageChanged;
        
        CloseCommand = new SimpleCommand(OnClose);
        OpenPayPalCommand = new SimpleCommand(OnOpenPayPal);
        CopyIbanCommand = new SimpleCommand(OnCopyIban);
        CopyReferenceCommand = new SimpleCommand(OnCopyReference);
    }

    public LocalizationService Localization => _localizationService;
    
    public string DonateSubtitle
    {
        get
        {
            var remaining = _totalCost - _currentDonations;
            var totalStr = _totalCost.ToString("N0");
            var currentStr = _currentDonations.ToString("N0");
            var remainingStr = remaining.ToString("N0");
            return string.Format(_localizationService.DonateSubtitle, totalStr, currentStr, remainingStr);
        }
    }
    
    private void LoadDonationData()
    {
        try
        {
            var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "donation.json");
            if (File.Exists(jsonPath))
            {
                var json = File.ReadAllText(jsonPath);
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
            Console.WriteLine($"Error loading donation data: {ex.Message}");
        }
    }
    
    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        // Notify UI that all localized properties have changed
        this.RaisePropertyChanged(nameof(Localization));
        this.RaisePropertyChanged(nameof(DonateSubtitle));
    }

    public ICommand CloseCommand { get; }
    public ICommand OpenPayPalCommand { get; }
    public ICommand CopyIbanCommand { get; }
    public ICommand CopyReferenceCommand { get; }

    public event EventHandler? CloseRequested;

    private void OnClose()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnOpenPayPal()
    {
        OpenUrl(PayPalUrl);
    }

    private async void OnCopyIban()
    {
        await CopyToClipboard(Iban);
    }

    private async void OnCopyReference()
    {
        await CopyToClipboard(PaymentReference);
    }

    private async System.Threading.Tasks.Task CopyToClipboard(string text)
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var clipboard = desktop.MainWindow?.Clipboard;
                if (clipboard != null)
                {
                    await clipboard.SetTextAsync(text);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to copy to clipboard: {ex.Message}");
        }
    }

    private void OpenUrl(string url)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to open URL: {ex.Message}");
        }
    }
}
