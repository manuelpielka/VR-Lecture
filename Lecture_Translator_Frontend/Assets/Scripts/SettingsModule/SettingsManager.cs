using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// Centralized manager responsible for coordinating all application settings,
/// including display mode, language, and environment selection.
///
/// This class acts as the **single entry point** for UI interactions with settings.
/// It wraps access to:
/// <list type="bullet">
/// <item><description><see cref="DisplayModeController"/> for light/dark mode and auto-switching.</description></item>
/// <item><description><see cref="LanguageManager"/> for localization and language switching.</description></item>
/// <item><description><see cref="EnvironmentManager"/> for environment scene management.</description></item>
/// <item><description><see cref="UserPreferencesManager"/> for saving/loading persistent settings via <c>PlayerPrefs</c>.</description></item>
/// </list>
/// </summary>

public class SettingsManager : MonoBehaviour
{

    /// <summary>
    /// Singleton instance of <see cref="SettingsManager"/>.
    /// Ensures only one instance persists across scenes.
    /// </summary>
    public static SettingsManager Instance { get; private set; }

    /// <summary>
    /// Internal language manager used for localization handling.
    /// </summary>
    private LanguageManager languageManager;

    private bool autoSwitch;
    private bool darkMode;

    /// <summary>
    /// Called once before <see cref="Start"/>.
    /// Initializes the singleton, creates the <see cref="LanguageManager"/>,
    /// and loads saved user preferences.
    /// </summary>
    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        languageManager = new LanguageManager();
        LoadSettings();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // 
        PlayerPrefs.DeleteKey("env_scene");
#endif
    }

    /// <summary>
    /// Called on the first frame.
    /// Applies display mode, initializes language manager, and restores saved language.
    /// </summary>
    private async void Start()
    {
        Debug.Log("SettingsManager Started");
        ApplyDisplayMode();

        await languageManager.InitializeAsync();

        string savedLanguage = UserPreferencesManager.LoadLanguageOrNull();
        if (string.IsNullOrEmpty(savedLanguage))
        {
            savedLanguage = "en";
            UserPreferencesManager.SaveLanguage(savedLanguage);
        }

        if (LanguageExists(savedLanguage))
        {
            await languageManager.SetLanguageAsync(savedLanguage);
        }
    }

    /// <summary>
    /// Loads user preferences from <see cref="PlayerPrefs"/> 
    /// (or applies default values if none are found).
    /// </summary>
    public void LoadSettings()
    {
        autoSwitch = UserPreferencesManager.LoadAutoAdjust();
        darkMode = UserPreferencesManager.LoadDarkMode();
        ApplyDisplayMode();
    }

    /// <summary>
    /// Applies display mode settings to the <see cref="DisplayModeController"/>.
    /// Ensures auto-switch and dark/light mode preferences are reflected.
    /// Logs an error if no <see cref="DisplayModeController"/> is present in the scene.
    /// </summary>
    private void ApplyDisplayMode()
    {
        Debug.Log("Applying Display Mode");

        if (DisplayModeController.Instance == null)
        {
            Debug.LogError("DisplayModeController.Instance is null!");
            return;
        }

        // autoSwitch
        DisplayModeController.Instance.SetAutoAdjust(autoSwitch);

        if (autoSwitch)
        {
            // autoSwitch
            return;
        }

        if (darkMode)
        {
            // Dark Mode
            DisplayModeController.Instance.SetDarkMode(true);
        }
        else
        {
            // Light Mode
            DisplayModeController.Instance.SetDarkMode(false);
        }
    }

    /// <summary>
    /// Updates settings (auto-switch and dark mode), saves them to <see cref="PlayerPrefs"/>,
    /// and applies them immediately.
    /// </summary>
    /// <param name="newAutoSwitch">New auto-switch setting.</param>
    /// <param name="newDarkMode">New dark mode setting.</param>
    public void ApplySettings(bool newAutoSwitch, bool newDarkMode)
    {
        autoSwitch = newAutoSwitch;
        darkMode = newDarkMode;

        UserPreferencesManager.SaveAutoAdjust(autoSwitch);
        UserPreferencesManager.SaveDarkMode(darkMode);

        ApplyDisplayMode();
    }

    /// <summary>
    /// Returns whether automatic dark/light mode switching is enabled.
    /// </summary>
    public bool GetAutoSwitch() => autoSwitch;

    /// <summary>
    /// Returns whether dark mode is currently enabled (only relevant if auto-switch is disabled).
    /// </summary>
    public bool GetDarkMode() => darkMode;

    /// <summary>
    /// Returns a dictionary of available languages (code ¡ú display name).
    /// </summary>
    public IReadOnlyDictionary<string, string> GetLanguages()
    {
        return languageManager.GetLanguages();
    }

    /// <summary>
    /// Returns the currently active language code (e.g., "en", "de").
    /// </summary>
    public string GetCurrentLanguageCode()
    {
        return languageManager.GetCurrentLanguageCode();
    }

    /// <summary>
    /// Checks whether the specified language code exists in the available set.
    /// </summary>
    public bool LanguageExists(string code)
    {
        return languageManager.LanguageExists(code);
    }

    /// <summary>
    /// Applies a new language asynchronously.
    /// Updates <see cref="UserPreferencesManager"/> and logs the result.
    /// Ignores invalid or non-existent codes.
    /// </summary>
    public async Task ApplyLanguageAsync(string code)
    {
        if (string.IsNullOrEmpty(code) || !LanguageExists(code)) return;

        await languageManager.SetLanguageAsync(code);
        UserPreferencesManager.SaveLanguage(code);
        Debug.Log($"[Lang] ApplyLanguageAsync target={code}");
        Debug.Log($"[Lang] SelectedLocale = {UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.Identifier.Code}");
    }

    /// <summary>
    /// Returns all available environment scene options from <see cref="EnvironmentManager"/>.
    /// Returns an empty list if no environment manager is available.
    /// </summary>
    public List<string> GetEnvironmentOptions()
    {
        return EnvironmentManager.Instance?.GetEnvironmentSceneNames() ?? new List<string>();
    }

    /// <summary>
    /// Returns the name of the currently active environment scene,
    /// or <c>null</c> if no <see cref="EnvironmentManager"/> is present.
    /// </summary>

    public string GetCurrentEnvironment()
    {
        return EnvironmentManager.Instance?.CurrentSceneName;
    }

    /// <summary>
    /// Applies a new environment scene by name.
    /// Does nothing if the scene name is null/empty or already active.
    /// </summary>
    public void ApplyEnvironment(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        if (sceneName == GetCurrentEnvironment()) return;

        EnvironmentManager.Instance.LoadEnvironment(sceneName);
    }

    /// <summary>
    /// Cleans up the singleton reference when this object is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
