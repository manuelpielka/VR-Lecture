using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class SettingsModule_ThemedElementTests
{
    private DisplayModeController ctrl;
    private GameObject goCtrl;
    private ColorTheme light, dark;

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteAll();
        UserPreferencesManager.ClearAll();

        light = ScriptableObject.CreateInstance<ColorTheme>();
        light.WindowBackground = new Color(0.90f, 0.90f, 0.90f, 1);
        light.PanelOnWindowBackground = new Color(0.80f, 0.80f, 0.80f, 1);
        light.ButtonBackground = new Color(0.70f, 0.70f, 0.70f, 1);
        light.ButtonFigure = new Color(0.60f, 0.60f, 0.60f, 1);
        light.ButtonText = Color.black;
        light.ContentText = new Color(0.2f, 0.2f, 0.2f, 1);
        light.TitleText = new Color(0.1f, 0.1f, 0.1f, 1);

        dark = ScriptableObject.CreateInstance<ColorTheme>();
        dark.WindowBackground = new Color(0.10f, 0.10f, 0.10f, 1);
        dark.PanelOnWindowBackground = new Color(0.20f, 0.20f, 0.20f, 1);
        dark.ButtonBackground = new Color(0.30f, 0.30f, 0.30f, 1);
        dark.ButtonFigure = new Color(0.40f, 0.40f, 0.40f, 1);
        dark.ButtonText = Color.white;
        dark.ContentText = new Color(0.8f, 0.8f, 0.8f, 1);
        dark.TitleText = new Color(0.9f, 0.9f, 0.9f, 1);

        goCtrl = new GameObject("Ctrl");
        ctrl = goCtrl.AddComponent<DisplayModeController>();
        ctrl.lightTheme = light;
        ctrl.darkTheme = dark;
        ctrl.SetAutoAdjust(false);
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
    public void Each_Role_Maps_To_Correct_Color_In_Light_And_Dark()
    {
        foreach (ThemeRole role in System.Enum.GetValues(typeof(ThemeRole)))
        {
            var go = new GameObject("UI_" + role);
            var img = go.AddComponent<Image>();
            var themed = go.AddComponent<ThemedElement>();
            themed.role = role;

            ctrl.SetDarkMode(false);
            InvokeApplyTheme(ctrl);
            Assert.True(Same(img.color, Get(light, role)),
                $"Light: {role} should be {Get(light, role)}");

            ctrl.SetDarkMode(true);
            InvokeApplyTheme(ctrl);
            Assert.True(Same(img.color, Get(dark, role)),
                $"Dark: {role} should be {Get(dark, role)}");

            Object.DestroyImmediate(go);
        }
    }

    private static Color Get(ColorTheme t, ThemeRole r)
    {
        switch (r)
        {
            case ThemeRole.WindowBackground: return t.WindowBackground;
            case ThemeRole.PanelOnWindowBackground: return t.PanelOnWindowBackground;
            case ThemeRole.ButtonBackground: return t.ButtonBackground;
            case ThemeRole.ButtonFigure: return t.ButtonFigure;
            case ThemeRole.ButtonText: return t.ButtonText;
            case ThemeRole.ContentText: return t.ContentText;
            case ThemeRole.TitleText: return t.TitleText;
            default: return Color.magenta;
        }
    }

    private static void InvokeApplyTheme(DisplayModeController c)
    {
        var m = typeof(DisplayModeController)
            .GetMethod("ApplyTheme", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        m.Invoke(c, null);
    }

    private static bool Same(Color a, Color b, float eps = 0.001f)
    {
        return Mathf.Abs(a.r - b.r) < eps &&
               Mathf.Abs(a.g - b.g) < eps &&
               Mathf.Abs(a.b - b.b) < eps &&
               Mathf.Abs(a.a - b.a) < eps;
    }
}