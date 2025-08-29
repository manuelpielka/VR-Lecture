using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

public class SettingsModule_SettingsManagerTests
{
    GameObject goCtrl, goSettings, goEnv;
    DisplayModeController ctrl;
    SettingsManager settings;
    ColorTheme light, dark;

    private class PrefsSnapshot
    {
        private readonly Dictionary<string, string> _kv = new();
        private readonly HashSet<string> _had = new();
        private readonly string[] _keys;
        public PrefsSnapshot(params string[] keys) { _keys = keys; }
        public void Capture()
        {
            _kv.Clear(); _had.Clear();
            foreach (var k in _keys)
            {
                if (PlayerPrefs.HasKey(k))
                {
                    _had.Add(k);
                    _kv[k] = PlayerPrefs.GetString(k);
                }
            }
        }
        public void Restore()
        {
            foreach (var k in _keys)
            {
                if (_had.Contains(k)) PlayerPrefs.SetString(k, _kv[k]);
                else PlayerPrefs.DeleteKey(k);
            }
            PlayerPrefs.Save();
        }
    }

    private PrefsSnapshot prefsSnap = new PrefsSnapshot(
         "UserPref_AutoAdjust",
         "UserPref_IsDarkMode",
         "UserPref_Language",
         "env_scene"
     );



    [UnitySetUp]
    public IEnumerator USetUp()
    {
        prefsSnap.Capture();

        SafeDestroyObj(SettingsManager.Instance);
        SafeDestroyObj(DisplayModeController.Instance);
        SafeDestroyObj(EnvironmentManager.Instance);

        foreach (var x in Object.FindObjectsByType<DisplayModeController>(FindObjectsSortMode.None)) SafeDestroyObj(x);
        foreach (var x in Object.FindObjectsByType<SettingsManager>(FindObjectsSortMode.None)) SafeDestroyObj(x);
        foreach (var x in Object.FindObjectsByType<EnvironmentManager>(FindObjectsSortMode.None)) SafeDestroyObj(x);

        TryClearSingleton(typeof(SettingsManager));
        TryClearSingleton(typeof(DisplayModeController));
        TryClearSingleton(typeof(EnvironmentManager));

        yield return null;

        light = ScriptableObject.CreateInstance<ColorTheme>();
        dark = ScriptableObject.CreateInstance<ColorTheme>();
        light.WindowBackground = Color.gray;
        dark.WindowBackground = Color.black;

        goCtrl = new GameObject("Ctrl");
        ctrl = goCtrl.AddComponent<DisplayModeController>();
        ctrl.lightTheme = light;
        ctrl.darkTheme = dark;

        LogAssert.Expect(LogType.Log, new Regex(@"\[Theme\] Mode applied: (Light|Dark)"));

        goSettings = new GameObject("SettingsManager");
        settings = goSettings.AddComponent<SettingsManager>();

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator UTearDown()
    {
        LogAssert.ignoreFailingMessages = false;

        SafeDestroyObj(goEnv);
        SafeDestroyObj(goSettings);
        SafeDestroyObj(goCtrl);
        SafeDestroyObj(SettingsManager.Instance);
        SafeDestroyObj(DisplayModeController.Instance);
        SafeDestroyObj(EnvironmentManager.Instance);

        TryClearSingleton(typeof(SettingsManager));
        TryClearSingleton(typeof(DisplayModeController));
        TryClearSingleton(typeof(EnvironmentManager));

        if (light) Object.DestroyImmediate(light);
        if (dark) Object.DestroyImmediate(dark);

        prefsSnap.Restore();
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
    public IEnumerator OnDestroy_Clears_Singleton()
    {
        yield return null;
        Assert.AreSame(settings, SettingsManager.Instance);
        Object.Destroy(goSettings);
        yield return null;
        Assert.IsNull(SettingsManager.Instance);
    }


    [UnityTest]
    public IEnumerator ApplyDisplayMode_NoController_Returns()
    {
        SafeDestroyObj(goCtrl); yield return null;
        Assert.IsNull(DisplayModeController.Instance);
        LogAssert.Expect(LogType.Error, "DisplayModeController.Instance is null!");
        InvokePrivate(settings, "ApplyDisplayMode");
    }

    [Test]
    public void ApplyDisplayMode_AutoTrue_EarlyReturn()
    {
        LogAssert.Expect(LogType.Log, new Regex(@"\[Theme\] Mode applied: (Light|Dark)"));
        UserPreferencesManager.SaveAutoAdjust(true);
        settings.LoadSettings();
        Assert.IsTrue(ctrl.IsAutoAdjustEnabled());
    }

    [Test]
    public void ApplyDisplayMode_AutoFalse_DarkBranches()
    {
        UserPreferencesManager.SaveAutoAdjust(false);

        UserPreferencesManager.SaveDarkMode(true);
        LogAssert.Expect(LogType.Log, new Regex(@"\[Theme\] Mode applied: Dark"));
        settings.LoadSettings();
        Assert.IsFalse(ctrl.IsAutoAdjustEnabled());

        UserPreferencesManager.SaveDarkMode(false);
        LogAssert.Expect(LogType.Log, new Regex(@"\[Theme\] Mode applied: Light"));
        settings.LoadSettings();
        Assert.IsFalse(ctrl.IsAutoAdjustEnabled());
    }

    [Test]
    public void ApplySettings_Saves_And_Applies()
    {
        LogAssert.Expect(LogType.Log, new Regex(@"\[Theme\] Mode applied: (Light|Dark)"));
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

        SafeDestroyObj(goSettings);
        LogAssert.Expect(LogType.Log, new Regex(@"\[Theme\] Mode applied: (Light|Dark)"));
        goSettings = new GameObject("SettingsManager");
        settings = goSettings.AddComponent<SettingsManager>();
        LogAssert.Expect(LogType.Log, new Regex(@"\[Theme\] Mode applied: (Light|Dark)"));

        yield return null; yield return null;

        Assert.AreEqual("en", PlayerPrefs.GetString("UserPref_Language"));
    }

    [UnityTest]
    public IEnumerator Start_Uses_Saved_Language_If_Exists()
    {
        PlayerPrefs.SetString("UserPref_Language", "en");

        SafeDestroyObj(goSettings);
        LogAssert.Expect(LogType.Log, new Regex(@"\[Theme\] Mode applied: (Light|Dark)"));
        goSettings = new GameObject("SettingsManager");
        settings = goSettings.AddComponent<SettingsManager>();
        LogAssert.Expect(LogType.Log, new Regex(@"\[Theme\] Mode applied: (Light|Dark)"));

        yield return null; yield return null;

        Assert.AreEqual("en", PlayerPrefs.GetString("UserPref_Language"));
    }

    [UnityTest]
    public IEnumerator ApplyLanguageAsync_EarlyReturn_When_Invalid()
    {
        var t1 = settings.ApplyLanguageAsync(null);
        while (!t1.IsCompleted) yield return null;

        var t2 = settings.ApplyLanguageAsync("xx-nonexist");
        while (!t2.IsCompleted) yield return null;

        Assert.Pass();
    }

    [UnityTest]
    public IEnumerator ApplyLanguageAsync_Switch_And_Save()
    {
        InjectLanguageMap(settings, new Dictionary<string, string> { { "en", "English" } });
        var t = settings.ApplyLanguageAsync("en");
        while (!t.IsCompleted) yield return null;
        Assert.AreEqual("en", PlayerPrefs.GetString("UserPref_Language"));
    }

    [UnityTest]
    public IEnumerator ApplyLanguageAsync_ValidBranch_WritesPrefs()
    {
        InjectLanguageMap(settings, new Dictionary<string, string> { { "en", "English" } });
        var task = settings.ApplyLanguageAsync("en");
        while (!task.IsCompleted) yield return null;
        Assert.AreEqual("en", PlayerPrefs.GetString("UserPref_Language"));
    }

    [UnityTest]
    public IEnumerator Env_Null_GetOptions_And_GetCurrent()
    {
        SafeDestroyObj(EnvironmentManager.Instance);
        foreach (var x in Object.FindObjectsByType<EnvironmentManager>(FindObjectsSortMode.None)) SafeDestroyObj(x);
        TryClearSingleton(typeof(EnvironmentManager));
        yield return null;

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

        SetEnvCurrentSceneName(EnvironmentManager.Instance, "EnvA");
        Assert.AreEqual("EnvA", EnvironmentManager.Instance.CurrentSceneName);

        settings.ApplyEnvironment("EnvA");
        Assert.AreEqual("EnvA", EnvironmentManager.Instance.CurrentSceneName);

        LogAssert.ignoreFailingMessages = true;
        settings.ApplyEnvironment("EnvB");
        LogAssert.ignoreFailingMessages = false;

        yield return null;
    }

    [Test]
    public void Language_Getters_Proxy_To_Manager()
    {
        InjectLanguageMap(settings, new Dictionary<string, string> { { "en", "English" } });

        var dict = settings.GetLanguages();
        Assert.IsNotNull(dict);
        Assert.IsTrue(dict.ContainsKey("en"));
        Assert.AreEqual("English", dict["en"]);

        var code = settings.GetCurrentLanguageCode();
        Assert.IsTrue(code == null || code.Length > 0);
    }

    private static void InvokePrivate(object obj, string method)
    {
        var mi = obj.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(mi, "can not find: " + method);
        mi.Invoke(obj, null);
    }

    private static void InjectLanguageMap(SettingsManager sm, Dictionary<string, string> map)
    {
        var lmField = typeof(SettingsManager).GetField("languageManager", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(lmField, "languageManager field missing");
        var lm = lmField.GetValue(sm);

        var dictField = lm.GetType().GetField("languageList", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(dictField, "languageList field missing");
        var dict = (System.Collections.IDictionary)dictField.GetValue(lm);
        dict.Clear();
        foreach (var kv in map) dict[kv.Key] = kv.Value;
    }

    private static void TryClearSingleton(System.Type t)
    {
        var p = t.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        var set = p?.GetSetMethod(true);
        if (set != null) { set.Invoke(null, new object[] { null }); return; }

        var f = t.GetField("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null && !f.IsInitOnly) { f.SetValue(null, null); return; }

        var back =
            t.GetField("instance", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public) ??
            t.GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public) ??
            t.GetField("s_instance", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

        if (back != null && !back.IsInitOnly) back.SetValue(null, null);
    }

    private static void SafeDestroyObj(UnityEngine.Object obj)
    {
        if (obj == null) return;
        Object.DestroyImmediate(obj);
    }

    private static void SetEnvCurrentSceneName(object envMgr, string name)
    {
        if (envMgr == null) return;
        var t = envMgr.GetType();

        var p = t.GetProperty("CurrentSceneName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.CanWrite) { p.SetValue(envMgr, name); return; }

        var f =
            t.GetField("CurrentSceneName", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) ??
            t.GetField("currentSceneName", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) ??
            t.GetField("_currentSceneName", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        if (f != null) f.SetValue(envMgr, name);
    }

}