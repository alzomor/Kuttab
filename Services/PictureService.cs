using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using System.Net;
using QuranSearch.Core.Interfaces;

namespace QuranSearchApp.Services
{
    public class PictureService : IPictureService
    {
        private readonly string _pictureBasePath;
        private readonly string _remoteBaseUrl = "https://everyayah.com/data/QuranText_jpg/";
        private readonly HttpClient _httpClient;
        private bool _useRemoteSource = false;

        public bool UseRemoteSource
        {
            get => _useRemoteSource;
            set => _useRemoteSource = value;
        }

        public PictureService()
        {
            // Initialize HTTP client with timeout
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10) // 10 second timeout for image loading
            };

            // Look for picture files in the same directory as the executable
            var exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _pictureBasePath = Path.Combine(exeDirectory, "QuranText_jpg");
            
            // Log the path for debugging
            Console.WriteLine($"[PictureService] Looking for picture files in: {_pictureBasePath}");
            Console.WriteLine($"[PictureService] Directory exists: {Directory.Exists(_pictureBasePath)}");
        }

        /// <summary>
        /// Gets the picture file path or URL for a specific Sura and Aya
        /// </summary>
        /// <param name="suraNumber">Sura number (1-114)</param>
        /// <param name="ayaNumber">Aya number</param>
        /// <returns>Full path or URL to the JPG file</returns>
        public string GetPicturePath(int suraNumber, int ayaNumber)
        {
            // Format: S_A.jpg (sura_aya with no leading zeros)
            string fileName = $"{suraNumber}_{ayaNumber}.jpg";
            
            if (_useRemoteSource)
            {
                return new Uri(new Uri(_remoteBaseUrl), fileName).ToString();
            }
            
            return Path.Combine(_pictureBasePath, fileName);
        }

        /// <summary>
        /// Checks if a picture file exists for the given Sura and Aya (synchronous)
        /// </summary>
        public bool PictureExists(int suraNumber, int ayaNumber)
        {
            if (_useRemoteSource)
            {
                // For remote source, assume exists (will fail gracefully on load)
                return true;
            }
            
            string filePath = GetPicturePath(suraNumber, ayaNumber);
            return File.Exists(filePath);
        }

        /// <summary>
        /// Checks if a picture file exists for the given Sura and Aya (async version)
        /// </summary>
        public async Task<bool> PictureExistsAsync(int suraNumber, int ayaNumber)
        {
            if (_useRemoteSource)
            {
                try
                {
                    var url = GetPicturePath(suraNumber, ayaNumber);
                    var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
                    return response.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PictureService] Error checking remote file: {ex.Message}");
                    return false;
                }
            }
            else
            {
                string filePath = GetPicturePath(suraNumber, ayaNumber);
                bool exists = File.Exists(filePath);
                
                // Debug logging
                Console.WriteLine($"[PictureService] Checking file: {filePath}");
                Console.WriteLine($"[PictureService] File exists: {exists}");
                Console.WriteLine($"[PictureService] Base path: {_pictureBasePath}");
                Console.WriteLine($"[PictureService] Directory exists: {Directory.Exists(_pictureBasePath)}");
                
                return exists;
            }
        }

        /// <summary>
        /// Loads a bitmap image from the specified path or URL
        /// Returns as object to match interface (will be Avalonia.Media.Imaging.Bitmap)
        /// </summary>
        async Task<object?> IPictureService.LoadBitmapAsync(string pathOrUrl)
        {
            return await LoadBitmapAsync(pathOrUrl);
        }

        /// <summary>
        /// Loads a bitmap image from the specified path or URL
        /// </summary>
        public async Task<Bitmap?> LoadBitmapAsync(string pathOrUrl)
        {
            try
            {
                if (_useRemoteSource && pathOrUrl.StartsWith("http"))
                {
                    // Extract sura and aya numbers from the URL
                    string fileName = Path.GetFileName(pathOrUrl);
                    if (fileName.EndsWith(".jpg"))
                    {
                        var parts = fileName.Replace(".jpg", "").Split('_');
                        if (parts.Length == 2 && int.TryParse(parts[0], out int sura) && int.TryParse(parts[1], out int aya))
                        {
                            // Use cache system
                            string? cachedPath = await GetOrDownloadPictureFileAsync(sura, aya);
                            if (!string.IsNullOrEmpty(cachedPath) && File.Exists(cachedPath))
                            {
                                return new Bitmap(cachedPath);
                            }
                        }
                    }
                    return null;
                }
                else if (File.Exists(pathOrUrl))
                {
                    return new Bitmap(pathOrUrl);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PictureService] Error loading bitmap: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets picture file from cache or downloads it if not cached
        /// </summary>
        private async Task<string?> GetOrDownloadPictureFileAsync(int suraNumber, int ayaNumber)
        {
            try
            {
                // Ensure cache directory exists
                if (!Directory.Exists(_pictureBasePath))
                {
                    Console.WriteLine($"[PictureService] Creating cache directory: {_pictureBasePath}");
                    Directory.CreateDirectory(_pictureBasePath);
                }

                // Check if file exists in cache
                string fileName = $"{suraNumber}_{ayaNumber}.jpg";
                string cachedFilePath = Path.Combine(_pictureBasePath, fileName);

                if (File.Exists(cachedFilePath))
                {
                    Console.WriteLine($"[PictureService] Using cached file: {cachedFilePath}");
                    return cachedFilePath;
                }

                // Download to cache
                string remoteUrl = new Uri(new Uri(_remoteBaseUrl), fileName).ToString();
                Console.WriteLine($"[PictureService] Downloading from: {remoteUrl}");
                Console.WriteLine($"[PictureService] Caching to: {cachedFilePath}");

                using var response = await _httpClient.GetAsync(remoteUrl, HttpCompletionOption.ResponseHeadersRead);
                Console.WriteLine($"[PictureService] Response status: {response.StatusCode}");
                response.EnsureSuccessStatusCode();

                await using (var fs = File.Create(cachedFilePath))
                {
                    await response.Content.CopyToAsync(fs);
                }

                var fileInfo = new FileInfo(cachedFilePath);
                Console.WriteLine($"[PictureService] Downloaded and cached file size: {fileInfo.Length} bytes");

                return cachedFilePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PictureService] Failed to get or download picture: {ex.Message}");
                return null;
            }
        }
    }
}
