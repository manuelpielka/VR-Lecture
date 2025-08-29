using UnityEngine;

/// <summary>
/// Centralized utility for saving and loading persistent user preferences 
/// such as dark mode, auto-adjust mode, environment, language, and tutorial progress.
/// 
/// Internally wraps <see cref="PlayerPrefs"/> for convenient typed access,
/// ensuring consistency across the application.
/// </summary>
public static class UserPreferencesManager
{
    // PlayerPrefs keys
    private const string IsDarkModeKey = "UserPref_IsDarkMode";
    private const string AutoAdjustKey = "UserPref_AutoAdjust";
    private const string EnvKey = "env_scene";
    private const string LanguageKey = "UserPref_Language";

    private const string TutorialcompletedKey = "UserPref_TutorialCompleted";

    /// <summary>
    /// Persists the dark mode preference.
    /// </summary>
    /// <param name="isDark">True for dark mode; False for light mode.</param>
    public static void SaveDarkMode(bool isDark)
    {
        Debug.Log($"Saving Dark Mode: {isDark}");
        PlayerPrefs.SetInt(IsDarkModeKey, isDark ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads the dark mode preference.
    /// </summary>
    /// <returns>
    /// True if dark mode is enabled; False if disabled.  
    /// Defaults to <c>false</c> when no preference has been saved.
    /// </returns>
    public static bool LoadDarkMode()
    {
        bool isDark = PlayerPrefs.GetInt(IsDarkModeKey, 0) == 1;
        Debug.Log($"Loaded Dark Mode: {isDark}");
        return isDark;
    }

    /// <summary>
    /// Persists whether automatic mode switching (light/dark) is enabled.
    /// </summary>
    /// <param name="enabled">True to enable auto-switch; False to disable.</param>
    public static void SaveAutoAdjust(bool enabled)
    {
        Debug.Log($"Saving Auto Adjust: {enabled}");
        PlayerPrefs.SetInt(AutoAdjustKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads the auto-adjust mode preference.
    /// </summary>
    /// <returns>
    /// True if auto-switch is enabled; False otherwise.  
    /// Defaults to <c>true</c> when no preference has been saved.
    /// </returns>
    public static bool LoadAutoAdjust()
    {
        bool autoAdjust = PlayerPrefs.GetInt(AutoAdjustKey, 1) == 1;
        Debug.Log($"Loaded Auto Adjust: {autoAdjust}");
        return autoAdjust;
    }

    /// <summary>
    /// Persists the preferred language code (e.g., "en", "de").
    /// </summary>
    /// <param name="code">Language code to save.</param>
    public static void SaveLanguage(string code)
    {
        Debug.Log($"Saving Language: {code}");
        PlayerPrefs.SetString(LanguageKey, code);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads the saved language code if available.
    /// </summary>
    /// <returns>
    /// The saved language code (e.g., "en"),  
    /// or <c>null</c> if no language preference exists.
    /// </returns>
    public static string LoadLanguageOrNull()
    {
        return PlayerPrefs.HasKey(LanguageKey) ? PlayerPrefs.GetString(LanguageKey) : null;
    }

    /// <summary>
    /// Clears all stored user preferences.  
    /// Useful for testing or when resetting the app to factory defaults.
    /// </summary>
    public static void ClearAll()
    {
        Debug.Log("Clearing all user preferences");
        PlayerPrefs.DeleteKey(IsDarkModeKey);
        PlayerPrefs.DeleteKey(AutoAdjustKey);
        PlayerPrefs.DeleteKey(TutorialcompletedKey);
    }

    /// <summary>
    /// Persists the tutorial completion state.
    /// </summary>
    /// <param name="completed">True if tutorial was completed; False otherwise.</param>
    public static void SaveTutorialCompleted(bool completed)
    {
        PlayerPrefs.SetInt(TutorialcompletedKey, completed ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads whether the tutorial has been completed.
    /// </summary>
    /// <returns>
    /// True if the tutorial was previously marked as completed;  
    /// False if not completed or no state saved.
    /// </returns>
    public static bool LoadTutorialCompleted()
    {
        return PlayerPrefs.GetInt(TutorialcompletedKey, 0) == 1;
    }
}

