using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using QuranSearch.Core.Services;
using QuranSearch.Core.ViewModels;

namespace QuranSearchApp.ViewModels;

public class DonateViewModel : ViewModelBase
{
    private readonly LocalizationService _localizationService;
    
    private const string PayPalUrl = "https://www.paypal.com/donate/?cmd=_s-xclick&hosted_button_id=ESTNXJLMMQQQS&ssrt=1765056010634";
    private const string Iban = "DE11 6805 0101 0014 3501 24";
    private const string PaymentReference = "Spende BBF-Bauprojekt | über die App Kuttab";

    public DonateViewModel() : this(new LocalizationService(new QuranSearchApp.Services.DesktopFileService()))
    {
    }

    public DonateViewModel(LocalizationService localizationService)
    {
        _localizationService = localizationService;
        
        // Subscribe to language changes
        _localizationService.LanguageChanged += OnLanguageChanged;
        
        CloseCommand = new SimpleCommand(OnClose);
        OpenPayPalCommand = new SimpleCommand(OnOpenPayPal);
        CopyIbanCommand = new SimpleCommand(OnCopyIban);
        CopyReferenceCommand = new SimpleCommand(OnCopyReference);
    }

    public LocalizationService Localization => _localizationService;
    
    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        // Notify UI that all localized properties have changed
        this.RaisePropertyChanged(nameof(Localization));
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
