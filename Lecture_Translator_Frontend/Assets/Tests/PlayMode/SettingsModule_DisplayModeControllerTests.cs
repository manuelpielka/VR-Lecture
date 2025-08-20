using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class SettingsModule_DisplayModeControllerTests
{
    private GameObject goCtrl;
    private DisplayModeController ctrl;
    private ColorTheme light, dark;

    // IThemeRefreshable
    private class DummyRefreshable : MonoBehaviour, IThemeRefreshable
    {
        public int Called;
        public void RefreshTheme() => Called++;
    }

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteAll();
        UserPreferencesManager.ClearAll();

        // 
        light = ScriptableObject.CreateInstance<ColorTheme>();
        light.WindowBackground = new Color(0.9f, 0.9f, 0.9f, 1);
        dark = ScriptableObject.CreateInstance<ColorTheme>();
        dark.WindowBackground = new Color(0.1f, 0.1f, 0.1f, 1);

        // 
        goCtrl = new GameObject("DisplayModeController_Test");
        ctrl = goCtrl.AddComponent<DisplayModeController>();
        ctrl.lightTheme = light;
        ctrl.darkTheme = dark;

        // Light
        ctrl.SetAutoAdjust(false);
        ctrl.SetDarkMode(false);
    }

    [TearDown]
    public void TearDown()
    {
        if (ctrl != null && DisplayModeController.Instance == ctrl)
            DisplayModeController.Instance = null;

        if (goCtrl != null) Object.DestroyImmediate(goCtrl);
        if (light != null) Object.DestroyImmediate(light);
        if (dark != null) Object.DestroyImmediate(dark);

        PlayerPrefs.DeleteAll();
    }

    [Test]
    public void ManualToggle_Works_When_AutoOff()
    {
        ctrl.SetAutoAdjust(false);
        ctrl.SetDarkMode(true);
        Assert.IsTrue(ctrl.IsDarkModeEnabled());

        ctrl.SetDarkMode(false);
        Assert.IsFalse(ctrl.IsDarkModeEnabled());
    }

    [Test]
    public void ManualToggle_Ignored_When_AutoOn()
    {
        ctrl.SetAutoAdjust(true);
        bool before = ctrl.IsDarkModeEnabled();
        ctrl.SetDarkMode(!before);
        Assert.AreEqual(before, ctrl.IsDarkModeEnabled(),
            "dark mode toggle should be ignored in auto switch mode");
    }

    [Test]
    public void ApplyTheme_Colors_ThemedElements_And_Refreshable_Is_Called()
    {
        // UI + ThemedElement
        var ui = new GameObject("UI");
        var img = ui.AddComponent<Image>();
        var themed = ui.AddComponent<ThemedElement>();
        themed.role = ThemeRole.WindowBackground;

        // 
        var refGO = new GameObject("Refreshable");
        var refreshable = refGO.AddComponent<DummyRefreshable>();

        // Light
        ctrl.SetAutoAdjust(false);
        ctrl.SetDarkMode(false);
        InvokeApplyTheme(ctrl);

        Assert.That(Approximately(img.color, light.WindowBackground));
        Assert.AreEqual(1, refreshable.Called, "Light refreshed");

        // Dark
        ctrl.SetDarkMode(true);
        InvokeApplyTheme(ctrl);

        Assert.That(Approximately(img.color, dark.WindowBackground));
        Assert.AreEqual(2, refreshable.Called, "Dark refreshed");

        Object.DestroyImmediate(ui);
        Object.DestroyImmediate(refGO);
    }

    // private ApplyTheme()
    private static void InvokeApplyTheme(DisplayModeController c)
    {
        var m = typeof(DisplayModeController)
            .GetMethod("ApplyTheme", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.IsNotNull(m);
        m.Invoke(c, null);
    }

    private static bool Approximately(Color a, Color b, float eps = 0.001f)
    {
        return Mathf.Abs(a.r - b.r) < eps &&
               Mathf.Abs(a.g - b.g) < eps &&
               Mathf.Abs(a.b - b.b) < eps &&
               Mathf.Abs(a.a - b.a) < eps;
    }
}