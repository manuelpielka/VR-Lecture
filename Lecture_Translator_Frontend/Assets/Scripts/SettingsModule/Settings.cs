using UnityEngine;

/// <summary>
/// An enumeration representing all configurable settings available within the application.
/// </summary>
public enum Settings
{
    // Playback speed of the lecture video (float, e.g., 1.0, 1.5)
    PLAYBACK_SPEED,
    // Font size for subtitles (int)
    SUBTITLE_FONT_SIZE,

    // Timestamp of the last watched position in the lecture (float, in seconds)
    LAST_WATCHED_TIME,

    // The ID of the last watched lecture (string)
    LAST_LECTURE_ID,

    // Application display mode (e.g., dark/light)
    DISPLAY_MODE,

    // Whether to automatically adjust display mode based on time
    AUTO_ADJUST
}
