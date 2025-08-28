using NUnit.Framework;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

public class SettingsModule_SettingsManagerTests
{
    GameObject goCtrl, goSettings, goEnv;
    DisplayModeController ctrl;
    SettingsManager settings;
    ColorTheme light, dark;

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteAll();

        light = ScriptableObject.CreateInstance<ColorTheme>();
        dark = ScriptableObject.CreateInstance<ColorTheme>();
        light.WindowBackground = Color.gray;
        dark.WindowBackground = Color.black;

        goCtrl = new GameObject("Ctrl");
        ctrl = goCtrl.AddComponent<DisplayModeController>();
        ctrl.lightTheme = light;
        ctrl.darkTheme = dark;

        goSettings = new GameObject("SettingsManager");
        settings = goSettings.AddComponent<SettingsManager>();
    }

    [TearDown]
    public IEnumerator TearDown()
    {
        if (goEnv) Object.Destroy(goEnv);
        if (goSettings) Object.Destroy(goSettings);
        if (goCtrl) Object.Destroy(goCtrl);

        if (light) Object.Destroy(light);
        if (dark) Object.Destroy(dark);

        PlayerPrefs.DeleteAll();
        yield return null;
    }

    [UnityTest]
    public IEnumerator Awake_PrimaryInstance_SetsSingleton()
    {
        yield return null;
        Assert.AreSame(settings, SettingsManager.Instance);
    }

    [UnityTest]
    public IEnumerator Awake_SecondInstance_DestroysItself()
    {
        yield return null;

        var go2 = new GameObject("SettingsManager2");
        var sm2 = go2.AddComponent<SettingsManager>();

        yield return null;

        Assert.AreSame(settings, SettingsManager.Instance);
        Assert.IsTrue(go2 == null || sm2 == null);
    }

    [Test]
    public void ApplyDisplayMode_NoController_Returns()
    {
        DisplayModeController.Instance = null;
        InvokePrivate(settings, "ApplyDisplayMode");
        Assert.Pass();
    }

    [Test]
    public void ApplyDisplayMode_AutoFalse_DarkBranches()
    {
        UserPreferencesManager.SaveAutoAdjust(false);
        UserPreferencesManager.SaveDarkMode(true);

        settings.LoadSettings();
        Assert.IsTrue(ctrl.IsAutoAdjustEnabled() == false);

        UserPreferencesManager.SaveDarkMode(false);
        settings.LoadSettings();
        Assert.IsTrue(ctrl.IsAutoAdjustEnabled() == false);
    }

    [Test]
    public void ApplySettings_Saves_And_Applies()
    {
        settings.ApplySettings(newAutoSwitch: false, newDarkMode: true);
        Assert.IsFalse(settings.GetAutoSwitch());
        Assert.IsTrue(settings.GetDarkMode());
        Assert.AreEqual(0, PlayerPrefs.GetInt("UserPref_AutoAdjust"));
        Assert.AreEqual(1, PlayerPrefs.GetInt("UserPref_IsDarkMode"));
    }

    [UnityTest]
    public IEnumerator Start_Sets_Default_Language_When_Empty()
    {
        PlayerPrefs.DeleteKey("UserPref_Language");

        Object.DestroyImmediate(goSettings);
        goSettings = new GameObject("SettingsManager");
        settings = goSettings.AddComponent<SettingsManager>();

        yield return null; yield return null;

        Assert.AreEqual("en", PlayerPrefs.GetString("UserPref_Language"));
    }

    [UnityTest]
    public IEnumerator Start_Uses_Saved_Language_If_Exists()
    {
        PlayerPrefs.SetString("UserPref_Language", "en");

        Object.DestroyImmediate(goSettings);
        goSettings = new GameObject("SettingsManager");
        settings = goSettings.AddComponent<SettingsManager>();
        yield return null; yield return null;

        Assert.AreEqual("en", PlayerPrefs.GetString("UserPref_Language"));
    }

    [UnityTest]
    public IEnumerator ApplyLanguageAsync_EarlyReturn_When_Invalid()
    {
        yield return null;
        var t1 = settings.ApplyLanguageAsync(null);
        while (!t1.IsCompleted) yield return null;
        var t2 = settings.ApplyLanguageAsync("xx-nonexist");
        while (!t2.IsCompleted) yield return null;
        Assert.Pass();
    }

    [UnityTest]
    public IEnumerator ApplyLanguageAsync_Switch_And_Save()
    {
        yield return null;
        var t = settings.ApplyLanguageAsync("en");
        while (!t.IsCompleted) yield return null;
        Assert.AreEqual("en", PlayerPrefs.GetString("UserPref_Language"));
    }

    [Test]
    public void Env_Null_GetOptions_And_GetCurrent()
    {
        if (EnvironmentManager.Instance != null)
        {
            Object.DestroyImmediate(EnvironmentManager.Instance.gameObject);
        }
        var options = settings.GetEnvironmentOptions();
        var current = settings.GetCurrentEnvironment();
        Assert.NotNull(options);
        Assert.AreEqual(0, options.Count);
        Assert.IsNull(current);
    }

    [UnityTest]
    public IEnumerator Env_ApplyEnvironment_All_Branches()
    {
        if (EnvironmentManager.Instance == null)
        {
            goEnv = new GameObject("EnvMgr");
            goEnv.AddComponent<EnvironmentManager>();
            yield return null;
        }

        settings.ApplyEnvironment("");
        EnvironmentManager.Instance.LoadEnvironment("EnvA");
        settings.ApplyEnvironment("EnvA");
        Assert.AreEqual("EnvA", EnvironmentManager.Instance.CurrentSceneName);

        settings.ApplyEnvironment("EnvB");
        Assert.AreEqual("EnvB", EnvironmentManager.Instance.CurrentSceneName);
    }

    private static void InvokePrivate(object obj, string method)
    {
        var mi = obj.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(mi, "can not find: " + method);
        mi.Invoke(obj, null);
    }
}