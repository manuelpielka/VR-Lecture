using GUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateLectureNoteWindow : CreateNoteWindow
{
    [SerializeField] private TextMeshProUGUI windowTitleText;
    //[SerializeField] private TMP_InputField titleTextBox;
    //[SerializeField] private TMP_InputField noteTextBox;
    [SerializeField] private Toggle CurrentTimeAsTitleToggle;
    [SerializeField] private Button discardButton;
    [SerializeField] private Button applyButton;
    private Lecture currentLecture;
    private LecturePlayerWindow lecturePlayerWindow;
    //private string originalTitle;
    //private bool isEditMode = false;

    public void Initialize(LecturePlayerWindow playerWindow, bool editMode, Note noteToEdit = null)
    {
        this.lecturePlayerWindow = playerWindow;
        this.currentLecture = playerWindow?.GetLecture();
        this.isEditMode = editMode;

        // Set window title
        windowTitleText.text = $"Note for: {currentLecture.GetName()}";

        if (isEditMode && noteToEdit != null)
        {
            FillFields(noteToEdit.Title, noteToEdit.Content);
            originalTitle = noteToEdit.Title;
        }
        else
        {
            FillFields("", "");
        }

        CurrentTimeAsTitleToggle.onValueChanged.RemoveAllListeners();
        CurrentTimeAsTitleToggle.isOn = false;
        CurrentTimeAsTitleToggle.interactable = !isEditMode;
        titleTextBox.interactable = true;


        if (!isEditMode)
        {
            CurrentTimeAsTitleToggle.onValueChanged.AddListener(OnToggleTimestampAsTitle);
        }

        // Fill input fields
        //titleTextBox.text = noteToEdit?.Title ?? "";
        //noteTextBox.text = noteToEdit?.Content ?? "";


        // Discard button
        discardButton.onClick.RemoveAllListeners();
        discardButton.onClick.AddListener(Close);

        // Apply button
        applyButton.onClick.RemoveAllListeners();
        applyButton.onClick.AddListener(Apply);
    }

    private void OnToggleTimestampAsTitle(bool isOn)
    {
        if (isOn)
        {
            double currentTime = lecturePlayerWindow.GetPlaybackManager().GetCurrentTime();
            titleTextBox.text = FormatTimeAsString(currentTime);
            titleTextBox.interactable = false;
        }
        else
        {
            titleTextBox.interactable = true;
        }
    }


    public override void Apply()
    {
        string title = (!isEditMode && CurrentTimeAsTitleToggle.isOn)
            ? FormatTimeAsString(lecturePlayerWindow.GetPlaybackManager().GetCurrentTime())
            : titleTextBox.text;

        string content = noteTextBox.text;

        if (string.IsNullOrWhiteSpace(title))
        {
            Debug.LogWarning("Note title cannot be empty.");
            return;
        }

        //OnNoteConfirmed?.Invoke(title, content);
        Close();
    }

    private string FormatTimeAsString(double time)
    {
        int h = (int)(time / 3600);
        int m = (int)((time % 3600) / 60);
        int s = (int)(time % 60);
        return $"{h:D2}-{m:D2}-{s:D2}";
    }
}
