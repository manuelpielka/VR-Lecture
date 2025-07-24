using TMPro;
using UnityEngine;

public class ThemeText : MonoBehaviour
{
    [SerializeField] private Color lightColor = Color.white;
    [SerializeField] private Color darkColor = Color.black;

    private TextMeshProUGUI text;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        ApplyTheme();
    }

    public void ApplyTheme()
    {
        if (DisplayModeController.Instance == null) return;

        text.color = DisplayModeController.Instance.IsDarkModeEnabled() ? darkColor : lightColor;
    }
}