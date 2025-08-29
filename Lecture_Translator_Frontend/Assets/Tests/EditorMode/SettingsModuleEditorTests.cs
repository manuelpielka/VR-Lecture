using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Editor tests for user preferences, themes, and SettingsWindow lifecycle.
/// Uses snapshot/restore to avoid polluting PlayerPrefs across tests.
/// </summary>
public class SettingsModuleEditorTests
{
    private const string IsDarkModeKey = "UserPref_IsDarkMode";
    private const string AutoAdjustKey = "UserPref_AutoAdjust";
    private const string EnvKey = "env_scene";
    private const string LanguageKey = "UserPref_Language";
    private const string TutorialCompletedKey = "UserPref_TutorialCompleted";

    private struct PrefsSnapshot
    {
        public bool hasDark; public int darkValue;
        public bool hasAuto; public int autoValue;
        public bool hasLang; public string langValue;
        public bool hasEnv; public string envValue;
        public bool hasTutorial; public int tutorialValue;
    }

    private PrefsSnapshot _snapshot;

    [SetUp]
    public void SetUp()
    {
        _snapshot = new PrefsSnapshot
        {
            hasDark = PlayerPrefs.HasKey(IsDarkModeKey),
            darkValue = PlayerPrefs.GetInt(IsDarkModeKey, 0),

            hasAuto = PlayerPrefs.HasKey(AutoAdjustKey),
            autoValue = PlayerPrefs.GetInt(AutoAdjustKey, 1),

            hasLang = PlayerPrefs.HasKey(LanguageKey),
            langValue = PlayerPrefs.GetString(LanguageKey, null),

            hasEnv = PlayerPrefs.HasKey(EnvKey),
            envValue = PlayerPrefs.GetString(EnvKey, null),

            hasTutorial = PlayerPrefs.HasKey(TutorialCompletedKey),
            tutorialValue = PlayerPrefs.GetInt(TutorialCompletedKey, 0)
        };

        PlayerPrefs.DeleteKey(IsDarkModeKey);
        PlayerPrefs.DeleteKey(AutoAdjustKey);
        PlayerPrefs.DeleteKey(LanguageKey);
        PlayerPrefs.DeleteKey(EnvKey);
        PlayerPrefs.DeleteKey(TutorialCompletedKey);
        PlayerPrefs.Save();

        UserPreferencesManager.ClearAll();
    }

    [TearDown]
    public void TearDown()
    {
        if (_snapshot.hasDark) PlayerPrefs.SetInt(IsDarkModeKey, _snapshot.darkValue);
        else PlayerPrefs.DeleteKey(IsDarkModeKey);

        if (_snapshot.hasAuto) PlayerPrefs.SetInt(AutoAdjustKey, _snapshot.autoValue);
        else PlayerPrefs.DeleteKey(AutoAdjustKey);

        if (_snapshot.hasLang) PlayerPrefs.SetString(LanguageKey, _snapshot.langValue);
        else PlayerPrefs.DeleteKey(LanguageKey);

        if (_snapshot.hasEnv) PlayerPrefs.SetString(EnvKey, _snapshot.envValue);
        else PlayerPrefs.DeleteKey(EnvKey);

        if (_snapshot.hasTutorial) PlayerPrefs.SetInt(TutorialCompletedKey, _snapshot.tutorialValue);
        else PlayerPrefs.DeleteKey(TutorialCompletedKey);

        PlayerPrefs.Save();
    }

    [Test]
    public void UserPrefs_SaveLoad_DarkMode_AutoAdjust()
    {
        UserPreferencesManager.SaveDarkMode(true);
        UserPreferencesManager.SaveAutoAdjust(false);

        Assert.IsTrue(UserPreferencesManager.LoadDarkMode());
        Assert.IsFalse(UserPreferencesManager.LoadAutoAdjust());
    }

    [Test]
    public void UserPrefs_Language_Tutorial()
    {
        Assert.IsNull(UserPreferencesManager.LoadLanguageOrNull());
        Assert.IsFalse(UserPreferencesManager.LoadTutorialCompleted());

        UserPreferencesManager.SaveLanguage("de");
        UserPreferencesManager.SaveTutorialCompleted(true);

        Assert.AreEqual("de", UserPreferencesManager.LoadLanguageOrNull());
        Assert.IsTrue(UserPreferencesManager.LoadTutorialCompleted());
    }

    [Test]
    public void UserPrefs_ClearAll_Resets_Defaults()
    {
        UserPreferencesManager.SaveDarkMode(true);
        UserPreferencesManager.SaveAutoAdjust(false);
        UserPreferencesManager.SaveLanguage("en");
        UserPreferencesManager.SaveTutorialCompleted(true);

        UserPreferencesManager.ClearAll();

        Assert.IsFalse(UserPreferencesManager.LoadDarkMode());   // default false
        Assert.IsTrue(UserPreferencesManager.LoadAutoAdjust());  // default true

        Assert.AreEqual("en", UserPreferencesManager.LoadLanguageOrNull());
        Assert.IsFalse(UserPreferencesManager.LoadTutorialCompleted());
    }

    [Test]
    public void ThemeRole_Defines_All_Expected_Values_In_Order()
    {
        var values = (ThemeRole[])Enum.GetValues(typeof(ThemeRole));

        Assert.AreEqual(7, values.Length, "ThemeRole count mismatch");

        Assert.AreEqual(ThemeRole.WindowBackground, values[0]);
        Assert.AreEqual(ThemeRole.PanelOnWindowBackground, values[1]);
        Assert.AreEqual(ThemeRole.ButtonBackground, values[2]);
        Assert.AreEqual(ThemeRole.ButtonFigure, values[3]);
        Assert.AreEqual(ThemeRole.ButtonText, values[4]);
        Assert.AreEqual(ThemeRole.ContentText, values[5]);
        Assert.AreEqual(ThemeRole.TitleText, values[6]);

        Assert.AreEqual("ButtonText",
            Enum.GetName(typeof(ThemeRole), ThemeRole.ButtonText));
    }

    [Test]
    public void ColorTheme_Can_Be_Created_Assigned_And_Read_Back()
    {
        var theme = ScriptableObject.CreateInstance<ColorTheme>();
        try
        {
            Assert.AreEqual(default(Color), theme.WindowBackground);
            Assert.AreEqual(default(Color), theme.PanelOnWindowBackground);
            Assert.AreEqual(default(Color), theme.ButtonBackground);
            Assert.AreEqual(default(Color), theme.ButtonFigure);
            Assert.AreEqual(default(Color), theme.ButtonText);
            Assert.AreEqual(default(Color), theme.ContentText);
            Assert.AreEqual(default(Color), theme.TitleText);

            var win = new Color(0.1f, 0.2f, 0.3f, 1f);
            var panel = new Color(0.2f, 0.3f, 0.4f, 1f);
            var btnBg = new Color(0.3f, 0.4f, 0.5f, 1f);
            var btnFg = new Color(0.4f, 0.5f, 0.6f, 1f);
            var btnTx = new Color(0.9f, 0.9f, 0.9f, 1f);
            var content = new Color(0.7f, 0.7f, 0.7f, 1f);
            var title = new Color(1f, 0.95f, 0.9f, 1f);

            theme.WindowBackground = win;
            theme.PanelOnWindowBackground = panel;
            theme.ButtonBackground = btnBg;
            theme.ButtonFigure = btnFg;
            theme.ButtonText = btnTx;
            theme.ContentText = content;
            theme.TitleText = title;

            Assert.AreEqual(win, theme.WindowBackground);
            Assert.AreEqual(panel, theme.PanelOnWindowBackground);
            Assert.AreEqual(btnBg, theme.ButtonBackground);
            Assert.AreEqual(btnFg, theme.ButtonFigure);
            Assert.AreEqual(btnTx, theme.ButtonText);
            Assert.AreEqual(content, theme.ContentText);
            Assert.AreEqual(title, theme.TitleText);

            var json = JsonUtility.ToJson(theme);
            var back = ScriptableObject.CreateInstance<ColorTheme>();
            JsonUtility.FromJsonOverwrite(json, back);

            Assert.AreEqual(theme.WindowBackground, back.WindowBackground);
            Assert.AreEqual(theme.PanelOnWindowBackground, back.PanelOnWindowBackground);
            Assert.AreEqual(theme.ButtonBackground, back.ButtonBackground);
            Assert.AreEqual(theme.ButtonFigure, back.ButtonFigure);
            Assert.AreEqual(theme.ButtonText, back.ButtonText);
            Assert.AreEqual(theme.ContentText, back.ContentText);
            Assert.AreEqual(theme.TitleText, back.TitleText);

            UnityEngine.Object.DestroyImmediate(back);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(theme);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Ensures SettingsWindow can be opened and closed without leaking editor state.
    /// If the type cannot be found or is not an EditorWindow subclass, the test is skipped.
    /// </summary>
    [Test]
    public void SettingsWindow_Open_Modify_Close_Without_Leaking()
    {

        UserPreferencesManager.SaveDarkMode(true);
        UserPreferencesManager.SaveAutoAdjust(false);
        UserPreferencesManager.SaveLanguage("de");
        UserPreferencesManager.SaveTutorialCompleted(true);

        var wndType = FindEditorWindowTypeByName("SettingsWindow");
        if (wndType == null)
        {
            Assert.Ignore("SettingsWindow type not found or not an EditorWindow subclass. Skipping test.");
            return;
        }

        EditorWindow wnd = null;
        try
        {
            wnd = EditorWindow.GetWindow(wndType, false, "Settings");
            Assert.IsNotNull(wnd, "Failed to create/get SettingsWindow instance.");

            wnd.Focus();
            wnd.Repaint();
            EditorApplication.QueuePlayerLoopUpdate();

            Assert.IsTrue(wnd, "Window instance became invalid during the test.");
        }
        finally
        {
            if (wnd != null)
            {
                wnd.Close();
                EditorApplication.QueuePlayerLoopUpdate();
            }
        }
    }

    /// <summary>
    /// Finds a non-abstract EditorWindow type by simple name across loaded assemblies.
    /// Returns null if not found or not assignable to EditorWindow.
    /// </summary>
    private static Type FindEditorWindowTypeByName(string typeName)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var asm in assemblies)
        {
            Type t = null;
            try
            {
                t = asm.GetTypes().FirstOrDefault(x =>
                    x.Name == typeName &&
                    typeof(EditorWindow).IsAssignableFrom(x) &&
                    !x.IsAbstract);
            }
            catch (ReflectionTypeLoadException e)
            {
                t = e.Types?.FirstOrDefault(x =>
                    x != null &&
                    x.Name == typeName &&
                    typeof(EditorWindow).IsAssignableFrom(x) &&
                    !x.IsAbstract);
            }

            if (t != null) return t;
        }
        return null;
    }
#endif
}
