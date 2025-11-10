using Android.Content;
using Android.Graphics;
using Android.Net;
using QuranSearch.Core.Interfaces;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace QuranSearch.Android.Services;

public class AndroidPictureService : IPictureService
{
    private readonly Context _context;
    private readonly HttpClient _httpClient = new();
    
    public AndroidPictureService(Context context)
    {
        _context = context;
    }
    
    public bool UseRemoteSource { get; set; } = true;
    
    public bool PictureExists(int surahNumber, int ayaNumber)
    {
        var local = GetLocalPicturePath(surahNumber, ayaNumber);
        return File.Exists(local);
    }
    
    public string GetPicturePath(int surahNumber, int ayaNumber)
    {
        // Always return local app file path; caller can load bitmap from it
        return GetLocalPicturePath(surahNumber, ayaNumber);
    }
    
    public async Task<object?> LoadBitmapAsync(string path)
    {
        try
        {
            // If the file exists locally, decode directly
            if (File.Exists(path))
            {
                await using var fs = File.OpenRead(path);
                return await BitmapFactory.DecodeStreamAsync(fs);
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    private string GetLocalPicturePath(int surahNumber, int ayaNumber)
    {
        var dir = global::System.IO.Path.Combine(_context.FilesDir!.AbsolutePath, "images", "QuranText_jpg");
        global::System.IO.Directory.CreateDirectory(dir);
        return global::System.IO.Path.Combine(dir, $"{surahNumber}_{ayaNumber}.jpg");
    }

    public async Task<bool> EnsurePictureAvailableAsync(int surahNumber, int ayaNumber)
    {
        var local = GetLocalPicturePath(surahNumber, ayaNumber);
        if (File.Exists(local)) return true;
        if (!UseRemoteSource) return false;

        if (!IsNetworkAvailable()) return false;

        var url = GetRemotePictureUrl(surahNumber, ayaNumber);
        var tmp = local + ".download";
        using var resp = await _httpClient.GetAsync(url);
        if (!resp.IsSuccessStatusCode) return false;
        await using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await resp.Content.CopyToAsync(fs);
        }
        if (File.Exists(local)) File.Delete(local);
        File.Move(tmp, local);
        return true;
    }

    private bool IsNetworkAvailable()
    {
        try
        {
            var cm = (ConnectivityManager?)_context.GetSystemService(Context.ConnectivityService);
#pragma warning disable CA1416
            var nw = cm?.ActiveNetworkInfo;
            return nw != null && nw.IsConnected;
#pragma warning restore CA1416
        }
        catch { return false; }
    }

    private string GetRemotePictureUrl(int surahNumber, int ayaNumber)
    {
        return $"https://everyayah.com/data/QuranText_jpg/{surahNumber}_{ayaNumber}.jpg";
    }
}
