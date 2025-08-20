using System.Collections.Generic;
using UnityEngine;

public class SubtitleManager : MonoBehaviour
{
    [SerializeField] private PlaybackManager playbackManager;
    [SerializeField] private TranscriptManager transcriptManager;

    private string currentLanguage = "English";

    //this constructor exists purley for testing and is not used in the actual code
    public SubtitleManager(PlaybackManager playbackManager, TranscriptManager transcriptManager)
    {
        this.playbackManager = playbackManager;
        this.transcriptManager = transcriptManager;
    }

    public string getCurrentLine()
    {
        string currentLine = "Loading...";
        try
        {
            currentLine = transcriptManager.GetTranscript(currentLanguage).getLine((float)playbackManager.GetCurrentTime());
        }
        catch (KeyNotFoundException)
        {
            //Most likely the Subtitles haven't loaded yet (or simply do not exist for this lecture)
            //Not really a problem when it happens, so we just log it
            Debug.Log($"Tried to load nonexistent Line: {currentLanguage}:{playbackManager.GetCurrentTime()}");
        }

        return currentLine;
    }

    public void SetLanguage(string language)
    {
        currentLanguage = language;
    }
}
