using UnityEngine;

/// <summary>
/// A class that manages the user's playback preferences such as speed,
/// subtitle font size.
/// This class does not store session-specific data like last watched lecture/time.
/// </summary>
public class PlaybackSettingsManager : MonoBehaviour
{
    // The speed multiplier used during playback (e.g., 1.0 for normal, 1.5 for faster playback)
    private float playbackSpeed;

    // Font size for subtitle display, adjustable for accessibility
    private int subtitleFontSize;

    /// <summary>
    /// Initializes default playback settings on first use or fallback.
    /// </summary>
    private void Awake()
    {
        // Load saved preferences or fallback to default
        playbackSpeed = UserPreferencesManager.LoadPlaybackSpeed();  // default = 1.0
        subtitleFontSize = UserPreferencesManager.LoadSubtitleFontSize(); // default = 16
    }

    /// <summary>
    /// Gets the current playback speed setting.
    /// </summary>
    /// <returns>A float value representing the speed multiplier (e.g., 1.0, 1.5).</returns>
    public float GetPlaybackSpeed()
    {
        return playbackSpeed;
    }

    /// <summary>
    /// Sets a new value for the playback speed.
    /// </summary>
    /// <param name="playbackSpeed">The desired speed multiplier (e.g., 1.0 for normal, 1.5 for faster).</param>
    public void SetPlaybackSpeed(float playbackSpeed)
    {
        this.playbackSpeed = playbackSpeed;
    }

    /// <summary>
    /// Gets the current subtitle font size setting.
    /// </summary>
    /// <returns>An integer representing the font size used for subtitles.</returns>
    public int GetSubtitleFontSize()
    {
        return subtitleFontSize;
    }

    /// <summary>
    /// Sets a new subtitle font size value.
    /// </summary>
    /// <param name="subtitleFontSize">The desired font size for subtitle display.</param>
    public void SetSubtitleFontSize(int subtitleFontSize)
    {
        this.subtitleFontSize = subtitleFontSize;
    }
}
