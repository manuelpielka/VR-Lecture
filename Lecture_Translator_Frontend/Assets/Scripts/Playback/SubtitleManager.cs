using System.Collections.Generic;
using UnityEngine;

public class SubtitleManager : MonoBehaviour
{
    [SerializeField] private PlaybackManager playbackManager;
    [SerializeField] private TranscriptManager transcriptManager;
    [SerializeField] private LecturePlayerWindow lecturePlayerWindow;

    private string currentLanguage = "English";

    // Update is called once per frame
    void Update()
    {
        string currentLine = "";
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
        
        lecturePlayerWindow.SetSubtitleText(currentLine);
    }

    public void SetLanguage(string language)
    {
        currentLanguage = language;
    }
}
