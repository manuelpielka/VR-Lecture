using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class SettingsWindow_PlayModeTests
{
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

    private GameObject goCtrl;         
    private GameObject goSettingsMgr;  
    private GameObject goEnvMgr;        
    private GameObject goWindow;        

    private DisplayModeController ctrl;
    private SettingsManager settings;
    private FakeEnvironmentManager env;

    private SettingsWindow window;

    private TMP_Dropdown backgroundDropdown;
    private Toggle autoSwitchToggle;
    private Toggle darkModeToggle;
    private TMP_Dropdown languageDropdown;
    private Button applyButton;
    private Button discardButton;

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

    private static void SetCurrentEnvUnsafe(string sceneName)
    {
        var f = typeof(EnvironmentManager)
            .GetField("currentSceneName", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(f, "EnvironmentManager.currentSceneName field not found");
        f.SetValue(EnvironmentManager.Instance, sceneName);
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

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        foreach (var x in Object.FindObjectsByType<SettingsManager>(FindObjectsSortMode.None))
            Object.DestroyImmediate(x.gameObject);
        foreach (var x in Object.FindObjectsByType<DisplayModeController>(FindObjectsSortMode.None))
            Object.DestroyImmediate(x.gameObject);
        foreach (var x in Object.FindObjectsByType<EnvironmentManager>(FindObjectsSortMode.None))
            Object.DestroyImmediate(x.gameObject);
        PlayerPrefs.DeleteAll();

        var prevIgnore = LogAssert.ignoreFailingMessages;
        LogAssert.ignoreFailingMessages = true;

        goCtrl = new GameObject("Ctrl");
        ctrl = goCtrl.AddComponent<DisplayModeController>();
        ctrl.lightTheme = MakeTheme(Color.gray);
        ctrl.darkTheme = MakeTheme(Color.black);

        goEnvMgr = new GameObject("EnvMgr");
        env = goEnvMgr.AddComponent<FakeEnvironmentManager>();

        goSettingsMgr = new GameObject("SettingsManager");
        settings = goSettingsMgr.AddComponent<SettingsManager>();

        yield return null;
        yield return null;

        LogAssert.ignoreFailingMessages = prevIgnore;

        goWindow = new GameObject("SettingsWindow");
        goWindow.SetActive(false); 
        window = goWindow.AddComponent<SettingsWindow>();

        backgroundDropdown = CreateUsableTMPDropdown("EnvDropdown");
        autoSwitchToggle = new GameObject("AutoSwitch").AddComponent<Toggle>();
        darkModeToggle = new GameObject("DarkMode").AddComponent<Toggle>();
        languageDropdown = CreateUsableTMPDropdown("LangDropdown");
        applyButton = CreateButton("ApplyBtn");
        discardButton = CreateButton("DiscardBtn");

        backgroundDropdown.transform.SetParent(goWindow.transform, false);
        autoSwitchToggle.transform.SetParent(goWindow.transform, false);
        darkModeToggle.transform.SetParent(goWindow.transform, false);
        languageDropdown.transform.SetParent(goWindow.transform, false);
        applyButton.transform.SetParent(goWindow.transform, false);
        discardButton.transform.SetParent(goWindow.transform, false);

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
        if (goWindow) Object.DestroyImmediate(goWindow);
        if (goSettingsMgr) Object.DestroyImmediate(goSettingsMgr);
        if (goCtrl) Object.DestroyImmediate(goCtrl);
        if (goEnvMgr) Object.DestroyImmediate(goEnvMgr);

        PlayerPrefs.DeleteAll();
        LogAssert.ignoreFailingMessages = false;
        yield return null;
    }

    private IEnumerator ActivateWindowExpecting(bool withResetButton)
    {
        if (withResetButton)
        {
            var canvas = new GameObject("Canvas");
            canvas.transform.SetParent(goWindow.transform, false);
            var panel = new GameObject("Panel");
            panel.transform.SetParent(canvas.transform, false);
            var resetGO = new GameObject("ResetTutorialButton");
            resetGO.transform.SetParent(panel.transform, false);
            resetGO.AddComponent<Image>();
            resetGO.AddComponent<Button>();
        }

        LogAssert.Expect(LogType.Log, "SettingsWindow Started");
        if (withResetButton)
            LogAssert.Expect(LogType.Log, "Reset Tutorial Button found and listening.");
        else
            LogAssert.Expect(LogType.Warning, new Regex("Reset Tutorial Button not found"));

        goWindow.SetActive(true);
        yield return null; 
    }

    [UnityTest]
    public IEnumerator Start_NoResetButton_ShowsWarning_And_BuildsDropdowns()
    {
        yield return ActivateWindowExpecting(withResetButton: false); 

        autoSwitchToggle.onValueChanged.Invoke(true);
        Assert.IsTrue(autoSwitchToggle.isOn);
        Assert.IsFalse(darkModeToggle.isOn);
        Assert.IsFalse(darkModeToggle.interactable);

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

        var lmField = typeof(SettingsManager).GetField("languageManager",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var languageManager = lmField.GetValue(SettingsManager.Instance);
        var dictField = languageManager.GetType().GetField("languageList",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var dict = (System.Collections.IDictionary)dictField.GetValue(languageManager);
        dict.Clear(); dict["en"] = "English"; dict["de"] = "Deutsch";

        SetCurrentEnvUnsafe("EnvA");

        var pendLangField = typeof(SettingsWindow).GetField("pendingLanguageCode",
            BindingFlags.NonPublic | BindingFlags.Instance);
        pendLangField.SetValue(window, "de");

        var envOptsField = typeof(SettingsWindow).GetField("envOptions",
            BindingFlags.NonPublic | BindingFlags.Instance);
        envOptsField.SetValue(window, new List<string> { "EnvA", "EnvB" });
        backgroundDropdown.onValueChanged.Invoke(1);

        LogAssert.Expect(LogType.Log, new Regex(@"Saving Auto Adjust"));                    
        LogAssert.Expect(LogType.Log, new Regex(@"Saving Dark Mode"));                       
        LogAssert.Expect(LogType.Log, "Applying Display Mode");                              
        LogAssert.Expect(LogType.Log, new Regex(@"\[Theme\] Mode applied: (Light|Dark)"));   
        LogAssert.Expect(LogType.Log, new Regex(@"Saving Language: de"));                   
        LogAssert.Expect(LogType.Log, new Regex(@"\[Lang\] ApplyLanguageAsync target=de")); 
        LogAssert.Expect(LogType.Log, new Regex(@"\[Lang\] SelectedLocale = de"));           
        LogAssert.Expect(LogType.Warning, new Regex(@"EnvironmentManager: Scene 'EnvB' is not in Build Settings\."));
        LogAssert.Expect(LogType.Log, new Regex(@"\[SettingsWindow\] Apply pressed: saved & applied\."));
        LogAssert.Expect(LogType.Log, new Regex(@"\[Lang\] current=.* pending=.*"));

        applyButton.onClick.Invoke();
        yield return null;

        SetCurrentEnvUnsafe("EnvB");
        Assert.AreEqual("EnvB", EnvironmentManager.Instance.CurrentSceneName);

        var pendField = typeof(SettingsWindow).GetField("pendingEnv",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNull(pendField.GetValue(window));

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
