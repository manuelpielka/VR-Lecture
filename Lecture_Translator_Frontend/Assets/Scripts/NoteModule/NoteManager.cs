using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Class <c>NoteManager</c> manages note creation, editing, deleting, and storage (locally).
/// </summary>
public class NoteManager
{
    /// <summary>
    /// A list of all created notes and is used for viewing, editing, saving and deleting.
    /// </summary>
    public List<Note> Notes { get; set; }

    /// <summary>
    /// // Initializes the Notes list so that it's ready for use when adding or accessing notes.
    /// </summary>
    public NoteManager()
    {
        Notes = new List<Note>();
    }

    /// <summary>
    /// Adds the new note and stores the note to the list of notes.
    /// </summary>
    /// <param name="note"></param> The new created note.
    public void AddNote(Note note)
    {
        if (note == null)
        {
            Debug.LogWarning("Add note failed: note is null!");
            return;
        }

        if (Notes.Contains(note))
        {
            Debug.LogWarning($"Note '{note.Title}' already exists.");
            return;
        }

        Notes.Add(note);
    }

    /// <summary>
    /// Deletes the created note and deletes the note from the list of notes.
    /// </summary>
    /// <param name="note"></param>The deleted note.
    public void DeleteNote(Note note)
    {
        if (note == null)
        {
            Debug.LogWarning("Delete note failed: note is null.");
            return;
        }

        if (!Notes.Contains(note))
        {
            Debug.LogWarning("Deleted note failed: the note is not found in the list.");
            return;
        }

        Notes.Remove(note);

        // Delete the corresponding JSON file (if it exists)
        string path = Path.Combine(Application.persistentDataPath, "notes", $"{note.Title}.json");

        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else
            {
                Debug.LogWarning($"Note '{note.Title}' removed from list, but file not found at {path}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to delete file for note '{note.Title}': {ex.Message}");
        }

    }

    /// <summary>
    /// Edits the given note by updating its title and/or content.
    /// </summary>
    /// <param name="note"></param>The note to edit.
    /// <param name="newTitle"></param> The new title to set.
    /// <param name="newContent"></param>The new content to set.
    public void EditNote(Note note, string newTitle, string newContent)
    {
        if (note == null)
        {
            Debug.LogWarning($"The note '{newTitle}' is not found.");
            return;
        }

        if (!Notes.Contains(note))
        {
            Debug.LogWarning($"The note '{newTitle}' is not found in the list.");
            return;
        }

        note.Title = newTitle;
        note.Content = newContent;
    }

    /// <summary>
    /// Saves the note locally in a JSON file for offline use.
    /// </summary>
    /// <param name="note"></param> The note to be saved.(The title of the note is also its file name.)
    public void SaveNote(Note note)
    {
        if (note == null)
        {
            Debug.LogWarning("Save failed: note is null.");
            return;
        }

        // Get the full path to the notes folder inside persistentDataPath.
        string folder = Application.persistentDataPath + "/notes";
        // If the folder does not exist yet, create it.
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        // Create the file path using the title of the note as the file name.
        string path = Path.Combine(folder, $"{note.Title}.json");
        string json = JsonUtility.ToJson(note, true);

        try
        {
            // Write the JSON string to the file.
            File.WriteAllText(path, json);
            Debug.Log($"Note '{note.Title}' saved at {path}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save note '{note.Title}': {ex.Message}");
        }
    }

    /// <summary>
    /// Loads the saved note from the local file for offline use.
    /// </summary>
    /// <param name="title"></param>The title of the note to load. (It must match the file name.)
    /// <returns>Return a <c>Note</c> object if the file is found and loaded successfully; Otherwise return null</returns>
    public Note LoadNote(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            Debug.LogWarning("Load failed: title is null or empty.");
            return null;
        }

        string path = Application.persistentDataPath + $"/notes/{title}.json";

        if (!File.Exists(path))
        {
            Debug.LogWarning($"Load failed: file not found at {path}");
            return null;
        }

        try
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<Note>(json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load note '{title}': {ex.Message}");
            return null;
        }
    }

}
