using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LanguageManager
{
    private bool hasInitialized = false;
    private Dictionary<string, string> languageList = new Dictionary<string, string>();

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

    public IReadOnlyDictionary<string, string> GetLanguages()
    {
        return languageList;
    }

    public string GetCurrentLanguageCode()
    {
        return LocalizationSettings.SelectedLocale?.Identifier.Code;
    }

    public bool LanguageExists(string code)
    {
        if (string.IsNullOrEmpty(code)) return false;
        return languageList.ContainsKey(code);
    }

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
            Debug.LogWarning($"[LanguageManager] 找不到代码为 {code} 的语言");
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
