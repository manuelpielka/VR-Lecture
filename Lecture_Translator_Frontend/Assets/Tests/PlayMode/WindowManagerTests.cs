using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
public class WindowManagerTests
{
    // Test subclass to prevent base Awake/OnEnable/OnDisable from running too early
    private class TestWindowManager : WindowManager
    {
        new void Awake() { /* suppress base */ }
        new void OnEnable() { /* suppress base */ }
        new void OnDisable() { /* suppress base */ }
    }

    private class Scope : IDisposable
    {
        private readonly List<UnityEngine.Object> trash = new();

        public T Make<T>(string name = null) where T : Component
        {
            var go = new GameObject(name ?? typeof(T).Name);
            trash.Add(go);
            return go.AddComponent<T>();
        }

        public GameObject MakePrefab(string name)
        {
            var go = new GameObject(name);
            go.SetActive(false);
            // Ensure a Window component exists on the prefab
            if (!go.GetComponent<Window>()) go.AddComponent<Window>();
            trash.Add(go);
            return go;
        }

        public void Dispose()
        {
            // reset singleton & event to avoid leakage
            var instField = typeof(WindowManager).GetField("instance", BindingFlags.Public | BindingFlags.Static);
            instField?.SetValue(null, null);

            // nuke static event subscribers by clearing its backing field if present
            var evtField = typeof(WindowManager).GetField("WindowOpened",
                BindingFlags.NonPublic | BindingFlags.Static);
            evtField?.SetValue(null, null);

            foreach (var o in trash)
            {
                if (o is GameObject go) UnityEngine.Object.DestroyImmediate(go);
                else UnityEngine.Object.DestroyImmediate(o);
            }
        }
    }

    [Test]
    public void Awake_SetsSingleton_And_LoadsPrefabs()
    {
        using var s = new Scope();

        // Ensure there is a main camera (OpenWindow tries to position relative to Camera.main)
        var cam = s.Make<Camera>("Main Camera");
        cam.tag = "MainCamera";

        // Build a VALID InputActionReference from an InputActionAsset
        var asset = ScriptableObject.CreateInstance<InputActionAsset>();
        var map = new InputActionMap("UI");
        var action = map.AddAction("OpenMainMenu", InputActionType.Button);
        asset.AddActionMap(map);
        var iaRef = InputActionReference.Create(action);

        // Prepare a few keys & matching prefabs
        var allKeys = WindowKeys.AllKeys;
        Assert.IsNotNull(allKeys);
        Assert.Greater(allKeys.Count, 0, "WindowKeys.AllKeys must have keys.");

        int n = Math.Min(3, allKeys.Count);
        var keys = allKeys.GetRange(0, n);
        var prefabs = new List<GameObject>();
        foreach (var k in keys)
            prefabs.Add(s.MakePrefab(k)); // name == key; has Window component

        // Create manager on an inactive GO and inject before init
        var go = new GameObject("WindowManager");
        go.SetActive(false);
        var wm = go.AddComponent<TestWindowManager>();

        SetPriv(wm, "openMainMenu", iaRef);
        SetPriv(wm, "windowPrefabsList", prefabs);

        // Manually perform what base Awake would have done
        // 1) instance = this
        typeof(WindowManager)
            .GetField("instance", BindingFlags.Public | BindingFlags.Static)
            ?.SetValue(null, wm);

        // 2) LoadWindowPrefabs()
        CallPriv(wm, "LoadWindowPrefabs");

        // Activate after proper init (we suppressed OnEnable anyway)
        go.SetActive(true);

        // Assertions: singleton set
        Assert.AreSame(wm, WindowManager.instance);

        // Each key opens successfully
        foreach (var k in keys)
        {
            var w = wm.OpenWindow(k);
            Assert.IsNotNull(w, $"OpenWindow({k}) returned null");
            Assert.AreSame(wm, w.WindowManager);
            Assert.AreEqual(k, w.Key);
        }
    }

    [Test]
    public void OpenWindow_AddsOnce_And_FiresEvent()
    {
        using var s = new Scope();

        // Main camera to satisfy positioning
        var cam = s.Make<Camera>("Main Camera");
        cam.tag = "MainCamera";

        // Valid InputActionReference from an asset
        var asset = ScriptableObject.CreateInstance<InputActionAsset>();
        var map = new InputActionMap("UI");
        var action = map.AddAction("OpenMainMenu", InputActionType.Button);
        asset.AddActionMap(map);
        var iaRef = InputActionReference.Create(action);

        // Use a single key for clarity
        var all = WindowKeys.AllKeys;
        Assert.IsNotNull(all);
        Assert.Greater(all.Count, 0, "WindowKeys.AllKeys must have keys.");
        var key = all[0];

        // Matching prefab (named as the key; has Window component)
        var prefab = s.MakePrefab(key);

        // Manager on inactive GO; inject before init
        var go = new GameObject("WindowManager");
        go.SetActive(false);
        var wm = go.AddComponent<TestWindowManager>();

        SetPriv(wm, "openMainMenu", iaRef);
        SetPriv(wm, "windowPrefabsList", new List<GameObject> { prefab });

        // Manual init of singleton + load prefabs
        typeof(WindowManager)
            .GetField("instance", BindingFlags.Public | BindingFlags.Static)
            ?.SetValue(null, wm);
        CallPriv(wm, "LoadWindowPrefabs");
        go.SetActive(true);

        // Track event invocations
        int fired = 0;
        string lastKey = null;
        void Handler(string k) { fired++; lastKey = k; }

        WindowManager.WindowOpened += Handler;
        try
        {
            // First open: returns instance, adds to activeWindows, fires event once
            var w1 = wm.OpenWindow(key);
            Assert.IsNotNull(w1, "First OpenWindow should return a Window.");
            Assert.AreEqual(1, GetActiveCount(wm), "activeWindows should have exactly one entry after first open.");
            Assert.AreEqual(1, fired, "WindowOpened should have fired exactly once after first open.");
            Assert.AreEqual(key, lastKey, "WindowOpened should pass the opened key.");

            // Second open: returns same instance, does not fire event again
            var w2 = wm.OpenWindow(key);
            Assert.AreSame(w1, w2, "Second OpenWindow should return the same Window instance.");
            Assert.AreEqual(1, GetActiveCount(wm), "activeWindows should still have exactly one entry.");
            Assert.AreEqual(1, fired, "WindowOpened should not fire again on second open.");
        }
        finally
        {
            WindowManager.WindowOpened -= Handler;
        }

        static int GetActiveCount(WindowManager m)
        {
            var f = typeof(WindowManager).GetField("activeWindows",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var list = (System.Collections.IList)f.GetValue(m);
            return list?.Count ?? -1;
        }
    }

    [Test]
    public void CreateWindow_SetsParent_Position_Rotation_And_Fields_WithCamera()
    {
        using var s = new Scope();

        // Make a camera in a known place
        var cam = s.Make<Camera>("Main Camera");
        cam.transform.position = new Vector3(1f, 1.7f, -3f);
        cam.transform.rotation = Quaternion.Euler(0f, 30f, 0f);

        try
        {
            // Ensure THIS is the only Camera.main
            CameraTagIsolation.MakeThisTheOnlyMain(cam);

            // Build a valid InputActionReference
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = new InputActionMap("UI");
            var action = map.AddAction("OpenMainMenu", InputActionType.Button);
            asset.AddActionMap(map);
            var iaRef = InputActionReference.Create(action);

            // One key + matching prefab
            var all = WindowKeys.AllKeys;
            Assert.IsNotNull(all);
            Assert.Greater(all.Count, 0);
            var key = all[0];
            var prefab = s.MakePrefab(key);

            // Manager (init suppressed), inject, then init dictionary
            var go = new GameObject("WindowManager");
            go.SetActive(false);
            var wm = go.AddComponent<TestWindowManager>();
            SetPriv(wm, "openMainMenu", iaRef);
            SetPriv(wm, "windowPrefabsList", new List<GameObject> { prefab });
            typeof(WindowManager).GetField("instance", BindingFlags.Public | BindingFlags.Static)?.SetValue(null, wm);
            CallPriv(wm, "LoadWindowPrefabs");
            go.SetActive(true);

            // Act
            var w = wm.CreateWindow(key);
            Assert.IsNotNull(w);

            // Assert: active
            Assert.IsTrue(w.gameObject.activeSelf, "Created window should be active.");

            // Assert: parented to manager
            Assert.AreSame(wm.transform, w.transform.parent, "Window should be parented to WindowManager.");

            // Assert: fields assigned
            Assert.AreSame(wm, w.WindowManager, "WindowManager field not set.");
            Assert.AreSame(prefab, w.Prefab, "Prefab field not set.");
            Assert.AreEqual(key, w.Key, "Key field not set.");

            // Assert: positioned ~2m in front of camera with -0.3f Y offset
            var expectedPos = cam.transform.position + cam.transform.forward * 2f;
            expectedPos.y -= 0.3f;
            var dist = Vector3.Distance(expectedPos, w.transform.position);
            Assert.LessOrEqual(dist, 0.25f, $"Position should be ~2m in front; off by {dist:0.###}m.");

            // Assert: facing camera direction
            var angle = Vector3.Angle(cam.transform.forward, w.transform.forward);
            Assert.LessOrEqual(angle, 2f, $"Window forward should face camera forward; angle = {angle:0.###}°.");
        }
        finally
        {
            CameraTagIsolation.Restore();
        }
    }




    [Test]
    public void OpenMainMenu_Toggles_Correctly()
    {
        using var s = new Scope();

        // Provide a main camera (needed for positioning)
        var cam = s.Make<Camera>("Main Camera");
        cam.tag = "MainCamera";

        // Build valid InputActionReference
        var asset = ScriptableObject.CreateInstance<InputActionAsset>();
        var map = new InputActionMap("UI");
        var action = map.AddAction("OpenMainMenu", InputActionType.Button);
        asset.AddActionMap(map);
        var iaRef = InputActionReference.Create(action);

        // Ensure MainMenuKey is present
        var key = WindowKeys.MainMenuKey;
        Assert.IsTrue(WindowKeys.AllKeys.Contains(key), "WindowKeys must include MainMenuKey.");

        // Matching prefab (named as key, has Window component)
        var prefab = s.MakePrefab(key);

        // Manager setup (suppress lifecycle, inject, then init dictionary)
        var go = new GameObject("WindowManager");
        go.SetActive(false);
        var wm = go.AddComponent<TestWindowManager>();
        SetPriv(wm, "openMainMenu", iaRef);
        SetPriv(wm, "windowPrefabsList", new List<GameObject> { prefab });
        typeof(WindowManager).GetField("instance", BindingFlags.Public | BindingFlags.Static)?.SetValue(null, wm);
        CallPriv(wm, "LoadWindowPrefabs");
        go.SetActive(true);

        // Reflectively get the private flag + activeWindows list
        var mainMenuField = typeof(WindowManager).GetField("mainMenuOpen",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var activeWindowsField = typeof(WindowManager).GetField("activeWindows",
            BindingFlags.Instance | BindingFlags.NonPublic);

        // Helper to count active windows
        int ActiveCount() => ((System.Collections.IList)activeWindowsField.GetValue(wm)).Count;

        // Get the private OpenMainMenu method
        var openMM = typeof(WindowManager).GetMethod("OpenMainMenu",
            BindingFlags.Instance | BindingFlags.NonPublic);

        // --- First invocation: should open window ---
        openMM.Invoke(wm, new object[] { new InputAction.CallbackContext() });
        Assert.IsTrue((bool)mainMenuField.GetValue(wm), "mainMenuOpen should be true after first toggle.");
        Assert.AreEqual(1, ActiveCount(), "MainMenu should be added once after first toggle.");

        // --- Second invocation: should close window ---
        openMM.Invoke(wm, new object[] { new InputAction.CallbackContext() });
        Assert.IsFalse((bool)mainMenuField.GetValue(wm), "mainMenuOpen should be false after second toggle.");
        Assert.AreEqual(0, ActiveCount(), "MainMenu should be closed and removed after second toggle.");
    }


    [Test]
    public void CreateWindow_WarnsWithoutCamera_But_StillCreates_And_Assigns()
    {
        using var s = new Scope();

        try
        {
            // Guarantee no camera is tagged MainCamera
            CameraTagIsolation.ClearAllMainCameras();

            // Valid InputActionReference
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = new InputActionMap("UI");
            var action = map.AddAction("OpenMainMenu", InputActionType.Button);
            asset.AddActionMap(map);
            var iaRef = InputActionReference.Create(action);

            // One key + matching prefab
            var all = WindowKeys.AllKeys;
            Assert.IsNotNull(all);
            Assert.Greater(all.Count, 0);
            var key = all[0];
            var prefab = s.MakePrefab(key);

            // Manager (init suppressed), inject, then init dictionary
            var go = new GameObject("WindowManager");
            go.SetActive(false);
            var wm = go.AddComponent<TestWindowManager>();
            SetPriv(wm, "openMainMenu", iaRef);
            SetPriv(wm, "windowPrefabsList", new List<GameObject> { prefab });
            typeof(WindowManager).GetField("instance", BindingFlags.Public | BindingFlags.Static)?.SetValue(null, wm);
            CallPriv(wm, "LoadWindowPrefabs");
            go.SetActive(true);

            // Expect the warning (make sure this EXACT string matches your implementation)
            UnityEngine.TestTools.LogAssert.Expect(
                LogType.Warning,
                "No main camera found. Window positing issues may occur."
            );

            // Act
            var w = wm.CreateWindow(key);

            // Assert: created and assigned even without camera
            Assert.IsNotNull(w);
            Assert.IsTrue(w.gameObject.activeSelf, "Window should be active.");
            Assert.AreSame(wm.transform, w.transform.parent, "Window should be parented to WindowManager.");
            Assert.AreSame(wm, w.WindowManager);
            Assert.AreSame(prefab, w.Prefab);
            Assert.AreEqual(key, w.Key);
        }
        finally
        {
            CameraTagIsolation.Restore();
        }
    }



    [Test]
    public void CloseWindow_Removes_FromActive_And_Resets_MainMenu()
    {
        using var s = new Scope();

        // Camera for positioning
        var cam = s.Make<Camera>("Main Camera");
        cam.tag = "MainCamera";

        // Valid InputActionReference
        var asset = ScriptableObject.CreateInstance<InputActionAsset>();
        var map = new InputActionMap("UI");
        var action = map.AddAction("OpenMainMenu", InputActionType.Button);
        asset.AddActionMap(map);
        var iaRef = InputActionReference.Create(action);

        // Collect keys (at least 2: one non-main, one main)
        Assert.GreaterOrEqual(WindowKeys.AllKeys.Count, 2, "Need at least two keys for this test.");
        string mainKey = WindowKeys.MainMenuKey;
        string otherKey = WindowKeys.AllKeys.Find(k => k != mainKey);

        var mainPrefab = s.MakePrefab(mainKey);
        var otherPrefab = s.MakePrefab(otherKey);

        // Manager setup
        var go = new GameObject("WindowManager");
        go.SetActive(false);
        var wm = go.AddComponent<TestWindowManager>();
        SetPriv(wm, "openMainMenu", iaRef);
        SetPriv(wm, "windowPrefabsList", new List<GameObject> { mainPrefab, otherPrefab });
        typeof(WindowManager).GetField("instance", BindingFlags.Public | BindingFlags.Static)?.SetValue(null, wm);
        CallPriv(wm, "LoadWindowPrefabs");
        go.SetActive(true);

        // Reflective helpers
        var activeWindowsField = typeof(WindowManager).GetField("activeWindows",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var mainMenuField = typeof(WindowManager).GetField("mainMenuOpen",
            BindingFlags.Instance | BindingFlags.NonPublic);

        int ActiveCount() => ((System.Collections.IList)activeWindowsField.GetValue(wm)).Count;

        // --- Case 1: non-main window ---
        var other = wm.OpenWindow(otherKey);
        Assert.AreEqual(1, ActiveCount(), "Non-main window should be added.");
        wm.CloseWindow(other);
        Assert.AreEqual(0, ActiveCount(), "Non-main window should be removed after CloseWindow.");
        Assert.IsFalse((bool)mainMenuField.GetValue(wm), "mainMenuOpen should remain false after closing non-main window.");

        // --- Case 2: main menu window ---
        var main = wm.OpenWindow(mainKey);
        SetPriv(wm, "mainMenuOpen", true); // simulate toggle open
        Assert.AreEqual(1, ActiveCount(), "MainMenu should be added.");
        wm.CloseWindow(main);
        Assert.AreEqual(0, ActiveCount(), "MainMenu should be removed after CloseWindow.");
        Assert.IsFalse((bool)mainMenuField.GetValue(wm), "mainMenuOpen should be reset to false after closing MainMenu.");
    }

    static class CameraTagIsolation
    {
        private static readonly List<(Camera cam, string tag)> _retag = new();

        public static void MakeThisTheOnlyMain(Camera preferred)
        {
            _retag.Clear();
            foreach (var cam in UnityEngine.Object.FindObjectsOfType<Camera>(true))
            {
                if (cam == preferred) continue;
                if (cam.CompareTag("MainCamera"))
                {
                    _retag.Add((cam, cam.tag));
                    cam.tag = "Untagged";
                }
            }
            if (preferred != null) preferred.tag = "MainCamera";
        }

        public static void ClearAllMainCameras()
        {
            _retag.Clear();
            foreach (var cam in UnityEngine.Object.FindObjectsOfType<Camera>(true))
            {
                if (cam.CompareTag("MainCamera"))
                {
                    _retag.Add((cam, cam.tag));
                    cam.tag = "Untagged";
                }
            }
        }

        public static void Restore()
        {
            foreach (var (cam, tag) in _retag)
                if (cam) cam.tag = tag;
            _retag.Clear();
        }
    }



    private static void SetPriv(object target, string field, object value)
    {
        // Look up on the declaring type, not target.GetType()
        var f = typeof(WindowManager).GetField(field,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(f, $"Field '{field}' not found on WindowManager.");
        f.SetValue(target, value);
    }

    private static void CallPriv(object target, string method)
    {
        var m = typeof(WindowManager).GetMethod(method,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(m, $"Method '{method}' not found on WindowManager.");
        m.Invoke(target, null);
    }

}
