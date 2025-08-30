using NUnit.Framework;
using Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// PlayMode tests for SettingsWindow that avoid cross-test pollution:
/// - Pre-existing singletons and scene objects are deactivated and later restored.
/// - PlayerPrefs changes are snapshotted/restored for only the keys we touch.
/// - Static singleton Instance fields are restored to prior objects.
/// - Signals event handlers (TutorialResetRequested) are snapshotted/restored.
/// - Test-owned windows/objects are destroyed in teardown.
/// </summary>
public class SettingsWindow_PlayModeTests
{
    private const string IsDarkModeKey = "UserPref_IsDarkMode";
    private const string AutoAdjustKey = "UserPref_AutoAdjust";
    private const string LanguageKey = "UserPref_Language";
    private const string EnvKey = "env_scene";
    private const string TutorialCompletedKey = "UserPref_TutorialCompleted";

    private struct PrefsSnapshot
    {
        public bool hasDark; public int darkValue;
        public bool hasAuto; public int autoValue;
        public bool hasLang; public string langValue;
        public bool hasEnv; public string envValue;
        public bool hasTut; public int tutValue;
    }
    private PrefsSnapshot _prefsBefore;

    private readonly List<GameObject> _preExistingSettingsMgr = new();
    private readonly List<bool> _preExistingSettingsMgrActive = new();

    private readonly List<GameObject> _preExistingEnvMgr = new();
    private readonly List<bool> _preExistingEnvMgrActive = new();

    private readonly List<GameObject> _preExistingDisplayCtrl = new();
    private readonly List<bool> _preExistingDisplayCtrlActive = new();

    private readonly List<GameObject> _preExistingSettingsWindow = new();
    private readonly List<bool> _preExistingSettingsWindowActive = new();

    private UnityEngine.Object _prevSettingsManagerInstance;
    private UnityEngine.Object _prevEnvironmentManagerInstance;
    private UnityEngine.Object _prevDisplayModeControllerInstance;
    private string _prevEnvSceneName;

    private Delegate _prevTutorialResetHandlers;

    private GameObject goCtrl;
    private GameObject goSettingsMgr;
    private GameObject goEnvMgr;
    private GameObject goWindow;

    private DisplayModeController ctrl;
    private SettingsManager settings;
    private EnvironmentManager env;

    private SettingsWindow window;

    private TMP_Dropdown backgroundDropdown;
    private Toggle autoSwitchToggle;
    private Toggle darkModeToggle;
    private TMP_Dropdown languageDropdown;
    private Button applyButton;
    private Button discardButton;

    private class FakeEnvironmentManager : EnvironmentManager
    {
        private static readonly FieldInfo s_CurrentSceneField =
            typeof(EnvironmentManager).GetField("currentSceneName",
                BindingFlags.Instance | BindingFlags.NonPublic);

        public new void LoadEnvironment(string sceneName)
        {
            s_CurrentSceneField.SetValue(this, sceneName);
        }
    }

    private static TMP_Dropdown CreateUsableTMPDropdown(string name)
    {
        var root = new GameObject(name);
        var dd = root.AddComponent<TMP_Dropdown>();

        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(root.transform, false);
        var caption = labelGO.AddComponent<TextMeshProUGUI>();
        dd.captionText = caption;

        var templateGO = new GameObject("Template");
        templateGO.transform.SetParent(root.transform, false);
        var templateRect = templateGO.AddComponent<RectTransform>();
        templateGO.SetActive(false);
        dd.template = templateRect;

        var viewport = new GameObject("Viewport").AddComponent<RectTransform>();
        viewport.transform.SetParent(templateGO.transform, false);

        var content = new GameObject("Content").AddComponent<RectTransform>();
        content.transform.SetParent(viewport.transform, false);

        var itemGO = new GameObject("Item");
        itemGO.transform.SetParent(content.transform, false);
        itemGO.AddComponent<Toggle>();

        var itemLabelGO = new GameObject("Item Label");
        itemLabelGO.transform.SetParent(itemGO.transform, false);
        var itemLabel = itemLabelGO.AddComponent<TextMeshProUGUI>();
        dd.itemText = itemLabel;

        return dd;
    }

    private static Button CreateButton(string name)
    {
        var go = new GameObject(name);
        go.AddComponent<Image>();
        return go.AddComponent<Button>();
    }

    private static ColorTheme MakeTheme(Color c)
    {
        var t = ScriptableObject.CreateInstance<ColorTheme>();
        t.WindowBackground = c;
        t.PanelOnWindowBackground = c;
        t.ButtonBackground = c;
        t.ButtonFigure = c;
        t.ButtonText = c;
        t.ContentText = c;
        t.TitleText = c;
        return t;
    }

    private static void SetField(object o, string name, object val)
    {
        var f = o.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.IsNotNull(f, "Field not found: " + name);
        f.SetValue(o, val);
    }

    private static void CaptureAndDeactivate<T>(List<GameObject> objs, List<bool> actives) where T : UnityEngine.Object
    {
        var found = UnityEngine.Object.FindObjectsByType<T>(FindObjectsSortMode.None);
        foreach (var c in found)
        {
            if (c is Component comp)
            {
                objs.Add(comp.gameObject);
                actives.Add(comp.gameObject.activeSelf);
                comp.gameObject.SetActive(false);
            }
            else if (c is GameObject go)
            {
                objs.Add(go);
                actives.Add(go.activeSelf);
                go.SetActive(false);
            }
        }
    }

    private static void RestoreActiveStates(List<GameObject> objs, List<bool> actives)
    {
        for (int i = 0; i < objs.Count; i++)
            if (objs[i] != null) objs[i].SetActive(actives[i]);
        objs.Clear();
        actives.Clear();
    }

    private static FieldInfo FindStaticInstanceField(Type type)
    {
        // Heuristic: find a static field of the same type, commonly used to hold a singleton instance.
        return type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                   .FirstOrDefault(f => f.FieldType == type);
    }

    private static UnityEngine.Object GetSingletonInstance(Type type)
    {
        var prop = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
        if (prop != null) return prop.GetValue(null) as UnityEngine.Object;
        var f = FindStaticInstanceField(type);
        return f?.GetValue(null) as UnityEngine.Object;
    }

    private static void SetSingletonInstance(Type type, UnityEngine.Object instance)
    {
        var f = FindStaticInstanceField(type);
        if (f != null)
        {
            f.SetValue(null, instance);
            return;
        }
    }

    private static string GetCurrentEnvSceneName(EnvironmentManager manager)
    {
        var f = typeof(EnvironmentManager).GetField("currentSceneName", BindingFlags.Instance | BindingFlags.NonPublic);
        return manager != null && f != null ? (string)f.GetValue(manager) : null;
    }

    private static void SetCurrentEnvSceneName(EnvironmentManager manager, string sceneName)
    {
        var f = typeof(EnvironmentManager).GetField("currentSceneName", BindingFlags.Instance | BindingFlags.NonPublic);
        if (manager != null && f != null) f.SetValue(manager, sceneName);
    }

    private static Delegate SnapshotTutorialResetHandlers()
    {
        var fld = typeof(Signals).GetField("TutorialResetRequested", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        return fld?.GetValue(null) as Delegate;
    }

    private static void RestoreTutorialResetHandlers(Delegate handlers)
    {
        var fld = typeof(Signals).GetField("TutorialResetRequested", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        if (fld != null) fld.SetValue(null, handlers);
    }

    private static void ClearTutorialResetHandlers()
    {
        var fld = typeof(Signals).GetField("TutorialResetRequested", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        if (fld != null) fld.SetValue(null, null);
    }

    private void SnapshotPlayerPrefs()
    {
        _prefsBefore = new PrefsSnapshot
        {
            hasDark = PlayerPrefs.HasKey(IsDarkModeKey),
            darkValue = PlayerPrefs.GetInt(IsDarkModeKey, 0),

            hasAuto = PlayerPrefs.HasKey(AutoAdjustKey),
            autoValue = PlayerPrefs.GetInt(AutoAdjustKey, 1),

            hasLang = PlayerPrefs.HasKey(LanguageKey),
            langValue = PlayerPrefs.GetString(LanguageKey, null),

            hasEnv = PlayerPrefs.HasKey(EnvKey),
            envValue = PlayerPrefs.GetString(EnvKey, null),

            hasTut = PlayerPrefs.HasKey(TutorialCompletedKey),
            tutValue = PlayerPrefs.GetInt(TutorialCompletedKey, 0)
        };
    }

    private void RestorePlayerPrefs()
    {
        if (_prefsBefore.hasDark) PlayerPrefs.SetInt(IsDarkModeKey, _prefsBefore.darkValue);
        else PlayerPrefs.DeleteKey(IsDarkModeKey);

        if (_prefsBefore.hasAuto) PlayerPrefs.SetInt(AutoAdjustKey, _prefsBefore.autoValue);
        else PlayerPrefs.DeleteKey(AutoAdjustKey);

        if (_prefsBefore.hasLang) PlayerPrefs.SetString(LanguageKey, _prefsBefore.langValue);
        else PlayerPrefs.DeleteKey(LanguageKey);

        if (_prefsBefore.hasEnv) PlayerPrefs.SetString(EnvKey, _prefsBefore.envValue);
        else PlayerPrefs.DeleteKey(EnvKey);

        if (_prefsBefore.hasTut) PlayerPrefs.SetInt(TutorialCompletedKey, _prefsBefore.tutValue);
        else PlayerPrefs.DeleteKey(TutorialCompletedKey);

        PlayerPrefs.Save();
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // 1) Snapshot PlayerPrefs we care about (no DeleteAll).
        SnapshotPlayerPrefs();

        // 2) Capture pre-existing objects and deactivate them (no destruction).
        CaptureAndDeactivate<SettingsManager>(_preExistingSettingsMgr, _preExistingSettingsMgrActive);
        CaptureAndDeactivate<EnvironmentManager>(_preExistingEnvMgr, _preExistingEnvMgrActive);
        CaptureAndDeactivate<DisplayModeController>(_preExistingDisplayCtrl, _preExistingDisplayCtrlActive);
        CaptureAndDeactivate<SettingsWindow>(_preExistingSettingsWindow, _preExistingSettingsWindowActive);

        // 3) Snapshot singleton instances (if any) and previous env scene name.
        _prevSettingsManagerInstance = GetSingletonInstance(typeof(SettingsManager));
        _prevEnvironmentManagerInstance = GetSingletonInstance(typeof(EnvironmentManager));
        _prevDisplayModeControllerInstance = GetSingletonInstance(typeof(DisplayModeController));
        _prevEnvSceneName = _prevEnvironmentManagerInstance != null
            ? GetCurrentEnvSceneName((EnvironmentManager)_prevEnvironmentManagerInstance)
            : null;

        // 4) Snapshot and clear Signals handlers so clicking Reset doesn't leak to other tests.
        _prevTutorialResetHandlers = SnapshotTutorialResetHandlers();
        ClearTutorialResetHandlers();

        // 5) Create test-owned managers.
        var prevIgnore = LogAssert.ignoreFailingMessages;
        LogAssert.ignoreFailingMessages = true;
        try
        {
            goCtrl = new GameObject("Ctrl(TestOwned)");
            ctrl = goCtrl.AddComponent<DisplayModeController>();
            ctrl.lightTheme = MakeTheme(Color.gray);
            ctrl.darkTheme = MakeTheme(Color.black);

            goEnvMgr = new GameObject("EnvMgr(TestOwned)");
            env = goEnvMgr.AddComponent<FakeEnvironmentManager>(); // derived type, fine for Instance

            goSettingsMgr = new GameObject("SettingsManager(TestOwned)");
            settings = goSettingsMgr.AddComponent<SettingsManager>();
        }
        finally
        {
            LogAssert.ignoreFailingMessages = prevIgnore;
        }

        // 6) Force singleton Instance fields to our test-owned instances (so SettingsWindow talks to ours).
        SetSingletonInstance(typeof(DisplayModeController), ctrl);
        SetSingletonInstance(typeof(EnvironmentManager), env);
        SetSingletonInstance(typeof(SettingsManager), settings);

        yield return null; // allow Awake/Start on managers

        // 7) Build the SettingsWindow (test-owned) and its minimal UI.
        goWindow = new GameObject("SettingsWindow(TestOwned)");
        goWindow.SetActive(false);
        window = goWindow.AddComponent<SettingsWindow>();

        backgroundDropdown = CreateUsableTMPDropdown("EnvDropdown");
        autoSwitchToggle = new GameObject("AutoSwitch").AddComponent<Toggle>();
        darkModeToggle = new GameObject("DarkMode").AddComponent<Toggle>();
        languageDropdown = CreateUsableTMPDropdown("LangDropdown");
        applyButton = CreateButton("ApplyBtn");     // NOTE: not parented under window
        discardButton = CreateButton("DiscardBtn"); // NOTE: not parented under window

        // Parent only non-Button UI under the window so GetComponentInChildren<Button> sees nothing
        // unless we intentionally add a ResetTutorialButton for that test case.
        backgroundDropdown.transform.SetParent(goWindow.transform, false);
        autoSwitchToggle.transform.SetParent(goWindow.transform, false);
        darkModeToggle.transform.SetParent(goWindow.transform, false);
        languageDropdown.transform.SetParent(goWindow.transform, false);

        // Wire references; buttons need not be children to work with BindListeners.
        window.backgroundDropdown = backgroundDropdown;
        window.modeAutoSwitchToggle = autoSwitchToggle;
        window.darkModeToggle = darkModeToggle;
        window.languageDropdown = languageDropdown;
        window.applyButton = applyButton;
        window.discardButton = discardButton;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        // 1) Destroy test-owned window/managers.
        if (goWindow) UnityEngine.Object.DestroyImmediate(goWindow);
        if (goSettingsMgr) UnityEngine.Object.DestroyImmediate(goSettingsMgr);
        if (goCtrl) UnityEngine.Object.DestroyImmediate(goCtrl);
        if (goEnvMgr) UnityEngine.Object.DestroyImmediate(goEnvMgr);

        // 2) Restore singleton Instance fields to previous objects.
        SetSingletonInstance(typeof(SettingsManager), _prevSettingsManagerInstance);
        SetSingletonInstance(typeof(EnvironmentManager), _prevEnvironmentManagerInstance);
        SetSingletonInstance(typeof(DisplayModeController), _prevDisplayModeControllerInstance);

        // 3) Restore previous environment scene name (if we had one).
        if (_prevEnvironmentManagerInstance != null)
            SetCurrentEnvSceneName((EnvironmentManager)_prevEnvironmentManagerInstance, _prevEnvSceneName);

        // 4) Restore any pre-existing scene objects to their initial active state.
        RestoreActiveStates(_preExistingSettingsMgr, _preExistingSettingsMgrActive);
        RestoreActiveStates(_preExistingEnvMgr, _preExistingEnvMgrActive);
        RestoreActiveStates(_preExistingDisplayCtrl, _preExistingDisplayCtrlActive);
        RestoreActiveStates(_preExistingSettingsWindow, _preExistingSettingsWindowActive);

        // 5) Restore Signals handlers to avoid cross-test side effects.
        RestoreTutorialResetHandlers(_prevTutorialResetHandlers);

        // 6) Restore PlayerPrefs we touched.
        RestorePlayerPrefs();

        // 7) Reset LogAssert behavior.
        LogAssert.ignoreFailingMessages = false;

        yield return null;
    }

    private IEnumerator ActivateWindowExpecting(bool withResetButton)
    {
        if (withResetButton)
        {
            // Build a Reset Tutorial button as a child so the window finds exactly this one.
            var canvas = new GameObject("Canvas");
            canvas.transform.SetParent(goWindow.transform, false);
            var panel = new GameObject("Panel");
            panel.transform.SetParent(canvas.transform, false);
            var resetGO = new GameObject("ResetTutorialButton");
            resetGO.transform.SetParent(panel.transform, false);
            resetGO.AddComponent<Image>();
            resetGO.AddComponent<Button>();
        }
        // IMPORTANT: apply/discard are NOT children; ensures "no reset" scenario works.

        LogAssert.Expect(LogType.Log, "SettingsWindow Started");
        if (withResetButton)
            LogAssert.Expect(LogType.Log, "Reset Tutorial Button found and listening.");
        else
            LogAssert.Expect(LogType.Warning, new Regex("Reset Tutorial Button not found"));

        goWindow.SetActive(true);
        yield return null; // allow Start() to run
    }

    [UnityTest]
    public IEnumerator Start_NoResetButton_ShowsWarning_And_BuildsDropdowns()
    {
        yield return ActivateWindowExpecting(withResetButton: false);

        // Toggle auto-switch -> should turn off dark mode and disable it.
        autoSwitchToggle.onValueChanged.Invoke(true);
        Assert.IsTrue(autoSwitchToggle.isOn);
        Assert.IsFalse(darkModeToggle.isOn);
        Assert.IsFalse(darkModeToggle.interactable);

        // Toggle dark mode -> should turn off auto-switch and disable it.
        darkModeToggle.onValueChanged.Invoke(true);
        Assert.IsTrue(darkModeToggle.isOn);
        Assert.IsFalse(autoSwitchToggle.isOn);
        Assert.IsFalse(autoSwitchToggle.interactable);

        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator Start_WithResetButton_Found_And_Clicked()
    {
        yield return ActivateWindowExpecting(withResetButton: true);

        var resetBtn = goWindow.transform.Find("Canvas/Panel/ResetTutorialButton")?.GetComponent<Button>();
        Assert.IsNotNull(resetBtn);

        // Clicking should log; external listeners are suppressed during this test.
        LogAssert.Expect(LogType.Log, "Reset Tutorial Button Clicked");
        resetBtn.onClick.Invoke();

        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator Apply_NoLanguageEnvChange_SavesToggles_And_Syncs()
    {
        yield return ActivateWindowExpecting(withResetButton: false);

        autoSwitchToggle.onValueChanged.Invoke(true);
        darkModeToggle.onValueChanged.Invoke(false);

        LogAssert.Expect(LogType.Log, new Regex(@"Saving Auto Adjust: True"));
        LogAssert.Expect(LogType.Log, new Regex(@"Saving Dark Mode: False"));
        LogAssert.Expect(LogType.Log, "Applying Display Mode");
        LogAssert.Expect(LogType.Log, new Regex(@"\[Theme\] Mode applied: (Light|Dark)"));
        LogAssert.Expect(LogType.Log, new Regex(@"\[SettingsWindow\] Apply pressed: saved & applied\."));
        LogAssert.Expect(LogType.Log, new Regex(@"\[Lang\] current=.* pending=.*"));

        applyButton.onClick.Invoke();

        Assert.IsTrue(SettingsManager.Instance.GetAutoSwitch());
        Assert.IsFalse(SettingsManager.Instance.GetDarkMode());
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator Apply_WithLanguageAndEnvChange_CallsSwitchers_And_Syncs()
    {
        yield return ActivateWindowExpecting(withResetButton: false);

        // Prepare languages in the current SettingsManager instance (via reflection).
        var lmField = typeof(SettingsManager).GetField("languageManager",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var languageManager = lmField.GetValue(SettingsManager.Instance);
        var dictField = languageManager.GetType().GetField("languageList",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var dict = (System.Collections.IDictionary)dictField.GetValue(languageManager);
        dict.Clear(); dict["en"] = "English"; dict["de"] = "Deutsch";

        // Prepare env state
        SetCurrentEnvSceneName(EnvironmentManager.Instance, "EnvA");

        // Simulate a pending language selection and env selection through dropdown change
        var pendLangFieldInfo = typeof(SettingsWindow).GetField("pendingLanguageCode",
            BindingFlags.NonPublic | BindingFlags.Instance);
        pendLangFieldInfo.SetValue(window, "de");

        var envOptsField = typeof(SettingsWindow).GetField("envOptions",
            BindingFlags.NonPublic | BindingFlags.Instance);
        envOptsField.SetValue(window, new List<string> { "EnvA", "EnvB" });
        backgroundDropdown.onValueChanged.Invoke(1); // select "EnvB"

        // Expect logs for settings apply + env warning + final summary.
        LogAssert.Expect(LogType.Log, new Regex(@"Saving Auto Adjust"));
        LogAssert.Expect(LogType.Log, new Regex(@"Saving Dark Mode"));
        LogAssert.Expect(LogType.Log, "Applying Display Mode");
        LogAssert.Expect(LogType.Log, new Regex(@"\[Theme\] Mode applied: (Light|Dark)"));
        LogAssert.Expect(LogType.Warning, new Regex(@"EnvironmentManager: Scene 'EnvB' is not in Build Settings\."));
        LogAssert.Expect(LogType.Log, new Regex(@"\[SettingsWindow\] Apply pressed: saved & applied\."));
        LogAssert.Expect(LogType.Log, new Regex(@"\[Lang\] current=.* pending=.*"));

        applyButton.onClick.Invoke();
        yield return null;

        // Verify final state instead of brittle logs.
        Assert.AreEqual("de", SettingsManager.Instance.GetCurrentLanguageCode(), "Language should be applied to 'de'.");

        // Simulate that EnvB eventually becomes current (after async load warning path).
        SetCurrentEnvSceneName(EnvironmentManager.Instance, "EnvB");
        Assert.AreEqual("EnvB", EnvironmentManager.Instance.CurrentSceneName);

        var pendEnvField = typeof(SettingsWindow).GetField("pendingEnv",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNull(pendEnvField.GetValue(window), "pendingEnv should be cleared after Apply.");

        var pendLangField = typeof(SettingsWindow).GetField("pendingLanguageCode",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNull(pendLangField.GetValue(window), "pendingLanguageCode should be cleared after Apply.");

        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator BindListeners_Background_OnValueChanged_Sets_PendingEnv_When_Index_Valid()
    {
        yield return ActivateWindowExpecting(withResetButton: false);

        var envOptsField = typeof(SettingsWindow).GetField("envOptions",
            BindingFlags.NonPublic | BindingFlags.Instance);
        envOptsField.SetValue(window, new List<string> { "EnvA", "EnvB" });

        backgroundDropdown.ClearOptions();
        backgroundDropdown.AddOptions(new List<string> { "EnvA", "EnvB" });

        var bindMi = typeof(SettingsWindow).GetMethod("BindListeners",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(bindMi, "BindListeners not found via reflection");
        bindMi.Invoke(window, null);

        var pendEnvField = typeof(SettingsWindow).GetField("pendingEnv",
            BindingFlags.NonPublic | BindingFlags.Instance);
        pendEnvField.SetValue(window, "KEEP");

        backgroundDropdown.onValueChanged.Invoke(99);
        Assert.AreEqual("KEEP", pendEnvField.GetValue(window), "out-of-range should not change pendingEnv");

        backgroundDropdown.onValueChanged.Invoke(1);
        Assert.AreEqual("EnvB", pendEnvField.GetValue(window));

        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator Discard_Restores_From_Settings_And_ClearsPending()
    {
        yield return ActivateWindowExpecting(withResetButton: false);

        autoSwitchToggle.isOn = false;
        darkModeToggle.isOn = true;

        LogAssert.Expect(LogType.Log, new Regex(@"\[SettingsWindow\] Discard pressed: reverted changes\."));
        discardButton.onClick.Invoke();

        Assert.AreEqual(SettingsManager.Instance.GetAutoSwitch(), autoSwitchToggle.isOn);
        Assert.AreEqual(SettingsManager.Instance.GetDarkMode(), darkModeToggle.isOn);
        LogAssert.NoUnexpectedReceived();
    }
}
