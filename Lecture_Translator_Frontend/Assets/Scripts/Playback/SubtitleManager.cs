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
        string currentLine = transcriptManager.GetTranscript(currentLanguage).getLine((float)playbackManager.GetCurrentTime());
        lecturePlayerWindow.SetSubtitleText(currentLine);
    }

    public void SetLanguage(string language)
    {
        currentLanguage = language;
    }
}
