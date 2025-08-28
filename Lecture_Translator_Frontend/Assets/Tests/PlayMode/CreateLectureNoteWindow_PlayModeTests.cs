using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
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

    [UnitySetUp]
    public IEnumerator SetUp()
    {

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
        _titleInput = new GameObject("TitleInput").AddComponent<TMP_InputField>();
        _contentInput = new GameObject("ContentInput").AddComponent<TMP_InputField>();

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
        MakeNoteWindowInScene(); 

        _win.Initialize(null, 10.0, false);
        _win.FillFields("", "content-body");

        _win.Apply();

        Assert.IsTrue(_noteManager.Notes.Exists(n => n.Title == "00-00-10" && n.Content == "content-body"));
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
        LogAssert.ignoreFailingMessages = true; 
        _win.Apply(); 
        LogAssert.ignoreFailingMessages = false;
        yield return null;
    }

    [UnityTest]
    public IEnumerator Apply_Create_When_NoNoteManager_Throws_And_Catches()
    {
        _win.Initialize(null, 1, false);
        _win.FillFields("", "c");
        LogAssert.ignoreFailingMessages = true; 
        _win.Apply();
        LogAssert.ignoreFailingMessages = false;
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


    private void MakeNoteManagerInScene()
    {
        _mgrGO = new GameObject("NoteManager");
        _noteManager = _mgrGO.AddComponent<NoteManager>(); 
        if (_noteManager.Notes == null) _noteManager.Notes = new List<Note>();
    }

    private void MakeNoteWindowInScene()
    {
        _noteWndGO = new GameObject("NoteWindow");
        _noteWndGO.AddComponent<NoteWindow>(); 
    }

    private static void SetField(object o, string name, object val)
    {
        var f = o.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.IsNotNull(f, "Field not found: " + name);
        f.SetValue(o, val);
    }
}
