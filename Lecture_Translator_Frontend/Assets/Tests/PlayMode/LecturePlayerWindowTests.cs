using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LecturePlayerWindowTests
{
    LecturePlayerWindow playerWindow;

    [SetUp]
    public void SetUp()
    {
        SceneManager.LoadScene("DebugScene");
        playerWindow = (LecturePlayerWindow)WindowManager.instance.CreateWindow(WindowKeys.LecturePlayerKey);
        Lecture example = SetUpExampleLecture();
        playerWindow.AssignLecture(example);
    }

    private Lecture SetUpExampleLecture()
    {
        List<string> example_languages = new List<string> { "Multilingual", "Chinese", "English", "German", "Spanish" };
        Lecture example = new Lecture("offline_test", "Data/Test/Other/offline_test.mp4", "Data/Test/Other/offline_test", example_languages);
        example.SetDownloaded(true);
        return example;
    }
    //WIP

    [Test]
    public void VideoControlsTest()
    {

    }

    [Test]
    public void WindowOpeningButtonsTest()
    {

    }

    [Test]
    public void CloseButtonTest()
    {
        
    }

}
