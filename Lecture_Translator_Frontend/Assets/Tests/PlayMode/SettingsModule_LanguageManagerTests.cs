using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.TestTools;

public class SettingsModule_LanguageManagerTests
{
    private LanguageManager lang;
    private Locale _prevSelected;
    private List<Locale> _originalLocales;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        yield return LocalizationSettings.InitializationOperation;

        _prevSelected = LocalizationSettings.SelectedLocale;
        _originalLocales = new List<Locale>(LocalizationSettings.AvailableLocales.Locales);

        lang = new LanguageManager();

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        var list = LocalizationSettings.AvailableLocales;
        foreach (var cur in list.Locales.ToList())
            list.RemoveLocale(cur);
        foreach (var orig in _originalLocales)
            list.AddLocale(orig);

        LocalizationSettings.SelectedLocale = _prevSelected;

        yield return null;
    }

    [UnityTest]
    public IEnumerator InitializeAsync_Idempotent_And_Query_Works()
    {
        var t1 = lang.InitializeAsync();
        while (!t1.IsCompleted) yield return null;

        var t2 = lang.InitializeAsync();
        while (!t2.IsCompleted) yield return null;

        var dict = lang.GetLanguages();
        Assert.IsNotNull(dict);
        Assert.Greater(dict.Count, 0, "Language list should not be empty after init.");

        var any = LocalizationSettings.AvailableLocales.Locales[0];
        Assert.IsTrue(lang.LanguageExists(any.Identifier.Code));

        Assert.IsFalse(lang.LanguageExists(null));
        Assert.IsFalse(lang.LanguageExists(""));
        Assert.IsFalse(lang.LanguageExists("xx-nonexist"));

        var code = lang.GetCurrentLanguageCode();
        Assert.IsTrue(code == null || code.Length > 0);

        var before = LocalizationSettings.SelectedLocale;
        LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(@"\[LanguageManager\] xx-nonexist not found"));
        var setTask = lang.SetLanguageAsync("xx-nonexist");
        while (!setTask.IsCompleted) yield return null;
        Assert.AreSame(before, LocalizationSettings.SelectedLocale);

        yield return null;
    }

    [UnityTest]
    public IEnumerator SetLanguageAsync_Returns_When_Target_Equals_Current()
    {
        var initTask = lang.InitializeAsync();
        while (!initTask.IsCompleted) yield return null;

        var locales = LocalizationSettings.AvailableLocales.Locales;
        Assert.Greater(locales.Count, 0, "At least one Locale must exist for this test.");
        var a = locales[0];
        LocalizationSettings.SelectedLocale = a;
        yield return null;

        var sameTask = lang.SetLanguageAsync(a.Identifier.Code);
        while (!sameTask.IsCompleted) yield return null;

        Assert.AreSame(a, LocalizationSettings.SelectedLocale);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SetLanguageAsync_Switches_When_Target_Differs()
    {
        var initTask = lang.InitializeAsync();
        while (!initTask.IsCompleted) yield return null;

        var list = LocalizationSettings.AvailableLocales;
        var locales = list.Locales;

        var a = locales[0];
        Locale b = locales.Count > 1 ? locales[1] : null;
        if (b == null)
        {
            var candidate = new LocaleIdentifier(a.Identifier.Code == "de" ? "fr" : "de");
            b = Locale.CreateLocale(candidate);
            list.AddLocale(b);
        }

        LocalizationSettings.SelectedLocale = a;
        yield return null;

        var switchTask = lang.SetLanguageAsync(b.Identifier.Code);
        while (!switchTask.IsCompleted) yield return null;

        Assert.AreSame(b, LocalizationSettings.SelectedLocale);
        yield return null;
    }
}
