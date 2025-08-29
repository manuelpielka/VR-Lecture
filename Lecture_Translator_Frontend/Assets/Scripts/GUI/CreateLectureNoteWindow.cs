using GUI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateLectureNoteWindow : CreateNoteWindow
{
    [SerializeField] private TextMeshProUGUI windowTitleText;
    [SerializeField] private Toggle useTimestampAsTitleToggle;

    private Lecture boundLecture;
    private string lectureTitleSnapshot = "Lecture";
    private double capturedTimeSeconds;
    private double createdAtSecondsForToggle;
    private NoteManager noteManager;

    public void Initialize(Lecture lecture, double timeSecondsAtOpen, bool isEditMode)
    {
        if (lecture == null)
        {
            InitializeInternal(null, "(Lecture deleted)", timeSecondsAtOpen, 0d, isEditMode);
            return;
        }
        InitializeInternal(lecture, lecture.GetName(), timeSecondsAtOpen, /*createdAtFromNote*/ 0d, isEditMode);
    }

    //public void Initialize(string lectureTitleSnapshot, double createdAtSecondsFromNote, bool isEditMode)
    //{
        //InitializeInternal(/*lecture*/ null, lectureTitleSnapshot, /*timeSecondsAtOpen*/ 0d, createdAtSecondsFromNote, isEditMode);
    //}

    private void InitializeInternal(Lecture lecture, string titleSnapshot, double timeSecondsAtOpen, double createdAtFromNote, bool isEditMode)
    {
        base.Initialize(isEditMode);

        noteManager = FindFirstObjectByType<NoteManager>();
        if (noteManager == null)
        {
            Debug.LogError("NoteManager not found in scene.");
        }

        boundLecture = lecture;
        lectureTitleSnapshot = string.IsNullOrEmpty(titleSnapshot) ? "(Lecture deleted)" : titleSnapshot;

        
        capturedTimeSeconds = Math.Max(0d, timeSecondsAtOpen);
        createdAtSecondsForToggle = createdAtFromNote;

        
        if (windowTitleText != null)
            windowTitleText.text = lectureTitleSnapshot;

       
        if (useTimestampAsTitleToggle != null)
        {
            useTimestampAsTitleToggle.isOn = true;          // true
            useTimestampAsTitleToggle.interactable = true;
            useTimestampAsTitleToggle.onValueChanged.RemoveListener(OnUseTimestampToggleChanged);
            useTimestampAsTitleToggle.onValueChanged.AddListener(OnUseTimestampToggleChanged);
            ApplyUseTimestampToggleToUI(true);
        }
    }

    public new void FillFields(string title, string content)
    {
        if (titleTextBox != null)
        {
            titleTextBox.text = title;
            titleTextBox.interactable = false; // 
        }

        if (noteTextBox != null) noteTextBox.text = content;
    }

    public override void Apply()
    {
        //var enforcedTitle = FormatTimestampForTitle(isEditMode ? createdAtSecondsForToggle : capturedTimeSeconds);
        string content = noteTextBox.text;

        try
        {
            if (isEditMode)
            {

                var noteToEdit = noteManager?.Notes?.Find(n => n.Title == originalTitle && n.LectureTitle == lectureTitleSnapshot);
                if (noteToEdit == null)
                {
                    Debug.LogWarning($"Edit failed: note '{originalTitle}' not found in NoteManager list.");
                    return;
                }
                noteToEdit.Content = content;
                noteManager.SaveNote(noteToEdit);
            }
            else
            {

                var newNote = new Note(titleTextBox.text, content)
                {
                    LectureTitle = lectureTitleSnapshot,          
                    CreatedAtSeconds = capturedTimeSeconds        
                };
                noteManager.AddNote(newNote);
                noteManager.SaveNote(newNote);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Apply failed: {ex.Message}");
            return;
        }


        var maybeNoteWindow = FindFirstObjectByType<NoteWindow>();
        if (maybeNoteWindow != null) maybeNoteWindow.LoadNotes();

        Close();
    }

    public override void Discard()
    {
        Close();
    }

    public void OnUseTimestampToggleChanged(bool isOn)
    {
        ApplyUseTimestampToggleToUI(isOn);
    }

    private void ApplyUseTimestampToggleToUI(bool useTimestamp)
    {
        if (titleTextBox == null) return;

        if (useTimestamp)
        {
            var secs = isEditMode ? createdAtSecondsForToggle : capturedTimeSeconds;
            titleTextBox.text = FormatTimestampForTitle(secs);
            titleTextBox.interactable = false;
        }
        else
        {
            titleTextBox.text = "";
            titleTextBox.interactable = true;
        }
    }

    private static string FormatTimestampForTitle(double seconds)
    {
        if (seconds < 0) seconds = 0;
        int total = Mathf.FloorToInt((float)seconds);
        int hh = total / 3600;
        int mm = (total % 3600) / 60;
        int ss = total % 60;
        return $"{hh:D2}-{mm:D2}-{ss:D2}";
    }

}
