using UnityEngine;

/// <summary>
/// The central manager for coordinating all settings modules,
/// acting as the single entry point for UI interaction.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    //[SerializeField] private EnvironmentManager environmentManager;

    public static SettingsManager Instance { get; private set; }

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

        // Load user preferences (if any)
        autoSwitch = UserPreferencesManager.LoadAutoAdjust();
        darkMode = UserPreferencesManager.LoadDarkMode(); LoadSettings();
    }

    private void Start()
    {
        Debug.Log("SettingsManager Started");
        ApplyDisplayMode();
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
}
