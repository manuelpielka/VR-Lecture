using UnityEngine;

/// <summary>
/// A class that manages the user's playback preferences such as speed,
/// subtitle font size, and last-watched position.
/// Used to persist and restore session settings for lectures.
/// </summary>
public class PlaybackSettingsManager : MonoBehaviour
{
    // The speed multiplier used during playback (e.g., 1.0 for normal, 1.5 for faster playback)
    private float playbackSpeed;

    // Font size for subtitle display, adjustable for accessibility
    private int subtitleFontSize;

    // Timestamp (in seconds) of where the user last left off in the lecture
    private float lastWatchedTime;

    // ID of the last watched lecture (used to resume playback)
    private string lastLectureId;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackSettingsManager"/> class 
    /// with default values for playback speed, subtitle font size, last watched time, and lecture ID.
    /// </summary>
    public PlaybackSettingsManager()
    {
        // Default values 
        // TODO: magic numbers!!!!!!!!!!!
        playbackSpeed = 1.0f;
        subtitleFontSize = 16;
        lastWatchedTime = 0.0f;
        lastLectureId = "";
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
    
    /// <summary>
    /// Gets the timestamp of the last watched position in the lecture.
    /// </summary>
    /// <returns>A float representing the time (in seconds) where the user left off.</returns>
    public float GetLastWatchedTime()
    {
        return lastWatchedTime;
    }

    /// <summary>
    /// Sets a new timestamp for the last watched position in the lecture.
    /// </summary>
    /// <param name="time">The time in seconds where the user left off.</param>
    public void SetLastWatchedTime(float time)
    {
        lastWatchedTime = time;
    }

    /// <summary>
    /// Gets the ID of the last watched lecture.
    /// </summary>
    /// <returns>A string containing the lecture ID.</returns>
    public string GetLastLectureId()
    {
        return lastLectureId;
    }

    /// <summary>
    /// Sets the ID of the last watched lecture.
    /// </summary>
    /// <param name="lectureId">A string representing the unique lecture ID.</param>
    public void SetLastLectureId(string lectureId)
    {
        lastLectureId = lectureId;
    }




    // TODOOOOOOO!!!!!
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
