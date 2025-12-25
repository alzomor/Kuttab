using System.IO;
using System.Threading.Tasks;

namespace Kuttab.Core.Interfaces;

/// <summary>
/// Platform-agnostic file service interface
/// Handles differences between desktop file system and Android assets/storage
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Get the base directory for application files
    /// Desktop: AppDomain.CurrentDomain.BaseDirectory
    /// Android: Application.Context.FilesDir or Assets
    /// </summary>
    string GetAppDataDirectory();
    
    /// <summary>
    /// Check if a file exists
    /// </summary>
    bool FileExists(string path);
    
    /// <summary>
    /// Read all text from a file
    /// </summary>
    Task<string> ReadAllTextAsync(string path);
    
    /// <summary>
    /// Read all text from a file synchronously
    /// </summary>
    string ReadAllText(string path);
    
    /// <summary>
    /// Get a stream for a file (useful for embedded resources)
    /// </summary>
    Stream GetFileStream(string path);
}
