using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// The central manager for coordinating all settings modules,
/// acting as the single entry point for UI interaction.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    //[SerializeField] private EnvironmentManager environmentManager;

    public static SettingsManager Instance { get; private set; }

    private LanguageManager languageManager;

    private bool autoSwitch;
    private bool darkMode;

    /// <summary>
    /// Called once before Start(). Ensures components are assigned and loads saved preferences.
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

        //string savedEnv = UserPreferencesManager.LoadEnvironmentSceneOrNull();
        //if (!string.IsNullOrEmpty(savedEnv))
        //{
            //EnvironmentManager.Instance.LoadEnvironment(savedEnv);
        //}
    }

    /// <summary>
    /// Load preferences from PlayerPrefs or default values if not found.
    /// </summary>
    public void LoadSettings()
    {
        autoSwitch = UserPreferencesManager.LoadAutoAdjust();
        darkMode = UserPreferencesManager.LoadDarkMode();
        ApplyDisplayMode();
    }

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

    public void ApplySettings(bool newAutoSwitch, bool newDarkMode)
    {
        autoSwitch = newAutoSwitch;
        darkMode = newDarkMode;

        UserPreferencesManager.SaveAutoAdjust(autoSwitch);
        UserPreferencesManager.SaveDarkMode(darkMode);

        ApplyDisplayMode();
    }

    public void SetEnvironmentById(string sceneId)
    {
        //environmentManager?.LoadEnvironment(sceneId);
        //UserPreferencesManager.SaveBackgroundSceneId(sceneId);
    }

    public bool GetAutoSwitch() => autoSwitch;
    public bool GetDarkMode() => darkMode;

    public IReadOnlyDictionary<string, string> GetLanguages()
    {
        return languageManager.GetLanguages();
    }

    public string GetCurrentLanguageCode()
    {
        return languageManager.GetCurrentLanguageCode();
    }

    public bool LanguageExists(string code)
    {
        return languageManager.LanguageExists(code);
    }

    public async Task ApplyLanguageAsync(string code)
    {
        if (string.IsNullOrEmpty(code) || !LanguageExists(code)) return;

        await languageManager.SetLanguageAsync(code);
        UserPreferencesManager.SaveLanguage(code);
        Debug.Log($"[Lang] ApplyLanguageAsync target={code}");
        Debug.Log($"[Lang] SelectedLocale = {UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.Identifier.Code}");
    }

    public List<string> GetEnvironmentOptions()
    {
        return EnvironmentManager.Instance?.GetEnvironmentSceneNames() ?? new List<string>();
    }

    public string GetCurrentEnvironment()
    {
        return EnvironmentManager.Instance?.CurrentSceneName;
    }

    public void ApplyEnvironment(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        if (sceneName == GetCurrentEnvironment()) return;

        EnvironmentManager.Instance.LoadEnvironment(sceneName);
        //UserPreferencesManager.SaveEnvironmentScene(sceneName);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
