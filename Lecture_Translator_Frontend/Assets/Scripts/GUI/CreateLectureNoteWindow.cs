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

    public void Initialize(string lectureTitleSnapshot, double createdAtSecondsFromNote, bool isEditMode)
    {
        InitializeInternal(/*lecture*/ null, lectureTitleSnapshot, /*timeSecondsAtOpen*/ 0d, createdAtSecondsFromNote, isEditMode);
    }

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

        
        capturedTimeSeconds = timeSecondsAtOpen;
        createdAtSecondsForToggle = createdAtFromNote;

        
        if (windowTitleText != null)
            windowTitleText.text = lectureTitleSnapshot;

       
        if (useTimestampAsTitleToggle != null)
        {
            useTimestampAsTitleToggle.onValueChanged.RemoveListener(OnUseTimestampToggleChanged);
            useTimestampAsTitleToggle.onValueChanged.AddListener(OnUseTimestampToggleChanged);
            ApplyUseTimestampToggleToUI(useTimestampAsTitleToggle.isOn);
        }
    }

    public new void FillFields(string title, string content)
    {
        base.FillFields(title, content);
        
        if (useTimestampAsTitleToggle != null)
        {
            ApplyUseTimestampToggleToUI(useTimestampAsTitleToggle.isOn);
        }
    }

    public override void Apply()
    {
        string title = titleTextBox.text;
        string content = noteTextBox.text;

        if (string.IsNullOrWhiteSpace(title))
        {
            Debug.LogWarning("Note title cannot be empty!");
            return;
        }

        try
        {
            if (isEditMode)
            {

                var noteToEdit = noteManager?.Notes?.Find(n => n.Title == originalTitle);
                if (noteToEdit == null)
                {
                    Debug.LogWarning($"Edit failed: note '{originalTitle}' not found in NoteManager list.");
                    return;
                }
                noteManager.EditNote(noteToEdit, title, content);
            }
            else
            {

                var newNote = new Note(title, content)
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

    private void OnUseTimestampToggleChanged(bool isOn)
    {
        ApplyUseTimestampToggleToUI(isOn);
    }

    private void ApplyUseTimestampToggleToUI(bool useTimestamp)
    {
        if (titleTextBox == null) return;

        if (useTimestamp)
        {

            double seconds = isEditMode ? createdAtSecondsForToggle : capturedTimeSeconds;
            titleTextBox.text = FormatTimestampForTitle(seconds);
            titleTextBox.interactable = false; 
        }
        else
        {
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
