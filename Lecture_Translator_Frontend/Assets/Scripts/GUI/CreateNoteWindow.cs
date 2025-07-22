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
    protected TextMeshProUGUI TitleTextBox;

    /// <summary>
    /// The text box of the note’s contents.
    /// </summary>
    [SerializeField]
    protected TextMeshProUGUI NoteTextBox;

    /// <summary>
    /// Reference to the NoteWindow in order to save a note.
    /// </summary>
    [SerializeField]
    private NoteWindow NoteWindow;

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
        string title = TitleTextBox.text;
        string content = NoteTextBox.text;

        // If a callback is assigned to OnNoteConfirmed, invoke it with the title and content
        if (OnNoteConfirmed != null)
        {
            OnNoteConfirmed.Invoke(title, content);
        }

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
    /// This method initializes the CreateNoteWindow with the appropriate mode and data.
    /// </summary>
    /// <param name="editMode">If true, the window is in edit mode and will update an existing note; if false, it will create a new note.</param>
    /// <param name="noteToEdit">The note to edit. This is only required when <paramref name="editMode"/> is true.</param>
    public void Initialize(bool editMode, Note noteToEdit = null)
    {
        this.isEditMode = editMode;

        if (isEditMode && noteToEdit != null)
        {
            TitleTextBox.text = noteToEdit.Title;
            NoteTextBox.text = noteToEdit.Content;
        }
    }
}
