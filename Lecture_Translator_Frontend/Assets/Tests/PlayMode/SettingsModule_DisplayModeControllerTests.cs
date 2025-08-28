using NUnit.Framework;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

#region(SetUp/TearDown/IsAutoAdjustEnabled/ApplyTheme/Update)
public class SettingsModule_DisplayModeController_Coverage
{
    private GameObject goCtrl;
    private DisplayModeController ctrl;
    private ColorTheme light, dark;

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

        ctrl.SetAutoAdjust(false);
    }

    [TearDown]
    public void TearDown()
    {
        if (ctrl && DisplayModeController.Instance == ctrl)
            DisplayModeController.Instance = null;

        if (goCtrl) UnityEngine.Object.DestroyImmediate(goCtrl);
        if (light) UnityEngine.Object.DestroyImmediate(light);
        if (dark) UnityEngine.Object.DestroyImmediate(dark);

        PlayerPrefs.DeleteAll();
    }

    [Test]
    public void IsAutoAdjustEnabled_Getter_Covered()
    {
        ctrl.SetAutoAdjust(false);
        Assert.IsFalse(ctrl.IsAutoAdjustEnabled());
        ctrl.SetAutoAdjust(true);
        Assert.IsTrue(ctrl.IsAutoAdjustEnabled());
    }

    [Test]
    public void ApplyTheme_Iterates_ThemedElements_Light_And_Dark()
    {
        var go = new GameObject("UI");
        var img = go.AddComponent<Image>();

        var elem = go.AddComponent<ThemedElement>();
        elem.role = ThemeRole.WindowBackground;

        // Light
        ctrl.SetDarkMode(false);
        ctrl.UpdateMode(); 
        Assert.AreEqual(Color.gray, img.color);

        // Dark
        ctrl.SetDarkMode(true);
        ctrl.UpdateMode();
        Assert.AreEqual(Color.black, img.color);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator Update_RefreshTimer_Triggers_RefreshMode()
    {
        SetPrivateField(ctrl, "refreshTimer", 0f);
        SetPrivateField(ctrl, "refreshInterval", 999f);

        ctrl.SetAutoAdjust(true);
        yield return null;

        float timer = (float)GetPrivateField(ctrl, "refreshTimer");

        Assert.Greater(timer, 0f);
    }

    [Test]
    public void RefreshMode_Calls_UpdateMode_Directly()
    {
        var light = ScriptableObject.CreateInstance<ColorTheme>();
        var dark = ScriptableObject.CreateInstance<ColorTheme>();
        light.WindowBackground = Color.gray;
        dark.WindowBackground = Color.black;

        var goCtrl = new GameObject("Ctrl");
        var ctrl = goCtrl.AddComponent<DisplayModeController>();
        ctrl.lightTheme = light;
        ctrl.darkTheme = dark;

        var go = new GameObject("UI");
        go.AddComponent<UnityEngine.UI.Image>();
        var elem = go.AddComponent<ThemedElement>();
        elem.role = ThemeRole.WindowBackground;

        ctrl.SetAutoAdjust(false);
        ctrl.SetDarkMode(true);
        ctrl.RefreshMode();

        if (DisplayModeController.Instance == ctrl) DisplayModeController.Instance = null;
        UnityEngine.Object.DestroyImmediate(go);
        UnityEngine.Object.DestroyImmediate(goCtrl);
        UnityEngine.Object.DestroyImmediate(light);
        UnityEngine.Object.DestroyImmediate(dark);
    }

    [Test]
    public void Update_InnerIf_Branch_Forced_By_PrivateInvoke()
    {
        var light = ScriptableObject.CreateInstance<ColorTheme>();
        var dark = ScriptableObject.CreateInstance<ColorTheme>();
        var goCtrl = new GameObject("Ctrl");
        var ctrl = goCtrl.AddComponent<DisplayModeController>();
        ctrl.lightTheme = light;
        ctrl.darkTheme = dark;

        // autoAdjust = true
        ctrl.SetAutoAdjust(true);

        SetPrivateField(ctrl, "refreshInterval", 0f);
        SetPrivateField(ctrl, "refreshTimer", 0f);

        var mi = typeof(DisplayModeController)
                  .GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(mi, "can not find Update()");
        mi.Invoke(ctrl, null);

        float timerAfter = (float)GetPrivateField(ctrl, "refreshTimer");
        Assert.AreEqual(0f, timerAfter, 1e-6f);

        if (DisplayModeController.Instance == ctrl) DisplayModeController.Instance = null;
        UnityEngine.Object.DestroyImmediate(goCtrl);
        UnityEngine.Object.DestroyImmediate(light);
        UnityEngine.Object.DestroyImmediate(dark);
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
        if (go1) UnityEngine.Object.DestroyImmediate(go1);
        if (go2) UnityEngine.Object.DestroyImmediate(go2);
        if (light) UnityEngine.Object.DestroyImmediate(light);
        if (dark) UnityEngine.Object.DestroyImmediate(dark);
    }
}
#endregion