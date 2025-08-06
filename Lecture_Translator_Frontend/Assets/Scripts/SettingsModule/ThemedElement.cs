using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThemedElement : MonoBehaviour
{
    public ThemeRole role;

    public void ApplyTheme(ColorTheme theme, bool isDarkMode)
    {
        Color colorToApply = GetColorByRole(theme);

        if (TryGetComponent<TextMeshProUGUI>(out var tmpText))
        {
            tmpText.color = colorToApply;
        }
        else if (TryGetComponent<Text>(out var uiText))
        {
            uiText.color = colorToApply;
        }
        else if (TryGetComponent<Image>(out var img))
        {
            img.color = colorToApply;
        }
        else if (TryGetComponent<RawImage>(out var rawImg))
        {
            rawImg.color = colorToApply;
        }
    }

    private Color GetColorByRole(ColorTheme theme)
    {
        switch (role)
        {
            case ThemeRole.WindowBackground: return theme.WindowBackground;
            case ThemeRole.PanelOnWindowBackground: return theme.PanelOnWindowBackground;
            case ThemeRole.ButtonBackground: return theme.ButtonBackground;
            case ThemeRole.ButtonFigure: return theme.ButtonFigure;
            case ThemeRole.ButtonText: return theme.ButtonText;
            case ThemeRole.ContentText: return theme.ContentText;
            case ThemeRole.DisabledText: return theme.DisabledText;
            default: return Color.magenta; // debug color for unknown role
        }
    }
}