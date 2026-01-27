using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Kuttab.Core.Services;
using Kuttab.Core.Interfaces;

namespace Kuttab.Services;

public class DesktopSettingsService
{
    private readonly IFileService _fileService;
    private const string SettingsFileName = "app_settings.json";
    
    public DesktopSettingsService(IFileService fileService)
    {
        _fileService = fileService;
    }
    
    public async Task<AppSettings> LoadSettingsAsync()
    {
        try
        {
            if (!_fileService.FileExists(SettingsFileName))
            {
                return new AppSettings(); // Return default settings
            }
            
            var json = await _fileService.ReadAllTextAsync(SettingsFileName);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);
            return settings ?? new AppSettings();
        }
        catch
        {
            return new AppSettings(); // Return default settings on error
        }
    }
    
    public async Task SaveSettingsAsync(AppSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            // Cast to DesktopFileService since we need WriteAllTextAsync
            if (_fileService is DesktopFileService desktopService)
            {
                await desktopService.WriteAllTextAsync(SettingsFileName, json);
            }
        }
        catch
        {
            // Silently fail on save errors
        }
    }
}

public class AppSettings
{
    public string SelectedLanguage { get; set; } = "en";
    public string SelectedQuranTextFile { get; set; } = "quran-uthmani-ver1.2.txt";
    public bool UseRemoteAudio { get; set; } = true;
    public bool UseRemoteImages { get; set; } = true;
    public string SelectedReciterFolder { get; set; } = "Husary_128kbps";
    public int FontSize { get; set; } = 18;
    public int SearchStartSurah { get; set; } = 1;
    public int SearchEndSurah { get; set; } = 114;
    public string SearchDomainType { get; set; } = "WholeQuran";
}
