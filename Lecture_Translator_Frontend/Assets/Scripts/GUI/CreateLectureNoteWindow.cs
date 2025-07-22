using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateLectureNoteWindow : CreateNoteWindow
{
    [SerializeField] private TextMeshProUGUI windowTitleText;
    [SerializeField] private Toggle CurrentTimeAsTitleToggle;
    private Lecture currentLecture;
    private LecturePlayerWindow lecturePlayerWindow;

    public void Initialize(LecturePlayerWindow playerWindow, bool editMode, Note noteToEdit = null)
    {
        this.lecturePlayerWindow = playerWindow;
        this.currentLecture = playerWindow?.GetLecture();
        this.isEditMode = editMode;

        if (currentLecture != null)
        {
            windowTitleText.text = $"Note for: {currentLecture.GetName()}";
        }
        else
        {
            windowTitleText.text = "Note";
        }

        base.Initialize(editMode, noteToEdit);
    }

    protected new void Apply()
    {
        string title = (!isEditMode && CurrentTimeAsTitleToggle.isOn)
            ? FormatTimeAsString(lecturePlayerWindow.GetPlaybackManager().GetCurrentTime())
            : titleTextBox.text;

        string content = noteTextBox.text;

        OnNoteConfirmed?.Invoke(title, content);
        Close();
    }

    private string FormatTimeAsString(double time)
    {
        int h = (int)(time / 3600);
        int m = (int)((time % 3600) / 60);
        int s = (int)(time % 60);
        return $"{h:D2}:{m:D2}:{s:D2}";
    }
}
