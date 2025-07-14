using UnityEngine;
using TMPro; 
using UnityEngine.UI;

/// <summary>
/// A class that handles automatic and manual switching between light and dark display modes
/// based on the system time.
/// </summary>
public class DisplayModeController : MonoBehaviour
{
    /// <summary>
    /// Indicates whether the current display mode is dark mode (true) or light mode (false).
    /// </summary>
    private bool isDarkModeEnabled;

    /// <summary>
    /// Whether the display mode should update automatically based on system time.
    /// </summary>
    private bool autoAdjust;

    private float refreshTimer = 0f;
    private float refreshInterval = 60f; // Refresh every 60 seconds

    void Awake()
    {
        // Initialize default values
        isDarkModeEnabled = false;
        autoAdjust = true;
    }

    void Start()
    {
        UpdateMode();
    }

    void Update()
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
    /// </summary>
    /// <param name="enabled">True to enable dark mode, false for light mode.</param>
    public void SetDarkMode(bool enabled)
    {
        if (!autoAdjust)
        {
            isDarkModeEnabled = enabled;
            ApplyMode();
        }
    }

    public void ToggleMode()
    {
        if (!autoAdjust)
        {
            isDarkModeEnabled = !isDarkModeEnabled;
            ApplyMode();
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
    }
    
    /// <summary>
    /// Manually triggers a mode refresh based on current time.
    /// Useful for UI buttons to apply current time-based theme.
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
        if (autoAdjust)
        {
            UpdateMode();
        }
    }

    /// <summary>
    /// Returns true if automatic adjustment is enabled; otherwise false.
    /// </summary>
    public bool IsAutoAdjustEnabled()
    {
        return autoAdjust;
    }

    /// <summary>
    /// Applies the current display mode setting to the system or application.
    /// (Stub for actual implementation, e.g., switching UI themes)
    /// </summary>
    private void ApplyMode()
    {
        if (Camera.main != null)
        {
            Camera.main.backgroundColor = isDarkModeEnabled ? Color.black : Color.white;
        }


        TextMeshProUGUI[] allTextElements = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        Color newTextColor = isDarkModeEnabled ? Color.white : Color.black;

        foreach (TextMeshProUGUI textElement in allTextElements)
        {
            textElement.color = newTextColor;
        }

        Text[] legacyTexts = Object.FindObjectsByType<Text>(FindObjectsSortMode.None);
        foreach (Text t in legacyTexts)
        {
            t.color = newTextColor;
        }


        // TODO
        // Debug.Log("Display mode applied: " + (isDarkModeEnabled ? "Dark" : "Light"));
    }
}
