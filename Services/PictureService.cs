using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using System.Net;

namespace QuranSearchApp.Services
{
    public class PictureService
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

            // Use the project directory structure
            var projectDir = AppDomain.CurrentDomain.BaseDirectory;
            
            // Navigate to the picture folder in the project
            _pictureBasePath = Path.Combine(projectDir, "..", "..", "..", "..", "QuranText_jpg");
            
            // If that doesn't work, try relative to current directory
            if (!Directory.Exists(_pictureBasePath))
            {
                _pictureBasePath = Path.Combine(Directory.GetCurrentDirectory(), "QuranText_jpg");
            }
            
            // Normalize the path
            _pictureBasePath = Path.GetFullPath(_pictureBasePath);
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
        /// Checks if a picture file exists for the given Sura and Aya
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
        /// </summary>
        public async Task<Bitmap?> LoadBitmapAsync(string pathOrUrl)
        {
            try
            {
                if (_useRemoteSource && pathOrUrl.StartsWith("http"))
                {
                    using var response = await _httpClient.GetAsync(pathOrUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        using var stream = await response.Content.ReadAsStreamAsync();
                        return new Bitmap(stream);
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
    }
}
