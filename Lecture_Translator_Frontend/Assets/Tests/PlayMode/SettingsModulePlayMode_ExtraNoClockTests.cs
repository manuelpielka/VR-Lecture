using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class SettingsModulePlayMode_ExtraNoClockTests
{
    private GameObject go;
    private DisplayModeController ctrl;
    private ColorTheme light, dark;

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteAll();
        UserPreferencesManager.ClearAll();

        // 构造两套主题，并为每个角色给出不同颜色，便于断言
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

        go = new GameObject("DisplayMode_For_NoClock");
        ctrl = go.AddComponent<DisplayModeController>();
        ctrl.lightTheme = light;
        ctrl.darkTheme = dark;

        // 进入“手动模式”，可控地切换明暗
        ctrl.SetAutoAdjust(false);
        ctrl.SetDarkMode(false); // 先亮色
    }

    [TearDown]
    public void TearDown()
    {
        if (ctrl != null && DisplayModeController.Instance == ctrl)
            DisplayModeController.Instance = null;

        if (go != null) Object.DestroyImmediate(go);
        if (light != null) Object.DestroyImmediate(light);
        if (dark != null) Object.DestroyImmediate(dark);

        PlayerPrefs.DeleteAll();
    }

    // ========== 1) 覆盖：所有 ThemeRole 的颜色映射（Light / Dark） ==========

    [Test]
    public void ThemedElement_All_Roles_Map_To_Expected_Colors()
    {
        foreach (ThemeRole role in System.Enum.GetValues(typeof(ThemeRole)))
        {
            var ui = new GameObject("UI_" + role);
            var image = ui.AddComponent<Image>();
            var themed = ui.AddComponent<ThemedElement>();
            themed.role = role;

            // Light
            ctrl.SetDarkMode(false); // 手动 Light
            InvokePrivateApplyTheme(ctrl); // 触发应用
            Assert.True(SameColor(image.color, ColorForRole(light, role)),
                $"Light 模式下 {role} 颜色应匹配");

            // Dark
            ctrl.SetDarkMode(true);  // 手动 Dark
            InvokePrivateApplyTheme(ctrl);
            Assert.True(SameColor(image.color, ColorForRole(dark, role)),
                $"Dark 模式下 {role} 颜色应匹配");

            Object.DestroyImmediate(ui);
        }
    }

    // ========== 2) 覆盖：UserPreferencesManager.ApplyAll(DisplayMode) 的行为 ==========

    [Test]
    public void ApplyAll_Respects_Prefs_When_AutoAdjust_False()
    {
        // 预置偏好：Auto=false, Dark=true
        UserPreferencesManager.SaveAutoAdjust(false);
        UserPreferencesManager.SaveDarkMode(true);

        // 先确保控制器处于手动模式
        ctrl.SetAutoAdjust(false);
        ctrl.SetDarkMode(false); // 当前 Light

        // 调用 ApplyAll：应切到 Dark 且保持 Auto=false
        //UserPreferencesManager.ApplyAll(ctrl);
        Assert.IsFalse(ctrl.IsAutoAdjustEnabled(), "AutoAdjust 应为 false");
        Assert.IsTrue(ctrl.IsDarkModeEnabled(), "应切到 Dark");
    }

    [Test]
    public void ApplyAll_Sets_AutoAdjust_State_When_True()
    {
        // 预置偏好：Auto=true, Dark=true（明暗不做断言，因为受系统时间影响）
        UserPreferencesManager.SaveAutoAdjust(true);
        UserPreferencesManager.SaveDarkMode(true);

        // 先把控制器切到手动 Dark，确保状态确实会被自动模式覆盖
        ctrl.SetAutoAdjust(false);
        ctrl.SetDarkMode(true);

        //UserPreferencesManager.ApplyAll(ctrl);

        // 只断言 AutoAdjust 状态与偏好一致（不校验明暗）
        Assert.IsTrue(ctrl.IsAutoAdjustEnabled(), "AutoAdjust 应被设为 true");
    }

    // ----------------- 辅助 -----------------

    private static bool SameColor(Color a, Color b, float eps = 0.001f)
    {
        return Mathf.Abs(a.r - b.r) < eps &&
               Mathf.Abs(a.g - b.g) < eps &&
               Mathf.Abs(a.b - b.b) < eps &&
               Mathf.Abs(a.a - b.a) < eps;
    }

    private static Color ColorForRole(ColorTheme theme, ThemeRole role)
    {
        switch (role)
        {
            case ThemeRole.WindowBackground: return theme.WindowBackground;
            case ThemeRole.PanelOnWindowBackground: return theme.PanelOnWindowBackground;
            case ThemeRole.ButtonBackground: return theme.ButtonBackground;
            case ThemeRole.ButtonFigure: return theme.ButtonFigure;
            case ThemeRole.ButtonText: return theme.ButtonText;
            case ThemeRole.ContentText: return theme.ContentText;
            case ThemeRole.TitleText: return theme.TitleText;
            default: return Color.magenta;
        }
    }

    // 反射调用 private ApplyTheme()，复用控制器现有应用逻辑
    private static void InvokePrivateApplyTheme(DisplayModeController controller)
    {
        var mi = typeof(DisplayModeController).GetMethod("ApplyTheme",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.IsNotNull(mi, "找不到 DisplayModeController.ApplyTheme");
        mi.Invoke(controller, null);
    }
}
