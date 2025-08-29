using UnityEngine;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine.UI;
using GUI;

/// <summary>
/// This class tests the MainMenuWindow's buttons.
/// </summary>
public class MainMenuWindowTests
{
    /// <summary>
    /// Setup the scene
    /// </summary>
    [SetUp]
    public void Setup()
    {
        SceneManager.LoadScene("Library Hall");
    }

    /// <summary>
    /// Test if the lecture browser window is opened when pressing the button.
    /// </summary>
    [UnityTest]
    public IEnumerator LectureBrowser_Test()
    {
        var windowManager = GameObject.Find("WindowManager").GetComponent<WindowManager>();
        yield return null;
        var menu = windowManager.OpenWindow("MainMenuWindow");
        yield return null;
        var browserButton = menu.transform.Find("Canvas/Image/Lectures/LectureBrowserButton").GetComponent<Button>();

        browserButton.onClick.Invoke();
        yield return null;

        var browserWindow = GameObject.Find("LectureBrowserWindow(Clone)").GetComponent<LectureBrowserWindow>();
        yield return null;
        Assert.IsNotNull(browserWindow, "LectureBrowserWindow should appear after clicking the button in the MainMenuWindow.");

        browserWindow.Close();
        yield return null;

        Assert.IsTrue(browserWindow == null, "LectureBrowserWindow should close after clicking the close button.");

        menu.Close();

        yield return null;

        Assert.IsTrue(menu == null, "MainMenuWindow should close after clicking the close button.");
    }

    /// <summary>
    /// Test if the note window opens when pressing the button.
    /// </summary>
    [UnityTest]
    public IEnumerator Notes_Test()
    {
        var windowManager = GameObject.Find("WindowManager").GetComponent<WindowManager>();
        yield return null;
        var menu = windowManager.OpenWindow("MainMenuWindow");
        yield return null;
        var notesButton = menu.transform.Find("Canvas/Image/Notes/NotesButton").GetComponent<Button>();

        notesButton.onClick.Invoke();
        yield return null;

        var notesWindow = GameObject.Find("NoteWindow(Clone)").GetComponent<NoteWindow>();
        yield return null;
        Assert.IsNotNull(notesWindow, "NoteWindow should appear after clicking the button in the MainMenuWindow.");

        notesWindow.Close();
        yield return null;

        Assert.IsTrue(notesWindow == null, "NoteWindow should close after clicking the close button.");

        menu.Close();

        yield return null;

        Assert.IsTrue(menu == null, "MainMenuWindow should close after clicking the close button.");
    }

    /// <summary>
    /// Test if the settings window opens when pressing the button.
    /// </summary>
    [UnityTest]
    public IEnumerator Settings_Test()
    {
        var windowManager = GameObject.Find("WindowManager").GetComponent<WindowManager>();
        yield return null;
        var menu = windowManager.OpenWindow("MainMenuWindow");
        yield return null;
        var settingsButton = menu.transform.Find("Canvas/Image/Settings/SettingsButton").GetComponent<Button>();

        settingsButton.onClick.Invoke();
        yield return null;

        var settingsWindow = GameObject.Find("SettingsWindow(Clone)").GetComponent<SettingsWindow>();
        yield return null;
        Assert.IsNotNull(settingsWindow, "SettingsWindow should appear after clicking the button in the MainMenuWindow.");

        settingsWindow.Close();
        yield return null;

        Assert.IsTrue(settingsWindow == null, "SettingsWindow should close after clicking the close button.");

        menu.Close();

        yield return null;

        Assert.IsTrue(menu == null, "MainMenuWindow should close after clicking the close button.");
    }
}
