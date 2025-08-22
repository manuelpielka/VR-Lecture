using System.Collections.Generic;
using UnityEngine;

using GUI;
using System.Reflection;
using UnityEngine.UI;

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
    /// Reference to the NoteManager used for saving, loading, editing, and deleting notes.
    /// </summary>
    [SerializeField] protected NoteManager noteManager; // Protected so that LectureNoteWindow can access...

    /// <summary>
    /// A reference to the NoteUtils class for utility functions.
    /// </summary>
    //private NoteUtils NoteUtils;

    /// <summary>
    /// The prefab of a note for creating new NoteGUI elements.
    /// </summary>
    [SerializeField] private GameObject notePrefab;

    /// <summary>
    /// The parent transform that contains all NoteGUI elements in the UI (e.g., the ScrollView content).
    /// </summary>
    [SerializeField] private Transform noteContainer;

    /// <summary>
    /// Button handler for the edit button of a specific note that opens the CreateNoteWindow to be able to edit the note.
    /// </summary>
    /// <param name="noteUI ">The noteUI element to edit</param>
    public void EditNote(NoteGUI noteUI)
    {
        string originalTitle = noteUI.titleTextBox.text;
        Note targetNote = noteManager.LoadNote(originalTitle);
        if (NoteUtils.IsNull(targetNote, "Edit failed: target note is null.")) return;

        Window window = WindowManager.OpenWindow(WindowKeys.CreateNoteKey);
        CreateNoteWindow createWindow = window as CreateNoteWindow;
        if (createWindow != null)
        {
            createWindow.noteWindow = this;
            createWindow.Initialize(true);
            createWindow.FillFields(targetNote.Title, targetNote.Content);
            createWindow.SetOriginalTitle(targetNote.Title);
        }

    }

    /// <summary>
    /// Button handler for the delete button of a specific note that deletes that note.
    /// </summary>
    /// <param name="noteUI">THe NoteUI element to delete.</param>
    public void DeleteNote(NoteGUI noteUI)
    {
        string title = noteUI.titleTextBox.text;

        //Note targetNote = noteManager.LoadNote(title);
        //if (NoteUtils.IsNull(targetNote, "Delete failed: note is null.")) return;

        noteManager.DeleteNoteByTitle(title);

        Notes.Remove(noteUI);
        Destroy(noteUI.gameObject);
    }

    /// <summary>
    /// Button handler for the create note button that opens the CreateNoteWindow.
    /// </summary>
    public void CreateNote()
    {
        Window window = WindowManager.OpenWindow(WindowKeys.CreateNoteKey);
        CreateNoteWindow createWindow = window as CreateNoteWindow;
        if (createWindow != null)
        {
            createWindow.noteWindow = this;
            createWindow.Initialize(false);
        }
    }

    /// <summary>
    /// This method loads all saved notes, creates NoteGUI elements and adds them to the notes list.
    /// </summary>
    public void LoadNotes()
    {
        if (notePrefab == null)
        {
            Debug.LogError("notePrefab is not assigned in the Inspector!");
            return;
        }

        if (noteContainer == null)
        {
            Debug.LogError("notesContainer is not assigned in the Inspector!");
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

        var notes = noteManager.LoadAllNotes();

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

    /// <summary>
    /// This method runs every time the user opens the Notes window, it reloads and shows the latest saved notes.
    /// </summary>
    private void OnEnable()
    {
        if (notePrefab == null || noteContainer == null)
            Initialize();

        LoadNotes();
    }

    /// <summary>
    /// Saves a newly created note using the given title and content.
    /// </summary>
    /// <param name="title">The title of the new note.</param>
    /// <param name="content">The content of the new note.</param>
    public void SaveNewNote(string title, string content)
    {
        Note newNote = new Note(title, content);
        Debug.Log($"The new note has been created: '{newNote.Title}'."); //

        noteManager.AddNote(newNote);
        Debug.Log($"The new created note has been added into the note list: '{newNote.Title}'."); //

        noteManager.SaveNote(newNote);
    }

    /// <summary>
    /// Saves changes to an existing note with updated title and content.
    /// </summary>
    /// <param name="originalTitle">The original title of the note (used to locate it).</param>
    /// <param name="newTitle">The updated title.</param>
    /// <param name="newContent">The updated content.</param>
    public void SaveEditedNote(string originalTitle, string newTitle, string newContent)
    {
        //Note editedNoteToSave = noteManager.LoadNote(originalTitle);
        Note editedNoteToSave = noteManager.Notes.Find(n => n.Title == originalTitle);
        if (NoteUtils.IsNull(editedNoteToSave, "Save failed: note is null.")) return;

        Debug.Log($"Found the note to edit: '{editedNoteToSave.Title}'."); //

        //editedNoteToSave.Title = newTitle;
        //editedNoteToSave.Content = newContent;
        //noteManager.SaveNote(editedNoteToSave);
        noteManager.EditNote(editedNoteToSave, newTitle, newContent);

        Debug.Log($"The new title of the note to edit is '{newTitle}'."); //
    }

    /// <summary>
    /// Called when the window is instantiated. Tries to find the NoteManager in the scene.
    /// </summary>
    private void Awake()
    {
        if (noteManager == null)
        {
            noteManager = FindFirstObjectByType<NoteManager>();
            if (noteManager == null)
            {
                Debug.LogError("NoteManager not found in scene.");
            }
        }
    }

    /// <summary>
    /// Initializes the window's prefab and container references if not set via Inspector.
    /// </summary>
    /// <param name="prefab">Optional note prefab to assign.</param>
    /// <param name="container">Optional container transform to assign.</param>
    private void Initialize(GameObject prefab = null, Transform container = null)
    {
        this.notePrefab = prefab ?? Resources.Load<GameObject>("NoteUI");
        this.noteContainer = container ?? GameObject.Find("Content")?.transform;

        if (notePrefab == null) Debug.LogError("NotePrefab not found!");
        if (noteContainer == null) Debug.LogError("NotesContainer not found!");
    }

}
