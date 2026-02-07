using System;
using System.Reflection;
using System.Windows.Input;
using ReactiveUI;
using Kuttab.Core.Services;
using Kuttab.Core.ViewModels;

namespace Kuttab.ViewModels;

public class InfoViewModel : ViewModelBase
{
    private readonly LocalizationService _localizationService;

    public InfoViewModel() : this(new LocalizationService(new Kuttab.Services.DesktopFileService()))
    {
    }

    public InfoViewModel(LocalizationService localizationService)
    {
        _localizationService = localizationService;
        CloseCommand = new SimpleCommand(OnClose);
    }

    public LocalizationService Localization => _localizationService;

    public string AppVersion
    {
        get
        {
            try
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                return $"v{version?.ToString(3) ?? "0.0"}";
            }
            catch
            {
                return "v0.0";
            }
        }
    }

    public ICommand CloseCommand { get; }

    public event EventHandler? CloseRequested;

    private void OnClose()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
