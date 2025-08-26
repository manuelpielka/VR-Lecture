using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;

/// <summary>
/// Class <c>NoteManager</c> manages note creation, editing, deleting, and storage (locally).
/// </summary>
public class NoteManager : MonoBehaviour
{
    /// <summary>
    /// Holds the singleton instance of the <see cref="NoteManager"/>.
    /// Ensures only one instance exists across scene loads.
    /// </summary>
    public static NoteManager Instance { get; private set; }

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
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        if (Notes == null)
        {
            Notes = new List<Note>();
        }
    }

    /// <summary>
    /// Clears the singleton reference when this instance is destroyed.
    /// Prevents stale references between tests or scene switches.
    /// </summary>
    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Adds the new note and stores the note to the list of notes.
    /// </summary>
    /// <param name="note">The new created note.</param>
    public void AddNote(Note note)
    {
        if (IsNull(note, "Add note failed: note is null.")) return;

        if (Notes.Contains(note))
        {
            Debug.LogWarning($"Note '{note.Title}' already exists.");
            return;
        }

        bool isGlobal = string.IsNullOrEmpty(note.LectureTitle);

        if (Notes.Exists(n =>
        n.Title == note.Title &&
        (isGlobal ? string.IsNullOrEmpty(n.LectureTitle)
                  : n.LectureTitle == note.LectureTitle) &&
        !ReferenceEquals(n, note)))
        {
            Debug.LogWarning(
                isGlobal
                ? $"A global note with title '{note.Title}' already exists."
                : $"A note with title '{note.Title}' already exists in lecture '{note.LectureTitle}'."
            );
            return;
        }

        Notes.Add(note);
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

        Note noteInList = Notes.Find(n => ReferenceEquals(n, note)) ?? Notes.Find(n => n.Title == note.Title && n.LectureTitle == note.LectureTitle);

        if (noteInList == null)
        {
            Debug.LogWarning($"The note '{note.Title}' is not found in the list (by title).");
            return;
        }

        bool isGlobal = string.IsNullOrEmpty(noteInList.LectureTitle);

        bool dupplicate = Notes.Any(n =>
        !ReferenceEquals(n, noteInList) &&
        n.Title == newTitle &&
        (isGlobal ? string.IsNullOrEmpty(n.LectureTitle) : n.LectureTitle == noteInList.LectureTitle));

        if (dupplicate)
        {
            Debug.LogWarning(isGlobal
                ? $"Edit failed: another global note titled '{newTitle}' already exists."
                : $"Edit failed: another note titled '{newTitle}' already exists in lecture '{noteInList.LectureTitle}'.");
            return;
        }

        //Delete the old file if the note title (file name) has changed
        if (newTitle != noteInList.Title)
        {
            string oldPath = GetPath(noteInList);
            string newPath = Path.Combine(GetFolder(noteInList), $"{newTitle}.json");

            try
            {
                if (File.Exists(oldPath) && oldPath != newPath)
                File.Move(oldPath, newPath);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to rename note file '{noteInList.Title}': {ex.Message}");
                return;
            }

        }

        noteInList.Title = newTitle;
        noteInList.Content = newContent;

        SaveNote(noteInList);
    }

    /// <summary>
    /// Saves the note locally in a JSON file for offline use.
    /// </summary>
    /// <param name="note">The note to be saved.(The title of the note is also its file name.)</param>
    public void SaveNote(Note note)
    {
        if (IsNull(note, "Save failed: note is null.")) return;
        string folder = GetFolder(note);

        // If the folder does not exist yet, create it.
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
            Debug.Log($"Created notes folder: {folder}");
        }

        string path = GetPath(note);
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
    /// <param name="title">The title of the note to load. (It must match the file name.)</param>
    /// <returns>Return a <c>Note</c> object if the file is found and loaded successfully; Otherwise return null</returns>
    public Note LoadNote(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            Debug.LogWarning("Load failed: title is null or empty.");
            return null;
        }

        string baseDirectory = Path.Combine(Application.persistentDataPath, "notes");
        if (!Directory.Exists(baseDirectory))
        {
            Debug.LogWarning("Load failed: notes base directory not found.");
            return null;
        }

        var matches = Directory.GetFiles(baseDirectory, $"{title}.json", SearchOption.AllDirectories);
        if (matches.Length == 0)
        {
            Debug.LogWarning($"Load failed: '{title}.json' not found under {baseDirectory}");
            return null;
        }
        if (matches.Length > 1)
        {
            Debug.LogWarning($"Load warning: multiple files named '{title}.json' found in different folders; returning the first.");
        }

        try
        {
            string json = File.ReadAllText(matches[0]);
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

        foreach (var file in Directory.GetFiles(folder, "*.json", SearchOption.AllDirectories))
        {
            try
            {
                string json = File.ReadAllText(file);
                Note note = JsonUtility.FromJson<Note>(json);
                if (note != null) allNotes.Add(note);
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
    /// Returns all general notes where <c>LectureTitle</c> is null or empty.
    /// </summary>
    /// <returns>Returns a filtered list of global notes</returns>
    public List<Note> GetGlobalNotes() =>
    Notes.FindAll(n => string.IsNullOrEmpty(n.LectureTitle));

    /// <summary>
    /// Returns all notes for the specified lecture title.
    /// </summary>
    /// <param name="lectureTitle">The lecture title to match.</param>
    /// <returns>Returns a filtered list of lecture notes.</returns>
    public List<Note> GetNotesForLecture(string lectureTitle) =>
    Notes.FindAll(n => n.LectureTitle == lectureTitle);

    /// <summary>
    /// Loads a global note by title directly from <c>/notes/global</c>.
    /// </summary>
    /// <param name="title">The title (file name) to load.</param>
    /// <returns>Returns the loaded <c>Note</c>; otherwise null.</returns>
    public Note LoadGlobalNoteByTitle(string title)
    {
        string folder = Path.Combine(Application.persistentDataPath, "notes", "global");
        string path = Path.Combine(folder, $"{title}.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Global note '{title}' not found.");
            return null;
        }

        try
        {
            return JsonUtility.FromJson<Note>(File.ReadAllText(path));
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Load global failed: {ex.Message}"); return null;
        }
    }

    /// <summary>
    /// Loads a lecture note by (<paramref name="lectureTitle"/>, <paramref name="title"/>) 
    /// directly from <c>/notes/lectures/{slug(lectureTitle)}</c>.
    /// </summary>
    /// <param name="lectureTitle">The lecture title.</param>
    /// <param name="title">The title (file name) to load.</param>
    /// <returns>Returns the loaded <c>Note</c>; otherwise null.</returns>
    public Note LoadNoteByTitleInLecture(string lectureTitle, string title)
    {
        string folder = Path.Combine(Application.persistentDataPath, "notes", "lectures", Slug(lectureTitle));
        string path = Path.Combine(folder, $"{title}.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Note '{title}' not found in lecture '{lectureTitle}'.");
            return null;
        }

        try
        {
            return JsonUtility.FromJson<Note>(File.ReadAllText(path));
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Load in lecture failed: {ex.Message}"); return null;
        }
    }

    /// <summary>
    /// Deletes a global note by title under <c>/notes/global</c> and refreshes the in-memory list.
    /// </summary>
    /// <param name="title">The title (file name) to delete.</param>
    public void DeleteGlobalNoteByTitle(string title)
    {
        var note = Notes.FirstOrDefault(n => string.IsNullOrEmpty(n.LectureTitle) && n.Title == title);
        if (note == null) { Debug.LogWarning("Delete failed: global note not found."); return; }

        Notes.Remove(note);

        string path = Path.Combine(Application.persistentDataPath, "notes", "global", $"{title}.json");

        if (File.Exists(path)) File.Delete(path);
        else Debug.LogWarning($"Global note removed from list, but file not found at {path}");

        Notes = LoadAllNotes();
    }

    /// <summary>
    /// Deletes a lecture note by (<paramref name="lectureTitle"/>, <paramref name="title"/>) 
    /// under <c>/notes/lectures/{slug(lectureTitle)}</c> and refreshes the list.
    /// </summary>
    /// <param name="lectureTitle">The lecture title the note belongs to</param>
    /// <param name="title">The title (file name) to delete.</param>
    public void DeleteNoteByTitleInLecture(string lectureTitle, string title)
    {
        var note = Notes.FirstOrDefault(n => n.LectureTitle == lectureTitle && n.Title == title);
        if (note == null)
        {
            Debug.LogWarning($"Delete failed: note '{title}' not found in lecture '{lectureTitle}'.");
            return;
        }
        Notes.Remove(note);

        string folder = Path.Combine(Application.persistentDataPath, "notes", "lectures", Slug(lectureTitle));
        string path = Path.Combine(folder, $"{title}.json");
        if (File.Exists(path)) File.Delete(path);
        else Debug.LogWarning($"Lecture note removed from list, but file not found at {path}");

        Notes = LoadAllNotes();
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

    /// <summary>
    /// Converts a string to a filesystem-friendly slug:
    /// replaces non [A-Za-z0-9] with '-', collapses repeats, trims, and lowercases.
    /// </summary>
    /// <param name="s">The input string.</param>
    /// <returns>Returns a slugged, lowercase string (or "untitled" if empty).</returns>
    private static string Slug(string s)
    {
        var t = Regex.Replace(s ?? "", "[^A-Za-z0-9]+", "-");
        t = Regex.Replace(t, "-{2,}", "-").Trim('-');

        return string.IsNullOrEmpty(t) ? "untitled" : t.ToLowerInvariant();
    }
    
    /// <summary>
    /// Gets the storage folder for the given note based on its scope:
    /// global → /notes/global, lecture → /notes/lectures/{slug(LectureTitle)}.
    /// </summary>
    /// <param name="n">The note to resolve.</param>
    /// <returns>Returns the absolute folder path for the note.</returns>
    private string GetFolder(Note n)
    {
        string baseDirectory = Path.Combine(Application.persistentDataPath, "notes");
        if (string.IsNullOrEmpty(n.LectureTitle))
            return Path.Combine(baseDirectory, "global");

        return Path.Combine(baseDirectory, "lectures", Slug(n.LectureTitle));
    }

    /// <summary>
    /// Builds the full file path for the given note in the form: {GetFolder(n)}/{Title}.json
    /// </summary>
    /// <param name="n">The note to resolve.</param>
    /// <returns>Returns the absolute JSON file path for the note.</returns>
    private string GetPath(Note n) => Path.Combine(GetFolder(n), $"{n.Title}.json");

}
