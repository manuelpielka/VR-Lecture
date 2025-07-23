using UnityEngine;
using TMPro;
using System.Runtime.Serialization.Json;

/// <summary>
/// Class <c>NoteGUI</c> is used to access the title and content text box of a note for easier editing.
/// </summary>
public class NoteGUI : MonoBehaviour
{
    /// <summary>
    /// The textbox that displays the title.
    /// </summary>
    [SerializeField] public TextMeshProUGUI titleTextBox;

    /// <summary>
    ///  The textbox that displays the note content.
    /// </summary>
    [SerializeField] public TextMeshProUGUI noteTextBox;

    /// <summary>
    /// Reference to the parent NoteWindow to access its methods.
    /// </summary>
    private NoteWindow noteWindowUI;

    /// <summary>
    /// Initializes the note by setting its parent window.
    /// </summary>
    /// <param name="parentWindow">The NoteWindow this note belongs to.</param>
    public void Initialize(NoteWindow parentWindow)
    {
        noteWindowUI = parentWindow;
    }

    /// <summary>
    /// Button handler for the delete button that removes this note and reloads the list.
    /// </summary>
    public void Delete()
    {
        if (noteWindowUI != null)
        {
            noteWindowUI.DeleteNote(this);
            noteWindowUI.LoadNotes();
        }
    }

    /// <summary>
    /// Button handler for the edit button that opens the editor window for this note.
    /// </summary>
    public void Edit()
    {
        Debug.Log("Edit button clicked!");
        noteWindowUI.EditNote(this);
    }
}
