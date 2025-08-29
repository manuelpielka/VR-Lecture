using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using BrowsingModule;

public class LectureBrowserWindowTests
{
    GUI.LectureBrowserWindow lectureBrowserWindow;

    [SetUp]
    public void Setup()
    {
        SceneManager.LoadScene("Library Hall");

        if (Directory.Exists("./Data/other"))
            Directory.Delete("./Data/other");
    }

    [TearDown]
    public void Teardown()
    {
        if (lectureBrowserWindow != null)
        {
            var closeButton = lectureBrowserWindow.transform.Find("Canvas/Panel/CloseButton").GetComponent<Button>();
            closeButton.onClick.Invoke();
        }

        if (Directory.Exists("./Data/other"))
            Directory.Delete("./Data/other");
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

    [UnityTest] // T2.5.2 Lecture Offline Access
    public IEnumerator SelectLecture_Offline_Test()
    {
        var windowManager = GameObject.Find("WindowManager").GetComponent<WindowManager>();
        yield return null;
        var lectureBrowserWindow = windowManager.OpenWindow("LectureBrowserWindow").GetComponent<GUI.LectureBrowserWindow>();
        yield return null;

        List<string> transcriptList = new List<string>();
        transcriptList.Add("English");

        Lecture testLecture = new Lecture("offline_test", "./Data/Test/other/offline_test.mp4", "Test/other/offline_test", transcriptList);
        testLecture.SetDownloaded(true);

        lectureBrowserWindow.SelectLecture(testLecture);

        yield return null;
        var lecturePlayerWindow = GameObject.Find("LecturePlaybackWindow(Clone)").GetComponent<LecturePlayerWindow>();

        yield return null;
        Assert.IsNotNull(lecturePlayerWindow, "LecturePlayerWindow should appear after selecting a Lecture.");

        var field = typeof(LecturePlayerWindow).GetField("lecture", BindingFlags.NonPublic | BindingFlags.Instance);
        
        Assert.AreEqual(testLecture, field.GetValue(lecturePlayerWindow));

        yield return null;

        lecturePlayerWindow.Close();
    }

    [UnityTest] // T2.5.1 Lecture Download + Deletion
    public IEnumerator DownloadDeleteLecture_Test()
    {
        var windowManager = GameObject.Find("WindowManager").GetComponent<WindowManager>();
        yield return null;
        var lectureBrowserWindow = windowManager.OpenWindow("LectureBrowserWindow").GetComponent<GUI.LectureBrowserWindow>();

        var manager = lectureBrowserWindow.GetComponent<BrowsingManager>();

        var apiClient = new FakeBrowsingApiClient();

        var field = typeof(BrowsingManager).GetField("apiclient", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(manager, apiClient);

        yield return null;

        var folderbutton = lectureBrowserWindow.transform.Find("Canvas/Panel/Lectures/Viewport/-/FolderUI(Clone)").GetComponent<Button>();

        Assert.IsNotNull(folderbutton);

        yield return new WaitForSeconds(1f);

        folderbutton.onClick.Invoke();

        yield return null;

        var lectureUI = lectureBrowserWindow.transform.Find("Canvas/Panel/Lectures/Viewport/--TestFolder/LectureUI(Clone)").GetComponent<GUI.LectureUI>();

        yield return null;

        lectureUI.OnDownloadClick();

        yield return null;

        Assert.IsTrue(File.Exists("./Data/Test/test.mp4"), "File does not exist.");

        yield return new WaitForSeconds(3f);

        lectureUI.OnDeleteClick();

        yield return null;

        Assert.IsTrue(!File.Exists("./Data/Test/test.mp4"), "File should be deleted.");

        yield return null;
    }

    [UnityTest] // T2.2.1 / T2.2.2
    public IEnumerator MockApi_Test()
    {
        var windowManager = GameObject.Find("WindowManager").GetComponent<WindowManager>();
        yield return null;
        var lectureBrowserWindow = windowManager.OpenWindow("LectureBrowserWindow").GetComponent<GUI.LectureBrowserWindow>();

        var manager = lectureBrowserWindow.GetComponent<BrowsingManager>();

        var apiClient = new FakeBrowsingApiClient();

        var field = typeof(BrowsingManager).GetField("apiclient", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(manager, apiClient);

        yield return null;

        var folderbutton = lectureBrowserWindow.transform.Find("Canvas/Panel/Lectures/Viewport/-/FolderUI(Clone)").GetComponent<Button>();

        Assert.IsNotNull(folderbutton);

        yield return new WaitForSeconds(1f);

        folderbutton.onClick.Invoke();

        yield return null;

        var backButton = lectureBrowserWindow.transform.Find("Canvas/Panel/Lectures/Viewport/--TestFolder/FolderUI(Clone)").GetComponent<Button>();
        var lectureUI = lectureBrowserWindow.transform.Find("Canvas/Panel/Lectures/Viewport/--TestFolder/LectureUI(Clone)").GetComponent<GUI.LectureUI>();

        Assert.IsNotNull(backButton);
        Assert.IsNotNull(lectureUI);

        yield return null;

        backButton.onClick.Invoke();

        yield return null;

        lectureBrowserWindow.SearchTextInputEnded("Test");
        yield return null;

        var searchParent = GameObject.Find("Canvas/Panel/Lectures/Viewport/SearchParent");

        Assert.IsTrue(2 == searchParent.transform.childCount);
    }
}

public class FakeBrowsingApiClient : IApiClient
{
    private int counter = 0;

    public Task<string> PostRequest(string url, string json)
    {
        if (url.Contains("/ltarchive/ls"))
        {
            counter++;
            if (counter == 1)
            {
                return Task.FromResult("[[\"TestFolder\",\"dir\",\"{ }\",false,\"read\"]]");
            }

            if (counter == 2)
            {
                return Task.FromResult("[[\"TestSession\",\"session\",\"{ }\",false,\"read\"], [\" Back\",\"dir\",\"{ }\",false,\"read\"]]");
            }
        }

        return Task.FromResult("");
    }

}