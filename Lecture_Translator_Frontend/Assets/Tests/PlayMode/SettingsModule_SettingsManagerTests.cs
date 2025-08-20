using NUnit.Framework;
using UnityEngine;

public class SettingsModule_SettingsManagerTests
{
    private GameObject goCtrl, goMgr;
    private DisplayModeController ctrl;
    private SettingsManager mgr;
    private ColorTheme light, dark;

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteAll();
        UserPreferencesManager.ClearAll();

        light = ScriptableObject.CreateInstance<ColorTheme>();
        dark = ScriptableObject.CreateInstance<ColorTheme>();
        light.WindowBackground = Color.gray;
        dark.WindowBackground = Color.black;

        goCtrl = new GameObject("Ctrl");
        ctrl = goCtrl.AddComponent<DisplayModeController>();
        ctrl.lightTheme = light;
        ctrl.darkTheme = dark;

        goMgr = new GameObject("Mgr");
        mgr = goMgr.AddComponent<SettingsManager>();

        // 
        mgr.LoadSettings();
    }

    [TearDown]
    public void TearDown()
    {
        if (ctrl != null && DisplayModeController.Instance == ctrl)
            DisplayModeController.Instance = null;

        Object.DestroyImmediate(goCtrl);
        Object.DestroyImmediate(goMgr);

        if (light != null) Object.DestroyImmediate(light);
        if (dark != null) Object.DestroyImmediate(dark);

        PlayerPrefs.DeleteAll();
    }

    [Test]
    public void ApplySettings_Persists_And_Drives_DisplayMode()
    {
        // Light
        mgr.ApplySettings(false, false);
        Assert.IsFalse(mgr.GetAutoSwitch());
        Assert.IsFalse(mgr.GetDarkMode());
        Assert.IsFalse(ctrl.IsDarkModeEnabled());

        // Dark
        mgr.ApplySettings(false, true);
        Assert.IsFalse(mgr.GetAutoSwitch());
        Assert.IsTrue(mgr.GetDarkMode());
        Assert.IsTrue(ctrl.IsDarkModeEnabled());
        Assert.IsTrue(UserPreferencesManager.LoadDarkMode());

        //
        mgr.ApplySettings(true, false);
        Assert.IsTrue(mgr.GetAutoSwitch());
        Assert.IsTrue(UserPreferencesManager.LoadAutoAdjust());
    }
}