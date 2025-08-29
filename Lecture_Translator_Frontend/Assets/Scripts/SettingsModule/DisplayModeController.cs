using System;
using UnityEngine;

/// <summary>
/// A singleton controller responsible for managing the application's display mode 
/// (light or dark theme).
///
/// It supports two operation modes:
/// <list type="bullet">
/// <item><description><b>Auto mode</b>: automatically switches between light and dark mode 
/// based on the system time (dark mode between 18:00¨C06:00).</description></item>
/// <item><description><b>Manual mode</b>: allows the user to explicitly enable or disable 
/// dark mode, overriding auto adjustment.</description></item>
/// </list>
///
/// Themes are applied by propagating colors from <see cref="ColorTheme"/> to all 
/// <see cref="ThemedElement"/> objects in the scene, and by refreshing any 
/// <see cref="IThemeRefreshable"/> windows.
/// </summary>
public class DisplayModeController : MonoBehaviour
{
    /// <summary>
    /// Global singleton instance of <see cref="DisplayModeController"/>.
    /// Ensures only one controller exists across scenes.
    /// </summary>
    public static DisplayModeController Instance;

    [Header("Theme Settings")]
    public ColorTheme lightTheme;
    public ColorTheme darkTheme;

    private bool isDarkModeEnabled = false;
    private bool autoAdjust = true;

    // Timer used to refresh the mode periodically in auto mode
    private float refreshTimer = 0f;

    // How often to check system time in seconds (e.g., every 60 seconds)
    private float refreshInterval = 60f; // Refresh every 60 seconds

    /// <summary>
    /// Ensures singleton initialization.
    /// Destroys duplicate instances and persists the controller across scene loads.
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
    }

    /// <summary>
    /// Called on the first frame.
    /// Applies the initial mode according to current settings and system time.
    /// </summary>
    private void Start()
    {
        UpdateMode(); // Will internally call ApplyMode()
    }

    /// <summary>
    /// Called every frame.
    /// In auto adjust mode, refreshes the display mode at regular intervals 
    /// (defined by <see cref="refreshInterval"/>).
    /// </summary>
    private void Update()
    {
        if (autoAdjust)
        {
            refreshTimer += Time.deltaTime;
            if (refreshTimer >= refreshInterval)
            {
                refreshTimer = 0f;
                RefreshMode();
            }
        }
    }

    /// <summary>
    /// Returns whether dark mode is currently active.
    /// </summary>
    public bool IsDarkModeEnabled()
    {
        return isDarkModeEnabled;
    }

    /// <summary>
    /// Enables or disables dark mode manually.
    /// Has no effect if auto adjust is enabled.
    /// </summary>
    /// <param name="enabled">True to enable dark mode, false for light mode.</param>
    public void SetDarkMode(bool enabled)
    {
        if (!autoAdjust)
        {
            isDarkModeEnabled = enabled;
            ApplyTheme();
        }
    }

    /// <summary>
    /// Updates the display mode according to the system time 
    /// (dark mode active between 18:00¨C06:00).
    /// Only executes when auto adjust is enabled.
    /// Always reapplies the theme after checking.
    /// </summary>
    public void UpdateMode()
    {
        if (autoAdjust)
        {
            int hour = DateTime.Now.Hour;
            isDarkModeEnabled = (hour >= 18 || hour < 6);
        }

        ApplyTheme();
    }

    /// <summary>
    /// Manually triggers a mode refresh.
    /// Useful for forcing a re-check from UI or tests.
    /// </summary>
    public void RefreshMode()
    {
        UpdateMode();
    }

    /// <summary>
    /// Enables or disables automatic adjustment of the theme 
    /// based on the system time.
    /// Re-applies the mode immediately after switching.
    /// </summary>
    /// <param name="enabled">True to enable auto adjust, false to disable.</param>
    public void SetAutoAdjust(bool enabled)
    {
        autoAdjust = enabled;
        UpdateMode();
    }

    /// <summary>
    /// Returns whether automatic time-based adjustment is currently enabled.
    /// </summary>
    public bool IsAutoAdjustEnabled()
    {
        return autoAdjust;
    }

    /// <summary>
    /// Applies the currently active theme (light or dark) 
    /// to all <see cref="ThemedElement"/> objects in the scene 
    /// and refreshes any <see cref="IThemeRefreshable"/> windows.
    /// Logs the applied mode for debugging.
    /// </summary>
    private void ApplyTheme()
    {
        ColorTheme activeTheme = isDarkModeEnabled ? darkTheme : lightTheme;

        foreach (var element in UnityEngine.Object.FindObjectsByType<ThemedElement>(FindObjectsSortMode.None))
        {
            element.ApplyTheme(activeTheme, isDarkModeEnabled);
        }

        foreach (var win in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (win is IThemeRefreshable refreshable) refreshable.RefreshTheme();
        }

        Debug.Log($"[Theme] Mode applied: {(isDarkModeEnabled ? "Dark" : "Light")}");
    }

    /// <summary>
    /// Cleans up the singleton reference when this controller is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
