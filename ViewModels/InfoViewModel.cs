using System;
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

    public string AppVersion => "v0.8 TEST VERSION";

    public ICommand CloseCommand { get; }

    public event EventHandler? CloseRequested;

    private void OnClose()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
