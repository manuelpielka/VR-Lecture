using NUnit.Framework;
using System;
using TMPro;
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

        light = ScriptableObject.CreateInstance<ColorTheme>();
        light.WindowBackground = new Color(0.90f, 0.90f, 0.90f, 1);
        light.PanelOnWindowBackground = new Color(0.80f, 0.80f, 0.80f, 1);
        light.ButtonBackground = new Color(0.70f, 0.70f, 0.70f, 1);
        light.ButtonFigure = new Color(0.60f, 0.60f, 0.60f, 1);
        light.ButtonText = new Color(0.10f, 0.10f, 0.10f, 1);
        light.ContentText = new Color(0.20f, 0.20f, 0.20f, 1);
        light.TitleText = new Color(0.30f, 0.30f, 0.30f, 1);

        dark = ScriptableObject.CreateInstance<ColorTheme>();
        dark.WindowBackground = new Color(0.10f, 0.10f, 0.10f, 1);
        dark.PanelOnWindowBackground = new Color(0.20f, 0.20f, 0.20f, 1);
        dark.ButtonBackground = new Color(0.30f, 0.30f, 0.30f, 1);
        dark.ButtonFigure = new Color(0.40f, 0.40f, 0.40f, 1);
        dark.ButtonText = new Color(0.90f, 0.90f, 0.90f, 1);
        dark.ContentText = new Color(0.80f, 0.80f, 0.80f, 1);
        dark.TitleText = new Color(0.70f, 0.70f, 0.70f, 1);

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
        if (goCtrl != null) UnityEngine.Object.DestroyImmediate(goCtrl);
        if (light != null) UnityEngine.Object.DestroyImmediate(light);
        if (dark != null) UnityEngine.Object.DestroyImmediate(dark);
        PlayerPrefs.DeleteAll();
    }

    [Test]
    public void ThemedElement_Maps_All_Roles_Across_All_Common_UI_Components()
    {
        // cover TextMeshProUGUI / Text / Image / RawImage
        TestOneComponent<TextMeshProUGUI>();
        TestOneComponent<Text>();
        TestOneComponent<Image>();
        TestOneComponent<RawImage>();
    }

    private void TestOneComponent<T>() where T : Component
    {
        foreach (ThemeRole role in Enum.GetValues(typeof(ThemeRole)))
        {
            var go = new GameObject(typeof(T).Name + "_" + role);
            go.AddComponent<T>();

            var themed = go.AddComponent<ThemedElement>();
            themed.role = role;

            // Light
            ctrl.SetDarkMode(false);
            InvokeApplyTheme(ctrl);
            Assert.That(ColorEquals(ReadColor<T>(go), Get(light, role)),
                $"{typeof(T).Name} Light: {role}");

            // Dark
            ctrl.SetDarkMode(true);
            InvokeApplyTheme(ctrl);
            Assert.That(ColorEquals(ReadColor<T>(go), Get(dark, role)),
                $"{typeof(T).Name} Dark: {role}");

            UnityEngine.Object.DestroyImmediate(go);
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

    private static Color ReadColor<T>(GameObject go) where T : Component
    {
        if (typeof(T) == typeof(TextMeshProUGUI))
            return go.GetComponent<TextMeshProUGUI>().color;
        if (typeof(T) == typeof(Text))
            return go.GetComponent<Text>().color;
        if (typeof(T) == typeof(Image))
            return go.GetComponent<Image>().color;
        if (typeof(T) == typeof(RawImage))
            return go.GetComponent<RawImage>().color;
        return default;
    }

    private static void InvokeApplyTheme(DisplayModeController c)
    {
        var m = typeof(DisplayModeController)
            .GetMethod("ApplyTheme", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        m.Invoke(c, null);
    }

    private static bool ColorEquals(Color a, Color b, float eps = 0.001f)
    {
        return Mathf.Abs(a.r - b.r) < eps &&
               Mathf.Abs(a.g - b.g) < eps &&
               Mathf.Abs(a.b - b.b) < eps &&
               Mathf.Abs(a.a - b.a) < eps;
    }

    [Test]
    public void Unknown_ThemeRole_Uses_Magenta_Debug_Color()
    {
        var theme = ScriptableObject.CreateInstance<ColorTheme>();

        var go = new GameObject("Themed_UnknownRole");
        var img = go.AddComponent<Image>();

        var elem = go.AddComponent<ThemedElement>();
        elem.role = (ThemeRole)999;

        elem.ApplyTheme(theme, isDarkMode: false);

        Assert.AreEqual(Color.magenta, img.color);

        UnityEngine.Object.DestroyImmediate(go);
        UnityEngine.Object.DestroyImmediate(theme);
    }
}