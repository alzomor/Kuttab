using System;
using System.IO;
using System.Threading.Tasks;
using Kuttab.Core.Interfaces;

namespace Kuttab.Services;

/// <summary>
/// Desktop implementation of IFileService
/// Uses standard .NET File API for file access
/// </summary>
public class DesktopFileService : IFileService
{
    public string GetAppDataDirectory()
    {
        return AppDomain.CurrentDomain.BaseDirectory;
    }

    public bool FileExists(string path)
    {
        var fullPath = Path.IsPathRooted(path) 
            ? path 
            : Path.Combine(GetAppDataDirectory(), path);
        return File.Exists(fullPath);
    }

    public async Task<string> ReadAllTextAsync(string path)
    {
        var fullPath = Path.IsPathRooted(path) 
            ? path 
            : Path.Combine(GetAppDataDirectory(), path);
        return await File.ReadAllTextAsync(fullPath);
    }

    public string ReadAllText(string path)
    {
        var fullPath = Path.IsPathRooted(path) 
            ? path 
            : Path.Combine(GetAppDataDirectory(), path);
        return File.ReadAllText(fullPath);
    }

    public Stream GetFileStream(string path)
    {
        var fullPath = Path.IsPathRooted(path) 
            ? path 
            : Path.Combine(GetAppDataDirectory(), path);
        return File.OpenRead(fullPath);
    }
}
