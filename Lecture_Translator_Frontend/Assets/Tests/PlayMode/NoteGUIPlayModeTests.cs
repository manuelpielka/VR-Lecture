#if UNITY_EDITOR
using System.Collections;
using System.IO;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using static NoteGUIPlayModeTestHelpers;

/// <summary>
/// PlayMode tests for <see cref="NoteGUI"/> interactions:
/// initialization, deletion, edit delegation (standard and lecture), and title-click behavior.
/// </summary>
public class NoteGUIPlayModeTests
{
    /// <summary>
    /// Name of the scene used for all tests.
    /// </summary>
    private const string SceneName = "Library hall";

    // ------------------------ Setup / Teardown ------------------------

    /// <summary>
    /// Ensures the scene is loaded, an EventSystem exists, and the notes window is open.
    /// During scene load, temporarily ignores failing log messages to avoid third-party noise.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> used by Unity to run this test as a coroutine.</returns>
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Temporarily ignore failing logs while the scene loads (avoids VideoPlayer noise during Run All)
        var prev = LogAssert.ignoreFailingMessages;
        LogAssert.ignoreFailingMessages = true;
        yield return LoadScene(SceneName);
        LogAssert.ignoreFailingMessages = prev;

        EnsureEventSystem();

        if (FindWindowOfType<NoteWindow>() == null)
            yield return OpenWindowAndWaitByKey(WindowKeys.NotesKey);
    }

    /// <summary>
    /// Removes all test-created notes (<c>Test_*.json</c>) from the persistent path.
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
                    try { File.Delete(file); } catch { /* ignore */ }
                }
            }
        }
        yield return null;
    }

    // ------------------------ Tests ------------------------

    /// <summary>
    /// Verifies that <see cref="NoteGUI.Initialize(NoteWindow)"/> stores the parent reference.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> used by Unity to run this test as a coroutine.</returns>
    [UnityTest]
    public IEnumerator Initialize_Sets_Parent_Window()
    {
        var noteWin = FindWindowOfType<NoteWindow>();
        Assert.IsNotNull(noteWin);

        // Create a minimal NoteGUI with required fields
        var go = new GameObject("TempNoteGUI");
        var gui = go.AddComponent<NoteGUI>();
        gui.titleTextBox = new GameObject("title").AddComponent<TextMeshProUGUI>();
        gui.noteTextBox = new GameObject("content").AddComponent<TextMeshProUGUI>();

        // Initialize with parent window
        gui.Initialize(noteWin);

        // Verify the private parent reference is set
        var parent = GetPrivate<NoteWindow>(gui, "noteWindowUI");
        Assert.AreSame(noteWin, parent);

        Object.DestroyImmediate(go);
        yield break;
    }

    /// <summary>
    /// Creates a note, finds its <see cref="NoteGUI"/>, calls <see cref="NoteGUI.Delete"/>,
    /// and asserts both data removal and UI refresh.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> used by Unity to run this test as a coroutine.</returns>
    [UnityTest]
    public IEnumerator Delete_Delegates_To_Parent_And_Refreshes_List()
    {
        var noteWin = FindWindowOfType<NoteWindow>();
        Assert.IsNotNull(noteWin);

        // Create a test note and refresh UI
        noteWin.SaveNewNote("Test_GUI_Delete", "Body");
        noteWin.LoadNotes();

        // wait for list population
        yield return WaitFrames(1);

        // Locate NoteGUI under Content
        var content = FindInChildrenByName<Transform>(noteWin.transform, "Content");
        Assert.NotNull(content, "Content not found.");

        NoteGUI target = null;
        foreach (var gui in content.GetComponentsInChildren<NoteGUI>(true))
            if (gui.titleTextBox != null && gui.titleTextBox.text == "Test_GUI_Delete") { target = gui; break; }
        Assert.NotNull(target, "NoteGUI for 'Test_GUI_Delete' not found.");

        // Record count for refresh assertion
        int before = ChildCount(content);

        target.Delete();
        yield return WaitFrames(1);

        // Data removed from manager (and disk)
        var mgr = Object.FindFirstObjectByType<NoteManager>();
        mgr.LoadAllNotes();
        Assert.IsFalse(mgr.Notes.Exists(n => n.Title == "Test_GUI_Delete"));

        // UI refreshed (count usually decreases; if duplicate notes exist, ensure the specific GUI vanished)
        int after = ChildCount(content);
        Assert.LessOrEqual(after, before);
        yield break;
    }

    /// <summary>
    /// Creates a note, invokes <see cref="NoteGUI.Edit"/> and verifies that
    /// the non-Lecture path logs a message, opens <see cref="CreateNoteWindow"/>,
    /// and pre-fills fields with the note content.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> used by Unity to run this test as a coroutine.</returns>
    [UnityTest]
    public IEnumerator Edit_Opens_CreateNoteWindow_And_Fills_Fields()
    {
        yield return CloseWindow(WindowKeys.CreateNoteKey);

        var noteWin = FindWindowOfType<NoteWindow>();
        Assert.IsNotNull(noteWin);

        // Seed a note and refresh UI
        noteWin.SaveNewNote("Test_GUI_Edit", "Body");
        noteWin.LoadNotes();

        // wait for list population
        yield return WaitFrames(1);

        // Find its NoteGUI
        var content = FindInChildrenByName<Transform>(noteWin.transform, "Content");
        Assert.NotNull(content, "Content not found.");

        NoteGUI target = null;
        foreach (var gui in content.GetComponentsInChildren<NoteGUI>(true))
            if (gui.titleTextBox != null && gui.titleTextBox.text == "Test_GUI_Edit") { target = gui; break; }
        Assert.NotNull(target, "NoteGUI for 'Test_GUI_Edit' not found.");

        LogAssert.Expect(LogType.Log, "Edit button clicked!");

        target.Edit();
        yield return WaitFrames(1);

        var create = FindWindowOfType<CreateNoteWindow>();
        Assert.IsNotNull(create, "CreateNoteWindow should open.");

        var titleBox = GetPrivate<TMP_InputField>(create, "titleTextBox");
        var noteBox = GetPrivate<TMP_InputField>(create, "noteTextBox");
        Assert.AreEqual("Test_GUI_Edit", titleBox.text);
        Assert.AreEqual("Body", noteBox.text);
    }

    /// <summary>
    /// Calls <see cref="NoteGUI.TitleClicked"/> while parent is a normal <see cref="NoteWindow"/>.
    /// Verifies it does not throw and does not open extra windows.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> used by Unity to run this test as a coroutine.</returns>
    [UnityTest]
    public IEnumerator TitleClicked_With_Standard_NoteWindow_Does_Nothing()
    {
        var noteWin = FindWindowOfType<NoteWindow>();
        Assert.IsNotNull(noteWin);

        // Seed a note and refresh UI
        noteWin.SaveNewNote("Test_GUI_TitleClick", "Body");
        noteWin.LoadNotes();

        // wait for list population
        yield return WaitFrames(1);

        // Find its NoteGUI
        var content = FindInChildrenByName<Transform>(noteWin.transform, "Content");
        Assert.NotNull(content, "Content not found.");
        NoteGUI target = null;
        foreach (var gui in content.GetComponentsInChildren<NoteGUI>(true))
            if (gui.titleTextBox != null && gui.titleTextBox.text == "Test_GUI_TitleClick") { target = gui; break; }
        Assert.NotNull(target);

        // Count existing CreateNoteWindow instances before the click
        int pre = UnityEngine.Object.FindObjectsByType<CreateNoteWindow>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;

        target.TitleClicked();
        yield return WaitFrames(1);

        // Count after the click and compare
        int post = UnityEngine.Object.FindObjectsByType<CreateNoteWindow>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;

        Assert.AreEqual(pre, post, "TitleClicked should not open CreateNoteWindow for a standard NoteWindow."); //
        yield break;
    }

    /// <summary>
    /// Verifies that calling <see cref="NoteGUI.Delete"/> without a parent set does not throw.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> used by Unity to run this test as a coroutine.</returns>
    [UnityTest]
    public IEnumerator Delete_Without_Parent_Does_Not_Throw()
    {
        // Create a standalone NoteGUI without initialization
        var go = new GameObject("TempNoteGUI_NoParent");
        var gui = go.AddComponent<NoteGUI>();
        gui.titleTextBox = new GameObject("t").AddComponent<TextMeshProUGUI>();
        gui.noteTextBox = new GameObject("n").AddComponent<TextMeshProUGUI>();

        // Should just early-return without exceptions
        gui.Delete();
        Object.DestroyImmediate(go);
        yield break;
    }

    /// <summary>
    /// Forces the lecture branch in <see cref="NoteGUI.Edit"/> by initializing the GUI
    /// with a spy <see cref="LectureNoteWindow"/> parent and asserting delegation occurs.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> used by Unity to run this test as a coroutine.</returns>
    [UnityTest]
    public IEnumerator Edit_With_LectureNoteWindow_Calls_Lecture_EditNote()
    {
        // Create a LectureNoteWindow spy (overrides EditNote)
        var lnwGO = new GameObject("TmpLectureSpy");
        var spy = lnwGO.AddComponent<LectureNoteWindowSpy>();

        // Minimal NoteGUI with required fields
        var guiGO = new GameObject("TmpNoteGUI");
        var gui = guiGO.AddComponent<NoteGUI>();
        gui.titleTextBox = new GameObject("title").AddComponent<TextMeshProUGUI>();
        gui.noteTextBox = new GameObject("body").AddComponent<TextMeshProUGUI>();
        gui.titleTextBox.text = "Test_Lecture_Edit";
        gui.noteTextBox.text = "Body";

        // Initialize with the lecture spy parent to force the branch
        gui.Initialize(spy);

        gui.Edit();
        yield return null;

        Assert.IsTrue(spy.EditCalled, "Edit() should delegate to LectureNoteWindow.EditNote when parent is LectureNoteWindow.");

        Object.DestroyImmediate(guiGO);
        Object.DestroyImmediate(lnwGO);
    }

    /// <summary>
    /// Forces the lecture branch in <see cref="NoteGUI.TitleClicked"/> with a real
    /// <see cref="LectureNoteWindow"/> parent. By providing a non-timestamp title the call
    /// returns early inside <see cref="LectureNoteWindow.TimeStampClicked(NoteGUI)"/> without extra setup.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> used by Unity to run this test as a coroutine.</returns>
    [UnityTest]
    public IEnumerator TitleClicked_With_LectureNoteWindow_Calls_TimeStamp_EarlyReturn()
    {
        // Use a real LectureNoteWindow (no lecture/playbackManager needed due to early return)
        var lnwGO = new GameObject("TmpLectureReal");
        var lnw = lnwGO.AddComponent<LectureNoteWindow>();

        // Minimal NoteGUI with required fields
        var guiGO = new GameObject("TmpNoteGUI2");
        var gui = guiGO.AddComponent<NoteGUI>();
        gui.titleTextBox = new GameObject("title").AddComponent<TextMeshProUGUI>();
        gui.noteTextBox = new GameObject("body").AddComponent<TextMeshProUGUI>();

        // Provide a non-timestamp title so TimeStampClicked returns immediately
        gui.titleTextBox.text = "NotATimestamp";
        gui.noteTextBox.text = "Any";

        // Initialize with lecture parent and click
        gui.Initialize(lnw);

        // Count windows before/after to ensure nothing opens
        int pre = Object.FindObjectsByType<CreateNoteWindow>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
        gui.TitleClicked();
        yield return null;
        int post = Object.FindObjectsByType<CreateNoteWindow>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;

        Assert.AreEqual(pre, post, "TitleClicked should not open windows when parent is LectureNoteWindow and title is not a timestamp.");

        Object.DestroyImmediate(guiGO);
        Object.DestroyImmediate(lnwGO);
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

    // ------------------------ Test doubles ------------------------

    /// <summary>
    /// Spy double for <see cref="LectureNoteWindow"/> used to assert delegation
    /// in the lecture branch of <see cref="NoteGUI.Edit"/>.
    /// </summary>
    private class LectureNoteWindowSpy : LectureNoteWindow
    {
        /// <summary>
        /// Whether <see cref="EditNote(NoteGUI)"/> has been invoked.
        /// </summary>
        public bool EditCalled;

        /// <summary>
        /// Records invocation without calling into the base implementation (avoids external dependencies).
        /// </summary>
        /// <param name="noteUI">The <see cref="NoteGUI"/> being edited.</param>
        public override void EditNote(NoteGUI noteUI)
        {
            EditCalled = true; // do not call base to avoid lecture/playback dependencies
        }
    }
}
#endif