using UnityEngine;
using UnityEngine.UI;

public class ThemeBackground : MonoBehaviour
{
    [SerializeField] private Color lightColor = new Color(129, 129, 129);
    [SerializeField] private Color darkColor = new Color(0.15f, 0.15f, 0.15f);
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        ApplyTheme();
    }

    public void ApplyTheme()
    {
        if (DisplayModeController.Instance == null) return;
        image.color = DisplayModeController.Instance.IsDarkModeEnabled() ? darkColor : lightColor;
    }
}
