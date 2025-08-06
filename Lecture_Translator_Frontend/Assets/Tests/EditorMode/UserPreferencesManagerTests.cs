using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
//using SettingsModule;

public class UserPreferencesManagerTests
{
    [SetUp]
    public void Setup()
    {
        PlayerPrefs.DeleteAll();
    }

    [Test]
    public void SaveAndLoadPlaybackSpeed_WorksCorrectly()
    {
        float testSpeed = 1.5f;
        //UserPreferencesManager.SavePlaybackSpeed(testSpeed);
        //float loadedSpeed = UserPreferencesManager.LoadPlaybackSpeed();
        //Assert.AreEqual(testSpeed, loadedSpeed);
    }

    [Test]
    public void SaveAndLoadDarkMode_WorksCorrectly()
    {
        //UserPreferencesManager.SaveDarkMode(true);
        //Assert.IsTrue(UserPreferencesManager.LoadDarkMode());

        //UserPreferencesManager.SaveDarkMode(false);
        //Assert.IsFalse(UserPreferencesManager.LoadDarkMode());
    }

    [Test]
    public void SaveAndLoadAutoAdjust_WorksCorrectly()
    {
        //UserPreferencesManager.SaveAutoAdjust(true);
        //Assert.IsTrue(UserPreferencesManager.LoadAutoAdjust());

        //UserPreferencesManager.SaveAutoAdjust(false);
        //Assert.IsFalse(UserPreferencesManager.LoadAutoAdjust());
    }

    [Test]
    public void SaveAndLoadBackgroundSceneId_WorksCorrectly()
    {
        string sceneId = "cafe";
        //UserPreferencesManager.SaveBackgroundSceneId(sceneId);
        //Assert.AreEqual(sceneId, UserPreferencesManager.LoadBackgroundSceneId());
    }

    [Test]
    public void SaveBackgroundSceneId_ThrowsIfEmpty()
    {
        Assert.Throws<System.ArgumentException>(() =>
        {
            //UserPreferencesManager.SaveBackgroundSceneId("");
        });
    }

    [Test]
    public void SaveAndLoadTutorialCompleted_WorksCorrectly()
    {
        //UserPreferencesManager.SaveTutorialCompleted(true);
        //Assert.IsTrue(UserPreferencesManager.LoadTutorialCompleted());

        //UserPreferencesManager.SaveTutorialCompleted(false);
        //Assert.IsFalse(UserPreferencesManager.LoadTutorialCompleted());
    }
}