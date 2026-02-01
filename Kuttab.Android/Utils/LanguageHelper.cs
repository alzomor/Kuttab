using Android.Content;
using Java.Util;

namespace Kuttab.Android.Utils;

public static class LanguageHelper
{
    private static readonly string[] SupportedLanguages = { "ar", "en", "de" };
    
    /// <summary>
    /// Gets the system language with fallback to English if not supported
    /// </summary>
    /// <returns>Language code (ar, en, or de)</returns>
    public static string GetSystemLanguageWithFallback()
    {
        try
        {
            // Get system locale
            var locale = Locale.Default;
            var systemLanguage = locale.Language.ToLowerInvariant();
            
            // Check if system language is supported
            if (Array.Exists(SupportedLanguages, lang => lang == systemLanguage))
            {
                return systemLanguage;
            }
            
            // Fallback to English if system language is not supported
            return "en";
        }
        catch
        {
            // If any error occurs, fallback to English
            return "en";
        }
    }
    
    /// <summary>
    /// Gets the saved language from preferences or system language as default
    /// </summary>
    /// <param name="context">Android context</param>
    /// <param name="prefsName">SharedPreferences name</param>
    /// <param name="keyName">Language key name</param>
    /// <returns>Language code (ar, en, or de)</returns>
    public static string GetLanguageFromPreferences(Context context, string prefsName = "QuranSearchSettings", string keyName = "Language")
    {
        try
        {
            var prefs = context.GetSharedPreferences(prefsName, FileCreationMode.Private);
            var savedLanguage = prefs?.GetString(keyName, null);
            
            // If no saved language, use system language with fallback
            if (string.IsNullOrEmpty(savedLanguage))
            {
                return GetSystemLanguageWithFallback();
            }
            
            // Validate saved language is supported
            if (Array.Exists(SupportedLanguages, lang => lang == savedLanguage))
            {
                return savedLanguage;
            }
            
            // If saved language is not supported, use system language with fallback
            return GetSystemLanguageWithFallback();
        }
        catch
        {
            // If any error occurs, fallback to English
            return "en";
        }
    }
    
    /// <summary>
    /// Checks if a language code is supported by the app
    /// </summary>
    /// <param name="languageCode">Language code to check</param>
    /// <returns>True if supported, false otherwise</returns>
    public static bool IsLanguageSupported(string languageCode)
    {
        return Array.Exists(SupportedLanguages, lang => lang == languageCode);
    }
}
