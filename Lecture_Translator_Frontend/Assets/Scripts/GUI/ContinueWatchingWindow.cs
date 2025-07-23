using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContinueWatchingWindow : Window
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private Lecture savedLecture;
    private float savedTime;

    public void Initialize(Lecture lecture, float time)
    {
        savedLecture = lecture;
        savedTime = time;

        string timestamp = FormatTime(savedTime);
        messageText.text = $"Continue watching <b>{lecture.GetName()}</b> at {timestamp}?";

        yesButton.onClick.AddListener(OnYes);
        noButton.onClick.AddListener(OnNo);
    }

    public void OnYes()
    {
        Lecture lecture = SessionStateManager.LoadLastWatchedLecture();
        float timestamp = SessionStateManager.LoadLastWatchedTime();

        LecturePlayerWindow player = WindowManager.CreateWindow(WindowKeys.LecturePlayerKey) as LecturePlayerWindow;
        player.AssignLecture(lecture);
        player.GetPlaybackManager().MoveTo(timestamp);

        Close();
    }

    public void OnNo()
    {
        Close();
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return $"{minutes:D2}:{seconds:D2}";
    }
}
