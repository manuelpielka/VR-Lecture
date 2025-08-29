using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

/// <summary>
/// A wrapper around Unity's Localization system that provides high-level methods 
/// for initializing, querying, and changing the application's language.
/// 
/// This manager loads all available locales from <see cref="LocalizationSettings"/> 
/// and exposes them in a simple <c>Dictionary&lt;string, string&gt;</c> where the key is 
/// the language code (e.g., "en", "de") and the value is the display name.
/// </summary>
public class LanguageManager
{
    private bool hasInitialized = false;
    private Dictionary<string, string> languageList = new Dictionary<string, string>();

    /// <summary>
    /// Initializes the language manager by waiting for the Unity Localization system 
    /// to finish its <see cref="LocalizationSettings.InitializationOperation"/>.
    /// 
    /// After initialization, all available locales are cached in <see cref="languageList"/>.
    /// Calling this method multiple times is safe; it will only initialize once.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (hasInitialized) return;
        await LocalizationSettings.InitializationOperation.Task;

        var availableLocales = LocalizationSettings.AvailableLocales.Locales;

        foreach (var locale in availableLocales)
        {
            string code = locale.Identifier.Code;
            string name = locale.LocaleName;
            languageList[code] = name;
        }

        hasInitialized = true;
    }

    /// <summary>
    /// Returns all loaded languages as a read-only dictionary mapping 
    /// language code ¡ú language name.
    /// </summary>
    public IReadOnlyDictionary<string, string> GetLanguages()
    {
        return languageList;
    }

    /// <summary>
    /// Gets the language code of the currently selected locale.
    /// For example: "en", "fr", "de".
    /// Returns <c>null</c> if no locale is selected.
    /// </summary>
    public string GetCurrentLanguageCode()
    {
        return LocalizationSettings.SelectedLocale?.Identifier.Code;
    }

    /// <summary>
    /// Checks whether a language code exists in the cached list of available languages.
    /// </summary>
    /// <param name="code">The ISO language code to check (e.g., "en").</param>
    /// <returns><c>true</c> if the language is available; otherwise <c>false</c>.</returns>
    public bool LanguageExists(string code)
    {
        if (string.IsNullOrEmpty(code)) return false;
        return languageList.ContainsKey(code);
    }

    /// <summary>
    /// Attempts to change the current application language to the specified code.
    /// 
    /// If the code is not found in the available locales, a warning is logged and no change is made.
    /// If the requested language is already the current locale, nothing happens.
    /// </summary>
    /// <param name="code">The ISO language code of the desired locale (e.g., "en").</param>
    public async Task SetLanguageAsync(string code)
    {
        // wait for initialising
        await LocalizationSettings.InitializationOperation.Task;

        // search language
        var availableLocales = LocalizationSettings.AvailableLocales.Locales;
        var targetLocale = availableLocales.FirstOrDefault(l => l.Identifier.Code == code);

        // if not found
        if (targetLocale == null)
        {
            Debug.LogWarning($"[LanguageManager] {code} not found");
            return;
        }

        // if same as current language
        if (LocalizationSettings.SelectedLocale == targetLocale)
        {
            return;
        }

        // different, switch language
        LocalizationSettings.SelectedLocale = targetLocale;
    }
}
