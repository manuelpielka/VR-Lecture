using UnityEngine;

/// <summary>
/// Handles saving and loading the last watched lecture and timestamp.
/// This is session-related and not part of long-term user preferences.
/// </summary>
public static class SessionStateManager
{
    private const string LastWatchedLectureKey = "Session_LastWatchedLecture";
    private const string LastWatchedTimeKey = "Session_LastWatchedTime";

    /// <summary>
    /// Saves the last watched lecture and its timestamp.
    /// Should be called before quitting the app or switching scenes.
    /// </summary>
    public static void SaveSessionState(Lecture lecture, float playbackTime)
    {
        if (lecture != null)
        {
            string json = JsonUtility.ToJson(lecture);
            PlayerPrefs.SetString(LastWatchedLectureKey, json);
            PlayerPrefs.SetFloat(LastWatchedTimeKey, playbackTime);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Loads the last watched lecture object, or null if not found.
    /// </summary>
    public static Lecture LoadLastWatchedLecture()
    {
        string json = PlayerPrefs.GetString(LastWatchedLectureKey, "");
        if (!string.IsNullOrEmpty(json))
        {
            return JsonUtility.FromJson<Lecture>(json);
        }
        return null;
    }

    /// <summary>
    /// Loads the last saved playback time, or returns 0 if not available.
    /// </summary>
    public static float LoadLastWatchedTime()
    {
        return PlayerPrefs.GetFloat(LastWatchedTimeKey, 0f);
    }

    /// <summary>
    /// Checks if a previous session state exists.
    /// </summary>
    public static bool HasPreviousSession()
    {
        return PlayerPrefs.HasKey(LastWatchedLectureKey);
    }

    /// <summary>
    /// Clears the saved session state, e.g., after user declines to resume.
    /// </summary>
    public static void ClearSession()
    {
        PlayerPrefs.DeleteKey(LastWatchedLectureKey);
        PlayerPrefs.DeleteKey(LastWatchedTimeKey);
    }
}
