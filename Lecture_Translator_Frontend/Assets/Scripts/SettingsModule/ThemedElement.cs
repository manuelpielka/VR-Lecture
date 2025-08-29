using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A UI component that automatically applies a color from the active <see cref="ColorTheme"/> 
/// based on its assigned <see cref="ThemeRole"/>.
/// Attach this script to any UI element (Text, Image, RawImage, etc.) that should respond to theme changes.
/// </summary>
public class ThemedElement : MonoBehaviour
{
    /// <summary>
    /// Defines which semantic role this element represents (e.g., window background, button text).
    /// Used to look up the correct color from the <see cref="ColorTheme"/>.
    /// </summary>
    public ThemeRole role;

    /// <summary>
    /// Applies the appropriate color from the given theme to this UI element.  
    /// Supports <see cref="TextMeshProUGUI"/>, <see cref="Text"/>, <see cref="Image"/>, and <see cref="RawImage"/>.
    /// </summary>
    /// <param name="theme">The active color theme.</param>
    /// <param name="isDarkMode">Whether the dark mode is currently active.  
    /// Currently not directly used by this method but can influence future extensions.</param>
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

    /// <summary>
    /// Retrieves the correct color for this element's <see cref="role"/> from the given <see cref="ColorTheme"/>.
    /// </summary>
    /// <param name="theme">The theme to pull colors from.</param>
    /// <returns>The color corresponding to the assigned <see cref="ThemeRole"/>; magenta if undefined.</returns>
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
            case ThemeRole.TitleText: return theme.TitleText;
            default: return Color.magenta; // debug color for unknown role
        }
    }
}