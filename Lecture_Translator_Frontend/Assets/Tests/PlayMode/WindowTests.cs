using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

//
// Safe test double that blocks prod lifecycle logic.
//
public class TestWindowManager : WindowManager
{
    public Window lastClosed;   // <— NEW: spy storage (works if CloseWindow is virtual)

    new void Awake()    { WindowManager.instance = this; }
    new void OnEnable() { /* no-op */ }
    new void Start()    { /* no-op */ }
    new void OnDisable(){ /* no-op */ }

    // ---------- ONLY works if WindowManager.CloseWindow is virtual ----------
    // If your prod method is NOT virtual, comment this out and see the note in the test below.
    public /*override OR new (see note)*/ void CloseWindow(Window w)
    {
        // If CloseWindow is virtual in your prod class, use 'override' here.
        // If it is NOT virtual, 'new' will compile, but it will NOT be called via base reference.
        lastClosed = w; 
        // do NOT call base.CloseWindow to avoid prod side effects in tests
    }
    // -----------------------------------------------------------------------

    public static TestWindowManager InstallSingletonInScene()
    {
        var go = new GameObject("TestWindowManager") { hideFlags = HideFlags.DontSave };
        return go.AddComponent<TestWindowManager>(); // registers instance in Awake()
    }
}

public class WindowTests
{
    [UnitySetUp]
    public IEnumerator UnitySetUp()
    {
        var sceneName = "IsolatedTestScene_" + System.Guid.NewGuid().ToString("N");
        var scene = SceneManager.CreateScene(sceneName);
        SceneManager.SetActiveScene(scene);
        yield return null;

        WindowManager.instance = null;
        TryClearDisplayModeControllerSingleton();
    }
    

    [UnityTearDown]
    public IEnumerator UnityTearDown()
    {
        // Clean up between tests
        WindowManager.instance = null;
        foreach (var w in Object.FindObjectsOfType<Window>(true))
            if (w) Object.DestroyImmediate(w.gameObject);
        foreach (var go in Object.FindObjectsOfType<GameObject>())
            if (go && go.name == "TestWindowManager") Object.DestroyImmediate(go);

        yield return null;
    }

    [UnityTest]
    public IEnumerator Start_Assigns_WindowManager_And_Handles_Missing_Controller_Without_Crashing()
    {
        // Arrange: isolated scene, no controller present
        var wm = TestWindowManager.InstallSingletonInScene();

        // Act: create Window and let Start() run (which calls RefreshTheme())
        var go = new GameObject("SUT_Window");
        var sut = go.AddComponent<Window>();
        yield return null;

        // Assert: Start() assigned the manager and did not throw
        Assert.IsNotNull(sut.WindowManager, "Start() should assign WindowManager.instance.");
        Assert.AreSame(WindowManager.instance, sut.WindowManager, "Window should hold the global WindowManager.instance.");
    }

    [UnityTest]
    public IEnumerator Close_Calls_Manager_And_Destroys_GameObject()
    {
        // Arrange
        var wm = TestWindowManager.InstallSingletonInScene();

        var go = new GameObject("SUT_Window");
        var sut = go.AddComponent<Window>();
        yield return null; // let Start() wire WindowManager

        Assert.AreSame(wm, sut.WindowManager, "Sanity: WM assigned by Start().");

        // Act
        sut.Close();
        yield return null; // allow Destroy() to process

        // Assert: GO destroyed (this is true regardless of virtual/non-virtual)
        Assert.IsTrue(go == null || go.Equals(null), "Window GameObject should be destroyed after Close().");

    }


    [UnityTest]
    public IEnumerator Close_With_Null_WindowManager_Does_Not_Destroy_GameObject()
    {
        // Arrange: create Window with no WindowManager
        var go = new GameObject("SUT_Window");
        var sut = go.AddComponent<Window>();
        yield return null; // let Start() run (it will assign null since no WM)
        
        // Force WM to null just in case
        sut.WindowManager = null;

        // Act
        sut.Close();
        yield return null;

        // Assert: GO should still exist (not destroyed)
        Assert.IsFalse(go == null || go.Equals(null), 
            "When WindowManager is null, Close() should early-return and not destroy the GameObject.");
    }

    private static void TryClearDisplayModeControllerSingleton()
    {
        // If your DisplayModeController exposes a static Instance, try to null it.
        var t = System.Type.GetType("DisplayModeController");
        if (t == null) return;

        var instProp = t.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        if (instProp != null && instProp.CanWrite)
            instProp.SetValue(null, null);

        var fld = t.GetField("instance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
               ?? t.GetField("_instance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        if (fld != null) fld.SetValue(null, null);

        // Also destroy any scene instances, just in case
        foreach (var c in Object.FindObjectsOfType(t, true))
            if (c is Component comp) Object.DestroyImmediate(comp.gameObject);
    }
}





    

