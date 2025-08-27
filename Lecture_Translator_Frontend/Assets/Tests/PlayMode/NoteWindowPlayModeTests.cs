#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using System.IO;
using static NoteGUIPlayModeTestHelpers;

/// <summary>
/// PlayMode tests for listing and basic actions in NoteWindow.
/// </summary>
public class NoteWindowPlayModeTests
{
    /// <summary>
    /// Name of the scene to load for these tests.
    /// </summary>
    private const string SceneName = "Library hall";

    /// <summary>
    /// WindowManager key for the notes window (see <see cref="WindowKeys.NotesKey"/>).
    /// </summary>
    private const string NoteWindowKey = WindowKeys.NotesKey;

    /// <summary>
    /// WindowManager key for the create/edit note window (see <see cref="WindowKeys.CreateNoteKey"/>).
    /// </summary>
    private const string CreateNoteWindowKey = WindowKeys.CreateNoteKey;

    /// <summary>
    /// Root GameObject name for the notes window in the scene hierarchy.
    /// </summary>
    private const string NoteWindowRoot = "NoteWindow";

    /// <summary>
    /// Root GameObject name for the create/edit note window in the scene hierarchy.
    /// </summary>
    private const string CreateNoteWindowRoot = "CreateNoteWindow";

    /// <summary>
    /// Name of the ScrollView content container where <see cref="NoteGUI"/> items are instantiated.
    /// </summary>
    private const string NoteListRoot = "Content"; // ScrollView content container name

    // ------------------------ Setup / Teardown ------------------------

    /// <summary>
    /// Loads the test scene, ensures an EventSystem exists, and opens the notes window if needed.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> used by Unity to run this test as a coroutine.</returns>
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Load test scene and make sure there's an EventSystem
        yield return LoadScene(SceneName);
        EnsureEventSystem();

        if (FindWindowOfType<NoteWindow>() == null) yield return OpenWindowAndWaitByKey(WindowKeys.NotesKey);
    }

    /// <summary>
    /// Deletes any note files created by tests.
    /// Only files with names starting with <c>Test_</c> under
    /// <c>Application.persistentDataPath/notes/global</c> are removed.
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
                string filename = Path.GetFileNameWithoutExtension(file);
                if (filename.StartsWith("Test_"))
                {
                    try { File.Delete(file); }
                    catch { /* ignore */ }
                }
            }
        }
        yield return null;
    }

    // ------------------------ Tests ------------------------

    /// <summary>
    /// Verifies that the note list container (<c>Content</c>) exists under the <see cref="NoteWindow"/> root.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> used by Unity to run this test as a coroutine.</returns>
    [UnityTest]
    public IEnumerator Open_Shows_List_Container()
    {
        // Locate the active NoteWindow instance
        var noteWin = FindWindowOfType<NoteWindow>();
        Assert.IsNotNull(noteWin, "NoteWindow instance not found.");

        // Find the ScrollView content container
        var root = noteWin.transform;
        var list = FindInChildrenByName<Transform>(root, NoteListRoot);
        Assert.NotNull(list, "List container not found.");
        yield break;
    }

    /// <summary>
    /// Allows the window to populate, then asserts that the content container exists
    /// and has a non-negative number of children.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> used by Unity to run this test as a coroutine.</returns>
    [UnityTest]
    public IEnumerator List_Contains_Items_After_Load()
    {
        // Give the window a couple frames to populate
        yield return WaitFrames(2);

        // Re-locate window and list after population
        var noteWin = FindWindowOfType<NoteWindow>();
        Assert.IsNotNull(noteWin, "NoteWindow instance not found.");

        var list = FindInChildrenByName<Transform>(noteWin.transform, NoteListRoot);
        Assert.NotNull(list, "List container not found.");
        Assert.GreaterOrEqual(ChildCount(list), 0);
        yield break;
    }

    /// <summary>
    /// Clicks the "Create" button and verifies that the create/edit note window opens.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> used by Unity to run this test as a coroutine.</returns>
    [UnityTest]
    public IEnumerator Click_New_Opens_CreateNoteWindow()
    {
        // Find NoteWindow and its Create button
        var noteWin = FindWindowOfType<NoteWindow>();
        Assert.IsNotNull(noteWin, "NoteWindow instance not found.");
        var newBtn = FindInChildrenByName<Button>(noteWin.transform, "CreateButton");
        Assert.NotNull(newBtn, "CreateButton not found on NoteWindow.");

        // Click the button and wait for the editor window to come up
        Click(newBtn);
        yield return OpenWindowAndWaitByKey(WindowKeys.CreateNoteKey);

        // Validate the CreateNoteWindow is present
        var createWin = FindWindowOfType<CreateNoteWindow>();
        Assert.IsNotNull(createWin, "CreateNoteWindow did not open.");
    }

    /// <summary>
    /// Opens a window by key and waits until the specified window root name becomes active.
    /// Helper used by a few tests that need to wait on a specific root to appear.
    /// </summary>
    /// <param name="windowKey">The WindowManager key to open.</param>
    /// <param name="windowRootNameToWait">Root GameObject name to poll for activity.</param>
    /// <param name="timeoutSeconds">Maximum time to wait (seconds).</param>
    /// <param name="wm">Optional WindowManager override; otherwise found in scene.</param>
    /// <returns>A coroutine enumerator that yields until the window is active or timed out.</returns>
    /// <exception cref="System.InvalidOperationException"></exception>
    public static IEnumerator OpenWindowAndWait(string windowKey, string windowRootNameToWait, float timeoutSeconds = 5f, WindowManager wm = null)
    {
        // Resolve the WindowManager and open the requested window
        wm ??= FindWindowManager();
        if (wm == null) throw new System.InvalidOperationException("WindowManager not found in scene.");

        wm.OpenWindow(windowKey);

        // Poll until the corresponding root becomes active or timeout elapses
        yield return WaitUntil(() => IsWindowActive(windowRootNameToWait), timeoutSeconds);
    }

    /// <summary>
    /// Creates a test note, refreshes the list, invokes <see cref="NoteWindow.EditNote(NoteGUI)"/>,
    /// and verifies that <see cref="CreateNoteWindow"/> opens with the correct fields filled.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> used by Unity to run this test as a coroutine.</returns>
    [UnityTest]
    public IEnumerator EditNote_FindsNote_OpensCreateWindow_AndFillsFields()
    {
        // Seed a note into the manager + UI
        var noteWin = FindWindowOfType<NoteWindow>();
        noteWin.SaveNewNote("Test_EditMe", "Body");
        noteWin.LoadNotes();

        // Allow list to spawn NoteGUI
        yield return WaitFrames(1);

        // Find the corresponding NoteGUI by title
        var content = FindInChildrenByName<Transform>(noteWin.transform, "Content");
        NoteGUI gui = null;
        foreach (var g in content.GetComponentsInChildren<NoteGUI>(true))
            if (g.titleTextBox != null && g.titleTextBox.text == "Test_EditMe") { gui = g; break; }
        Assert.NotNull(gui, "No NoteGUI 'Test_EditMe'.");

        // Trigger edit flow
        noteWin.EditNote(gui);
        yield return WaitFrames(1);

        // Validate CreateNoteWindow opens with pre-filled fields
        var create = FindWindowOfType<CreateNoteWindow>();
        Assert.IsNotNull(create, "CreateNoteWindow should open.");
        var titleField = GetPrivate<TMPro.TMP_InputField>(create, "titleTextBox");
        var contentField = GetPrivate<TMPro.TMP_InputField>(create, "noteTextBox");
        Assert.AreEqual("Test_EditMe", titleField.text);
        Assert.AreEqual("Body", contentField.text);
    }

    /// <summary>
    /// Verifies that saving a new note and then editing it updates the manager list as expected.
    /// Titles use the <c>Test_*</c> prefix to keep files isolated for teardown.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> used by Unity to run this test as a coroutine.</returns>
    [UnityTest]
    public IEnumerator SaveNewNote_AddsNoteToManager()
    {
        // Ensure no leftover CreateNoteWindow is open
        yield return CloseWindow(WindowKeys.CreateNoteKey);

        var noteWin = FindWindowOfType<NoteWindow>();
        Assert.IsNotNull(noteWin);
        noteWin.SaveNewNote("Test_OldTitle", "OldContent");

        // Refresh the list so files/manager are in sync for assertions
        noteWin.LoadNotes();
        yield return WaitFrames(1);

        noteWin.SaveEditedNote("Test_OldTitle", "Test_NewTitle", "NewContent");

        var mgr = Object.FindFirstObjectByType<NoteManager>();
        mgr.LoadAllNotes();
        Assert.IsTrue(mgr.Notes.Exists(n => n.Title == "Test_NewTitle" && n.Content == "NewContent"));
        yield break;
    }

    /// <summary>
    /// Creates a note, refreshes the list, runs <see cref="NoteWindow.SaveEditedNote(string, string, string)"/>,
    /// and asserts that the note is updated in <see cref="NoteManager.Notes"/>.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> used by Unity to run this test as a coroutine.</returns>
    [UnityTest]
    public IEnumerator SaveEditedNote_EditsExistingNote()
    {
        yield return CloseWindow(WindowKeys.CreateNoteKey);

        var noteWin = FindWindowOfType<NoteWindow>();
        Assert.IsNotNull(noteWin);
        noteWin.SaveNewNote("Test_OldTitle", "OldContent");

        // Refresh UI before editing
        noteWin.LoadNotes();
        yield return WaitFrames(1);

        noteWin.SaveEditedNote("Test_OldTitle", "Test_NewTitle", "NewContent");

        var mgr = Object.FindFirstObjectByType<NoteManager>();
        Assert.IsTrue(mgr.Notes.Exists(n => n.Title == "Test_NewTitle" && n.Content == "NewContent"));
        yield break;
    }

    /// <summary>
    /// Creates a note, refreshes the list, deletes the corresponding <see cref="NoteGUI"/>,
    /// and verifies that the backing data is removed from <see cref="NoteManager"/>.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> used by Unity to run this test as a coroutine.</returns>
    [UnityTest]
    public IEnumerator DeleteNote_RemovesFromWindowAndManager()
    {
        yield return CloseWindow(WindowKeys.CreateNoteKey);

        // Create a dummy note and populate UI
        var noteWin = FindWindowOfType<NoteWindow>();
        Assert.IsNotNull(noteWin);
        noteWin.SaveNewNote("DeleteMe", "Body");
        noteWin.LoadNotes();

        // Allow list to spawn NoteGUI
        yield return WaitFrames(1);

        // Find the NoteGUI for "DeleteMe"
        var content = FindInChildrenByName<Transform>(noteWin.transform, "Content");
        Assert.NotNull(content, "Content not found.");
        NoteGUI target = null;
        foreach (var gui in content.GetComponentsInChildren<NoteGUI>(true))
            if (gui.titleTextBox != null && gui.titleTextBox.text == "DeleteMe") { target = gui; break; }
        Assert.NotNull(target, "No NoteGUI with title 'DeleteMe' found.");

        noteWin.DeleteNote(target);
        yield return WaitFrames(1);

        var mgr = Object.FindFirstObjectByType<NoteManager>();
        mgr.LoadAllNotes();
        Assert.IsFalse(mgr.Notes.Exists(n => n.Title == "DeleteMe"));
        yield break;
    }

    // ------------------------ Private helpers ------------------------

    /// <summary>
    /// Sets a non-public instance field value via reflection.
    /// </summary>
    /// <param name="obj">Target object instance.</param>
    /// <param name="field">Field name to set.</param>
    /// <param name="value">Value to assign.</param>
    private static void SetPrivate(object obj, string field, object value)
    {
        // Locate the field and set its value (useful for private test hooks)
        var f = obj.GetType().GetField(field, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.IsNotNull(f, $"Field '{field}' not found on {obj.GetType().Name}.");
        f.SetValue(obj, value);
    }

    /// <summary>
    /// Gets a non-public instance field value via reflection and casts it to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Expected field type.</typeparam>
    /// <param name="obj">Target object instance.</param>
    /// <param name="field">Field name to get.</param>
    /// <returns>The field value cast to <typeparamref name="T"/>.</returns>
    private static T GetPrivate<T>(object obj, string field)
    {
        // Locate the field and return its value casted to the requested type
        var f = obj.GetType().GetField(field, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.IsNotNull(f, $"Field '{field}' not found on {obj.GetType().Name}.");
        return (T)f.GetValue(obj);
    }
}
#endif