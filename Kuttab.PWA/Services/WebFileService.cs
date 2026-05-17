using Kuttab.Core.Interfaces;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Kuttab.PWA.Services;

/// <summary>
/// IFileService implementation that fetches files from wwwroot/data/ via HttpClient.
/// </summary>
public class WebFileService : IFileService
{
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, string> _cache = new();

    public WebFileService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string GetAppDataDirectory()
    {
        return "data/";
    }

    public bool FileExists(string path)
    {
        // In browser context, we assume data files exist
        return true;
    }

    public async Task<string> ReadAllTextAsync(string path)
    {
        if (_cache.TryGetValue(path, out var cached))
            return cached;

        var url = $"data/{path}";
        var content = await _httpClient.GetStringAsync(url);
        _cache[path] = content;
        return content;
    }

    public string ReadAllText(string path)
    {
        // Synchronous read - return cached value or empty
        if (_cache.TryGetValue(path, out var cached))
            return cached;
        
        // In WASM, synchronous HTTP is not supported, return empty and rely on async
        return string.Empty;
    }

    public Stream GetFileStream(string path)
    {
        // Return a stream from cached content if available
        if (_cache.TryGetValue(path, out var cached))
            return new MemoryStream(Encoding.UTF8.GetBytes(cached));
        
        return new MemoryStream();
    }
}
