using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

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

    private TMP_InputField CreateTMPInputField(string name)
    {
        var go = new GameObject(name);
        var input = go.AddComponent<TMP_InputField>();

        var viewport = new GameObject("Viewport").AddComponent<RectTransform>();
        viewport.SetParent(go.transform, false);
        input.textViewport = viewport;

        var textGO = new GameObject("Text").AddComponent<TextMeshProUGUI>();
        textGO.rectTransform.SetParent(viewport, false);
        input.textComponent = textGO;

        // var ph = new GameObject("Placeholder").AddComponent<TextMeshProUGUI>();
        // ph.rectTransform.SetParent(viewport, false);
        // input.placeholder = ph;

        return input;
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        foreach (var x in UnityEngine.Object.FindObjectsByType<SettingsManager>(FindObjectsSortMode.None))
            UnityEngine.Object.DestroyImmediate(x.gameObject);
        foreach (var x in UnityEngine.Object.FindObjectsByType<DisplayModeController>(FindObjectsSortMode.None))
            UnityEngine.Object.DestroyImmediate(x.gameObject);
        foreach (var x in UnityEngine.Object.FindObjectsByType<CreateLectureNoteWindow>(FindObjectsSortMode.None))
            UnityEngine.Object.DestroyImmediate(x.gameObject);
        foreach (var x in UnityEngine.Object.FindObjectsByType<NoteManager>(FindObjectsSortMode.None))
            UnityEngine.Object.DestroyImmediate(x.gameObject);
        foreach (var x in UnityEngine.Object.FindObjectsByType<NoteWindow>(FindObjectsSortMode.None))
            UnityEngine.Object.DestroyImmediate(x.gameObject);

        _root = new GameObject("CreateLectureNoteWindowRoot");
        _win = _root.AddComponent<CreateLectureNoteWindow>();

        _windowTitle = new GameObject("WindowTitle").AddComponent<TextMeshProUGUI>();
        _toggle = new GameObject("UseTimestampToggle").AddComponent<Toggle>();

        _titleInput = CreateTMPInputField("TitleInput");
        _contentInput = CreateTMPInputField("ContentInput");

        SetField(_win, "windowTitleText", _windowTitle);
        SetField(_win, "useTimestampAsTitleToggle", _toggle);
        SetField(_win, "titleTextBox", _titleInput);
        SetField(_win, "noteTextBox", _contentInput);

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (_noteWndGO) UnityEngine.Object.DestroyImmediate(_noteWndGO);
        if (_mgrGO) UnityEngine.Object.DestroyImmediate(_mgrGO);
        if (_root) UnityEngine.Object.DestroyImmediate(_root);

        yield return null;
    }

    [UnityTest]
    public IEnumerator Initialize_LectureNull_SetsDeletedTitle_And_ConfigToggle()
    {
        MakeNoteManagerInScene();

        _win.Initialize(null, /*timeAtOpen*/ 3661.9, /*isEdit*/ false);
        _win.FillFields("any", "hello");

        Assert.AreEqual("(Lecture deleted)", _windowTitle.text);
        Assert.AreEqual("01-01-01", _titleInput.text);
        Assert.IsFalse(_titleInput.interactable);
        Assert.IsTrue(_toggle.isOn);
        Assert.IsFalse(_toggle.interactable);

        _toggle.onValueChanged.Invoke(false);
        Assert.AreEqual("01-01-01", _titleInput.text);

        yield return null;
    }

    [UnityTest]
    public IEnumerator Initialize_TitleSnapshot_By_Internal()
    {
        MakeNoteManagerInScene();

        var mi = typeof(CreateLectureNoteWindow).GetMethod("InitializeInternal",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(mi);

        mi.Invoke(_win, new object[] { null, "MyLecture", 2.5, 0.0, false });

        Assert.AreEqual("MyLecture", _windowTitle.text);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Apply_Create_AddsNote_SavesNote_LoadsWindow_And_Closes()
    {
        MakeNoteManagerInScene();

        _win.Initialize(null, 10.0, false);
        _win.FillFields("", "content-body");

        LogAssert.Expect(LogType.Log, new Regex(@"JSON content being saved:"));
        LogAssert.Expect(LogType.Log, new Regex(@"Note '00-00-10' saved at"));

        _win.Apply();

        Assert.IsTrue(_noteManager.Notes.Exists(n => n.Title == "00-00-10" && n.Content == "content-body"));

        LogAssert.NoUnexpectedReceived();
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

        _win.Apply();

        Assert.AreEqual("NEW-CONTENT", note.Content);
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

        //LogAssert.ignoreFailingMessages = true; 
        _win.Apply();
        //LogAssert.ignoreFailingMessages = false;
        LogAssert.NoUnexpectedReceived();
        yield return null;
    }

    [UnityTest]
    public IEnumerator Apply_Create_When_NoNoteManager_Throws_And_Catches()
    {
        LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("NoteManager not found"));
        _win.Initialize(null, 1, false);
        _win.FillFields("", "c");

        LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Apply failed"));
        _win.Apply();

        LogAssert.NoUnexpectedReceived();
        yield return null;
    }

    [UnityTest]
    public IEnumerator Discard_Just_Calls_Close()
    {
        _win.Discard(); 
        yield return null;
    }

    [Test]
    public void FormatTimestampForTitle_Boundaries()
    {
        var mi = typeof(CreateLectureNoteWindow)
            .GetMethod("FormatTimestampForTitle", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.AreEqual("00-00-00", mi.Invoke(null, new object[] { -5.0 }));
        Assert.AreEqual("01-01-01", mi.Invoke(null, new object[] { 3661.0 }));
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

        Assert.AreEqual("UnitTestLecture", _windowTitle.text);
        Assert.IsTrue(_toggle.isOn);
        Assert.IsFalse(_toggle.interactable);

        yield return null;
    }

    private void MakeNoteManagerInScene()
    {
        if (NoteManager.Instance != null)
            UnityEngine.Object.DestroyImmediate(NoteManager.Instance.gameObject);

        foreach (var x in UnityEngine.Object.FindObjectsByType<NoteManager>(FindObjectsSortMode.None))
            UnityEngine.Object.DestroyImmediate(x.gameObject);

        _mgrGO = new GameObject("NoteManager");
        _mgrGO.AddComponent<NoteManager>();

        _noteManager = NoteManager.Instance;
        if (_noteManager.Notes == null) _noteManager.Notes = new List<Note>();
    }

    private static void SetField(object o, string name, object val)
    {
        var f = o.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.IsNotNull(f, "Field not found: " + name);
        f.SetValue(o, val);
    }


}
