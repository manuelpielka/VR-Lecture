using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// PlayMode tests for CreateLectureNoteWindow. These tests avoid cross-test
/// pollution by temporarily deactivating any pre-existing NoteManager, NoteWindow,
/// or CreateLectureNoteWindow instances, and restoring them after each test.
/// All test-owned objects are created under a dedicated root and cleaned up.
/// </summary>
public class CreateLectureNoteWindow_PlayModeTests
{
    private GameObject _root;
    private CreateLectureNoteWindow _win;

    private TextMeshProUGUI _windowTitle;
    private Toggle _toggle;
    private TMP_InputField _titleInput;
    private TMP_InputField _contentInput;

    private GameObject _mgrGO;
    private NoteManager _noteManager;
    private GameObject _noteWndGO;
    private GameObject _noteUIPrefab;

    private readonly List<GameObject> _preExistingNoteManagers = new List<GameObject>();
    private readonly List<bool> _preExistingNoteManagersActive = new List<bool>();

    private readonly List<GameObject> _preExistingNoteWindows = new List<GameObject>();
    private readonly List<bool> _preExistingNoteWindowsActive = new List<bool>();

    private readonly List<GameObject> _preExistingCreateLectureNoteWindows = new List<GameObject>();
    private readonly List<bool> _preExistingCreateLectureNoteWindowsActive = new List<bool>();

    private readonly List<AudioListener> _preAudio = new();
    private readonly List<bool> _preAudioEnabled = new();
    private readonly List<bool> _preAudioGOActive = new();
    private GameObject _testCamGO;

    private void CaptureAndFixAudioListeners()
    {
        foreach (var al in UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
        {
            _preAudio.Add(al);
            _preAudioEnabled.Add(al.enabled);
            _preAudioGOActive.Add(al.gameObject.activeSelf);
        }

        if (_preAudio.Count == 0)
        {
            _testCamGO = new GameObject("TestCamera (CreateLectureNoteWindowTests)");
            _testCamGO.AddComponent<Camera>();
            _testCamGO.AddComponent<AudioListener>();
            return;
        }

        _preAudio[0].gameObject.SetActive(true);
        _preAudio[0].enabled = true;
        for (int i = 1; i < _preAudio.Count; i++)
            if (_preAudio[i] != null) _preAudio[i].enabled = false;
    }

    private void RestoreAudioListeners()
    {
        if (_testCamGO) UnityEngine.Object.DestroyImmediate(_testCamGO);

        for (int i = 0; i < _preAudio.Count; i++)
        {
            var al = _preAudio[i];
            if (al == null) continue;
            al.enabled = _preAudioEnabled[i];
            if (al.gameObject) al.gameObject.SetActive(_preAudioGOActive[i]);
        }
        _preAudio.Clear();
        _preAudioEnabled.Clear();
        _preAudioGOActive.Clear();
    }

    private static TMP_InputField CreateTMPInputField(string name)
    {
        var go = new GameObject(name);
        var input = go.AddComponent<TMP_InputField>();

        var viewportGO = new GameObject("Viewport");
        var viewport = viewportGO.AddComponent<RectTransform>();
        viewport.SetParent(go.transform, false);
        input.textViewport = viewport;

        var textGO = new GameObject("Text");
        var text = textGO.AddComponent<TextMeshProUGUI>();
        text.rectTransform.SetParent(viewport, false);
        input.textComponent = text;

        return input;
    }

    private static TextMeshProUGUI CreateTMPLabel(string name)
    {
        var go = new GameObject(name);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = "";
        return tmp;
    }

    /// <summary>
    /// Record and temporarily deactivate any pre-existing objects of the given type.
    /// </summary>
    private static void CaptureAndDeactivatePreExisting<T>(
        List<GameObject> storeObjects,
        List<bool> storeActiveStates
    ) where T : UnityEngine.Object
    {
        // FindObjectsByType is cross-scene; we must not destroy them.
        var comps = UnityEngine.Object.FindObjectsByType<T>(FindObjectsSortMode.None);
        foreach (var c in comps)
        {
            if (c is Component comp)
            {
                var go = comp.gameObject;
                storeObjects.Add(go);
                storeActiveStates.Add(go.activeSelf);
                go.SetActive(false);
            }
            else if (c is GameObject go)
            {
                storeObjects.Add(go);
                storeActiveStates.Add(go.activeSelf);
                go.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Restore previously deactivated objects to their original activeSelf state.
    /// </summary>
    private static void RestorePreExisting(List<GameObject> objs, List<bool> actives)
    {
        for (int i = 0; i < objs.Count; i++)
        {
            if (objs[i] != null) objs[i].SetActive(actives[i]);
        }
        objs.Clear();
        actives.Clear();
    }



    private void MakeNoteManagerInScene()
    {
        // Create a test-owned NoteManager that the window can discover.
        _mgrGO = new GameObject("NoteManager(TestOwned)");
        _noteManager = _mgrGO.AddComponent<NoteManager>();
        if (_noteManager.Notes == null) _noteManager.Notes = new List<Note>();
    }

    private void MakeNoteWindowInScene()
    {
        if (_noteManager == null) MakeNoteManagerInScene();

        _noteWndGO = new GameObject("NoteWindow(TestOwned)");
        _noteWndGO.SetActive(false);
        var wnd = _noteWndGO.AddComponent<NoteWindow>();

        // Provide a container and a note prefab with minimal required fields.
        var containerGO = new GameObject("Content");
        containerGO.transform.SetParent(_noteWndGO.transform, false);

        _noteUIPrefab = new GameObject("NoteUIPrefab(TestOwned)");
        var noteGui = _noteUIPrefab.AddComponent<NoteGUI>();
        var title = CreateTMPLabel("TitleText");
        var body = CreateTMPLabel("BodyText");
        title.transform.SetParent(_noteUIPrefab.transform, false);
        body.transform.SetParent(_noteUIPrefab.transform, false);

        SetField(noteGui, "titleTextBox", title);
        SetField(noteGui, "noteTextBox", body);

        SetField(wnd, "noteContainer", containerGO.transform);
        SetField(wnd, "notePrefab", _noteUIPrefab);

        // Initialize the NoteWindow if it has an Initialize method.
        var mi = typeof(NoteWindow).GetMethod(
            "Initialize",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null, new[] { typeof(GameObject), typeof(Transform) }, null
        );
        mi?.Invoke(wnd, new object[] { _noteUIPrefab, containerGO.transform });

        _noteWndGO.SetActive(true);
    }

    private static void SetField(object o, string name, object val)
    {
        var f = o.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.IsNotNull(f, "Field not found: " + name);
        f.SetValue(o, val);
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        CaptureAndFixAudioListeners();

        // Capture and temporarily deactivate any pre-existing instances (no destruction).
        CaptureAndDeactivatePreExisting<NoteManager>(_preExistingNoteManagers, _preExistingNoteManagersActive);
        CaptureAndDeactivatePreExisting<NoteWindow>(_preExistingNoteWindows, _preExistingNoteWindowsActive);
        CaptureAndDeactivatePreExisting<CreateLectureNoteWindow>(_preExistingCreateLectureNoteWindows, _preExistingCreateLectureNoteWindowsActive);

        // Build test-owned UI hierarchy and window under a dedicated root.
        _root = new GameObject("CreateLectureNoteWindowRoot(TestOwned)");
        _win = _root.AddComponent<CreateLectureNoteWindow>();

        _windowTitle = new GameObject("WindowTitle").AddComponent<TextMeshProUGUI>();
        _toggle = new GameObject("UseTimestampToggle").AddComponent<Toggle>();
        _titleInput = CreateTMPInputField("TitleInput");
        _contentInput = CreateTMPInputField("ContentInput");

        SetField(_win, "windowTitleText", _windowTitle);
        SetField(_win, "useTimestampAsTitleToggle", _toggle);
        SetField(_win, "titleTextBox", _titleInput);
        SetField(_win, "noteTextBox", _contentInput);

        // Ensure logs are not ignored unless explicitly toggled in a test.
        LogAssert.ignoreFailingMessages = false;

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        // Clean up test-owned objects.
        if (_noteWndGO) UnityEngine.Object.DestroyImmediate(_noteWndGO);
        if (_noteUIPrefab) UnityEngine.Object.DestroyImmediate(_noteUIPrefab);
        if (_mgrGO) UnityEngine.Object.DestroyImmediate(_mgrGO);
        if (_root) UnityEngine.Object.DestroyImmediate(_root);

        // Restore any pre-existing objects to their original active state.
        RestorePreExisting(_preExistingNoteManagers, _preExistingNoteManagersActive);
        RestorePreExisting(_preExistingNoteWindows, _preExistingNoteWindowsActive);
        RestorePreExisting(_preExistingCreateLectureNoteWindows, _preExistingCreateLectureNoteWindowsActive);

        RestoreAudioListeners();

        // Reset LogAssert to a safe default after each test.
        LogAssert.ignoreFailingMessages = false;

        yield return null;
    }
    [UnityTest]
    public IEnumerator Initialize_LectureNull_SetsDeletedTitle_And_ConfigToggle()
    {
        MakeNoteManagerInScene();

        _win.Initialize(null, /*timeAtOpen*/ 3661.9, /*isEdit*/ false);
        _win.FillFields("any", "hello");
        _win.OnUseTimestampToggleChanged(true);

        Assert.AreEqual("(Lecture deleted)", _windowTitle.text);
        Assert.AreEqual("01-01-01", _titleInput.text);
        Assert.IsFalse(_titleInput.interactable);
        Assert.IsTrue(_toggle.isOn);

        _toggle.onValueChanged.Invoke(false);
        Assert.AreEqual("01-01-01", _titleInput.text);

        LogAssert.NoUnexpectedReceived();
        yield return null;
    }

    [UnityTest]
    public IEnumerator Initialize_TitleSnapshot_By_Internal()
    {
        MakeNoteManagerInScene();

        var mi = typeof(CreateLectureNoteWindow).GetMethod("InitializeInternal",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(mi, "InitializeInternal not found (non-public).");

        mi.Invoke(_win, new object[] { null, "MyLecture", 2.5, 0.0, false });

        Assert.AreEqual("MyLecture", _windowTitle.text);
        LogAssert.NoUnexpectedReceived();
        yield return null;
    }

    [UnityTest]
    public IEnumerator Initialize_With_Lecture_Covers_GetName_Branch()
    {
        MakeNoteManagerInScene();

        var lecture = new Lecture(
           name: "UnitTestLecture",
           videoSource: "v.mp4",
           transcriptSource: "t.json",
           transcriptLanguages: new List<string> { "en" }
        );

        _win.Initialize(lecture, /*timeSecondsAtOpen*/ 7.5, /*isEditMode*/ false);
        _win.FillFields("", "content");
        _win.OnUseTimestampToggleChanged(true);

        Assert.AreEqual("UnitTestLecture", _windowTitle.text);
        Assert.IsTrue(_toggle.isOn);

        LogAssert.NoUnexpectedReceived();
        yield return null;
    }

    [UnityTest]
    public IEnumerator Apply_Create_AddsNote_SavesNote_And_Closes()
    {
        MakeNoteManagerInScene();

        _win.Initialize(null, 10.0, false);
        _win.FillFields("", "content-body");
        _win.OnUseTimestampToggleChanged(true);

        // Expect logs from NoteManager saving
        LogAssert.Expect(LogType.Log, new Regex(@"JSON content being saved:"));
        LogAssert.Expect(LogType.Log, new Regex(@"Note '00-00-10' saved at"));

        _win.Apply();

        Assert.IsTrue(_noteManager.Notes.Exists(n => n.Title == "00-00-10" && n.Content == "content-body"));
        LogAssert.NoUnexpectedReceived();
        yield return null;
    }

    [UnityTest]
    public IEnumerator Apply_Create_LoadsWindow_When_Present()
    {
        MakeNoteManagerInScene();
        MakeNoteWindowInScene();

        _win.Initialize(null, 5.0, false);
        _win.FillFields("", "x");
        _win.OnUseTimestampToggleChanged(true);

        LogAssert.Expect(LogType.Log, new Regex(@"JSON content being saved:"));
        LogAssert.Expect(LogType.Log, new Regex(@"Note '00-00-05' saved at"));

        var prev = LogAssert.ignoreFailingMessages;
        LogAssert.ignoreFailingMessages = true;
        try
        {
            _win.Apply();
        }
        finally
        {
            LogAssert.ignoreFailingMessages = prev;
        }

        Assert.IsTrue(_noteManager.Notes.Exists(n => n.Title == "00-00-05"));
        yield return null;
    }

    [UnityTest]
    public IEnumerator Apply_Edit_UpdatesNote_When_Found()
    {
        MakeNoteManagerInScene();

        var note = new Note("00-00-00", "old") { LectureTitle = "(Lecture deleted)" };
        _noteManager.Notes.Add(note);

        SetField(_win, "isEditMode", true);
        SetField(_win, "originalTitle", "00-00-00");

        _win.Initialize(null, /*openTime*/ 123, /*edit*/ true);
        _win.FillFields("", "NEW-CONTENT");

        LogAssert.Expect(LogType.Log, new Regex(@"JSON content being saved:"));
        LogAssert.Expect(LogType.Log, new Regex(@"Note '00-00-00' saved at"));

        _win.Apply();

        Assert.AreEqual("NEW-CONTENT", note.Content);
        LogAssert.NoUnexpectedReceived();
        yield return null;
    }

    [UnityTest]
    public IEnumerator Apply_Edit_NoteNotFound_Returns()
    {
        MakeNoteManagerInScene();

        SetField(_win, "isEditMode", true);
        SetField(_win, "originalTitle", "NO-SUCH-TITLE");

        _win.Initialize(null, 0, true);
        _win.FillFields("", "x");

        LogAssert.Expect(LogType.Warning, new Regex(@"Edit failed: note 'NO-SUCH-TITLE' not found"));

        _win.Apply();

        LogAssert.NoUnexpectedReceived();
        yield return null;
    }

    [UnityTest]
    public IEnumerator Apply_Create_When_NoNoteManager_Throws_And_Catches()
    {
        LogAssert.Expect(LogType.Error, new Regex(@"NoteManager not found"));
        _win.Initialize(null, 1, false);
        _win.FillFields("", "c");

        LogAssert.Expect(LogType.Error, new Regex(@"Apply failed"));
        _win.Apply();

        LogAssert.NoUnexpectedReceived();
        yield return null;
    }

    [UnityTest]
    public IEnumerator Discard_Just_Calls_Close()
    {
        _win.Discard();
        LogAssert.NoUnexpectedReceived();
        yield return null;
        // Window is test-owned and destroyed in TearDown to avoid leaks.
    }

    [Test]
    public void FormatTimestampForTitle_Boundaries()
    {
        var mi = typeof(CreateLectureNoteWindow)
            .GetMethod("FormatTimestampForTitle", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(mi, "FormatTimestampForTitle not found (private static).");

        Assert.AreEqual("00-00-00", mi.Invoke(null, new object[] { -5.0 }));
        Assert.AreEqual("01-01-01", mi.Invoke(null, new object[] { 3661.0 }));
    }
}