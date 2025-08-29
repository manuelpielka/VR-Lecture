using UnityEngine;

/// <summary>
/// Interface for UI components or game objects that support theme refreshing.
/// 
/// Classes implementing this interface can respond to theme changes
/// (e.g., switching between light and dark modes) by re-applying
/// their colors, fonts, or other visual settings.
/// 
/// Typically, <see cref="DisplayModeController"/> will look for all
/// <see cref="IThemeRefreshable"/> objects in the scene and call
/// <see cref="RefreshTheme"/> whenever the theme is reapplied.
/// </summary>
public interface IThemeRefreshable
{
    /// <summary>
    /// Called when the global theme changes or needs to be reapplied.
    /// Implement this method to refresh the object's visual appearance
    /// according to the current active <see cref="ColorTheme"/>.
    /// </summary>
    void RefreshTheme();
}