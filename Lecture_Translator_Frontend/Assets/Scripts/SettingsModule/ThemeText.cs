using TMPro;
using UnityEngine;

/// <summary>
/// Applies appropriate text color theme (light or dark mode) to a TextMeshProUGUI component.
/// Listens to theme change events and updates accordingly.
/// </summary>
public class ThemeText : MonoBehaviour
{ 
    [SerializeField] private Color lightModeColor = Color.black;
    [SerializeField] private Color darkModeColor = Color.white;

    private TextMeshProUGUI text;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Initializes reference and applies current theme.
    /// </summary>
    void Awake()
    {
        Debug.Log("[ThemeText] Awake called on " + gameObject.name);
        text = GetComponent<TextMeshProUGUI>();
        if (text == null)
        {
            Debug.LogWarning($"[ThemeText] Missing TextMeshProUGUI component on {gameObject.name}");
            return;
        }

        ApplyTheme();
    }

    public void ApplyTheme()
    {
        //if (DisplayModeController.Instance == null) return;

        //text.color = DisplayModeController.Instance.IsDarkModeEnabled() ? darkModeColor : lightModeColor;
    }

    private void OnEnable()
    {
        //DisplayModeController.OnThemeChanged += ApplyTheme;
    }

    private void OnDisable()
    {
        //DisplayModeController.OnThemeChanged -= ApplyTheme;
    }
}