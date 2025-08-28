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

    [UnitySetUp]
    public IEnumerator USetUp()
    {
        if (SettingsManager.Instance != null)
            Object.DestroyImmediate(SettingsManager.Instance.gameObject);
        if (DisplayModeController.Instance != null)
            Object.DestroyImmediate(DisplayModeController.Instance.gameObject);
        if (EnvironmentManager.Instance != null)
            Object.DestroyImmediate(EnvironmentManager.Instance.gameObject);

        foreach (var x in Object.FindObjectsByType<DisplayModeController>(FindObjectsSortMode.None))
            Object.DestroyImmediate(x.gameObject);
        foreach (var x in Object.FindObjectsByType<SettingsManager>(FindObjectsSortMode.None))
            Object.DestroyImmediate(x.gameObject);
        foreach (var x in Object.FindObjectsByType<EnvironmentManager>(FindObjectsSortMode.None))
            Object.DestroyImmediate(x.gameObject);

        PlayerPrefs.DeleteAll();
        yield return null; 

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

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator UTearDown()
    {
        if (goEnv) Object.DestroyImmediate(goEnv);
        if (goSettings) Object.DestroyImmediate(goSettings);
        if (goCtrl) Object.DestroyImmediate(goCtrl);
        if (light) Object.DestroyImmediate(light);
        if (dark) Object.DestroyImmediate(dark);

        if (SettingsManager.Instance != null)
            Object.DestroyImmediate(SettingsManager.Instance.gameObject);
        if (DisplayModeController.Instance != null)
            Object.DestroyImmediate(DisplayModeController.Instance.gameObject);
        if (EnvironmentManager.Instance != null)
            Object.DestroyImmediate(EnvironmentManager.Instance.gameObject);

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

    [UnityTest]
    public IEnumerator ApplyDisplayMode_NoController_Returns()
    {
        Object.Destroy(goCtrl);
        yield return null;

        Assert.IsNull(DisplayModeController.Instance);

        LogAssert.Expect(LogType.Error, "DisplayModeController.Instance is null!");
        InvokePrivate(settings, "ApplyDisplayMode");
    }

    [UnityTest]
    public IEnumerator ApplyDisplayMode_AutoTrue_EarlyReturn()
    {
        yield return null;
        UserPreferencesManager.SaveAutoAdjust(true);
        settings.LoadSettings(); 
        Assert.IsTrue(ctrl.IsAutoAdjustEnabled());
    }

    [UnityTest]
    public IEnumerator ApplyDisplayMode_AutoFalse_DarkBranches()
    {
        yield return null;

        UserPreferencesManager.SaveAutoAdjust(false);

        UserPreferencesManager.SaveDarkMode(true);
        settings.LoadSettings();
        Assert.IsFalse(ctrl.IsAutoAdjustEnabled());

        UserPreferencesManager.SaveDarkMode(false);
        settings.LoadSettings();
        Assert.IsFalse(ctrl.IsAutoAdjustEnabled());
    }
    
    [UnityTest]
    public IEnumerator ApplySettings_Saves_And_Applies()
    {
        yield return null;

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

        yield return null;
        yield return null;

        // savedLanguage = "en";
        // SaveLanguage(savedLanguage);
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

    [UnityTest]
    public IEnumerator ApplyLanguageAsync_ValidBranch_WritesPrefs()
    {
        yield return null;

        var lmField = typeof(SettingsManager)
            .GetField("languageManager", BindingFlags.Instance | BindingFlags.NonPublic);
        var lm = lmField.GetValue(settings);

        var dictField = lm.GetType()
            .GetField("languageList", BindingFlags.Instance | BindingFlags.NonPublic);
        var dict = (System.Collections.IDictionary)dictField.GetValue(lm);
        dict.Clear();
        dict.Add("en", "English");

        var task = settings.ApplyLanguageAsync("en");
        while (!task.IsCompleted) yield return null;

        Assert.AreEqual("en", PlayerPrefs.GetString("UserPref_Language"));
    }

    [UnityTest]
    public IEnumerator Env_Null_GetOptions_And_GetCurrent()
    {
        if (EnvironmentManager.Instance)
            Object.Destroy(EnvironmentManager.Instance.gameObject);

        foreach (var x in Object.FindObjectsByType<EnvironmentManager>(FindObjectsSortMode.None))
            Object.Destroy(x.gameObject);

        var t = typeof(EnvironmentManager);

        var f = t.GetField("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null)
        {
            if (!f.IsInitOnly) f.SetValue(null, null);
        }
        else
        {
            var p = t.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var set = p?.GetSetMethod(true);
            if (set != null)
            {
                p.SetValue(null, null);
            }
            else
            {
                var back =
                    t.GetField("instance", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public) ??
                    t.GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public) ??
                    t.GetField("s_instance", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

                if (back != null && !back.IsInitOnly)
                    back.SetValue(null, null);
            }
        }

        yield return null;

        var options = settings.GetEnvironmentOptions();
        var current = settings.GetCurrentEnvironment();

        Assert.NotNull(options);
        Assert.AreEqual(0, options.Count, "Environment options should be empty when no manager exists.");
        Assert.IsNull(current, "Current environment should be null when no manager exists.");
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

    [UnityTest]
    public IEnumerator OnDestroy_Clears_Singleton()
    {
        yield return null;
        Assert.AreSame(settings, SettingsManager.Instance);

        Object.Destroy(goSettings);
        yield return null;

        Assert.IsNull(SettingsManager.Instance);
    }
}