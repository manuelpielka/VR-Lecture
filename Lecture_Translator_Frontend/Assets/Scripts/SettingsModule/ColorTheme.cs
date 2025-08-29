using UnityEngine;

/// <summary>
/// A ScriptableObject asset that defines a set of colors used by the UI theme system.
/// Each field corresponds to a semantic <see cref="ThemeRole"/> that can be applied 
/// to UI elements via <see cref="ThemedElement"/>.
/// </summary>
[CreateAssetMenu(menuName = "Theme/Color Theme")]
public class ColorTheme : ScriptableObject
{
    /// <summary>
    /// The background color for the main application window or root panels.
    /// </summary>
    public Color WindowBackground;

    /// <summary>
    /// The background color for secondary panels layered on top of a window background.
    /// </summary>
    public Color PanelOnWindowBackground;

    /// <summary>
    /// The fill/background color of buttons.
    /// </summary>
    public Color ButtonBackground;

    /// <summary>
    /// The color used for button icons, figures, or graphical elements.
    /// </summary>
    public Color ButtonFigure;

    /// <summary>
    /// The text color of button labels.
    /// </summary>
    public Color ButtonText;

    /// <summary>
    /// The standard text color for body/content text in the UI.
    /// </summary>
    public Color ContentText;

    /// <summary>
    /// The text color used for titles or headers.
    /// </summary>
    public Color TitleText;
}
