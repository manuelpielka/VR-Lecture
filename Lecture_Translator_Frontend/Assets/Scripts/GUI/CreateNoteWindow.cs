using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Class <c>CreateNoteWindow</c> is used to create and edit notes.
/// </summary>
public class CreateNoteWindow : Window
{
    /// <summary>
    /// The text box of the title of the note.
    /// </summary>
    [SerializeField]
    protected TMP_InputField titleTextBox;

    /// <summary>
    /// The text box of the note’s contents.
    /// </summary>
    [SerializeField]
    protected TMP_InputField noteTextBox;

    /// <summary>
    /// Reference to the NoteWindow in order to save a note.
    /// </summary>
    public NoteWindow NoteWindow { get; set; }

    protected bool isEditMode;

    /// <summary>
    /// Called when the user confirms the note (by pressing the apply button).
    /// Passes the title and content of the note.
    /// The first parameter is the note title, and the second is the note content.
    /// </summary>
    public Action<string, string> OnNoteConfirmed;


    /// <summary>
    /// Button handler for the apply button that saves the inputted title and content as a new note or as an edit to an existing note.
    /// </summary>
    public void Apply()
    {
        string title = titleTextBox.text;
        string content = noteTextBox.text;

        if (string.IsNullOrWhiteSpace(title))
        {
            Debug.LogWarning("Note title cannot be empty!");
            return;
        }

        if (isEditMode)
        {
            NoteWindow.SaveEditedNote(title, title, content);
        }
        else
        {
            NoteWindow.SaveNewNote(title, content);
        }

        WindowManager.CloseWindow(this);
        NoteWindow.LoadNotes();

        Close();
    }

    /// <summary>
    /// Button handler for the discard button that closes the window without saving changes.
    /// </summary>
    public void Discard()
    {
        Close();
    }

    /// <summary>
    /// Initializes the CreateNoteWindow with the appropriate mode and resets fields if necessary.
    /// </summary>
    /// <param name="isEditMode">If true, window is in edit mode; otherwise, it's in create mode.</param>
    public void Initialize(bool isEditMode)
    {
        this.isEditMode = isEditMode;

        if (!isEditMode)
        {
            titleTextBox.text = "";
            noteTextBox.text = "";
        }
    }

    /// <summary>
    /// Fills the title and content fields with the given values (used when editing).
    /// </summary>
    /// <param name="title">The title of the note.</param>
    /// <param name="content">The content of the note.</param>
    public void FillFields(string title, string content)
    {
        titleTextBox.text = title;
        noteTextBox.text = content;
    }
}
