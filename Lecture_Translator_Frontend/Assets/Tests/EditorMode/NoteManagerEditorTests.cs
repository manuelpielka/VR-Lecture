using NUnit.Framework;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.TestTools;
using System.Text.RegularExpressions;

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
    /// Base folder that contains all notes related subfolders.
    /// </summary>
    private string BaseNotesFolder => Path.Combine(Application.persistentDataPath, "notes");

    /// <summary>
    /// Folder for global notes.
    /// </summary>
    private string GlobalFolder => Path.Combine(BaseNotesFolder, "global");

    /// <summary>
    /// Returns the full file path for a global note by title.
    /// </summary>
    private string FileOfGlobal(string title) => Path.Combine(GlobalFolder, $"{title}.json");

    /// <summary>
    /// Returns the full file path for a lecture note by lecture and title.
    /// </summary>
    private string FileOfInLecture(string lectureTitle, string title)
    {
        var slug = Regex.Replace(lectureTitle.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        return Path.Combine(BaseNotesFolder, "lectures", slug, $"{title}.json");
    }

    // -------------------- TEST LIFECYCLE --------------------

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

        if (Directory.Exists(BaseNotesFolder)) Directory.Delete(BaseNotesFolder, true);
    }

    /// <summary>
    /// Called after each test.
    /// Cleans up the temporary GameObject and deletes the notes folder to reset the state.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        if (host != null) Object.DestroyImmediate(host);
        if (Directory.Exists(BaseNotesFolder)) Directory.Delete(BaseNotesFolder, true);
    }

    // -------------------- ADD --------------------

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
        mgr.AddNote(n); // Blocks same instance
        Assert.That(mgr.Notes.Count, Is.EqualTo(1));
        Assert.That(mgr.Notes[0], Is.SameAs(n));
    }

    /// <summary>
    /// Verifies that AddNote rejects a different instance with the same title in the same scope.
    /// </summary>
    [Test]
    public void AddNote_Rejects_Different_Instance_With_Same_Title_In_Same_Scope()
    {
        var a = new Note("Dup", "c1"); // global
        var b = new Note("Dup", "c2");
        mgr.AddNote(a);
        mgr.AddNote(b); // Should be rejected
        Assert.That(mgr.Notes.Count, Is.EqualTo(1));
        Assert.That(mgr.Notes[0], Is.SameAs(a));
    }

    // -------------------- SAVE --------------------

    /// <summary>
    /// Verifies that SaveNote immediately returns when given null and does not create the notes folder.
    /// </summary>
    [Test]
    public void SaveNote_Null_EarlyReturn_NoFolderCreated()
    {
        mgr.SaveNote(null);
        Assert.That(Directory.Exists(BaseNotesFolder), Is.False);
    }

    /// <summary>
    /// Verifies that SaveNote creates the notes folder if missing and writes a file to disk.
    /// </summary>
    [Test]
    public void SaveNote_CreatesFolder_And_WritesFile()
    {
        var n = new Note("S", "payload");
        mgr.SaveNote(n);
        Assert.That(File.Exists(FileOfGlobal("S")), Is.True);
    }

    /// <summary>
    /// Verifies that SaveNote logs an error when the target path is not writable,
    /// by pre-creating a directory with the same name as the target file (RO.json).
    /// This reliably throws on all platforms and covers the Debug.LogError branch.
    /// </summary>
    [Test]
    public void SaveNote_WriteFails_LogsError()
    {
        // Arrange: make notes/global/RO.json a directory so writing a file there will fail
        var go = new GameObject();
        var mgr = go.AddComponent<NoteManager>();
        var n = new Note("RO", "x");

        var globalFolder = Path.Combine(Application.persistentDataPath, "notes", "global");
        var badPathAsDir = Path.Combine(globalFolder, "RO.json");
        Directory.CreateDirectory(badPathAsDir); // <-- "file" path is a directory now

        // Expect: SaveNote should log an error
        LogAssert.Expect(LogType.Error, new Regex("Failed to save note"));

        // Act
        mgr.SaveNote(n);

        // Cleanup
        if (Directory.Exists(badPathAsDir)) Directory.Delete(badPathAsDir, true);
    }

    // -------------------- LOAD (single) --------------------

    /// <summary>
    /// Verifies that LoadNote logs a warning and returns null
    /// when the base notes directory does not exist.
    /// Covers the "base directory not found" branch.
    /// </summary>
    [Test]
    public void LoadNote_BaseDirectoryMissing_LogsWarning_AndReturnsNull()
    {
        // Arrange: ensure the base notes folder is deleted
        if (Directory.Exists(BaseNotesFolder)) Directory.Delete(BaseNotesFolder, true);
        LogAssert.Expect(LogType.Warning, "Load failed: notes base directory not found.");
        var loaded = mgr.LoadNote("Anything");
        Assert.IsNull(loaded);
    }

    /// <summary>
    /// Verifies that LoadNote returns null when the provided title is blank or whitespace.
    /// </summary>
    [Test]
    public void LoadNote_BlankTitle_ReturnsNull()
    {
        var loaded = mgr.LoadNote("   ");
        Assert.IsNull(loaded);
    }

    /// <summary>
    /// Verifies that LoadNote returns null when base directory exists but the specific file is missing.
    /// </summary>
    [Test]
    public void LoadNote_BaseExists_But_FileMissing_ReturnsNull()
    {
        Directory.CreateDirectory(BaseNotesFolder);
        var loaded = mgr.LoadNote("MissingFile");
        Assert.IsNull(loaded);
    }

    /// <summary>
    /// Verifies that LoadNote logs an error and returns null when the JSON is invalid,
    /// covering the exception path in the single-file loader.
    /// </summary>
    [Test]
    public void LoadNote_BadJson_LogsError_ReturnsNull()
    {
        // Arrange: create an invalid JSON file under notes/global
        var baseFolder = Path.Combine(Application.persistentDataPath, "notes", "global");
        Directory.CreateDirectory(baseFolder);
        File.WriteAllText(Path.Combine(baseFolder, "Bad1.json"), "{ not json"); // malformed

        LogAssert.Expect(LogType.Error, new Regex("Failed to load note"));

        var go = new GameObject();
        var mgr = go.AddComponent<NoteManager>();
        var loaded = mgr.LoadNote("Bad1");

        Assert.IsNull(loaded);
    }

    /// <summary>
    /// Verifies that LoadNote warns when multiple files with same title exist in different folders,
    /// and still returns the first one successfully.
    /// </summary>
    [Test]
    public void LoadNote_MultipleMatches_ReturnsFirst_WithWarning()
    {
        Directory.CreateDirectory(GlobalFolder);
        var lecFolder = Path.Combine(BaseNotesFolder, "lectures", "lec");
        Directory.CreateDirectory(lecFolder);

        File.WriteAllText(Path.Combine(GlobalFolder, "Same.json"), JsonUtility.ToJson(new Note("Same", "G")));
        File.WriteAllText(Path.Combine(lecFolder, "Same.json"), JsonUtility.ToJson(new Note("Same", "L")));

        LogAssert.Expect(LogType.Warning, new Regex("multiple files named 'Same.json'"));
        var loaded = mgr.LoadNote("Same");
        Assert.IsNotNull(loaded);
    }

    /// <summary>
    /// Verifies that saving and then loading a note preserves its title and content.
    /// Also checks that the JSON file contains the expected fields.
    /// </summary>
    [Test]
    public void Save_Then_Load_RoundTrip_ContentMatches()
    {
        var n = new Note("Round", "Hello");
        mgr.SaveNote(n);

        var text = File.ReadAllText(FileOfGlobal("Round"));
        var compact = new string(text.Where(c => !char.IsWhiteSpace(c)).ToArray());
        StringAssert.Contains("\"Title\":\"Round\"", compact);
        StringAssert.Contains("\"Content\":\"Hello\"", compact);

        var loaded = mgr.LoadNote("Round");
        Assert.That(loaded.Title, Is.EqualTo("Round"));
        Assert.That(loaded.Content, Is.EqualTo("Hello"));
    }

    /// <summary>
    /// Verifies that LoadGlobalNoteByTitle logs an error and returns null on invalid JSON.
    /// </summary>
    [Test]
    public void LoadGlobalNoteByTitle_BadJson_LogsError_ReturnsNull()
    {
        var global = Path.Combine(Application.persistentDataPath, "notes", "global");
        Directory.CreateDirectory(global);
        File.WriteAllText(Path.Combine(global, "GX.json"), "{ not json");

        LogAssert.Expect(LogType.Error, new Regex("Load global failed"));
        var mgr = new GameObject().AddComponent<NoteManager>();
        Assert.IsNull(mgr.LoadGlobalNoteByTitle("GX"));
    }

    /// <summary>
    /// Verifies that LoadNoteByTitleInLecture logs an error and returns null on invalid JSON.
    /// </summary>
    [Test]
    public void LoadNoteByTitleInLecture_BadJson_LogsError_ReturnsNull()
    {
        var lecture = "Intro 101";
        var slug = "intro-101";
        var folder = Path.Combine(Application.persistentDataPath, "notes", "lectures", slug);
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "LX.json"), "{ not json");

        LogAssert.Expect(LogType.Error, new Regex("Load in lecture failed"));
        var mgr = new GameObject().AddComponent<NoteManager>();
        Assert.IsNull(mgr.LoadNoteByTitleInLecture(lecture, "LX"));
    }

    // -------------------- LOAD (batch) --------------------

    /// <summary>
    /// Verifies that LoadAllNotes returns an empty list when the folder is missing,
    /// and also clears the Notes list to stay in sync.
    /// </summary>
    [Test]
    public void LoadAllNotes_FolderMissing_ReturnsEmpty_AndSyncsNotes()
    {
        var list = mgr.LoadAllNotes();
        Assert.That(list, Is.Empty);
        Assert.That(mgr.Notes, Is.Empty);
    }

    /// <summary>
    /// Verifies that LoadAllNotes returns all saved notes from JSON files with correct content.
    /// </summary>
    [Test]
    public void LoadAllNotes_Returns_All_Saved_Files()
    {
        mgr.SaveNote(new Note("N1", "C1"));
        mgr.SaveNote(new Note("N2", "C2"));

        var list = mgr.LoadAllNotes();
        CollectionAssert.AreEquivalent(new[] { "N1", "N2" }, list.Select(x => x.Title));
        Assert.That(list.First(x => x.Title == "N1").Content, Is.EqualTo("C1"));
        Assert.That(list.First(x => x.Title == "N2").Content, Is.EqualTo("C2"));
    }

    /// <summary>
    /// Verifies that LoadAllNotes logs an error and skips invalid files
    /// when encountering a corrupted JSON.
    /// Ensures the exception handling and Debug.LogError branch are covered.
    /// </summary>
    [Test]
    public void LoadAllNotes_BadJson_LogsError_And_SkipsFile()
    {
        // Arrange: create a folder with an invalid JSON file
        var baseFolder = Path.Combine(Application.persistentDataPath, "notes");
        Directory.CreateDirectory(baseFolder);
        File.WriteAllText(Path.Combine(baseFolder, "bad.json"), "{ not json"); // malformed JSON

        LogAssert.Expect(LogType.Error, new Regex("Failed to load note"));

        // Act: attempt to load all notes
        var go = new GameObject();
        var mgr = go.AddComponent<NoteManager>();
        var list = mgr.LoadAllNotes();

        // Assert: invalid file should be ignored, resulting in an empty list
        Assert.That(list, Is.Empty);
    }

    // -------------------- EDIT --------------------

    /// <summary>
    /// Verifies that EditNote immediately returns when given a null note.
    /// </summary>
    [Test]
    public void EditNote_Null_EarlyReturn()
    {
        mgr.EditNote(null, "X", "Y");
        Assert.Pass();
    }

    /// <summary>
    /// Verifies that EditNote immediately returns when the note is not in the list,
    /// and does not create any file.
    /// </summary>
    [Test]
    public void EditNote_NoteNotInList_EarlyReturn_NoFileCreated()
    {
        var ghost = new Note("Ghost", "g");
        mgr.EditNote(ghost, "New", "n");
        Assert.That(Directory.Exists(BaseNotesFolder), Is.False);
    }

    /// <summary>
    /// Verifies that renaming is rejected when another note already uses the target title.
    /// </summary>
    [Test]
    public void EditNote_Rename_Rejected_When_TargetTitleExists()
    {
        var a = new Note("A", "1"); var b = new Note("B", "2");
        mgr.AddNote(a); mgr.AddNote(b);

        mgr.EditNote(a, "B", "new"); // collision
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

        mgr.EditNote(n, "Same", "new"); // skip rename, update + save
        var loaded = mgr.LoadNote("Same");
        Assert.That(loaded.Content, Is.EqualTo("new"));
        Assert.That(File.Exists(FileOfGlobal("Same")), Is.True);
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
        mgr.SaveNote(n); // ensure Old.json exists

        mgr.EditNote(n, "New", "c2");

        Assert.That(File.Exists(FileOfGlobal("Old")), Is.False);
        Assert.That(File.Exists(FileOfGlobal("New")), Is.True);
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
        mgr.AddNote(n); // no X.json on disk

        mgr.EditNote(n, "Y", "d"); // skip rename, still saves new
        Assert.That(File.Exists(FileOfGlobal("X")), Is.False);
        Assert.That(File.Exists(FileOfGlobal("Y")), Is.True);
        var re = mgr.LoadNote("Y");
        Assert.That(re.Content, Is.EqualTo("d"));
    }

    /// <summary>
    /// Verifies EditNote catches exceptions when renaming to an existing file causes File.Move to throw.
    /// </summary>
    [Test]
    public void EditNote_FileMove_Throws_IsCaught()
    {
        var n = new Note("OldZ", "c");
        mgr.AddNote(n);
        mgr.SaveNote(n);

        Directory.CreateDirectory(GlobalFolder);
        File.WriteAllText(Path.Combine(GlobalFolder, "NewZ.json"), "{}"); // Make target exist

        LogAssert.Expect(LogType.Error, new Regex("Failed to rename note file"));
        mgr.EditNote(n, "NewZ", "new");
    }

    // -------------------- LOOKUP (by title) --------------------

    /// <summary>
    /// Verifies that SaveNote + LoadGlobalNoteByTitle works for a global note,
    /// and that GetGlobalNotes returns the saved note.
    /// </summary>
    [Test]
    public void GlobalNote_Save_Then_Load_ByTitle_And_Listing_Works()
    {
        var n = new Note("G1", "global content"); // Global
        mgr.AddNote(n);
        mgr.SaveNote(n);

        var loaded = mgr.LoadGlobalNoteByTitle("G1");
        Assert.IsNotNull(loaded);
        Assert.AreEqual("G1", loaded.Title);
        Assert.AreEqual("global content", loaded.Content);

        var all = mgr.LoadAllNotes();
        CollectionAssert.Contains(all.Select(x => x.Title).ToArray(), "G1");

        var globals = mgr.GetGlobalNotes();
        CollectionAssert.Contains(globals.Select(x => x.Title).ToArray(), "G1");
    }

    /// <summary>
    /// Verifies that SaveNote + LoadNoteByTitleInLecture works for a lecture note,
    /// and that GetNotesForLecture returns the saved note.
    /// </summary>
    [Test]
    public void LectureNote_Save_Then_Load_ByTitleInLecture_And_Listing_Works()
    {
        var lecture = "My Lecture 101"; // slug: my-lecture-101
        var n = new Note("L1", "lecture content") { LectureTitle = lecture };
        mgr.AddNote(n);
        mgr.SaveNote(n);

        var loaded = mgr.LoadNoteByTitleInLecture(lecture, "L1");
        Assert.IsNotNull(loaded);
        Assert.AreEqual("L1", loaded.Title);
        Assert.AreEqual("lecture content", loaded.Content);
        Assert.AreEqual(lecture, loaded.LectureTitle);

        var list = mgr.LoadAllNotes();
        var forLecture = mgr.GetNotesForLecture(lecture);
        Assert.That(forLecture.Count, Is.EqualTo(1));
        Assert.AreEqual("L1", forLecture[0].Title);
    }

    /// <summary>
    /// Verifies not found branches for direct loaders: LoadGlobalNoteByTitle / LoadNoteByTitleInLecture.
    /// </summary>
    [Test]
    public void Load_ByTitle_NotFound_Branches()
    {
        LogAssert.Expect(LogType.Warning, new Regex("Global note 'NG' not found"));
        Assert.IsNull(mgr.LoadGlobalNoteByTitle("NG"));

        LogAssert.Expect(LogType.Warning, new Regex("not found in lecture"));
        Assert.IsNull(mgr.LoadNoteByTitleInLecture("Some Lec", "NX"));
    }

    // -------------------- DELETE --------------------

    /// <summary>
    /// Verifies delete-by-title early return when the note is not present in the in-memory list.
    /// </summary>
    [Test]
    public void Delete_ByTitle_NoteNotInList_EarlyReturn()
    {
        LogAssert.Expect(LogType.Warning, "Delete failed: global note not found.");
        mgr.DeleteGlobalNoteByTitle("NOPE");

        LogAssert.Expect(LogType.Warning, new Regex("not found in lecture"));
        mgr.DeleteNoteByTitleInLecture("L", "NOPE");
    }

    /// <summary>
    /// Verifies that DeleteGlobalNoteByTitle deletes the file when it exists,
    /// and refreshes the in-memory list from disk.
    /// </summary>
    [Test]
    public void DeleteGlobalNoteByTitle_FileExists_DeletesAndReloads()
    {
        var n = new Note("DG1", "c");
        mgr.AddNote(n);
        mgr.SaveNote(n); // ensure file exists

        mgr.DeleteGlobalNoteByTitle("DG1");
        Assert.That(File.Exists(Path.Combine(GlobalFolder, "DG1.json")), Is.False);
        Assert.That(mgr.Notes, Is.Empty);
    }

    /// <summary>
    /// Verifies that DeleteGlobalNoteByTitle logs a warning when the file is missing,
    /// while still removing the note and reloading the in-memory list.
    /// </summary>
    [Test]
    public void DeleteGlobalNoteByTitle_FileMissing_LogsWarning_AndReloads()
    {
        var n = new Note("Gone", "c");   // not saved → no file on disk
        mgr.AddNote(n);

        LogAssert.Expect(LogType.Warning, new Regex("Global note removed from list, but file not found"));
        mgr.DeleteGlobalNoteByTitle("Gone");

        Assert.That(mgr.Notes, Is.Empty);
    }

    /// <summary>
    /// Verifies that DeleteNoteByTitleInLecture removes the item and logs a warning
    /// when the file is missing; still refreshes the list from disk (which becomes empty).
    /// </summary>
    [Test]
    public void DeleteNoteByTitleInLecture_FileMissing_WarnsAndReloads()
    {
        var lecture = "Intro to XR";
        var n = new Note("D1", "x") { LectureTitle = lecture };
        mgr.AddNote(n); // not saved → no file on disk

        mgr.DeleteNoteByTitleInLecture(lecture, "D1");
        Assert.That(File.Exists(FileOfInLecture(lecture, "D1")), Is.False);
        Assert.That(mgr.Notes, Is.Empty);
    }
}
