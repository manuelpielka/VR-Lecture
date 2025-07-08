using UnityEngine;

public class SubtitleManager : MonoBehaviour
{
    public PlaybackManager playbackManager;
    public TranscriptManager transcriptManager;

    //doesn't exist yet
    //LecturePlayerWindow lecturePlayerWindow;

    private string currentLanguage;

    // Update is called once per frame
    void Update()
    {
        string currentLine = transcriptManager.GetTranscript(currentLanguage).getLine(playbackManager.GetCurrentTime());
        //lecturePlayerWindow
    }

    public void SetLanguage(string language)
    {
        currentLanguage = language;
    }
}
