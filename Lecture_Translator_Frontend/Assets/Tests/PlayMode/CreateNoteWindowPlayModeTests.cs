#if UNITY_EDITOR
using System.Collections;
using System.IO;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using static NoteGUIPlayModeTestHelpers;

/// <summary>
/// PlayMode tests for <see cref="CreateNoteWindow"/> covering create/edit flows,
/// validation (empty title), discarding, and field population.
/// Uses the <c>Test_*</c> prefix for titles and cleans up persisted files in <see cref="TearDown"/>.
/// </summary>
public class CreateNoteWindowPlayModeTests
{
    /// <summary>
    /// Name of the scene used by these tests.
    /// </summary>
    private const string SceneName = "Library hall";

    // ------------------------ Setup / Teardown ------------------------

    /// <summary>
    /// Loads the test scene, ensures an <see cref="UnityEngine.EventSystems.EventSystem"/> exists,
    /// and makes sure a <see cref="NoteWindow"/> is available (the create window is opened from it).
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> used by Unity to run this test as a coroutine.</returns>
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        yield return LoadScene(SceneName);
        EnsureEventSystem();

        if (FindWindowOfType<NoteWindow>() == null)
            yield return OpenWindowAndWaitByKey(WindowKeys.NotesKey);
    }

    /// <summary>
    /// Deletes any test-created note files (<c>Test_*.json</c>) from
    /// <c>Application.persistentDataPath/notes/global</c> so runs stay isolated.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> used by Unity to run this test as a coroutine.</returns>
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        string notesPath = Path.Combine(Application.persistentDataPath, "notes", "global");
        if (Directory.Exists(notesPath))
        {
            foreach (var file in Directory.GetFiles(notesPath, "*.json"))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                if (name.StartsWith("Test_"))
                {
                    try { File.Delete(file); } catch { }
                }
            }
        }
        yield return null;
    }

    // ------------------------ Tests ------------------------

    /// <summary>
    /// Verifies that <see cref="CreateNoteWindow.Initialize(bool)"/> with <c>false</c>
    /// clears both input fields (create mode).
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> used by Unity to run this test as a coroutine.</returns>
    [UnityTest]
    public IEnumerator Initialize_CreateMode_ClearsFields()
    {
        var noteWin = FindWindowOfType<NoteWindow>();
        Assert.IsNotNull(noteWin);

        noteWin.CreateNote();
        yield return WaitFrames(1);

        var create = FindWindowOfType<CreateNoteWindow>();
        Assert.IsNotNull(create, "CreateNoteWindow did not open.");

        // Read input fields via reflection
        var title = GetPrivate<TMP_InputField>(create, "titleTextBox");
        var body = GetPrivate<TMP_InputField>(create, "noteTextBox");

        // In create mode both fields should be empty
        Assert.AreEqual(string.Empty, title.text);
        Assert.AreEqual(string.Empty, body.text);
        yield break;
    }

    /// <summary>
    /// In create mode, <see cref="CreateNoteWindow.Apply"/> should persist the new note,
    /// ask the <see cref="NoteWindow"/> to reload, and close itself.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> used by Unity to run this test as a coroutine.</returns>
    [UnityTest]
    public IEnumerator Apply_Create_Saves_Refreshes_And_Closes()
    {
        var noteWin = FindWindowOfType<NoteWindow>();
        noteWin.CreateNote();

        // Wait for the editor window to appear
        yield return WaitFrames(1);

        // Get the editor and fill fields
        var create = FindWindowOfType<CreateNoteWindow>();
        var title = GetPrivate<TMP_InputField>(create, "titleTextBox");
        var body = GetPrivate<TMP_InputField>(create, "noteTextBox");

        title.text = "Test_NewNote";
        body.text = "Hello";
        create.Apply();

        // Give some frames for close + list reload
        yield return WaitFrames(2);

        // The create window should be closed by now
        Assert.IsNull(FindWindowOfType<CreateNoteWindow>());

        // Verify the data has been persisted
        var mgr = Object.FindFirstObjectByType<NoteManager>();
        mgr.LoadAllNotes(); // refresh from disk
        Assert.IsTrue(mgr.Notes.Exists(n => n.Title == "Test_NewNote" && n.Content == "Hello"));
        yield break;
    }

    /// <summary>
    /// In edit mode, <see cref="CreateNoteWindow.Apply"/> should update the existing note,
    /// ask the <see cref="NoteWindow"/> to reload, and close itself.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> used by Unity to run this test as a coroutine.</returns>
    [UnityTest]
    public IEnumerator Apply_Edit_EditsExisting_Refreshes_And_Closes()
    {
        var noteWin = FindWindowOfType<NoteWindow>();

        // Seed an existing note into the list
        noteWin.SaveNewNote("Test_EditOld", "OldBody");
        noteWin.LoadNotes();

        // Allow the list to populate
        yield return WaitFrames(1);

        // Find its NoteGUI in the spawned list
        var content = FindInChildrenByName<Transform>(noteWin.transform, "Content");
        NoteGUI gui = null;
        foreach (var g in content.GetComponentsInChildren<NoteGUI>(true))
            if (g.titleTextBox != null && g.titleTextBox.text == "Test_EditOld") { gui = g; break; }
        Assert.NotNull(gui, "Missing NoteGUI 'Test_EditOld'.");

        // Enter edit mode for that note
        noteWin.EditNote(gui);
        yield return WaitFrames(1);

        // Validate the editor pre-fills with old values
        var create = FindWindowOfType<CreateNoteWindow>();
        Assert.IsNotNull(create, "CreateNoteWindow should open in edit mode.");

        var title = GetPrivate<TMP_InputField>(create, "titleTextBox");
        var body = GetPrivate<TMP_InputField>(create, "noteTextBox");
        Assert.AreEqual("Test_EditOld", title.text);
        Assert.AreEqual("OldBody", body.text);

        title.text = "Test_EditNew";
        body.text = "NewBody";
        create.Apply();

        // Give frames for close + reload
        yield return WaitFrames(2);

        // Editor should be closed
        Assert.IsNull(FindWindowOfType<CreateNoteWindow>());

        // Verify that the note has been updated and the old entry no longer exists
        var mgr = Object.FindFirstObjectByType<NoteManager>();
        mgr.LoadAllNotes();
        Assert.IsTrue(mgr.Notes.Exists(n => n.Title == "Test_EditNew" && n.Content == "NewBody"));
        Assert.IsFalse(mgr.Notes.Exists(n => n.Title == "Test_EditOld"));
        yield break;
    }

    /// <summary>
    /// When the title is empty, <see cref="CreateNoteWindow.Apply"/> should warn and early-return
    /// without saving or closing the window.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> used by Unity to run this test as a coroutine.</returns>
    [UnityTest]
    public IEnumerator Apply_EmptyTitle_LogsWarning_StaysOpen_And_NoSave()
    {
        var noteWin = FindWindowOfType<NoteWindow>();
        noteWin.CreateNote();
        yield return WaitFrames(1);

        // Access fields
        var create = FindWindowOfType<CreateNoteWindow>();
        var title = GetPrivate<TMP_InputField>(create, "titleTextBox");
        var body = GetPrivate<TMP_InputField>(create, "noteTextBox");

        title.text = "";
        body.text = "ignored";
        LogAssert.Expect(LogType.Warning, "Note title cannot be empty!");
        create.Apply();

        // Give time for logging
        yield return WaitFrames(1);

        // Window should remain open and no data should be saved
        Assert.IsNotNull(FindWindowOfType<CreateNoteWindow>());

        var mgr = Object.FindFirstObjectByType<NoteManager>();
        mgr.LoadAllNotes();
        Assert.IsFalse(mgr.Notes.Exists(n => n.Content == "ignored"));
        yield break;
    }

    /// <summary>
    /// <see cref="CreateNoteWindow.FillFields(string, string)"/> should populate both input fields.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> used by Unity to run this test as a coroutine.</returns>
    [UnityTest]
    public IEnumerator FillFields_Sets_TextBoxes()
    {
        var noteWin = FindWindowOfType<NoteWindow>();
        noteWin.CreateNote();

        // Wait a frame for instantiation
        yield return WaitFrames(1);

        // Use the public API to set both fields
        var create = FindWindowOfType<CreateNoteWindow>();
        create.FillFields("ABC", "DEF");

        // Verify values are applied to the TMP fields
        var title = GetPrivate<TMP_InputField>(create, "titleTextBox");
        var body = GetPrivate<TMP_InputField>(create, "noteTextBox");
        Assert.AreEqual("ABC", title.text);
        Assert.AreEqual("DEF", body.text);
        yield break;
    }

    /// <summary>
    /// <see cref="CreateNoteWindow.Discard"/> should close the window and not persist anything.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> used by Unity to run this test as a coroutine.</returns>
    [UnityTest]
    public IEnumerator Discard_Closes_WithoutSaving()
    {
        var noteWin = FindWindowOfType<NoteWindow>();
        noteWin.CreateNote();

        // Wait for instantiation
        yield return WaitFrames(1);

        // Type unsaved values, then discard them
        var create = FindWindowOfType<CreateNoteWindow>();
        var title = GetPrivate<TMP_InputField>(create, "titleTextBox");
        var body = GetPrivate<TMP_InputField>(create, "noteTextBox");
        title.text = "Test_Discarded";
        body.text = "ShouldNotSave";

        // Discard changes (should close)
        create.Discard();

        // Give one frame for the window to close
        yield return WaitFrames(1);

        // Confirm the window is closed and nothing persisted
        Assert.IsNull(FindWindowOfType<CreateNoteWindow>());

        var mgr = Object.FindFirstObjectByType<NoteManager>();
        mgr.LoadAllNotes();
        Assert.IsFalse(mgr.Notes.Exists(n => n.Title == "Test_Discarded"));
        yield break;
    }

    // ------------------------ Private helpers ------------------------
    
    /// <summary>
    /// Gets a non-public instance field via reflection and casts it to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Expected field type.</typeparam>
    /// <param name="obj">Target instance.</param>
    /// <param name="field">Non-public field name.</param>
    /// <returns>The field value cast to <typeparamref name="T"/>.</returns>
    private static T GetPrivate<T>(object obj, string field)
    {
        var f = obj.GetType().GetField(field,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.IsNotNull(f, $"Field '{field}' not found on {obj.GetType().Name}.");
        return (T)f.GetValue(obj);
    }
}
#endif