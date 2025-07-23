using UnityEngine;

/// <summary>
/// A class that manages the user's playback preferences such as speed,
/// subtitle font size.
/// This class does not store session-specific data like last watched lecture/time.
/// </summary>
public class PlaybackSettingsManager : MonoBehaviour
{
    private LecturePlayerWindow lecturePlayerWindow;

    [SerializeField] private PlaybackManager playbackManager;
    // The speed multiplier used during playback (e.g., 1.0 for normal, 1.5 for faster playback)
    private float playbackSpeed;

    // Font size for subtitle display, adjustable for accessibility
    private int subtitleFontSize;

    /// <summary>
    /// Initializes default playback settings on first use or fallback.
    /// </summary>
    private void Awake()
    {
        if (playbackManager == null)
        {
            playbackManager = FindFirstObjectByType<PlaybackManager>();
        }
        lecturePlayerWindow = FindFirstObjectByType<LecturePlayerWindow>();

        playbackSpeed = UserPreferencesManager.LoadPlaybackSpeed();
        subtitleFontSize = UserPreferencesManager.LoadSubtitleFontSize();

        playbackManager?.SetPlaybackSpeed(playbackSpeed);
        lecturePlayerWindow?.SetSubtitleFontSize(subtitleFontSize);
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

        if (playbackManager != null)
        {
            playbackManager.SetPlaybackSpeed(playbackSpeed);
        }
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
        lecturePlayerWindow?.SetSubtitleFontSize(subtitleFontSize);
    }
}
