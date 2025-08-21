using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Reflection;

public class LectureBrowserWindowTests
{
    [SetUp]
    public void Setup()
    {
        SceneManager.LoadScene("Library Hall");
    }

    [TearDown]
    public void Teardown()
    {

    }

    [UnityTest] // T1.4.1 / T2.1
    public IEnumerator OpenAndClose_Test()
    {
        var windowManager = GameObject.Find("WindowManager").GetComponent<WindowManager>();
        yield return null;
        var menu = windowManager.OpenWindow("MainMenuWindow");
        yield return null;
        var browserButton = menu.transform.Find("Canvas/Image/Lectures/LectureBrowserButton").GetComponent<Button>();

        browserButton.onClick.Invoke();
        yield return null;

        var browserWindow = GameObject.Find("LectureBrowserWindow(Clone)");
        Assert.IsNotNull(browserWindow, "LectureBrowserWindow should appear after clicking the button in the MainMenuWindow.");

        var closeButton = browserWindow.transform.Find("Canvas/Panel/CloseButton").GetComponent<Button>();
        yield return null;
        closeButton.onClick.Invoke();
        yield return null;

        Assert.IsTrue(browserWindow == null, "Browser window should be closed");
    }

    [UnityTest] // T5.2 Lecture Offline Access | Covers T7.2
    public IEnumerator SelectLecture_Offline_Test()
    {
        var windowManager = GameObject.Find("WindowManager").GetComponent<WindowManager>();
        yield return null;
        var lectureBrowserWindow = windowManager.OpenWindow("LectureBrowserWindow").GetComponent<GUI.LectureBrowserWindow>();
        yield return null;

        List<string> transcriptList = new List<string>();
        transcriptList.Add("English");

        Lecture testLecture = new Lecture("offline_test", "./Data/other/offline_test.mp4", "other/offline_test", transcriptList);

        lectureBrowserWindow.SelectLecture(testLecture);

        yield return null;
        var lecturePlayerWindow = GameObject.Find("LecturePlaybackWindow(Clone)").GetComponent<LecturePlayerWindow>();

        yield return null;
        Assert.IsNotNull(lecturePlayerWindow, "LecturePlayerWindow should appear after selecting a Lecture.");

        var field = typeof(LecturePlayerWindow).GetField("lecture", BindingFlags.NonPublic | BindingFlags.Instance);
        

        Assert.AreEqual(testLecture, field.GetValue(lecturePlayerWindow));
    }

    [UnityTest] // T5.1 Lecture Download | Covers T7.1
    public IEnumerator DownloadLecture_Test()
    {
        var windowManager = GameObject.Find("WindowManager").GetComponent<WindowManager>();
        yield return null;
        var lectureBrowserWindow = windowManager.OpenWindow("LectureBrowserWindow").GetComponent<GUI.LectureBrowserWindow>();
        yield return null;

        Lecture testLecture = new Lecture("Test", "", "other/offline_test", new List<string>());

        lectureBrowserWindow.DownloadLecture(testLecture);
    }

    [UnityTest] // T2.2.1 / T2.2.2
    public IEnumerator Search_Test()
    {
        var windowManager = GameObject.Find("WindowManager").GetComponent<WindowManager>();
        yield return null;
        var lectureBrowserWindow = windowManager.OpenWindow("LectureBrowserWindow").GetComponent<GUI.LectureBrowserWindow>();
        yield return null;

        lectureBrowserWindow.SearchTextInputEnded("Test");
        yield return null;

        var searchParent = GameObject.Find("Canvas/Panel/Lectures/Viewport/SearchParent");

        Assert.IsTrue(3 == searchParent.transform.childCount); // Only works for structure: Data/Test/Other/offline_test.mp4 ; Data/Test/Other/offline_test/
    }
}
