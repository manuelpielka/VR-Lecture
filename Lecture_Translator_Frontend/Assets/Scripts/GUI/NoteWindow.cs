using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class <c>NoteWindow</c> is used to display all general notes and is opened through the Main- MenuWindow. 
/// Here the user can look at their created notes, delete or edit them and create new ones.
/// </summary>
public class NoteWindow : Window
{
    /// <summary>
    /// The list of all existing NoteGUI elements.
    /// </summary>
    private List<NoteGUI> Notes;

    /// <summary>
    /// The prefab of a note for creating new NoteGUI elements.
    /// </summary>
    private GameObject NotePrefab;

    /// <summary>
    /// A reference to the NoteManager for saving, loading, editing, deleting and creating notes.
    /// </summary>
    private NoteManager NoteManager;

    /// <summary>
    /// A reference to the NoteUtils class for utility functions.
    /// </summary>
    private NoteUtils NoteUtils;


    /// <summary>
    /// Button handler for the edit button of a specific note that opens the CreateNoteWindow to be able to edit the note.
    /// </summary>
    /// <param name="note"></param>
    private void EditNote(NoteGUI note)
    {

    }

    /// <summary>
    /// Button handler for the delete button of a specific note that deletes that note.
    /// </summary>
    /// <param name="note"></param>
    private void DeleteNote(NoteGUI note)
    {

    }

    /// <summary>
    /// Button handler for the create note button that opens the CreateNoteWindow.
    /// </summary>
    private void CreateNote()
    {

    }

    /// <summary>
    /// This method loads all saved notes, creates NoteGUI elements and adds them to the notes list.
    /// </summary>
    private void LoadNotes()
    {

    }

    /// <summary>
    /// This method saves the values of the corresponding noteGUI to the NoteManager.
    /// </summary>
    /// <param name="note"></param>
    /// <param name="newTitle"></param>
    /// <param name="newContent"></param>
    public void SaveNote(NoteGUI note, string newTitle, string newContent)
    {

    }
}
