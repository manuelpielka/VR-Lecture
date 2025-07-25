using TMPro;
using UnityEngine;

public class ThemeText : MonoBehaviour
{
    [SerializeField] private Color lightModeColor = Color.black;
    [SerializeField] private Color darkModeColor = Color.white;

    private TextMeshProUGUI text;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        ApplyTheme();
    }

    public void ApplyTheme()
    {
        if (DisplayModeController.Instance == null) return;

        text.color = DisplayModeController.Instance.IsDarkModeEnabled() ? darkModeColor : lightModeColor;
    }

    private void OnEnable()
    {
        DisplayModeController.OnThemeChanged += ApplyTheme;
    }

    private void OnDisable()
    {
        DisplayModeController.OnThemeChanged -= ApplyTheme;
    }
}