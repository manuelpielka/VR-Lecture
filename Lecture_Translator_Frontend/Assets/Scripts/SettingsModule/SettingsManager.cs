using UnityEngine;

/// <summary>
/// The central manager for coordinating all settings modules,
/// acting as the single entry point for UI interaction.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    [SerializeField] private EnvironmentManager environmentManager;


    // Reference to display mode controller
    private DisplayModeController displayModeController;

    /// <summary>
    /// Called once before Start(). Ensures components are assigned and loads saved preferences.
    /// </summary>
    private void Awake()
    {

        displayModeController = FindFirstObjectByType<DisplayModeController>();

        // Load user preferences (if any)
        LoadUserPreferences();
    }

    /// <summary>
    /// Load preferences from PlayerPrefs or default values if not found.
    /// </summary>
    private void LoadUserPreferences()
    {
        if (displayModeController != null)
        {
            displayModeController.SetAutoAdjust(UserPreferencesManager.LoadAutoAdjust());
            displayModeController.SetDarkMode(UserPreferencesManager.LoadDarkMode());
        }
    }

    /// <summary>
    /// Enables or disables auto-adjust mode and stores it.
    /// </summary>
    public void SetAutoAdjust(bool enabled)
    {
        displayModeController?.SetAutoAdjust(enabled);
        UserPreferencesManager.SaveAutoAdjust(enabled);
    }

    /// <summary>
    /// Manually sets dark/light mode (only works if autoAdjust is off).
    /// </summary>
    public void SetDarkMode(bool enabled)
    {
        displayModeController?.SetDarkMode(enabled);
        UserPreferencesManager.SaveDarkMode(enabled);
    }

    public void SetEnvironmentById(string sceneId)
    {
        environmentManager?.LoadEnvironment(sceneId);
        UserPreferencesManager.SaveBackgroundSceneId(sceneId);
    }
    public void ApplyAll()
    {
        UserPreferencesManager.SaveAll(displayModeController);
        UserPreferencesManager.ApplyAll(displayModeController);
    }
}
