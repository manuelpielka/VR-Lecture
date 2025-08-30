using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System;
public class TutorialManagerTests
{
    private class MockStep : ITutorialStep
    {
        private event Action completedInternal;

        public event Action StepCompleted
        {
            add { completedInternal += value; SubscriberCount++; }
            remove { completedInternal -= value; SubscriberCount--; }
        }

        public bool Started { get; private set; }
        public int SubscriberCount { get; private set; }

        public void StartStep() => Started = true;

        public void TriggerComplete() => completedInternal?.Invoke();

        public void EndStep()
        {
            throw new NotImplementedException();
        }
    }

    public class StubWelcomeStep : WelcomeStepTutorial, ITutorialStep
    {
        public new event Action StepCompleted;
        public bool Started { get; private set; }
        public new void StartStep() { Started = true; }
        public void TriggerComplete() => StepCompleted?.Invoke();

    }

    public class StubBrowseLectureStep : BrowseLectureStepTutorial, ITutorialStep
    {
        public new event Action StepCompleted;
        public bool Started { get; private set; }
        public new void StartStep() { Started = true; }
        public void TriggerComplete() => StepCompleted?.Invoke();
    }

    public class StubPlaybackControlsStep : PlaybackControlsStepTutorial, ITutorialStep
    {
        public new event Action StepCompleted;
        public bool Started { get; private set; }
        public new void StartStep() { Started = true; }
        public void TriggerComplete() => StepCompleted?.Invoke();
    }

    public class StubOpenTranscriptStep : OpenTranscriptStepTutorial, ITutorialStep
    {
        public new event Action StepCompleted;
        public bool Started { get; private set; }
        public new void StartStep() { Started = true; }
        public void TriggerComplete() => StepCompleted?.Invoke();
    }

    public class StubAskAvatarStep : AskAvatarStepTutorial, ITutorialStep
    {
        public new event Action StepCompleted;
        public bool Started { get; private set; }
        public new void StartStep() { Started = true; }
        public void TriggerComplete() => StepCompleted?.Invoke();
    }

    public class StubEndStep : EndStepTutorial, ITutorialStep
    {
        public new event Action StepCompleted;
        public bool Started { get; private set; }
        public new void StartStep() { Started = true; }
        public void TriggerComplete() => StepCompleted?.Invoke();
    }



    [UnityTest]
    public IEnumerator StartTutorial_ActivatesOverlay_And_StartsFirstStep()
    {
        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.tag = "MainCamera";

        var overlay = new GameObject("OverlayContainer");
        overlay.SetActive(false);

        var managerGO = new GameObject("TutorialManager");
        var manager = managerGO.AddComponent<TutorialManager>();
        manager.enabled = false;

        SetPrivateField(manager, "OverlayContainer", overlay);

        var step0 = new MockStep();
        var step1 = new MockStep();
        var customSteps = new List<ITutorialStep> { step0, step1 };
        SetPrivateField(manager, "steps", customSteps);

        manager.StartTutorial();

        yield return null;

        Assert.IsTrue(overlay.activeSelf, "OverlayContainer should be active after StartTutorial().");
        Assert.IsTrue(step0.Started, "First step’s StartStep() should have been called.");
        Assert.IsFalse(step1.Started, "Second step should not have started yet.");
    }

    [UnityTest]
    public IEnumerator StartNextStep_OnCompletion_UnsubscribesAndStartsFollowingStep()
    {
        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.tag = "MainCamera";

        var overlay = new GameObject("OverlayContainer");
        overlay.SetActive(false);

        var managerGO = new GameObject("TutorialManager");
        var manager = managerGO.AddComponent<TutorialManager>();
        manager.enabled = false;

        SetPrivateField(manager, "OverlayContainer", overlay);

        var step0 = new MockStep();
        var step1 = new MockStep();
        var steps = new List<ITutorialStep> { step0, step1 };
        SetPrivateField(manager, "steps", steps);

        manager.StartTutorial();
        yield return null;

        Assert.IsTrue(overlay.activeSelf, "Overlay should be active after StartTutorial.");
        Assert.IsTrue(step0.Started, "Step 0 must have started.");
        Assert.AreEqual(1, step0.SubscriberCount, "Manager should subscribe to Step 0 completion.");
        Assert.IsFalse(step1.Started, "Step 1 must not start yet.");
        Assert.AreEqual(0, step1.SubscriberCount, "Step 1 must not be subscribed yet.");

        step0.TriggerComplete();
        yield return null;

        Assert.AreEqual(0, step0.SubscriberCount, "Manager must unsubscribe from Step 0 after completion.");
        Assert.IsTrue(step1.Started, "Step 1 should start after Step 0 completes.");
        Assert.AreEqual(1, step1.SubscriberCount, "Manager should subscribe to Step 1 completion.");

        step0.TriggerComplete();
        yield return null;

        Assert.IsTrue(step1.Started, "Step 1 should remain started (no side-effect from triggering Step 0 again).");
    }

    [UnityTest]
    public IEnumerator Completing_Last_Step_DisablesOverlay_And_PersistsCompletion()
    {
        PlayerPrefs.DeleteKey("UserPref_TutorialCompleted");
        PlayerPrefs.Save();

        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.tag = "MainCamera";

        var overlay = new GameObject("OverlayContainer");
        overlay.SetActive(false);

        var managerGO = new GameObject("TutorialManager");
        var manager = managerGO.AddComponent<TutorialManager>();
        manager.enabled = false;

        SetPrivateField(manager, "OverlayContainer", overlay);
        var step0 = new MockStep();
        var step1 = new MockStep();
        var steps = new List<ITutorialStep> { step0, step1 };
        SetPrivateField(manager, "steps", steps);

        manager.StartTutorial();
        yield return null;

        Assert.IsTrue(overlay.activeSelf, "Overlay should be active while tutorial is running.");

        step0.TriggerComplete();
        yield return null;
        Assert.IsTrue(step1.Started, "Last step should have started after step0 completion.");

        step1.TriggerComplete();
        yield return null;

        Assert.IsFalse(overlay.activeSelf, "Overlay should be disabled after the tutorial ends.");

        Assert.IsTrue(
            PlayerPrefs.HasKey("UserPref_TutorialCompleted"),
            "Completion flag key should exist after finishing the tutorial."
        );

        var stored = PlayerPrefs.GetInt("UserPref_TutorialCompleted", -1);
        Assert.IsTrue(stored == 1 || stored == -1,
            "If stored as int, expected 1. If stored differently, adjust this assertion to match your implementation.");
    }

    [UnityTest]
    public IEnumerator ResetTutorial_Headless_ClearsCompletion_And_RestartsAtStep0()
    {
        PlayerPrefs.SetInt("UserPref_TutorialCompleted", 1);
        PlayerPrefs.Save();

        if (Camera.main == null)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            camGO.AddComponent<Camera>();
        }

        var overlay = new GameObject("OverlayContainer");
        overlay.SetActive(false);

        var mgrGO = new GameObject("TutorialManager");
        var mgr = mgrGO.AddComponent<TutorialManager>();
        mgr.enabled = false;

        SetPrivateField(mgr, "OverlayContainer", overlay);

        var stepsRoot = new GameObject("StepsRoot");
        var welcome = stepsRoot.AddComponent<StubWelcomeStep>();
        var browse = stepsRoot.AddComponent<StubBrowseLectureStep>();
        var play = stepsRoot.AddComponent<StubPlaybackControlsStep>();
        var openT = stepsRoot.AddComponent<StubOpenTranscriptStep>();
        var ask = stepsRoot.AddComponent<StubAskAvatarStep>();
        var end = stepsRoot.AddComponent<StubEndStep>();

        SetPrivateField(mgr, "welcomeStep", welcome);
        SetPrivateField(mgr, "browseLectureStep", browse);
        SetPrivateField(mgr, "playbackControlsStep", play);
        SetPrivateField(mgr, "openTranscriptStep", openT);
        SetPrivateField(mgr, "askAvatarStep", ask);
        SetPrivateField(mgr, "endStep", end);

        mgr.ResetTutorial();
        yield return null;

        Assert.IsFalse(PlayerPrefs.HasKey("UserPref_TutorialCompleted"),
            "ResetTutorial should delete the completion key.");

        Assert.IsTrue(overlay.activeSelf, "Overlay should be active after ResetTutorial.");

        Assert.IsTrue(welcome.Started, "First step (welcome) should be started after ResetTutorial.");

        var idxF = typeof(TutorialManager).GetField("currentStepIndex",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(idxF);
        var idx = (int)idxF.GetValue(mgr);
        Assert.AreEqual(0, idx, "Tutorial should restart at step index 0.");
    }

    [UnityTest]
    public IEnumerator Start_Skips_When_Tutorial_Already_Completed()
    {
        PlayerPrefs.SetInt("UserPref_TutorialCompleted", 1);
        PlayerPrefs.Save();

        if (Camera.main == null)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            camGO.AddComponent<Camera>();
        }

        var overlay = new GameObject("OverlayContainer");
        overlay.SetActive(false);

        var go = new GameObject("TutorialManager");
        var manager = go.AddComponent<TutorialManager>();

        var overlayF = typeof(TutorialManager).GetField("OverlayContainer",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(overlayF, "Reflection failed: OverlayContainer field not found.");
        overlayF.SetValue(manager, overlay);

        yield return null;

        Assert.IsFalse(overlay.activeSelf,
            "Overlay should remain inactive because Start() must skip when tutorial is already completed.");

        var stepsF = typeof(TutorialManager).GetField("steps",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(stepsF, "Reflection failed: steps field not found.");
        var steps = stepsF.GetValue(manager) as List<ITutorialStep>;
        Assert.IsNotNull(steps, "steps list should exist.");
        Assert.AreEqual(0, steps.Count,
            "steps should remain empty if Start() detects completion and skips.");

        var idxF = typeof(TutorialManager).GetField("currentStepIndex",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(idxF, "Reflection failed: currentStepIndex field not found.");
        var idx = (int)idxF.GetValue(manager);
        Assert.AreEqual(0, idx,
            "currentStepIndex should remain 0 when Start() exits early.");
    }


    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var f = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(f, $"Field '{fieldName}' not found via reflection.");
        f.SetValue(target, value);
    }

}
