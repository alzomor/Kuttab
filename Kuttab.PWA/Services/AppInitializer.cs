using Kuttab.Core.Services;
using System.Net.Http;
using System.Threading.Tasks;

namespace Kuttab.PWA.Services;

/// <summary>
/// Pre-loads data files into the WebFileService cache so that
/// synchronous reads (used by LocalizationService, QuranSearchService) work in WASM.
/// </summary>
public class AppInitializer
{
    private readonly WebFileService _fileService;
    private readonly HttpClient _httpClient;

    public bool IsInitialized { get; private set; }

    public AppInitializer(WebFileService fileService, HttpClient httpClient)
    {
        _fileService = fileService;
        _httpClient = httpClient;
    }

    public async Task InitializeAsync(string language = "ar")
    {
        if (IsInitialized) return;

        // Pre-load localization files into cache
        var langFiles = new[] { "Strings.ar.json", "Strings.en.json", "Strings.de.json", "Strings.es.json", "Strings.tr.json", "Strings.fr.json", "Strings.ja.json" };
        foreach (var file in langFiles)
        {
            try
            {
                var path = $"Localization/{file}";
                await _fileService.ReadAllTextAsync(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not pre-load {file}: {ex.Message}");
            }
        }

        // Pre-load quran text
        try
        {
            await _fileService.ReadAllTextAsync("quran-uthmani-ver1.2.txt");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not pre-load quran text: {ex.Message}");
        }

        // Pre-load rules
        try
        {
            await _fileService.ReadAllTextAsync("rules.json");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not pre-load rules: {ex.Message}");
        }

        IsInitialized = true;
    }
}
