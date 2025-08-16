using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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
    public void UserPrefs_SaveLoad_DarkMode_And_AutoAdjust_Work()
    {
        // 保存
        UserPreferencesManager.SaveDarkMode(true);
        UserPreferencesManager.SaveAutoAdjust(false);

        // 读取
        Assert.IsTrue(UserPreferencesManager.LoadDarkMode(), "DarkMode 应为 true");
        Assert.IsFalse(UserPreferencesManager.LoadAutoAdjust(), "AutoAdjust 应为 false");
    }

    [Test]
    public void UserPrefs_ClearAll_Resets_To_Defaults()
    {
        UserPreferencesManager.SaveDarkMode(true);
        UserPreferencesManager.SaveAutoAdjust(false);

        UserPreferencesManager.ClearAll();

        // Clear 后的默认：Dark=false, AutoAdjust=true
        Assert.IsFalse(UserPreferencesManager.LoadDarkMode(), "默认 DarkMode 应为 false");
        Assert.IsTrue(UserPreferencesManager.LoadAutoAdjust(), "默认 AutoAdjust 应为 true");
    }

    [Test]
    public void UserPrefs_Language_And_Environment_SaveLoad()
    {
        const string lang = "de";
        const string env = "DemoScene";

        Assert.IsNull(UserPreferencesManager.LoadLanguageOrNull(), "未设置时 language 应为 null");
        Assert.IsNull(UserPreferencesManager.LoadEnvironmentSceneOrNull(), "未设置时 env 应为 null");

        UserPreferencesManager.SaveLanguage(lang);
        UserPreferencesManager.SaveEnvironmentScene(env);

        Assert.AreEqual(lang, UserPreferencesManager.LoadLanguageOrNull());
        Assert.AreEqual(env, UserPreferencesManager.LoadEnvironmentSceneOrNull());
    }

    [Test]
    public void UserPrefs_Tutorial_Flag_SaveLoad()
    {
        Assert.IsFalse(UserPreferencesManager.LoadTutorialCompleted(), "默认未完成教程");
        UserPreferencesManager.SaveTutorialCompleted(true);
        Assert.IsTrue(UserPreferencesManager.LoadTutorialCompleted(), "保存后应为已完成");
    }
}
