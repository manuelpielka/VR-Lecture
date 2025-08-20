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
    }

    //WIP

}
