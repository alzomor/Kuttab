using Android.Content;
using Android.Graphics;
using QuranSearch.Core.Interfaces;
using System.Threading.Tasks;

namespace QuranSearch.Android.Services;

public class AndroidPictureService : IPictureService
{
    private readonly Context _context;
    
    public AndroidPictureService(Context context)
    {
        _context = context;
    }
    
    public bool UseRemoteSource { get; set; } = false;
    
    public bool PictureExists(int surahNumber, int ayaNumber)
    {
        var filename = $"QuranText_jpg/{surahNumber}_{ayaNumber}.jpg";
        try
        {
            using var stream = _context.Assets?.Open(filename);
            return stream != null;
        }
        catch
        {
            return false;
        }
    }
    
    public string GetPicturePath(int surahNumber, int ayaNumber)
    {
        return $"QuranText_jpg/{surahNumber}_{ayaNumber}.jpg";
    }
    
    public async Task<object?> LoadBitmapAsync(string path)
    {
        try
        {
            using var stream = _context.Assets?.Open(path);
            if (stream != null)
            {
                return await BitmapFactory.DecodeStreamAsync(stream);
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
}
