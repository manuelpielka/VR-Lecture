using System;
using UnityEngine;

/// <summary>
/// A class that handles automatic and manual switching between light and dark display modes
/// based on the system time and user preferences.
/// </summary>
public class DisplayModeController : MonoBehaviour
{
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
    /// Called before Start(). Loads saved preferences for mode and auto adjust.
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
    /// Called on the first frame. Applies initial mode based on loaded preferences.
    /// </summary>
    private void Start()
    {
        UpdateMode(); // Will internally call ApplyMode()
    }

    /// <summary>
    /// Called every frame. If auto adjust is on, refresh display mode every minute.
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
    /// Returns true if the current display mode is dark mode, otherwise returns false.
    /// </summary>RefreshMode();
    public bool IsDarkModeEnabled()
    {
        return isDarkModeEnabled;
    }

    /// <summary>
    /// Enables or disables dark mode manually.
    /// Only effective when autoAdjust is false.
    /// Also saves the user's preference.
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
    /// Toggles dark mode manually. Only works when autoAdjust is false.
    /// </summary>
    //public void ToggleMode()
    //{
        //if (!autoAdjust)
        //{
            //isDarkModeEnabled = !isDarkModeEnabled;
            //UserPreferencesManager.SaveDarkMode(isDarkModeEnabled);
            //ApplyTheme();
        //}
    //}

    /// <summary>
    /// Updates the display mode based on the current system time.
    /// Only works when autoAdjust is enabled.
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
    /// Manually triggers a mode refresh (used by UI buttons if needed).
    /// </summary>
    public void RefreshMode()
    {
        UpdateMode();
    }

    /// <summary>
    /// Enables or disables automatic mode adjustment based on system time.
    /// </summary>
    /// <param name="enabled">True to enable auto adjust; false to disable.</param>
    public void SetAutoAdjust(bool enabled)
    {
        autoAdjust = enabled;
        UpdateMode();
    }

    /// <summary>
    /// Returns true if automatic adjustment is enabled; otherwise false.
    /// </summary>
    public bool IsAutoAdjustEnabled()
    {
        return autoAdjust;
    }

    /// <summary>
    /// Applies the current display mode visually (background and text colors).
    /// </summary>
    private void ApplyTheme()
    {
        ColorTheme activeTheme = isDarkModeEnabled ? darkTheme : lightTheme;

        foreach (var element in UnityEngine.Object.FindObjectsByType<ThemedElement>(FindObjectsSortMode.None))
        {
            element.ApplyTheme(activeTheme, isDarkModeEnabled);
        }

        foreach (var win in UnityEngine.Object.FindObjectsByType<Window>(FindObjectsSortMode.None))
        {
            win.RefreshTheme();
        }

        Debug.Log($"[Theme] Mode applied: {(isDarkModeEnabled ? "Dark" : "Light")}");
    }
}
