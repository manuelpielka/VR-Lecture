using NUnit.Framework;
using UnityEngine;

public class SettingsModuleEditorTests
{
    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteAll();
        UserPreferencesManager.ClearAll();
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteAll();
    }

    [Test]
    public void UserPrefs_SaveLoad_DarkMode_AutoAdjust()
    {
        UserPreferencesManager.SaveDarkMode(true);
        UserPreferencesManager.SaveAutoAdjust(false);

        Assert.IsTrue(UserPreferencesManager.LoadDarkMode());
        Assert.IsFalse(UserPreferencesManager.LoadAutoAdjust());
    }

    [Test]
    public void UserPrefs_Language_Environment_Tutorial()
    {
        Assert.IsNull(UserPreferencesManager.LoadLanguageOrNull());
        Assert.IsNull(UserPreferencesManager.LoadEnvironmentSceneOrNull());
        Assert.IsFalse(UserPreferencesManager.LoadTutorialCompleted());

        UserPreferencesManager.SaveLanguage("de");
        UserPreferencesManager.SaveEnvironmentScene("DemoScene");
        UserPreferencesManager.SaveTutorialCompleted(true);

        Assert.AreEqual("de", UserPreferencesManager.LoadLanguageOrNull());
        Assert.AreEqual("DemoScene", UserPreferencesManager.LoadEnvironmentSceneOrNull());
        Assert.IsTrue(UserPreferencesManager.LoadTutorialCompleted());
    }

    [Test]
    public void UserPrefs_ClearAll_Resets_Defaults()
    {
        UserPreferencesManager.SaveDarkMode(true);
        UserPreferencesManager.SaveAutoAdjust(false);
        UserPreferencesManager.ClearAll();

        Assert.IsFalse(UserPreferencesManager.LoadDarkMode()); // default false
        Assert.IsTrue(UserPreferencesManager.LoadAutoAdjust()); // default true
    }
}
