using System.Collections.Generic;
using UnityEngine;

using GUI;
using System.Reflection;
/// <summary>
/// Class <c>NoteWindow</c> is used to display all general notes and is opened through the Main- MenuWindow. 
/// Here the user can look at their created notes, delete or edit them and create new ones.
/// </summary>
public class NoteWindow : Window
{
    /// <summary>
    /// The list of all existing NoteGUI elements.
    /// </summary>
    private List<NoteGUI> Notes = new List<NoteGUI>();

    /// <summary>
    /// A reference to the NoteManager for saving, loading, editing, deleting and creating notes.
    /// </summary>
    [SerializeField]
    private NoteManager NoteManager;

    /// <summary>
    /// A reference to the NoteUtils class for utility functions.
    /// </summary>
    //private NoteUtils NoteUtils;

    /// <summary>
    /// The prefab of a note for creating new NoteGUI elements.
    /// </summary>
    [SerializeField]
    private GameObject NotePrefab;

    /// <summary>
    /// Reference to the CreateNoteWindow prefab for creating or editing notes.
    /// </summary>
    [SerializeField]
    private GameObject createNoteWindowPrefab;

    /// <summary>
    /// The parent transform that contains all NoteGUI elements in the UI (e.g., the ScrollView content).
    /// </summary>
    [SerializeField]
    private Transform notesContainer;




    /// <summary>
    /// Button handler for the edit button of a specific note that opens the CreateNoteWindow to be able to edit the note.
    /// </summary>
    /// <param name="note "></param>
    private void EditNote(NoteGUI note)
    {
        string title = note.TitleTextBox.text;
        Note targetNote = NoteManager.LoadNote(title);

        if (NoteUtils.IsNull(targetNote, "Edit failed: target note is null.")) return;

        GameObject windowGO = Instantiate(createNoteWindowPrefab);
        CreateNoteWindow createWindow = windowGO.GetComponent<CreateNoteWindow>();

        createWindow.Initialize(true, targetNote);

        // Assigns a callback to handle what happens when the user confirms the edit of an existing note
        createWindow.OnNoteConfirmed = (title, content) =>
        {
            NoteManager.EditNote(targetNote, title, content);
            NoteManager.SaveNote(targetNote);

            LoadNotes();
        };
    }

    /// <summary>
    /// Button handler for the delete button of a specific note that deletes that note.
    /// </summary>
    /// <param name="note"></param>
    private void DeleteNote(NoteGUI note)
    {
        string title = note.TitleTextBox.text;
        Note targetNote = NoteManager.LoadNote(title);

        if (NoteUtils.IsNull(targetNote, "Delete failed: target note is null.")) return;

        NoteManager.DeleteNote(targetNote);
        // TODO: Destroy the note(GameObject) from UI
    }

    /// <summary>
    /// Button handler for the create note button that opens the CreateNoteWindow.
    /// </summary>
    private void CreateNote()
    {
        GameObject windowGO = Instantiate(createNoteWindowPrefab);
        CreateNoteWindow createWindow = windowGO.GetComponent<CreateNoteWindow>();

        //createWindow.Prefab = createNoteWindowPrefab;

        createWindow.Initialize(false);

        // Assigns a callback to handle what happens when the user confirms the creation of a new note
        createWindow.OnNoteConfirmed = (title, content) =>
        {
            Note newNote = new Note { Title = title, Content = content };
            NoteManager.AddNote(newNote);
            NoteManager.SaveNote(newNote);

            LoadNotes();
        };
    }

    /// <summary>
    /// This method loads all saved notes, creates NoteGUI elements and adds them to the notes list.
    /// </summary>
    private void LoadNotes()
    {
        // Remove all existing NoteGUI elements from the UI
        foreach (var noteGUI in Notes)
        {
            //TODO: Destroy the noteGUI elements(GameObjects) from the UI.
        }
        Notes.Clear();

        var notes = NoteManager.LoadAllNotes();

        foreach (var note in notes)
        {
            // Create a new note UI and put it inside the notes container
            GameObject noteGO = Instantiate(NotePrefab, notesContainer);
            NoteGUI noteGUI = noteGO.GetComponent<NoteGUI>();

            noteGUI.TitleTextBox.text = note.Title;
            noteGUI.NoteTextBox.text = note.Content;

            Notes.Add(noteGUI);
        }

    }

    /// <summary>
    /// This method saves the values of the corresponding noteGUI to the NoteManager.
    /// </summary>
    /// <param name="note">The note to save.</param>
    /// <param name="newTitle">The note title to save.</param>
    /// <param name="newContent">The note content to save.</param>
    public void SaveNote(NoteGUI note, string newTitle, string newContent)
    {
        var noteToSave = NoteManager.LoadNote(note.TitleTextBox.text);

        if (NoteUtils.IsNull(noteToSave, "Save failed: note is null.")) return;

        noteToSave.Title = newTitle;
        noteToSave.Content = newContent;
        NoteManager.SaveNote(noteToSave);

    }

    /// <summary>
    /// This method runs every time the user opens the Notes window, it reloads and shows the latest saved notes.
    /// </summary>
    private void OnEnable()
    {
        LoadNotes();
    }
}
