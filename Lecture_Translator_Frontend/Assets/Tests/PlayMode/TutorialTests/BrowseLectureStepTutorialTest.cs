using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class BrowseLectureStepTutorialTests
{

    [SetUp]
    public void Setup()
    {
        SceneManager.LoadScene("Library Hall");
    }
    [TearDown]
    public void Teardown()
    {
        // 1) Close common windows if the WindowManager exists
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

        // 2) Force-unsubscribe anyone still listening & destroy all step instances
        foreach (var step in Object.FindObjectsByType<BrowseLectureStepTutorial>(FindObjectsSortMode.None))
        {
            try { step.EndStep(); } catch { /* ignore */ }
            Object.DestroyImmediate(step.gameObject);
        }

        // 3) Kill any overlay clones left behind (in case a test bailed early)
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go && go.name.Contains("WelcomeOverlayPrefab"))
                Object.DestroyImmediate(go);
        }
        ResetWindowOpenedEvent();
    }

    private static void SafeClose(WindowManager wm, string key)
    {
        if (wm == null || string.IsNullOrEmpty(key)) return;

        try
        {
            // 1) Close by direct scene object name
            var go = GameObject.Find(key);
            if (go)
            {
                var win = go.GetComponent<Window>();
                if (win) { wm.CloseWindow(win); return; }
            }

            // 2) Close any live Window components whose names match (handle "(Clone)")
            foreach (var w in Resources.FindObjectsOfTypeAll<Window>())
            {
                if (!w || !w.gameObject.scene.IsValid()) continue; // skip assets/prefabs
                if (w.name == key || w.name.Contains(key))
                {
                    if (!w.gameObject.activeInHierarchy) w.gameObject.SetActive(true);
                    wm.CloseWindow(w);
                    return;
                }
            }

            // IMPORTANT: do NOT open anything here. If it’s not present, we’re done.
        }
        catch { /* swallow teardown errors */ }
    }



    private static void ResetWindowOpenedEvent()
    {
        // Try instance field first
        var wmGo = GameObject.Find("WindowManager");
        var wm = wmGo ? wmGo.GetComponent<WindowManager>() : null;

        // Instance event backing field
        if (wm != null)
        {
            var f = typeof(WindowManager).GetField("WindowOpened",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (f != null) { try { f.SetValue(wm, null); } catch { } }
        }

        // Static event backing field (if defined that way in your codebase)
        var fs = typeof(WindowManager).GetField("WindowOpened",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        if (fs != null) { try { fs.SetValue(null, null); } catch { } }
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





    [UnityTest]
    public IEnumerator StartStep_Skips_When_LectureBrowser_Already_Open()
    {
        yield return null; // let scene load

        DisableRuntimePlaybackSystems();


        var wm = GameObject.Find("WindowManager").GetComponent<WindowManager>();

        // Mark LectureBrowser as already open (no side effects, no real prefabs)
        InsertDummyActiveWindow(wm, "LectureBrowserWindow");

        // SUT
        var sutGO = new GameObject("BrowseLectureStep_SUT");
        var sut = sutGO.AddComponent<BrowseLectureStepTutorial>();

        // Inject a harmless overlay to avoid Instantiate(null) if prod code instantiates before skip check
        var overlayPrefab = new GameObject("WelcomeOverlayPrefab");
        typeof(BrowseLectureStepTutorial)
            .GetField("OverlayPrefab", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(sut, overlayPrefab);

        int completedCount = 0;
        sut.StepCompleted += () => completedCount++;

        sut.StartStep();
        yield return null;

        Assert.AreEqual(1, completedCount, "StepCompleted should fire once when skipping.");
    }
    private static Window InsertDummyActiveWindow(WindowManager wm, string key)
    {
        // Create a fake "prefab" with the same name as the window key
        var prefabGO = new GameObject(key);

        // Create a runtime instance and attach a Window component
        var instanceGO = new GameObject(key + "(Instance)");
        var win = instanceGO.AddComponent<Window>();

        // Fill the fields that a real created Window would have
        var winType = typeof(Window);
        winType.GetField("<WindowManager>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
               ?.SetValue(win, wm);
        winType.GetField("<Prefab>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
               ?.SetValue(win, prefabGO);
        winType.GetField("<Key>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
               ?.SetValue(win, key);

        // Push it into private List<Window> activeWindows
        var f = typeof(WindowManager).GetField("activeWindows", BindingFlags.Instance | BindingFlags.NonPublic);
        var list = (IList)f.GetValue(wm);
        list.Add(win);

        return win;
    }


    [UnityTest]
    public IEnumerator StartStep_CreatesOverlay_And_PositionsIt()
    {

        yield return null; // let scene load
        DisableRuntimePlaybackSystems();

        // Ensure Main Menu is NOT open (fresh scene should be fine)
        var sutGO = new GameObject("WelcomeStep_SUT");
        var sut = sutGO.AddComponent<BrowseLectureStepTutorial>();

        // Build a minimal prefab with a Button child so StartStep can wire it (no click in this test).
        var overlayPrefab = new GameObject("WelcomeOverlayPrefab");
        var buttonGO = new GameObject("NextButton");
        buttonGO.transform.SetParent(overlayPrefab.transform, false);
        buttonGO.AddComponent<RectTransform>();
        buttonGO.AddComponent<Image>();  // Button requires a target graphic
        buttonGO.AddComponent<Button>();

        // Inject the private [SerializeField] OverlayPrefab via reflection
        typeof(BrowseLectureStepTutorial)
            .GetField("OverlayPrefab", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(sut, overlayPrefab);

        // Expected from TutorialManager static getters
        Vector3 expectedPos = TutorialManager.GetStepPosition();
        Vector3 expectedForward = TutorialManager.GetStepForward();

        // Act
        sut.StartStep();
        yield return null;

        // Find the instantiated overlay (Unity names it "<prefab name>(Clone)")
        var overlayInstance = GameObject.Find("WelcomeOverlayPrefab(Clone)");
        Assert.IsNotNull(overlayInstance, "Overlay should be instantiated from OverlayPrefab.");

        // Assert position (with small tolerance) and orientation (match forward direction)
        float posDelta = Vector3.Distance(expectedPos, overlayInstance.transform.position);
        Assert.LessOrEqual(posDelta, 0.01f, $"Overlay position should match TutorialManager.GetStepPosition(). Δ={posDelta}");

        float angleDelta = Vector3.Angle(expectedForward, overlayInstance.transform.forward);
        Assert.LessOrEqual(angleDelta, 1.0f, $"Overlay forward should align with TutorialManager.GetStepForward(). Δ°={angleDelta}");
    }

    [UnityTest]
    public IEnumerator NextButton_Click_Completes_And_CleansUp()
    {
        yield return null; // let scene load
        DisableRuntimePlaybackSystems();

        // SUT
        var sutGO = new GameObject("WelcomeStep_SUT");
        var sut = sutGO.AddComponent<BrowseLectureStepTutorial>();

        // Minimal overlay prefab with a Button child
        var overlayPrefab = new GameObject("WelcomeOverlayPrefab");
        var btnGO = new GameObject("NextButton");
        btnGO.transform.SetParent(overlayPrefab.transform, false);
        btnGO.AddComponent<RectTransform>();
        btnGO.AddComponent<Image>();
        var button = btnGO.AddComponent<Button>();

        // Inject private OverlayPrefab via reflection
        typeof(BrowseLectureStepTutorial)
            .GetField("OverlayPrefab", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(sut, overlayPrefab);

        // Track completion
        int completedCount = 0;
        sut.StepCompleted += () => completedCount++;

        // Start step -> overlay should appear
        sut.StartStep();
        yield return null;

        // Grab the instantiated overlay and its button
        var overlayInstance = GameObject.Find("WelcomeOverlayPrefab(Clone)");
        Assert.IsNotNull(overlayInstance, "Overlay should be instantiated before clicking Next.");

        var runtimeButton = overlayInstance.GetComponentInChildren<Button>(true);
        Assert.IsNotNull(runtimeButton, "Overlay should contain a Button to proceed.");

        // Click Next
        runtimeButton.onClick.Invoke();
        yield return null;      // allow EndStep() to run
        yield return null;      // allow Destroy(overlayInstance) to complete

        // Assert: StepCompleted fired exactly once
        Assert.AreEqual(1, completedCount, "StepCompleted should fire exactly once on Next click.");

        // Assert: overlay destroyed (should not be found anymore)
        var stillThere = GameObject.Find("WelcomeOverlayPrefab(Clone)");
        Assert.IsNull(stillThere, "Overlay should be destroyed after completion.");
    }

    [UnityTest]
    public IEnumerator WindowOpened_LectureBrowser_Completes_And_CleansUp()
    {
        yield return null; // let scene load
        DisableRuntimePlaybackSystems();

        var wm = GameObject.Find("WindowManager").GetComponent<WindowManager>();
        // Ensure main menu is NOT open initially so StartStep doesn't auto-skip
        // (fresh scene is assumed closed; if not, close it via your API here)

        // SUT
        var sutGO = new GameObject("WelcomeStep_SUT");
        var sut = sutGO.AddComponent<BrowseLectureStepTutorial>();

        // Minimal overlay prefab with a Button child (not clicked in this test,
        // but ensures an overlay exists to be cleaned up by EndStep)
        var overlayPrefab = new GameObject("WelcomeOverlayPrefab");
        var btnGO = new GameObject("NextButton");
        btnGO.transform.SetParent(overlayPrefab.transform, false);
        btnGO.AddComponent<RectTransform>();
        btnGO.AddComponent<Image>();
        btnGO.AddComponent<Button>();

        // Inject private OverlayPrefab
        typeof(BrowseLectureStepTutorial)
            .GetField("OverlayPrefab", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(sut, overlayPrefab);

        int completedCount = 0;
        sut.StepCompleted += () => completedCount++;

        // Start step -> overlay should appear and event listener should subscribe
        sut.StartStep();
        yield return null;

        var overlayInstance = GameObject.Find("WelcomeOverlayPrefab(Clone)");
        Assert.IsNotNull(overlayInstance, "Overlay should be instantiated before window event.");

        // Act: open Main Menu via WindowManager to raise WindowOpened(MainMenu)
        FireWindowOpened("LectureBrowserWindow");
        yield return null; // allow HandleWindowOpened -> EndStep to run
        yield return null; // allow Destroy(overlayInstance) to complete

        // Assert: completed once
        Assert.AreEqual(1, completedCount, "StepCompleted should fire exactly once on Main Menu open.");

        // Assert: overlay destroyed
        var stillThere = GameObject.Find("WelcomeOverlayPrefab(Clone)");
        Assert.IsNull(stillThere, "Overlay should be destroyed after completion via window event.");
    }

    [UnityTest]
    public IEnumerator WindowOpened_OtherWindow_DoesNothing()
    {
        yield return null; // let scene load
        DisableRuntimePlaybackSystems();

        var wm = GameObject.Find("WindowManager").GetComponent<WindowManager>();

        // SUT
        var sutGO = new GameObject("WelcomeStep_SUT");
        var sut = sutGO.AddComponent<BrowseLectureStepTutorial>();

        // Minimal overlay prefab with a Button child
        var overlayPrefab = new GameObject("WelcomeOverlayPrefab");
        var btnGO = new GameObject("NextButton");
        btnGO.transform.SetParent(overlayPrefab.transform, false);
        btnGO.AddComponent<RectTransform>();
        btnGO.AddComponent<Image>();
        btnGO.AddComponent<Button>();

        // Inject private OverlayPrefab
        typeof(BrowseLectureStepTutorial)
            .GetField("OverlayPrefab", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(sut, overlayPrefab);

        int completedCount = 0;
        sut.StepCompleted += () => completedCount++;

        // Start step (should instantiate overlay and subscribe listener)
        sut.StartStep();
        yield return null;

        var overlayInstance = GameObject.Find("WelcomeOverlayPrefab(Clone)");
        Assert.IsNotNull(overlayInstance, "Overlay should exist before opening another window.");

        // Act: open a different window (NOT MainMenu)
        // Use a key that exists in your project, e.g., "SettingsWindow"
        wm.OpenWindow("SettingsWindow");
        yield return null; // allow any handlers to run

        // Assert: no completion, overlay still present
        Assert.AreEqual(0, completedCount, "StepCompleted should NOT fire when a non-main-menu window opens.");

        var stillThere = GameObject.Find("WelcomeOverlayPrefab(Clone)");
        Assert.IsNotNull(stillThere, "Overlay should remain when a non-main-menu window opens.");
    }

    [UnityTest]
    public IEnumerator Completion_Fires_Once_When_Both_NextClick_And_LectureBrowser_Happen()
    {
        yield return null; // let scene load
        DisableRuntimePlaybackSystems();

        var wm = GameObject.Find("WindowManager").GetComponent<WindowManager>();

        // SUT
        var sutGO = new GameObject("WelcomeStep_SUT");
        var sut = sutGO.AddComponent<BrowseLectureStepTutorial>();

        // Minimal overlay prefab with a Button child
        var overlayPrefab = new GameObject("WelcomeOverlayPrefab");
        var btnGO = new GameObject("NextButton");
        btnGO.transform.SetParent(overlayPrefab.transform, false);
        btnGO.AddComponent<RectTransform>();
        btnGO.AddComponent<Image>();
        btnGO.AddComponent<Button>();

        // Inject private OverlayPrefab
        typeof(BrowseLectureStepTutorial)
            .GetField("OverlayPrefab", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(sut, overlayPrefab);

        int completedCount = 0;
        sut.StepCompleted += () => completedCount++;

        // Start step (creates overlay and subscribes to WindowOpened)
        sut.StartStep();
        yield return null;

        // Grab runtime overlay and button
        var overlayInstance = GameObject.Find("WelcomeOverlayPrefab(Clone)");
        Assert.IsNotNull(overlayInstance, "Overlay should be present before triggering race.");
        var runtimeButton = overlayInstance.GetComponentInChildren<Button>(true);
        Assert.IsNotNull(runtimeButton, "Overlay should contain a Button.");

        // Act 1: Click Next first
        runtimeButton.onClick.Invoke();
        yield return null;   // allow EndStep() to run
        yield return null;   // allow Destroy to complete

        // Sanity: completion occurred and overlay is gone
        Assert.AreEqual(1, completedCount, "StepCompleted should have fired once after Next click.");
        Assert.IsNull(GameObject.Find("WelcomeOverlayPrefab(Clone)"), "Overlay should be destroyed after Next click.");

        // Act 2: Now open LectureBrowserWindow (would trigger again if not properly unsubscribed)
        FireWindowOpened("LectureBrowserWindow");
        yield return null;

        // Assert: still exactly once (no double-fire)
        Assert.AreEqual(1, completedCount, "StepCompleted must not fire again after window open.");
    }

    [UnityTest]
    public IEnumerator EndStep_Is_Idempotent()
    {
        yield return null; // let scene load
        DisableRuntimePlaybackSystems();

        // SUT
        var sutGO = new GameObject("WelcomeStep_SUT");
        var sut = sutGO.AddComponent<BrowseLectureStepTutorial>();

        // Minimal overlay prefab with a Button child
        var overlayPrefab = new GameObject("WelcomeOverlayPrefab");
        var btnGO = new GameObject("NextButton");
        btnGO.transform.SetParent(overlayPrefab.transform, false);
        btnGO.AddComponent<RectTransform>();
        btnGO.AddComponent<Image>();
        btnGO.AddComponent<Button>();

        // Inject private OverlayPrefab
        typeof(BrowseLectureStepTutorial)
            .GetField("OverlayPrefab", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(sut, overlayPrefab);

        // Track completion to ensure EndStep() itself does not raise it
        int completedCount = 0;
        sut.StepCompleted += () => completedCount++;

        // Start step -> overlay should appear
        sut.StartStep();
        yield return null;

        // Sanity: overlay exists
        Assert.IsNotNull(GameObject.Find("WelcomeOverlayPrefab(Clone)"), "Overlay should exist after StartStep().");

        // Act: call EndStep() twice
        sut.EndStep();  // first call should destroy overlay and unsubscribe
        yield return null;
        sut.EndStep();  // second call should be a no-op
        yield return null;

        // Assert: overlay destroyed and not recreated
        Assert.IsNull(GameObject.Find("WelcomeOverlayPrefab(Clone)"), "Overlay should be destroyed after EndStep() and not recreated by a second call.");

        // Assert: no completion fired by EndStep()
        Assert.AreEqual(0, completedCount, "EndStep() must not invoke StepCompleted.");
    }

    [UnityTest]
    public IEnumerator StartStep_DoesNot_Complete_Without_UserAction_Or_LectureBrowser()
    {
        yield return null; // let scene load
        DisableRuntimePlaybackSystems();

        // SUT
        var sutGO = new GameObject("WelcomeStep_SUT");
        var sut = sutGO.AddComponent<BrowseLectureStepTutorial>();

        // Minimal overlay prefab with a Button child (we won't click it in this test)
        var overlayPrefab = new GameObject("WelcomeOverlayPrefab");
        var btnGO = new GameObject("NextButton");
        btnGO.transform.SetParent(overlayPrefab.transform, false);
        btnGO.AddComponent<RectTransform>();
        btnGO.AddComponent<Image>();
        btnGO.AddComponent<Button>();

        // Inject private OverlayPrefab
        typeof(BrowseLectureStepTutorial)
            .GetField("OverlayPrefab", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(sut, overlayPrefab);

        int completedCount = 0;
        sut.StepCompleted += () => completedCount++;

        // Act: Start normally (Main Menu is not open, and we won't click Next)
        sut.StartStep();
        yield return null;

        // Assert: no completion yet
        Assert.AreEqual(0, completedCount, "StepCompleted must not fire on StartStep() alone.");

        // Sanity: overlay exists and remains
        var overlayInstance = GameObject.Find("WelcomeOverlayPrefab(Clone)");
        Assert.IsNotNull(overlayInstance, "Overlay should exist after StartStep() when not skipping.");
    }

    private static void FireWindowOpened(string key)
    {
        var evt = typeof(WindowManager).GetField(
            "WindowOpened",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public
        );
        var del = (System.Action<string>)evt?.GetValue(null);
        del?.Invoke(key);
    }


}

