using System;
using UnityEngine;
using TMPro;

/// <summary>
/// This class represents a lecture specific note window.
/// </summary>
public class LectureNoteWindow : NoteWindow
{
    /// <summary>
    /// The lecture linked to this note window.
    /// </summary>
    private Lecture lecture;

    /// <summary>
    /// Reference to the playbackmanager.
    /// </summary>
    private PlaybackManager playbackManager;

    /// <summary>
    /// The text box of the title.
    /// </summary>
    [SerializeField] private TextMeshProUGUI titleTextbox;

    /// <summary>
    /// Initialises the window and sets its values.
    /// </summary>
    /// <param name="lecture"> The lecture linked to this note window. </param>
    /// <param name="playbackManager"> Reference to the playbackmanager. </param>
    public void SetValues(Lecture lecture, PlaybackManager playbackManager)
    {
        this.lecture = lecture;
        this.playbackManager = playbackManager;

        titleTextbox.text = "Notes for Lecture: " + lecture.GetName();

        if (notePrefab == null || noteContainer == null)
            base.Initialize();

        LoadNotes();
    }

    /// <summary>
    /// Button handler for the timestamp title.
    /// </summary>
    /// <param name="note"></param>
    public void TimeStampClicked(NoteGUI note)
    {
        string timestampString = note.titleTextBox.text;

        if (!timestampString.Contains("-")) return;

        string[] timestamps = timestampString.Split("-");

        if (timestamps.Length != 3) return;

        try
        {
            int hh = int.Parse(timestamps[0]);
            int mm = int.Parse(timestamps[1]);
            int ss = int.Parse(timestamps[2]);

            int seconds = hh * 3600 + mm * 60 + ss;

            playbackManager.MoveTo(seconds);
        }
        catch(FormatException)
        {
            return;
        }
    }

    /// <summary>
    /// Button handler for the edit button of a specific note that opens the CreateLectureNoteWindow to be able to edit the note.
    /// </summary>
    /// <param name="noteUI ">The noteUI element to edit</param>
    public override void EditNote(NoteGUI noteUI)
    {
        string originalTitle = noteUI.titleTextBox.text;
        Note targetNote = noteManager.LoadNoteByTitleInLecture(lecture.GetName(), originalTitle);
        if (NoteUtils.IsNull(targetNote, "Edit failed: target note is null.")) return;

        Window window = WindowManager.OpenWindow(WindowKeys.CreateLectureNoteKey);
        CreateLectureNoteWindow createWindow = window as CreateLectureNoteWindow;
        if (createWindow != null)
        {
            createWindow.noteWindow = this;
            createWindow.Initialize(lecture, targetNote.CreatedAtSeconds, true);
            createWindow.FillFields(targetNote.Title, targetNote.Content);
            createWindow.SetOriginalTitle(targetNote.Title);
        }
    }

    /// <summary>
    /// Button handler for the delete button of a specific note that deletes that note.
    /// </summary>
    /// <param name="noteUI">THe NoteUI element to delete.</param>
    public override void DeleteNote(NoteGUI noteUI)
    {
        string title = noteUI.titleTextBox.text;
        noteManager.DeleteNoteByTitleInLecture(lecture.GetName(), title);

        Notes.Remove(noteUI);
        Destroy(noteUI.gameObject);
    }

    /// <summary>
    /// Button handler for the create note button that opens the CreateNoteWindow.
    /// </summary>
    public override void CreateNote()
    {
        Window window = WindowManager.OpenWindow(WindowKeys.CreateLectureNoteKey);
        CreateLectureNoteWindow createWindow = window as CreateLectureNoteWindow;
        if (createWindow != null)
        {
            createWindow.noteWindow = this;
            createWindow.Initialize(lecture, playbackManager.GetCurrentTime(), false);
        }
    }

    /// <summary>
    /// This method loads all saved notes, creates NoteGUI elements and adds them to the notes list.
    /// </summary>
    public override void LoadNotes()
    {
        if (notePrefab == null) { Debug.LogError("notePrefab is not assigned in the Inspector!"); return; }
        if (noteContainer == null) { Debug.LogError("notesContainer is not assigned in the Inspector!"); return; }

        if (lecture == null)
        {
            Debug.LogWarning("LectureNoteWindow: lecture is null (SetValues not called yet). Skip LoadNotes this time.");
            return;
        }

        foreach (var noteGUI in Notes)
        {
            if (noteGUI != null && noteGUI.gameObject != null)
            {
                Destroy(noteGUI.gameObject);
            }
        }
        Notes.Clear();

        noteManager.LoadAllNotes();

        var notes = noteManager.GetNotesForLecture(lecture.GetName());

        foreach (var note in notes)
        {
            GameObject noteGO = Instantiate(notePrefab, noteContainer);
            NoteGUI noteUI = noteGO.GetComponent<NoteGUI>();

            if (noteUI == null)
            {
                Debug.LogError("Instantiated note prefab is missing NoteGUI component.");
                continue;
            }

            noteUI.titleTextBox.text = note.Title;
            noteUI.noteTextBox.text = note.Content;
            noteUI.Initialize(this);

            Notes.Add(noteUI);
        }
    }
}
