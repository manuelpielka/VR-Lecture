using UnityEngine;

/// <summary>
/// A centralized manager for saving and loading user preferences such as 
/// playback speed, subtitle font size, and display mode.
/// Wraps Unity's PlayerPrefs for cleaner usage.
/// </summary>
public static class UserPreferencesManager
{
    // PlayerPrefs keys
    private const string PlaybackSpeedKey = "UserPref_PlaybackSpeed";
    private const string SubtitleFontSizeKey = "UserPref_SubtitleFontSize";
    private const string IsDarkModeKey = "UserPref_IsDarkMode";
    private const string AutoAdjustKey = "UserPref_AutoAdjust";
    private const string BackgroundSceneIdKey = "UserPref_BackgroundSceneId";

    /// <summary>
    /// Saves the user's preferred playback speed.
    /// </summary>
    public static void SavePlaybackSpeed(float speed)
    {
        PlayerPrefs.SetFloat(PlaybackSpeedKey, speed);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads the saved playback speed or returns 1.0f by default.
    /// </summary>
    public static float LoadPlaybackSpeed()
    {
        return PlayerPrefs.GetFloat(PlaybackSpeedKey, 1.0f); // TODO
    }

    /// <summary>
    /// Saves the user's subtitle font size preference.
    /// </summary>
    public static void SaveSubtitleFontSize(int size)
    {
        PlayerPrefs.SetInt(SubtitleFontSizeKey, size);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads the saved subtitle font size or returns 16 by default.
    /// </summary>
    public static int LoadSubtitleFontSize()
    {
        return PlayerPrefs.GetInt(SubtitleFontSizeKey, 16); // TODO
    }

    /// <summary>
    /// Saves whether the app should be in dark mode.
    /// </summary>
    public static void SaveDarkMode(bool isDark)
    {
        PlayerPrefs.SetInt(IsDarkModeKey, isDark ? 1 : 0); // TODO
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads the dark mode setting or returns false by default.
    /// </summary>
    public static bool LoadDarkMode()
    {
        return PlayerPrefs.GetInt(IsDarkModeKey, 0) == 1; // TODO
    }

    /// <summary>
    /// Saves whether automatic mode switching is enabled.
    /// </summary>
    public static void SaveAutoAdjust(bool enabled)
    {
        PlayerPrefs.SetInt(AutoAdjustKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads whether automatic mode switching is enabled, default is true.
    /// </summary>
    public static bool LoadAutoAdjust()
    {
        return PlayerPrefs.GetInt(AutoAdjustKey, 1) == 1; // Default: auto-adjust ON
    }

    public static void SaveBackgroundSceneId(string sceneId)
    {
        PlayerPrefs.SetString(BackgroundSceneIdKey, sceneId);
        PlayerPrefs.Save();
    }

    public static string LoadBackgroundSceneId()
    {
        return PlayerPrefs.GetString(BackgroundSceneIdKey, "cafe");
    }


    /// <summary>
    /// Applies all saved user preferences to the given playback and display controllers.
    /// </summary>
    public static void ApplyAll(PlaybackSettingsManager playback, DisplayModeController display)
    {
        playback.SetPlaybackSpeed(LoadPlaybackSpeed());
        playback.SetSubtitleFontSize(LoadSubtitleFontSize());

        display.SetDarkMode(LoadDarkMode());
        display.SetAutoAdjust(LoadAutoAdjust());
    }

    /// <summary>
    /// Saves all user preferences from the current playback and display state.
    /// </summary>
    public static void SaveAll(PlaybackSettingsManager playback, DisplayModeController display)
    {
        SavePlaybackSpeed(playback.GetPlaybackSpeed());
        SaveSubtitleFontSize(playback.GetSubtitleFontSize());

        SaveDarkMode(display.IsDarkModeEnabled());
        SaveAutoAdjust(display.IsAutoAdjustEnabled());

        PlayerPrefs.Save(); // Ensure everything is written
    }

    /// <summary>
    /// Clears all user preferences (useful for testing or reset button).
    /// </summary>
    public static void ClearAll()
    {
        PlayerPrefs.DeleteKey(PlaybackSpeedKey);
        PlayerPrefs.DeleteKey(SubtitleFontSizeKey);
        PlayerPrefs.DeleteKey(IsDarkModeKey);
        PlayerPrefs.DeleteKey(AutoAdjustKey);
    }
}
