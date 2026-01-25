using System;
using System.Windows.Input;
using ReactiveUI;
using Kuttab.Core.Services;
using Kuttab.Core.ViewModels;

namespace Kuttab.ViewModels;

public class TajweedRuleViewModel : ViewModelBase
{
    private readonly LocalizationService _localizationService;
    private string _ruleName = string.Empty;
    private string _ruleExplanation = string.Empty;

    public TajweedRuleViewModel() : this(new LocalizationService(new Kuttab.Services.DesktopFileService()))
    {
    }

    public TajweedRuleViewModel(LocalizationService localizationService)
    {
        _localizationService = localizationService;
        CloseCommand = new SimpleCommand(OnClose);
    }

    public LocalizationService Localization => _localizationService;

    public string RuleName
    {
        get => _ruleName;
        set => this.RaiseAndSetIfChanged(ref _ruleName, value);
    }

    public string RuleExplanation
    {
        get => _ruleExplanation;
        set => this.RaiseAndSetIfChanged(ref _ruleExplanation, value);
    }

    public ICommand CloseCommand { get; }

    public event EventHandler? CloseRequested;

    public void SetRule(string arabicRuleName)
    {
        RuleName = arabicRuleName;
        
        // Get the localized rule name for display
        var languageCode = _localizationService.CurrentLanguage;
        var localizedName = RuleNameTranslator.GetLocalizedName(arabicRuleName, languageCode);
        
        // If the localized name is different from Arabic, show both
        if (languageCode != "ar" && localizedName != arabicRuleName)
        {
            RuleName = $"{localizedName}\n({arabicRuleName})";
        }
        else
        {
            RuleName = arabicRuleName;
        }
        
        // Get the explanation based on current language
        RuleExplanation = TajweedRulesExplanation.GetExplanation(arabicRuleName, languageCode);
    }

    private void OnClose()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
