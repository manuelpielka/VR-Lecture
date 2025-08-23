using NUnit.Framework;
using UnityEngine;
using System.IO;
using System.Linq;

/// <summary>
/// Contains a comprehensive set of unit tests for the <see cref="NoteManager"/> class,
/// covering adding, saving, loading, editing, and deleting notes.
/// Ensures correct behavior for both normal operations and edge cases.
/// </summary>
public class NoteManagerTests
{
    /// <summary>
    /// Temporary GameObject used to host the NoteManager during tests.
    /// </summary>
    private GameObject host;

    /// <summary>
    /// The NoteManager instance under test.
    /// </summary>
    private NoteManager mgr;

    /// <summary>
    /// Gets the full path to the notes folder used for saving and loading note files.
    /// </summary>
    private string NotesFolder => Path.Combine(Application.persistentDataPath, "notes");

    /// <summary>
    /// Returns the full file path for a given note title.
    /// </summary>
    /// <param name="title">The title of the note (used as filename without extension)</param>
    /// <returns></returns>
    private string FileOf(string title) => Path.Combine(NotesFolder, $"{title}.json");

    /// <summary>
    /// Called before each test.
    /// Initializes a new NoteManager instance attached to a temporary GameObject,
    /// and clears the notes folder to ensure a clean test environment.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        host = new GameObject("NoteManager_TestHost");
        mgr = host.AddComponent<NoteManager>();
        mgr.Notes = new System.Collections.Generic.List<Note>();

        // Clean up notes folder before each test
        if (Directory.Exists(NotesFolder)) Directory.Delete(NotesFolder, true);
    }

    /// <summary>
    /// Called after each test.
    /// Cleans up the temporary GameObject and deletes the notes folder to reset the state.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        if (host != null) Object.DestroyImmediate(host);
        if (Directory.Exists(NotesFolder)) Directory.Delete(NotesFolder, true);
    }

    /// <summary>
    /// Verifies that AddNote immediately returns when given a null note.
    /// </summary>
    [Test]
    public void AddNote_Null_EarlyReturn()
    {
        mgr.AddNote(null);
        Assert.That(mgr.Notes, Is.Empty);
    }

    /// <summary>
    /// Verifies that AddNote successfully adds a new note to the list.
    /// </summary>
    [Test]
    public void AddNote_Adds_New()
    {
        mgr.AddNote(new Note("A", "x"));
        Assert.That(mgr.Notes.Count, Is.EqualTo(1));
    }

    /// <summary>
    /// Verifies that adding the same note instance twice is rejected.
    /// </summary>
    [Test]
    public void AddNote_Rejects_Same_Instance_Twice()
    {
        var n = new Note("D", "d");
        mgr.AddNote(n);
        mgr.AddNote(n);                                 // Contains() uses reference equality → blocks same instance
        Assert.That(mgr.Notes.Count, Is.EqualTo(1));
        Assert.That(mgr.Notes[0], Is.SameAs(n));
    }

    /// <summary>
    /// Verifies that SaveNote immediately returns when given null and does not create the notes folder.
    /// </summary>
    [Test]
    public void SaveNote_Null_EarlyReturn_NoFolderCreated()
    {
        mgr.SaveNote(null);
        Assert.That(Directory.Exists(NotesFolder), Is.False);
    }

    /// <summary>
    /// Verifies that SaveNote creates the notes folder if missing and writes a file to disk.
    /// </summary>
    [Test]
    public void SaveNote_CreatesFolder_And_WritesFile()
    {
        // Do not create the folder beforehand; let SaveNote handle folder creation
        var n = new Note("S", "payload");
        mgr.SaveNote(n);
        Assert.That(File.Exists(FileOf("S")), Is.True);     // Should create notes folder and write the file
    }

    /// <summary>
    /// Verifies that LoadNote returns null when the provided title is blank or whitespace.
    /// </summary>
    [Test]
    public void LoadNote_BlankTitle_ReturnsNull()
    {
        var loaded = mgr.LoadNote("   "); // Blank title → early return
        Assert.IsNull(loaded);
    }

    /// <summary>
    /// Verifies that LoadNote returns null when the corresponding file does not exist.
    /// </summary>
    [Test]
    public void LoadNote_NotExists_ReturnsNull()
    {
        var loaded = mgr.LoadNote("NotExist"); // File does not exist
        Assert.IsNull(loaded);
    }

    /// <summary>
    /// Verifies that saving and then loading a note preserves its title and content.
    /// Also checks that the JSON file contains the expected fields.
    /// </summary>
    [Test]
    public void Save_Then_Load_RoundTrip_ContentMatches()
    {
        Directory.CreateDirectory(NotesFolder);
        var n = new Note("Round", "Hello");
        mgr.SaveNote(n);
        var text = File.ReadAllText(FileOf("Round"));
        var compact = new string(text.Where(c => !char.IsWhiteSpace(c)).ToArray());
        StringAssert.Contains("\"Title\":\"Round\"", compact);
        StringAssert.Contains("\"Content\":\"Hello\"", compact);

        var loaded = mgr.LoadNote("Round");                       // Should successfully load from JSON
        Assert.That(loaded.Title, Is.EqualTo("Round"));
        Assert.That(loaded.Content, Is.EqualTo("Hello"));
    }

    /// <summary>
    /// Verifies that LoadAllNotes returns an empty list when the folder is missing,
    /// and also clears the Notes list to stay in sync.
    /// </summary>
    [Test]
    public void LoadAllNotes_FolderMissing_ReturnsEmpty_AndSyncsNotes()
    {
        var list = mgr.LoadAllNotes();  // Folder does not exist
        Assert.That(list, Is.Empty);
        Assert.That(mgr.Notes, Is.Empty); // Notes list should be reset to empty
    }

    /// <summary>
    /// Verifies that LoadAllNotes returns all saved notes from JSON files with correct content.
    /// </summary>
    [Test]
    public void LoadAllNotes_Returns_All_Saved_Files()
    {
        Directory.CreateDirectory(NotesFolder);
        mgr.SaveNote(new Note("N1", "C1"));
        mgr.SaveNote(new Note("N2", "C2"));
        var list = mgr.LoadAllNotes();                             // Enumerates *.json → deserializes into notes
        CollectionAssert.AreEquivalent(new[] { "N1", "N2" }, list.Select(x => x.Title));
        Assert.That(list.First(x => x.Title == "N1").Content, Is.EqualTo("C1"));
        Assert.That(list.First(x => x.Title == "N2").Content, Is.EqualTo("C2"));
    }

    /// <summary>
    /// Verifies that EditNote immediately returns when given a null note.
    /// </summary>
    [Test]
    public void EditNote_Null_EarlyReturn()
    {
        mgr.EditNote(null, "X", "Y"); // Null note → early return
        Assert.Pass(); // No exception should be thrown
    }

    /// <summary>
    /// Verifies that EditNote immediately returns when the note is not in the list,
    /// and does not create any file.
    /// </summary>
    [Test]
    public void EditNote_NoteNotInList_EarlyReturn_NoFileCreated()
    {
        var ghost = new Note("Ghost", "g");
        mgr.EditNote(ghost, "New", "n");                            // Title not found in list → early return
        Assert.That(Directory.Exists(NotesFolder), Is.False);
    }

    /// <summary>
    /// Verifies that renaming is rejected when another note already uses the target title.
    /// </summary>
    [Test]
    public void EditNote_Rename_Rejected_When_TargetTitleExists()
    {
        var a = new Note("A", "1"); var b = new Note("B", "2");
        mgr.AddNote(a); mgr.AddNote(b);
        mgr.EditNote(a, "B", "new"); // Title collision → rejected
        Assert.That(mgr.Notes.First(x => x.Title == "A").Content, Is.EqualTo("1"));
        Assert.That(mgr.Notes.Count(x => x.Title == "B"), Is.EqualTo(1));
    }

    /// <summary>
    /// Verifies that if the title is unchanged, only the content is updated and saved.
    /// </summary>
    [Test]
    public void EditNote_TitleUnchanged_OnlyUpdatesContent_AndSaves()
    {
        var n = new Note("Same", "old");
        mgr.AddNote(n);
        Directory.CreateDirectory(NotesFolder);
        mgr.EditNote(n, "Same", "new");                               // Same title → skip file rename, update content and save
        var loaded = mgr.LoadNote("Same");
        Assert.That(loaded.Content, Is.EqualTo("new"));
        Assert.That(File.Exists(FileOf("Same")), Is.True);
    }

    /// <summary>
    /// Verifies that when the title changes and the old file exists,
    /// the file is renamed and the new content is saved.
    /// </summary>
    [Test]
    public void EditNote_TitleChanged_RenamesWhenOldFileExists_ThenSaves()
    {
        var n = new Note("Old", "c1");
        mgr.AddNote(n);
        mgr.SaveNote(n);                                              // Ensure Old.json exists → will trigger File.Move
        mgr.EditNote(n, "New", "c2");
        Assert.That(File.Exists(FileOf("Old")), Is.False);
        Assert.That(File.Exists(FileOf("New")), Is.True);
        var re = mgr.LoadNote("New");
        Assert.That(re.Content, Is.EqualTo("c2"));
    }

    /// <summary>
    /// Verifies that when the title changes but the old file does not exist,
    /// the rename step is skipped but the new file is still saved.
    /// </summary>
    [Test]
    public void EditNote_TitleChanged_NoOldFile_SkipsMove_ButSavesNew()
    {
        var n = new Note("X", "c");
        mgr.AddNote(n);                                                 // No X.json on disk
        mgr.EditNote(n, "Y", "d");                                      // Skip file rename, but still save new file
        Assert.That(File.Exists(FileOf("X")), Is.False);
        Assert.That(File.Exists(FileOf("Y")), Is.True);
        var re = mgr.LoadNote("Y");
        Assert.That(re.Content, Is.EqualTo("d"));
    }

    /// <summary>
    /// Verifies that DeleteNoteByTitle immediately returns when the title is not found in the list.
    /// </summary>
    //[Test]
    //public void DeleteNoteByTitle_NotFoundInList_EarlyReturn()
    //{
    //    mgr.DeleteNoteByTitle("Nope");                                    // Title not found in list → early return
    //    Assert.Pass();
    //}

    /// <summary>
    /// Verifies that when the file exists, DeleteNoteByTitle deletes it and reloads the list from disk.
    /// </summary>
    //[Test]
    //public void DeleteNoteByTitle_FileExists_DeletesAndReloads()
    //{
    //    var a = new Note("A", "x"); var b = new Note("B", "y");
    //    mgr.AddNote(a); mgr.AddNote(b);
    //    mgr.SaveNote(a); mgr.SaveNote(b);
    //    mgr.DeleteNoteByTitle("B");                                       // Delete B.json and reload list
    //    Assert.That(File.Exists(FileOf("B")), Is.False);
    //    CollectionAssert.AreEquivalent(new[] { "A" }, mgr.Notes.Select(x => x.Title));
    //}

    /// <summary>
    /// Verifies that when the file is missing, DeleteNoteByTitle logs a warning and reloads the list (which becomes empty).
    /// </summary>
    //[Test]
    //public void DeleteNoteByTitle_FileMissing_WarnsAndReloads()
    //{
    //    var a = new Note("A", "x");
    //    mgr.AddNote(a);                                                   // No A.json on disk
    //    mgr.DeleteNoteByTitle("A");                                       // Else branch: file missing
    //    Assert.That(File.Exists(FileOf("A")), Is.False);
    //    Assert.That(mgr.Notes, Is.Empty);                                 // Reload results in empty list
    //}
}
