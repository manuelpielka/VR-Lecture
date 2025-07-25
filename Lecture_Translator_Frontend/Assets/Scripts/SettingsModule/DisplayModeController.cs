using UnityEngine;
using TMPro; 
using UnityEngine.UI;

/// <summary>
/// A class that handles automatic and manual switching between light and dark display modes
/// based on the system time and user preferences.
/// </summary>
public class DisplayModeController : MonoBehaviour
{
    public delegate void ThemeChanged();
    public static event ThemeChanged OnThemeChanged;
    public static DisplayModeController Instance;
    /// <summary>
    /// Indicates whether the current display mode is dark mode (true) or light mode (false).
    /// </summary>
    private bool isDarkModeEnabled;

    /// <summary>
    /// Whether the display mode should update automatically based on system time.
    /// </summary>
    private bool autoAdjust;

    // Timer used to refresh the mode periodically in auto mode
    private float refreshTimer = 0f;

    // How often to check system time in seconds (e.g., every 60 seconds)
    private float refreshInterval = 60f; // Refresh every 60 seconds

    /// <summary>
    /// Called before Start(). Loads saved preferences for mode and auto adjust.
    /// </summary>
    private void Awake()
    {
        Instance = this;
        // Load user preferences from storage
        isDarkModeEnabled = UserPreferencesManager.LoadDarkMode();
        autoAdjust = UserPreferencesManager.LoadAutoAdjust();
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
    /// </summary>
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
            ApplyMode();
            UserPreferencesManager.SaveDarkMode(enabled);
        }
    }

    /// <summary>
    /// Toggles dark mode manually. Only works when autoAdjust is false.
    /// </summary>
    public void ToggleMode()
    {
        if (!autoAdjust)
        {
            isDarkModeEnabled = !isDarkModeEnabled;
            ApplyMode();
            UserPreferencesManager.SaveDarkMode(isDarkModeEnabled);
        }
    }

    /// <summary>
    /// Updates the display mode based on the current system time.
    /// Only works when autoAdjust is enabled.
    /// </summary>
    public void UpdateMode()
    {
        if (autoAdjust)
        {
            int hour = System.DateTime.Now.Hour;
            isDarkModeEnabled = (hour >= 18 || hour < 6); // Dark mode from 6pm to 6am magic number
            ApplyMode();
        }
        else
        {
            ApplyMode(); // Apply manually saved preference
        }
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
        UserPreferencesManager.SaveAutoAdjust(enabled);
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
    private void ApplyMode()
    {
        // Set camera background color
        //if (Camera.main != null)
        //{
        // Camera.main.backgroundColor = isDarkModeEnabled ? Color.black : Color.white;
        //}

        // Update all TMP texts
        //TextMeshProUGUI[] allTextElements = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        //Color newTextColor = isDarkModeEnabled ? Color.white : Color.black;

        //foreach (TextMeshProUGUI textElement in allTextElements)
        //{
        //textElement.color = newTextColor;
        //}

        // Also update legacy UnityEngine.UI.Text if any
        //Text[] legacyTexts = Object.FindObjectsByType<Text>(FindObjectsSortMode.None);
        //foreach (Text t in legacyTexts)
        //{
        //t.color = newTextColor;
        //}

        //Image[] allImages = Object.FindObjectsByType<Image>(FindObjectsSortMode.None);
        //Color backgroundColor = isDarkModeEnabled ? Color.black : Color.white;

        //foreach (Image img in allImages)
        //{
        //img.color = backgroundColor;
        //}

        //Window[] allWindows = Object.FindObjectsByType<Window>(FindObjectsSortMode.None);
        //foreach (var win in allWindows)
        //{
        //win.RefreshTheme();
        //}

        // Optional: Debug log
        // Debug.Log($"Display mode applied: {(isDarkModeEnabled ? "Dark" : "Light")}");
        foreach (var text in Object.FindObjectsByType<ThemeText>(FindObjectsSortMode.None))
        {
            text.ApplyTheme();
        }

        foreach (var bg in Object.FindObjectsByType<ThemeBackground>(FindObjectsSortMode.None))
        {
            bg.ApplyTheme();
        }
        //OnThemeChanged?.Invoke();
        Window[] allWindows = Object.FindObjectsByType<Window>(FindObjectsSortMode.None);
        foreach (var win in allWindows)
        {
            win.RefreshTheme();
        }

        Debug.Log($"[Theme] Mode applied: {(isDarkModeEnabled ? "Dark" : "Light")}");
    }
}
