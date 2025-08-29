using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Class <c>CreateNoteWindow</c> is used to create and edit notes.
/// </summary>
public class CreateNoteWindow : Window
{
    /// <summary>
    /// Fallback empty string for resetting input fields.
    /// </summary>
    private const string EMPTY_STRING = "";

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
    public NoteWindow noteWindow { get; set; }

    /// <summary>
    /// Flag indicating whether the window is in edit mode.
    /// </summary>
    protected bool isEditMode;

    /// <summary>
    /// Stores the original title of the note (used for editing and saving changes).
    /// </summary>
    protected string originalTitle;


    /// <summary>
    /// Button handler for the apply button that saves the inputted title and content as a new note or as an edit to an existing note.
    /// </summary>
    public virtual void Apply()
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
            noteWindow.SaveEditedNote(originalTitle, title, content);
        }
        else
        {
            noteWindow.SaveNewNote(title, content);
        }

        noteWindow.LoadNotes();

        Close();
    }

    /// <summary>
    /// Button handler for the discard button that closes the window without saving changes.
    /// </summary>
    public virtual void Discard()
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
            titleTextBox.text = EMPTY_STRING;
            noteTextBox.text = EMPTY_STRING;
        }
    }

    /// <summary>
    /// Fills the title and content fields with the given values (used when editing).
    /// Used when editing an existing note.
    /// </summary>
    /// <param name="title">The title of the note.</param>
    /// <param name="content">The content of the note.</param>
    public void FillFields(string title, string content)
    {
        titleTextBox.text = title;
        noteTextBox.text = content;
    }

    /// <summary>
    /// Sets the original title of the note.
    /// Used to identify the note when saving edits.
    /// </summary>
    /// <param name="title"></param>
    public void SetOriginalTitle(string title)
    {
        originalTitle = title;
    }

}
