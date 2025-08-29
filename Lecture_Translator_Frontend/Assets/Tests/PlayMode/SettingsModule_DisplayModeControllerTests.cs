using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

#region(SetUp/TearDown/IsAutoAdjustEnabled/ApplyTheme/Update)
public class SettingsModule_DisplayModeController_Coverage
{
    private GameObject goCtrl;
    private DisplayModeController ctrl;
    private ColorTheme light, dark;
    private struct ColorSnapshot
    {
        public Component comp;
        public Color color;
    }
    private readonly List<ColorSnapshot> _themeSnapshots = new List<ColorSnapshot>();

    [SetUp]
    public void SetUp()
    {

        if (DisplayModeController.Instance != null)
            Object.DestroyImmediate(DisplayModeController.Instance.gameObject);
        foreach (var x in Object.FindObjectsByType<DisplayModeController>(FindObjectsSortMode.None))
            Object.DestroyImmediate(x.gameObject);
        try { DisplayModeController.Instance = null; } catch { }

        SnapshotExistingThemedElements();

        light = ScriptableObject.CreateInstance<ColorTheme>();
        dark = ScriptableObject.CreateInstance<ColorTheme>();
        light.WindowBackground = Color.gray;
        dark.WindowBackground = Color.black;

        goCtrl = new GameObject("Ctrl");
        ctrl = goCtrl.AddComponent<DisplayModeController>();
        ctrl.lightTheme = light;
        ctrl.darkTheme = dark;

        LogAssert.Expect(LogType.Log, new Regex(@"\[Theme\] Mode applied: (Light|Dark)"));
        ctrl.SetAutoAdjust(false);
    }

    [TearDown]
    public void TearDown()
    {
        RestoreThemedElements();

        LogAssert.ignoreFailingMessages = false;

        if (goCtrl) Object.DestroyImmediate(goCtrl);
        if (DisplayModeController.Instance != null && DisplayModeController.Instance == ctrl)
            DisplayModeController.Instance = null;

        if (light) Object.DestroyImmediate(light);
        if (dark) Object.DestroyImmediate(dark);

    }

    [Test]
    public void OnDestroy_Clears_Instance_When_Self()
    {
        if (DisplayModeController.Instance != null)
            Object.DestroyImmediate(DisplayModeController.Instance.gameObject);
        foreach (var x in Object.FindObjectsByType<DisplayModeController>(FindObjectsSortMode.None))
            Object.DestroyImmediate(x.gameObject);

        var lt = ScriptableObject.CreateInstance<ColorTheme>();
        var dk = ScriptableObject.CreateInstance<ColorTheme>();

        var go = new GameObject("Ctrl_OnDestroy");
        var c = go.AddComponent<DisplayModeController>();
        c.lightTheme = lt; c.darkTheme = dk;

        Assert.AreSame(c, DisplayModeController.Instance);

        Object.DestroyImmediate(go);
        Assert.IsNull(DisplayModeController.Instance);

        Object.DestroyImmediate(lt);
        Object.DestroyImmediate(dk);
    }

    [Test]
    public void IsAutoAdjustEnabled_Getter_Covered()
    {
        LogAssert.Expect(LogType.Log, new Regex(@"\[Theme\] Mode applied: (Light|Dark)"));
        ctrl.SetAutoAdjust(false);
        Assert.IsFalse(ctrl.IsAutoAdjustEnabled());

        LogAssert.Expect(LogType.Log, new Regex(@"\[Theme\] Mode applied: (Light|Dark)"));
        ctrl.SetAutoAdjust(true);
        Assert.IsTrue(ctrl.IsAutoAdjustEnabled());
    }

    [Test]
    public void IsDarkModeEnabled_Getter_Reflects_Current_Mode()
    {
        LogAssert.Expect(LogType.Log, new Regex(@"\[Theme\] Mode applied: (Light|Dark)"));
        ctrl.SetAutoAdjust(false);

        LogAssert.Expect(LogType.Log, new Regex(@"\[Theme\] Mode applied: Light"));
        ctrl.SetDarkMode(false);
        Assert.IsFalse(ctrl.IsDarkModeEnabled(), "Expected Light mode (isDarkModeEnabled == false)");

        LogAssert.Expect(LogType.Log, new Regex(@"\[Theme\] Mode applied: Dark"));
        ctrl.SetDarkMode(true);
        Assert.IsTrue(ctrl.IsDarkModeEnabled(), "Expected Dark mode (isDarkModeEnabled == true)");
    }

    [Test]
    public void ApplyTheme_Iterates_ThemedElements_Light_And_Dark()
    {
        var go = new GameObject("UI");
        var img = go.AddComponent<Image>();
        var elem = go.AddComponent<ThemedElement>();
        elem.role = ThemeRole.WindowBackground;

        ctrl.SetDarkMode(false);
        LogAssert.Expect(LogType.Log, new Regex(@"\[Theme\] Mode applied: Light"));
        ctrl.UpdateMode();
        Assert.AreEqual(Color.gray, img.color);

        ctrl.SetDarkMode(true);
        LogAssert.Expect(LogType.Log, new Regex(@"\[Theme\] Mode applied: Dark"));
        ctrl.UpdateMode();
        Assert.AreEqual(Color.black, img.color);

        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator Update_RefreshTimer_Triggers_RefreshMode()
    {
        SetPrivateField(ctrl, "refreshTimer", 0f);
        SetPrivateField(ctrl, "refreshInterval", 999f);

        LogAssert.Expect(LogType.Log, new Regex(@"\[Theme\] Mode applied: (Light|Dark)"));
        ctrl.SetAutoAdjust(true);
        yield return null;

        float timer = (float)GetPrivateField(ctrl, "refreshTimer");
        Assert.Greater(timer, 0f);
    }

    [Test]
    public void RefreshMode_Calls_UpdateMode_Directly()
    {
        var go = new GameObject("UI2");
        go.AddComponent<Image>();
        var elem = go.AddComponent<ThemedElement>();
        elem.role = ThemeRole.WindowBackground;

        ctrl.SetAutoAdjust(false);
        ctrl.SetDarkMode(true);

        LogAssert.Expect(LogType.Log, new Regex(@"\[Theme\] Mode applied: Dark"));
        ctrl.RefreshMode();

        Object.DestroyImmediate(go);
    }

    [Test]
    public void Update_InnerIf_Branch_Forced_By_PrivateInvoke()
    {
        LogAssert.Expect(LogType.Log, new Regex(@"\[Theme\] Mode applied: (Light|Dark)"));
        ctrl.SetAutoAdjust(true);

        SetPrivateField(ctrl, "refreshInterval", 0f);
        SetPrivateField(ctrl, "refreshTimer", 0f);

        var mi = typeof(DisplayModeController).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(mi, "can not find Update()");

        LogAssert.Expect(LogType.Log, new Regex(@"\[Theme\] Mode applied: (Light|Dark)"));
        mi.Invoke(ctrl, null);

        float timerAfter = (float)GetPrivateField(ctrl, "refreshTimer");
        Assert.AreEqual(0f, timerAfter, 1e-6f);
    }

    private void SnapshotExistingThemedElements()
    {
        _themeSnapshots.Clear();
        foreach (var te in Object.FindObjectsByType<ThemedElement>(FindObjectsSortMode.None))
        {
            if (te == null) continue;
            if (te.TryGetComponent<Text>(out var uiText))
                _themeSnapshots.Add(new ColorSnapshot { comp = uiText, color = uiText.color });
            else if (te.TryGetComponent<TMPro.TextMeshProUGUI>(out var tmp))
                _themeSnapshots.Add(new ColorSnapshot { comp = tmp, color = tmp.color });
            else if (te.TryGetComponent<Image>(out var img))
                _themeSnapshots.Add(new ColorSnapshot { comp = img, color = img.color });
            else if (te.TryGetComponent<RawImage>(out var raw))
                _themeSnapshots.Add(new ColorSnapshot { comp = raw, color = raw.color });
        }
    }

    private void RestoreThemedElements()
    {
        foreach (var snap in _themeSnapshots)
        {
            if (snap.comp == null) continue;
            switch (snap.comp)
            {
                case Text t: t.color = snap.color; break;
                case TMPro.TextMeshProUGUI tmp: tmp.color = snap.color; break;
                case Image img: img.color = snap.color; break;
                case RawImage raw: raw.color = snap.color; break;
            }
        }
        _themeSnapshots.Clear();
    }

    private static void SetPrivateField(object obj, string name, object value)
    {
        var f = obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(f, "can not find " + name);
        f.SetValue(obj, value);
    }

    private static object GetPrivateField(object obj, string name)
    {
        var f = obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(f, "can not find " + name);
        return f.GetValue(obj);
    }
}
#endregion

#region duplicate(no SetUp)
public class SettingsModule_DisplayModeController_Awake_Duplicate
{
    [UnityTest]
    public IEnumerator Second_Instance_Destroys_Itself()
    {
        if (DisplayModeController.Instance != null)
            Object.DestroyImmediate(DisplayModeController.Instance.gameObject);
        foreach (var x in Object.FindObjectsByType<DisplayModeController>(FindObjectsSortMode.None))
            Object.DestroyImmediate(x.gameObject);
        yield return null;

        var light = ScriptableObject.CreateInstance<ColorTheme>();
        var dark = ScriptableObject.CreateInstance<ColorTheme>();

        var go1 = new GameObject("Ctrl1");
        var c1 = go1.AddComponent<DisplayModeController>();
        c1.lightTheme = light; c1.darkTheme = dark;

        var go2 = new GameObject("Ctrl2");
        var c2 = go2.AddComponent<DisplayModeController>();
        c2.lightTheme = light; c2.darkTheme = dark;

        yield return null;

        Assert.AreSame(c1, DisplayModeController.Instance);

        Assert.IsTrue(go2 == null || c2 == null);

        if (DisplayModeController.Instance == c1) DisplayModeController.Instance = null;
        if (go1) Object.DestroyImmediate(go1);
        if (go2) Object.DestroyImmediate(go2);
        Object.DestroyImmediate(light);
        Object.DestroyImmediate(dark);
    }
}
#endregion