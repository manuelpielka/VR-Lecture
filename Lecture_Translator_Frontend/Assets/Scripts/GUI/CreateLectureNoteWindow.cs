using GUI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A specialized note creation window that is bound to a <see cref="Lecture"/>.
/// 
/// This class extends <see cref="CreateNoteWindow"/> and provides functionality
/// for creating and editing lecture-specific notes. The title can either be
/// auto-generated from the playback timestamp or manually edited, depending
/// on the toggle state.
/// </summary>
public class CreateLectureNoteWindow : CreateNoteWindow
{
    [SerializeField] private TextMeshProUGUI windowTitleText;
    [SerializeField] private Toggle useTimestampAsTitleToggle;

    private Lecture boundLecture;
    private string lectureTitleSnapshot = "Lecture";
    private double capturedTimeSeconds;
    private double createdAtSecondsForToggle;
    private NoteManager noteManager;

    /// <summary>
    /// Initializes this window for a lecture note.
    /// </summary>
    /// <param name="lecture">The lecture associated with the note. If null, "(Lecture deleted)" will be shown.</param>
    /// <param name="timeSecondsAtOpen">The timestamp (in seconds) at which the window was opened.</param>
    /// <param name="isEditMode">True if editing an existing note; false if creating a new one.</param>
    public void Initialize(Lecture lecture, double timeSecondsAtOpen, bool isEditMode)
    {
        if (lecture == null)
        {
            InitializeInternal(null, "(Lecture deleted)", timeSecondsAtOpen, 0d, isEditMode);
            return;
        }
        InitializeInternal(lecture, lecture.GetName(), timeSecondsAtOpen, /*createdAtFromNote*/ 0d, isEditMode);
    }

    /// <summary>
    /// Internal helper for initialization logic, sets up the lecture binding, 
    /// timestamps, UI state, and connects toggle listeners.
    /// </summary>
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

    /// <summary>
    /// Fills the UI fields with a given note title and content.
    /// Used when editing or displaying an existing note.
    /// </summary>
    public new void FillFields(string title, string content)
    {
        if (titleTextBox != null)
        {
            titleTextBox.text = title;
            titleTextBox.interactable = false;
        }

        if (noteTextBox != null) noteTextBox.text = content;
    }

    /// <summary>
    /// Applies the current note by either creating a new note 
    /// or updating an existing one via <see cref="NoteManager"/>.
    /// </summary>
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

    /// <summary>
    /// Discards the note (ignores changes) and closes the window.
    /// </summary>

    public override void Discard()
    {
        Close();
    }

    /// <summary>
    /// Callback for the "use timestamp as title" toggle.
    /// When enabled, automatically sets the title to the playback timestamp.
    /// </summary>
    public void OnUseTimestampToggleChanged(bool isOn)
    {
        ApplyUseTimestampToggleToUI(isOn);
    }

    /// <summary>
    /// Updates the title input field based on whether the timestamp should be used.
    /// </summary>
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
            titleTextBox.interactable = true;
        }
    }

    /// <summary>
    /// Converts a number of seconds into an HH-MM-SS formatted string.
    /// Used for generating human-readable timestamps as note titles.
    /// </summary>
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
