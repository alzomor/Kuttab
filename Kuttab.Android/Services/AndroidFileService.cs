using Android.Content;
using Kuttab.Core.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace Kuttab.Android.Services;

public class AndroidFileService : IFileService
{
    private readonly Context _context;
    
    public AndroidFileService(Context context)
    {
        _context = context;
    }
    
    public string GetAppDataDirectory()
    {
        return _context.FilesDir?.AbsolutePath ?? string.Empty;
    }
    
    public bool FileExists(string path)
    {
        // Check assets first
        try
        {
            using var stream = _context.Assets?.Open(path);
            return stream != null;
        }
        catch
        {
            // Check internal storage
            var fullPath = Path.Combine(GetAppDataDirectory(), path);
            return File.Exists(fullPath);
        }
    }
    
    public async Task<string> ReadAllTextAsync(string path)
    {
        try
        {
            // Try reading from assets
            using var stream = _context.Assets?.Open(path);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            }
        }
        catch
        {
            // Fall through to internal storage
        }
        
        // Read from internal storage
        var fullPath = Path.Combine(GetAppDataDirectory(), path);
        return await File.ReadAllTextAsync(fullPath);
    }
    
    public string ReadAllText(string path)
    {
        try
        {
            // Try reading from assets
            using var stream = _context.Assets?.Open(path);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }
        catch
        {
            // Fall through to internal storage
        }
        
        // Read from internal storage
        var fullPath = Path.Combine(GetAppDataDirectory(), path);
        return File.ReadAllText(fullPath);
    }
    
    public Stream GetFileStream(string path)
    {
        try
        {
            var stream = _context.Assets?.Open(path);
            if (stream != null)
                return stream;
        }
        catch
        {
            // Fall through to file system
        }
        
        var fullPath = Path.Combine(GetAppDataDirectory(), path);
        if (File.Exists(fullPath))
        {
            return File.OpenRead(fullPath);
        }
        
        throw new FileNotFoundException($"File not found: {path}");
    }
}
