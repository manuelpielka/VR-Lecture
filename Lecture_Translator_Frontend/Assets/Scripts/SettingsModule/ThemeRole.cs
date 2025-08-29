using UnityEngine;

/// <summary>
/// Defines semantic roles for themed UI elements.  
/// Each role represents a specific part of the interface that can be styled 
/// differently depending on the active theme (e.g., light or dark).
/// </summary>
public enum ThemeRole
{
    /// <summary>
    /// The background of the main application window.
    /// </summary>
    WindowBackground,

    /// <summary>
    /// The background of panels that appear on top of the main window.
    /// </summary>
    PanelOnWindowBackground,

    /// <summary>
    /// The fill background of standard buttons.
    /// </summary>
    ButtonBackground,

    /// <summary>
    /// The icon, figure, or graphical part inside a button.
    /// </summary>
    ButtonFigure,

    /// <summary>
    /// The text label displayed on a button.
    /// </summary>
    ButtonText,

    /// <summary>
    /// Generic content text (e.g., body text, labels, descriptions).
    /// </summary>
    ContentText,

    /// <summary>
    /// Large title or heading text used in windows or panels.
    /// </summary>
    TitleText
}
