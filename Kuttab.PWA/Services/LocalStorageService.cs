using Microsoft.JSInterop;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kuttab.PWA.Services;

/// <summary>
/// Wrapper around browser localStorage for persisting settings.
/// </summary>
public class LocalStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        if (string.IsNullOrEmpty(json))
            return default;
        return JsonSerializer.Deserialize<T>(json);
    }

    public async Task<string?> GetStringAsync(string key)
    {
        return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
    }

    public async Task SetStringAsync(string key, string value)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
    }

    public async Task RemoveItemAsync(string key)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }
}
