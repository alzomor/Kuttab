using System;
using System.IO;

namespace QuranSearchApp.Services
{
    public class PictureService
    {
        private readonly string _pictureBasePath;

        public PictureService()
        {
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
        /// Gets the picture file path for a specific Sura and Aya
        /// </summary>
        /// <param name="suraNumber">Sura number (1-114)</param>
        /// <param name="ayaNumber">Aya number</param>
        /// <returns>Full path to the JPG file</returns>
        public string GetPictureFilePath(int suraNumber, int ayaNumber)
        {
            // Format: S_A.jpg (sura_aya with no leading zeros)
            string fileName = $"{suraNumber}_{ayaNumber}.jpg";
            return Path.Combine(_pictureBasePath, fileName);
        }

        /// <summary>
        /// Checks if a picture file exists for the given Sura and Aya
        /// </summary>
        public bool PictureFileExists(int suraNumber, int ayaNumber)
        {
            string filePath = GetPictureFilePath(suraNumber, ayaNumber);
            bool exists = File.Exists(filePath);
            
            // Debug logging
            Console.WriteLine($"[PictureService] Checking file: {filePath}");
            Console.WriteLine($"[PictureService] File exists: {exists}");
            Console.WriteLine($"[PictureService] Base path: {_pictureBasePath}");
            Console.WriteLine($"[PictureService] Directory exists: {Directory.Exists(_pictureBasePath)}");
            
            return exists;
        }

        /// <summary>
        /// Gets the base path for picture files
        /// </summary>
        public string GetBasePath()
        {
            return _pictureBasePath;
        }
    }
}
