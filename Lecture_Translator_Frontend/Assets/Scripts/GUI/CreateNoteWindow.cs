using NUnit.Framework.Internal;
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
    private TextMeshProUGUI TitleTextBox;

    /// <summary>
    /// The text box of the note’s contents.
    /// </summary>
    private TextMeshProUGUI NoteTextBox;

    /// <summary>
    /// Reference to the NoteWindow in order to save a note.
    /// </summary>
    private NoteWindow NoteWindow;


    /// <summary>
    /// Button handler for the apply button that saves the inputted title and content as a new note or as an edit to an existing note.
    /// </summary>
    private void Apply()
    {

    }

    /// <summary>
    /// Button handler for the discard button that closes the window without saving changes.
    /// </summary>
    private void Discard()
    {

    }
}
