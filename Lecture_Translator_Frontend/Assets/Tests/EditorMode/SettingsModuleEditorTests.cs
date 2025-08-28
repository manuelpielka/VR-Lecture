using NUnit.Framework;
using UnityEngine;

public class SettingsModuleEditorTests
{
    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteAll();
        UserPreferencesManager.ClearAll();
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteAll();
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
        //Assert.IsNull(UserPreferencesManager.LoadEnvironmentSceneOrNull());
        Assert.IsFalse(UserPreferencesManager.LoadTutorialCompleted());

        UserPreferencesManager.SaveLanguage("de");
        //UserPreferencesManager.SaveEnvironmentScene("DemoScene");
        UserPreferencesManager.SaveTutorialCompleted(true);

        Assert.AreEqual("de", UserPreferencesManager.LoadLanguageOrNull());
        //Assert.AreEqual("DemoScene", UserPreferencesManager.LoadEnvironmentSceneOrNull());
        Assert.IsTrue(UserPreferencesManager.LoadTutorialCompleted());
    }

    [Test]
    public void UserPrefs_ClearAll_Resets_Defaults()
    {
        UserPreferencesManager.SaveDarkMode(true);
        UserPreferencesManager.SaveAutoAdjust(false);
        UserPreferencesManager.ClearAll();

        Assert.IsFalse(UserPreferencesManager.LoadDarkMode()); // default false
        Assert.IsTrue(UserPreferencesManager.LoadAutoAdjust()); // default true
    }

    [Test]
    public void ThemeRole_Defines_All_Expected_Values_In_Order()
    {
        var values = (ThemeRole[])System.Enum.GetValues(typeof(ThemeRole));

        Assert.AreEqual(7, values.Length, "ThemeRole count mismatch");

        Assert.AreEqual(ThemeRole.WindowBackground, values[0]);
        Assert.AreEqual(ThemeRole.PanelOnWindowBackground, values[1]);
        Assert.AreEqual(ThemeRole.ButtonBackground, values[2]);
        Assert.AreEqual(ThemeRole.ButtonFigure, values[3]);
        Assert.AreEqual(ThemeRole.ButtonText, values[4]);
        Assert.AreEqual(ThemeRole.ContentText, values[5]);
        Assert.AreEqual(ThemeRole.TitleText, values[6]);

        Assert.AreEqual("ButtonText",
            System.Enum.GetName(typeof(ThemeRole), ThemeRole.ButtonText));
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

            Object.DestroyImmediate(back);
        }
        finally
        {
            Object.DestroyImmediate(theme);
        }
    }
}
