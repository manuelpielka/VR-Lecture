using System.Collections.Generic;
using UnityEngine;

public class SubtitleManager : MonoBehaviour
{
    private const string DEFAULT_LANGUAGE = "Multilingual";
    [SerializeField] private PlaybackManager playbackManager;
    [SerializeField] private TranscriptManager transcriptManager;
    private string currentLanguage = DEFAULT_LANGUAGE;

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
            //no idea why this happens initially, but it's not intended. so we overwrite it
            if (currentLanguage == null) currentLanguage = DEFAULT_LANGUAGE;
            currentLine = transcriptManager.GetTranscript(currentLanguage).getLine((float)playbackManager.GetCurrentTime());
        }
        catch (KeyNotFoundException)
        {
            //Most likely the Subtitles haven't loaded yet (or simply do not exist for this lecture)
            //Not really a problem when it happens, so we just log it
            //Debug.Log($"Tried to load nonexistent Line: {currentLanguage}:{playbackManager.GetCurrentTime()}");

            //Sometimes our default language will not be available, causing this exception
            //We just choose the first available language in this case
            List<string> availableLanguges = transcriptManager.GetAvailableLanguages();
            if (!availableLanguges.Contains(currentLanguage))
            {
                Debug.Log("Language automatically changed to: " + availableLanguges[0]);
                currentLanguage = availableLanguges[0];
            }
        }

        transcriptManager.GetTranscript(currentLanguage);

        return currentLine;
    }

    public void SetLanguage(string language)
    {
        currentLanguage = language;
    }
}
