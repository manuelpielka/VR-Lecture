using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class EndStepTutorialTests
{
    [SetUp]
    public void Setup()
    {
        SceneManager.LoadScene("Library Hall");
    }
    [TearDown]
    public void Teardown()
    {
        var wmGO = GameObject.Find("WindowManager");
        if (wmGO)
        {
            var wm = wmGO.GetComponent<WindowManager>();
            SafeClose(wm, "MainMenuWindow");
            SafeClose(wm, "LectureBrowserWindow");
            SafeClose(wm, "LecturePlayerWindow");
            SafeClose(wm, "NotesWindow");
            SafeClose(wm, "LectureNoteWindow");
            SafeClose(wm, "CreateNoteWindow");
            SafeClose(wm, "CreateLectureNoteWindow");
            SafeClose(wm, "TranscriptWindow");
            SafeClose(wm, "DialogueWindow");
            SafeClose(wm, "SettingsWindow");

        }

        foreach (var step in Object.FindObjectsByType<EndStepTutorial>(FindObjectsSortMode.None))
        {
            try { step.EndStep(); } catch { }
            Object.DestroyImmediate(step.gameObject);
        }

        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go && go.name.Contains("WelcomeOverlayPrefab"))
                Object.DestroyImmediate(go);
        }
        ResetWindowOpenedEvent();
    }



    [UnityTest]
    public IEnumerator StartStep_CreatesOverlay_And_PositionsIt()
    {
        yield return null;
        DisableRuntimePlaybackSystems();

        var sutGO = new GameObject("WelcomeStep_SUT");
        var sut = sutGO.AddComponent<EndStepTutorial>();

        var overlayPrefab = new GameObject("WelcomeOverlayPrefab");
        var buttonGO = new GameObject("NextButton");
        buttonGO.transform.SetParent(overlayPrefab.transform, false);
        buttonGO.AddComponent<RectTransform>();
        buttonGO.AddComponent<Image>();
        buttonGO.AddComponent<Button>();

        typeof(EndStepTutorial)
            .GetField("OverlayPrefab", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(sut, overlayPrefab);

        Vector3 expectedPos = TutorialManager.GetStepPosition();
        Vector3 expectedForward = TutorialManager.GetStepForward();

        sut.StartStep();
        yield return null;

        var overlayInstance = GameObject.Find("WelcomeOverlayPrefab(Clone)");
        Assert.IsNotNull(overlayInstance, "Overlay should be instantiated from OverlayPrefab.");

        float posDelta = Vector3.Distance(expectedPos, overlayInstance.transform.position);
        Assert.LessOrEqual(posDelta, 0.01f, $"Overlay position should match TutorialManager.GetStepPosition(). Δ={posDelta}");

        float angleDelta = Vector3.Angle(expectedForward, overlayInstance.transform.forward);
        Assert.LessOrEqual(angleDelta, 1.0f, $"Overlay forward should align with TutorialManager.GetStepForward(). Δ°={angleDelta}");
    }

    [UnityTest]
    public IEnumerator NextButton_Click_Completes_And_CleansUp()
    {
        yield return null;
        DisableRuntimePlaybackSystems();

        var sutGO = new GameObject("WelcomeStep_SUT");
        var sut = sutGO.AddComponent<EndStepTutorial>();

        var overlayPrefab = new GameObject("WelcomeOverlayPrefab");
        var btnGO = new GameObject("NextButton");
        btnGO.transform.SetParent(overlayPrefab.transform, false);
        btnGO.AddComponent<RectTransform>();
        btnGO.AddComponent<Image>();
        var button = btnGO.AddComponent<Button>();

        typeof(EndStepTutorial)
            .GetField("OverlayPrefab", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(sut, overlayPrefab);

        int completedCount = 0;
        sut.StepCompleted += () => completedCount++;

        sut.StartStep();
        yield return null;

        var overlayInstance = GameObject.Find("WelcomeOverlayPrefab(Clone)");
        Assert.IsNotNull(overlayInstance, "Overlay should be instantiated before clicking Next.");

        var runtimeButton = overlayInstance.GetComponentInChildren<Button>(true);
        Assert.IsNotNull(runtimeButton, "Overlay should contain a Button to proceed.");

        runtimeButton.onClick.Invoke();
        yield return null;
        yield return null;

        Assert.AreEqual(1, completedCount, "StepCompleted should fire exactly once on Next click.");

        var stillThere = GameObject.Find("WelcomeOverlayPrefab(Clone)");
        Assert.IsNull(stillThere, "Overlay should be destroyed after completion.");
    }

    [UnityTest]
    public IEnumerator WindowOpened_OtherWindow_DoesNothing()
    {
        yield return null;
        DisableRuntimePlaybackSystems();

        var wm = GameObject.Find("WindowManager").GetComponent<WindowManager>();

        var sutGO = new GameObject("WelcomeStep_SUT");
        var sut = sutGO.AddComponent<EndStepTutorial>();

        var overlayPrefab = new GameObject("WelcomeOverlayPrefab");
        var btnGO = new GameObject("NextButton");
        btnGO.transform.SetParent(overlayPrefab.transform, false);
        btnGO.AddComponent<RectTransform>();
        btnGO.AddComponent<Image>();
        btnGO.AddComponent<Button>();

        typeof(EndStepTutorial)
            .GetField("OverlayPrefab", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(sut, overlayPrefab);

        int completedCount = 0;
        sut.StepCompleted += () => completedCount++;

        sut.StartStep();
        yield return null;

        var overlayInstance = GameObject.Find("WelcomeOverlayPrefab(Clone)");
        Assert.IsNotNull(overlayInstance, "Overlay should exist before opening another window.");

        wm.OpenWindow("SettingsWindow");
        yield return null;

        Assert.AreEqual(0, completedCount, "StepCompleted should NOT fire when a non-main-menu window opens.");

        var stillThere = GameObject.Find("WelcomeOverlayPrefab(Clone)");
        Assert.IsNotNull(stillThere, "Overlay should remain when a non-main-menu window opens.");
    }

    [UnityTest]
    public IEnumerator EndStep_Is_Idempotent()
    {
        yield return null;
        DisableRuntimePlaybackSystems();

        var sutGO = new GameObject("WelcomeStep_SUT");
        var sut = sutGO.AddComponent<EndStepTutorial>();

        var overlayPrefab = new GameObject("WelcomeOverlayPrefab");
        var btnGO = new GameObject("NextButton");
        btnGO.transform.SetParent(overlayPrefab.transform, false);
        btnGO.AddComponent<RectTransform>();
        btnGO.AddComponent<Image>();
        btnGO.AddComponent<Button>();

        typeof(EndStepTutorial)
            .GetField("OverlayPrefab", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(sut, overlayPrefab);

        int completedCount = 0;
        sut.StepCompleted += () => completedCount++;

        sut.StartStep();
        yield return null;

        Assert.IsNotNull(GameObject.Find("WelcomeOverlayPrefab(Clone)"), "Overlay should exist after StartStep().");

        sut.EndStep();
        yield return null;
        sut.EndStep();
        yield return null;

        Assert.IsNull(GameObject.Find("WelcomeOverlayPrefab(Clone)"), "Overlay should be destroyed after EndStep() and not recreated by a second call.");
        Assert.AreEqual(0, completedCount, "EndStep() must not invoke StepCompleted.");
    }

    [UnityTest]
    public IEnumerator StartStep_DoesNot_Complete_Without_UserAction()
    {
        yield return null;
        DisableRuntimePlaybackSystems();

        var sutGO = new GameObject("WelcomeStep_SUT");
        var sut = sutGO.AddComponent<EndStepTutorial>();

        var overlayPrefab = new GameObject("WelcomeOverlayPrefab");
        var btnGO = new GameObject("NextButton");
        btnGO.transform.SetParent(overlayPrefab.transform, false);
        btnGO.AddComponent<RectTransform>();
        btnGO.AddComponent<Image>();
        btnGO.AddComponent<Button>();

        typeof(EndStepTutorial)
            .GetField("OverlayPrefab", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(sut, overlayPrefab);

        int completedCount = 0;
        sut.StepCompleted += () => completedCount++;

        sut.StartStep();
        yield return null;

        Assert.AreEqual(0, completedCount, "StepCompleted must not fire on StartStep() alone.");

        var overlayInstance = GameObject.Find("WelcomeOverlayPrefab(Clone)");
        Assert.IsNotNull(overlayInstance, "Overlay should exist after StartStep() when not skipping.");
    }



    private static void SafeClose(WindowManager wm, string key)
    {
        if (wm == null || string.IsNullOrEmpty(key)) return;

        try
        {
            var go = GameObject.Find(key);
            if (go)
            {
                var win = go.GetComponent<Window>();
                if (win) { wm.CloseWindow(win); return; }
            }

            foreach (var w in Resources.FindObjectsOfTypeAll<Window>())
            {
                if (!w || !w.gameObject.scene.IsValid()) continue;
                if (w.name == key || w.name.Contains(key))
                {
                    if (!w.gameObject.activeInHierarchy) w.gameObject.SetActive(true);
                    wm.CloseWindow(w);
                    return;
                }
            }

        }
        catch { }
    }

    private static void DisableRuntimePlaybackSystems()
    {
        foreach (var lpw in Object.FindObjectsByType<LecturePlayerWindow>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            lpw.enabled = false;

        foreach (var sm in Object.FindObjectsByType<SubtitleManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            sm.enabled = false;

        foreach (var tm in Object.FindObjectsByType<TranscriptManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            tm.enabled = false;
    }

    private static void ResetWindowOpenedEvent()
    {
        var wmGo = GameObject.Find("WindowManager");
        var wm = wmGo ? wmGo.GetComponent<WindowManager>() : null;

        if (wm != null)
        {
            var f = typeof(WindowManager).GetField("WindowOpened",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (f != null) { try { f.SetValue(wm, null); } catch { } }
        }

        var fs = typeof(WindowManager).GetField("WindowOpened",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        if (fs != null) { try { fs.SetValue(null, null); } catch { } }
    }

}
