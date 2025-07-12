using UnityEngine;

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

    /// <summary>
    /// Constructor that initializes default values for the display mode controller.
    /// </summary>
    public DisplayModeController()
    {
        isDarkModeEnabled = true;
        autoAdjust = true;
        UpdateMode();
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

    /// <summary>
    /// Updates the display mode based on the current system time.
    /// Only works when autoAdjust is enabled.
    /// </summary>
    public void UpdateMode()
    {
        if (autoAdjust)
        {
            int hour = System.DateTime.Now.Hour;
            isDarkModeEnabled = (hour >= 18 || hour < 6); // Dark mode from 6pm to 6am
            ApplyMode();
        }
    }

    /// <summary>
    /// Applies the current display mode setting to the system or application.
    /// (Stub for actual implementation, e.g., switching UI themes)
    /// </summary>
    private void ApplyMode()
    {
        // TODO
        Debug.Log("Display mode applied: " + (isDarkModeEnabled ? "Dark" : "Light"));
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
