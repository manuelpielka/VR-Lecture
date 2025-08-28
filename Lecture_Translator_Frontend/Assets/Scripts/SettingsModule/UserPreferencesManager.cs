using UnityEngine;

/// <summary>
/// A centralized manager for saving and loading user preferences such as 
/// playback speed, subtitle font size, and display mode.
/// Wraps Unity's PlayerPrefs for cleaner usage.
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
    /// Saves whether the app should be in dark mode.
    /// </summary>
    public static void SaveDarkMode(bool isDark)
    {
        Debug.Log($"Saving Dark Mode: {isDark}");
        PlayerPrefs.SetInt(IsDarkModeKey, isDark ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads the dark mode setting or returns false by default.
    /// </summary>
    public static bool LoadDarkMode()
    {
        bool isDark = PlayerPrefs.GetInt(IsDarkModeKey, 0) == 1;
        Debug.Log($"Loaded Dark Mode: {isDark}");
        return isDark;
    }

    /// <summary>
    /// Saves whether automatic mode switching is enabled.
    /// </summary>
    public static void SaveAutoAdjust(bool enabled)
    {
        Debug.Log($"Saving Auto Adjust: {enabled}");
        PlayerPrefs.SetInt(AutoAdjustKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads whether automatic mode switching is enabled, default is true.
    /// </summary>
    public static bool LoadAutoAdjust()
    {
        bool autoAdjust = PlayerPrefs.GetInt(AutoAdjustKey, 1) == 1;
        Debug.Log($"Loaded Auto Adjust: {autoAdjust}");
        return autoAdjust;
    }

    public static void SaveLanguage(string code)
    {
        Debug.Log($"Saving Language: {code}");
        PlayerPrefs.SetString(LanguageKey, code);
        PlayerPrefs.Save();
    }

    public static string LoadLanguageOrNull()
    {
        return PlayerPrefs.HasKey(LanguageKey) ? PlayerPrefs.GetString(LanguageKey) : null;
    }

    //public static void SaveEnvironmentScene(string name)
    //{
        //PlayerPrefs.SetString(EnvKey, name);
        //PlayerPrefs.Save();
    //}

    //public static string LoadEnvironmentSceneOrNull()
    //{
        //return PlayerPrefs.HasKey(EnvKey) ? PlayerPrefs.GetString(EnvKey) : null;
    //}

    /// <summary>
    /// Applies all saved user preferences to the given playback and display controllers.
    /// </summary>
    //public static void ApplyAll(DisplayModeController display)
    //{
        //try
        //{
            //display.SetDarkMode(LoadDarkMode());
            //display.SetAutoAdjust(LoadAutoAdjust());
        //}
        //catch (System.Exception ex)
        //{
            //Debug.LogError($"Error applying preferences: {ex.Message}");
        //}
    //}

    /// <summary>
    /// Saves all user preferences from the current playback and display state.
    /// </summary>
    //public static void SaveAll(DisplayModeController display)
    //{
        //try
        //{
            //SaveDarkMode(display.IsDarkModeEnabled());
            //SaveAutoAdjust(display.IsAutoAdjustEnabled());
            //PlayerPrefs.Save();
        //}
        //catch (System.Exception ex)
        //{
            //Debug.LogError($"Error saving preferences: {ex.Message}");
        //}
    //}

    /// <summary>
    /// Clears all user preferences (useful for testing or reset button).
    /// </summary>
    public static void ClearAll()
    {
        Debug.Log("Clearing all user preferences");
        PlayerPrefs.DeleteKey(IsDarkModeKey);
        PlayerPrefs.DeleteKey(AutoAdjustKey);
        PlayerPrefs.DeleteKey(TutorialcompletedKey);
    }

    /// <summary>
    /// Saves whether the tutorial was completed.
    /// </summary>
    public static void SaveTutorialCompleted(bool completed)
    {
        PlayerPrefs.SetInt(TutorialcompletedKey, completed ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads the tutorial completion state.
    /// </summary>
    public static bool LoadTutorialCompleted()
    {
        return PlayerPrefs.GetInt(TutorialcompletedKey, 0) == 1;
    }
}

