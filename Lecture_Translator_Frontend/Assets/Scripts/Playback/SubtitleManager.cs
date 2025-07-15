using UnityEngine;

public class SubtitleManager : MonoBehaviour
{
    [SerializeField] private PlaybackManager playbackManager;
    [SerializeField] private TranscriptManager transcriptManager;

    //doesn't exist yet
    //LecturePlayerWindow lecturePlayerWindow;

    private string currentLanguage;

    // Update is called once per frame
    void Update()
    {
        string currentLine = transcriptManager.GetTranscript(currentLanguage).getLine((int)playbackManager.GetCurrentTime());
        //lecturePlayerWindow
    }

    public void SetLanguage(string language)
    {
        currentLanguage = language;
    }
}
