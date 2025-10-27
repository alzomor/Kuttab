using System.Threading.Tasks;

namespace QuranSearch.Core.Interfaces;

/// <summary>
/// Platform-agnostic picture/image service interface
/// Desktop and Android will provide their own implementations
/// </summary>
public interface IPictureService
{
    bool UseRemoteSource { get; set; }
    
    string? GetPicturePath(int surahNumber, int ayaNumber);
    bool PictureExists(int surahNumber, int ayaNumber);
    
    // Platform-specific: Returns platform-specific image type
    // Desktop: Avalonia.Media.Imaging.Bitmap
    // Android: Android.Graphics.Bitmap or Avalonia equivalent
    Task<object?> LoadBitmapAsync(string path);
}
