using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Class <c>NoteManager</c> manages note creation, editing, deleting, and storage (locally).
/// </summary>
public class NoteManager : MonoBehaviour
{
    /// <summary>
    /// A list of all created notes and is used for viewing, editing, saving and deleting.
    /// </summary>
    public List<Note> Notes { get; set; }

    /// <summary>
    /// Ensures only one NoteManager exists and persists across scene loads.
    /// Initializes the Notes list only once.
    /// </summary>
    private void Awake()
    {
        // Check if another NoteManager already exists in the scene
        if (Object.FindObjectsByType<NoteManager>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(this.gameObject);
            return;
        }
        // Persist this object between scenes
        DontDestroyOnLoad(this.gameObject);

        if (Notes == null)
        {
            Notes = new List<Note>();
        }
    }


    /// <summary>
    /// Adds the new note and stores the note to the list of notes.
    /// </summary>
    /// <param name="note"></param> The new created note.
    public void AddNote(Note note)
    {
        if (IsNull(note, "Add note failed: note is null.")) return;

        if (Notes.Contains(note))
        {
            Debug.LogWarning($"Note '{note.Title}' already exists.");
            return;
        }

        Notes.Add(note);
    }

    /// <summary>
    /// Deletes a note from the list and removes its corresponding JSON file from disk.
    /// </summary>
    /// <param name="title">The title of the note to delete.</param>
    public void DeleteNoteByTitle(string title)
    {
        Note noteToRemove = Notes.Find(n => n.Title == title);
        if (noteToRemove == null)
        {
            Debug.LogWarning("Delete failed: note with title not found in Notes list.");
            return;
        }

        Notes.Remove(noteToRemove);

        // Construct full path to JSON file
        string folder = Path.Combine(Application.persistentDataPath, "notes");
        string path = Path.Combine(folder, $"{title}.json");

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Note file deleted: {path}");
        }
        else
        {
            Debug.LogWarning($"Note '{title}' removed from list, but file not found at {path}");
        }
    }

    /// <summary>
    /// Edits the given note by updating its title and/or content.
    /// If the title has changed, the corresponding file is renamed.
    /// This method does NOT save the updated note to local storage.
    /// </summary>
    /// <param name="note">The note to edit.</param>
    /// <param name="newTitle">The new title to set.</param> 
    /// <param name="newContent">The new content to set.</param>
    public void EditNote(Note note, string newTitle, string newContent)
    {

        if (IsNull(note, "Edit failed: note is null.")) return;

        if (!Notes.Contains(note))
        {
            Debug.LogWarning($"The note '{newTitle}' is not found in the list.");
            return;
        }

        // Check if newTitle already exists in another note
        foreach (var existingNote in Notes)
        {
            if (existingNote.Title == newTitle && existingNote != note)
            {
                Debug.LogWarning($"Edit failed: another note with title '{newTitle}' already exists.");
                return;
            }
        }

        //Delete the old file if the note title (file name) has changed
        if (newTitle != note.Title)
        {
            string folder = Path.Combine(Application.persistentDataPath, "notes");
            string oldPath = Path.Combine(folder, $"{note.Title}.json");
            string newPath = Path.Combine(folder, $"{newTitle}.json");

            try
            {
                if (File.Exists(oldPath))
                {
                    //rename the file name
                    File.Move(oldPath, newPath);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to rename note file '{note.Title}': {ex.Message}");
            }

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
        if (IsNull(note, "Save failed: note is null.")) return;

        // Get the full path to the notes folder inside persistentDataPath.
        string folder = Application.persistentDataPath + "/notes";
        // If the folder does not exist yet, create it.
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
            Debug.LogWarning($"The note '{note.Title}' is not found in the note folder.");
        }

        // Create the file path using the title of the note as the file name.
        string path = Path.Combine(folder, $"{note.Title}.json");
        string json = JsonUtility.ToJson(note, true);
        Debug.Log($"JSON content being saved: {json}");

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

    /// <summary>
    /// Loads all saved notes from the local files.
    /// </summary>
    /// <returns>A list of <c>Note</c> objects loaded from local storage.</returns>
    /// If the notes folder does not exist or no valid files are found, returns an empty list.
    public List<Note> LoadAllNotes()
    {
        List<Note> allNotes = new List<Note>();
        string folder = Path.Combine(Application.persistentDataPath, "notes");

        if (!Directory.Exists(folder))
        {
            Notes = allNotes;
            return allNotes;
        }

        foreach (var file in Directory.GetFiles(folder, "*.json"))
        {
            try
            {
                string json = File.ReadAllText(file);
                Note note = JsonUtility.FromJson<Note>(json);
                if (note != null)
                {
                    allNotes.Add(note);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load note from {file}: {ex.Message}");
            }

        }
        Notes = allNotes;
        return allNotes;
    }

    /// <summary>
    /// Thsi method checks if the provided object is null and logs a warning with a custom message.
    /// </summary>
    /// <typeparam name="T">The type of object being checked.</typeparam>
    /// <param name="obj">The object to check.</param>
    /// <param name="warningMessage">The warning message to log if the object is null.</param>
    /// <returns>True if the object is null; Otherwise false.</returns>
    private static bool IsNull<T>(T obj, string warningMessage)
    {
        if (obj == null)
        {
            Debug.LogWarning(warningMessage);
            return true;
        }
        return false;
    }
}
