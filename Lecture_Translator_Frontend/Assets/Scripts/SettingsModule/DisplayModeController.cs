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
        isDarkModeEnabled = false;
        autoAdjust = false;
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
