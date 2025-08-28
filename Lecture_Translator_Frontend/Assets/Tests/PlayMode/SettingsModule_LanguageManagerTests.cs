using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.TestTools;

public class SettingsModule_LanguageManagerTests
{
    private LanguageManager lang;

    [SetUp]
    public void SetUp()
    {
        lang = new LanguageManager();
    }

    [UnityTest]
    public IEnumerator InitializeAsync_Then_Query_Methods_Work()
    {
        yield return LocalizationSettings.InitializationOperation;

        var initTask = lang.InitializeAsync();
        while (!initTask.IsCompleted) yield return null;

        var initTask2 = lang.InitializeAsync();
        while (!initTask2.IsCompleted) yield return null;

        var dict = lang.GetLanguages();
        Assert.IsNotNull(dict);

        Assert.IsFalse(lang.LanguageExists(null));
        Assert.IsFalse(lang.LanguageExists("xx-nonexist"));

        var code = lang.GetCurrentLanguageCode();
        Assert.IsTrue(code == null || code.Length > 0);

        var setTask = lang.SetLanguageAsync("xx-nonexist");
        while (!setTask.IsCompleted) yield return null;

        Assert.Pass();
    }

    /// <summary>
    /// cover SelectedLocale == targetLocale, return
    /// </summary>
    [UnityTest]
    public IEnumerator SetLanguageAsync_Returns_When_Target_Equals_Current()
    {
        // 1) Localization
        yield return LocalizationSettings.InitializationOperation;
        var initTask = lang.InitializeAsync();
        while (!initTask.IsCompleted) yield return null;

        // 2) A
        var locales = LocalizationSettings.AvailableLocales.Locales;
        Assert.Greater(locales.Count, 0, "at least one Locale");
        var a = locales[0];
        LocalizationSettings.SelectedLocale = a;
        yield return null; 

        // 3) SetLanguageAsync(A)
        var sameTask = lang.SetLanguageAsync(a.Identifier.Code);
        while (!sameTask.IsCompleted) yield return null;

        // A
        Assert.AreSame(a, LocalizationSettings.SelectedLocale);
    }

    /// <summary>
    /// cover different lang, SelectedLocale = targetLocale
    /// </summary>
    [UnityTest]
    public IEnumerator SetLanguageAsync_Switches_When_Target_Differs()
    {
        // 1)
        yield return LocalizationSettings.InitializationOperation;
        var initTask = lang.InitializeAsync();
        while (!initTask.IsCompleted) yield return null;

        // 2)
        var locales = LocalizationSettings.AvailableLocales.Locales;
        Locale a = locales[0];
        Locale b = null;

        if (locales.Count > 1)
        {
            b = locales[1];
        }
        else
        {
            var id = new LocaleIdentifier("de");
            if (a.Identifier.Code == "de") id = new LocaleIdentifier("fr");
            b = Locale.CreateLocale(id);
            LocalizationSettings.AvailableLocales.AddLocale(b);
        }

        // 3) A
        LocalizationSettings.SelectedLocale = a;
        yield return null;

        // 4) SetLanguageAsync(B)
        var switchTask = lang.SetLanguageAsync(b.Identifier.Code);
        while (!switchTask.IsCompleted) yield return null;

        // 5) B
        Assert.AreSame(b, LocalizationSettings.SelectedLocale);
    }
}
