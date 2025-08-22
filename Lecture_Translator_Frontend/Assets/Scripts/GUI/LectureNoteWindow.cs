using System;
using UnityEngine;
using TMPro;

public class LectureNoteWindow : NoteWindow
{
    private Lecture lecture;

    private PlaybackManager playbackManager;

    [SerializeField] private TextMeshProUGUI titleTextbox;


    public void SetValues(Lecture lecture, PlaybackManager playbackManager)
    {
        this.lecture = lecture;
        this.playbackManager = playbackManager;

        titleTextbox.text = "Notes for Lecture: " + lecture.GetName();
    }

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
    public new void EditNote(NoteGUI noteUI)
    {
        string originalTitle = noteUI.titleTextBox.text;
        Note targetNote = noteManager.LoadNote(originalTitle);
        if (NoteUtils.IsNull(targetNote, "Edit failed: target note is null.")) return;

        Window window = WindowManager.OpenWindow(WindowKeys.CreateLectureNoteKey);
        CreateLectureNoteWindow createWindow = window as CreateLectureNoteWindow;
        if (createWindow != null)
        {
            createWindow.noteWindow = this;
            createWindow.Initialize(lecture, playbackManager.GetCurrentTime(), true);
            createWindow.FillFields(targetNote.Title, targetNote.Content);
            createWindow.SetOriginalTitle(targetNote.Title);
        }
    }

    /// <summary>
    /// Button handler for the create note button that opens the CreateNoteWindow.
    /// </summary>
    public new void CreateNote()
    {
        Window window = WindowManager.OpenWindow(WindowKeys.CreateLectureNoteKey);
        CreateLectureNoteWindow createWindow = window as CreateLectureNoteWindow;
        if (createWindow != null)
        {
            createWindow.noteWindow = this;
            createWindow.Initialize(lecture, playbackManager.GetCurrentTime(), false);
        }
    }
}
