#if UNITY_EDITOR
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using NUnit.Framework;

/// <summary>
/// Class <c>NoteGUIPlayModeTestHelpers</c> provides helper utilities used by
/// Note-related GUI PlayMode tests (scene load, window control, UI lookup, clicks, typing).
/// </summary>
public static class NoteGUIPlayModeTestHelpers
{
    /// <summary>
    /// Opens a window by <paramref name="windowKey"/> via <see cref="WindowManager"/>
    /// and waits until the created instance is active in the hierarchy.
    /// </summary>
    /// <param name="windowKey">The WindowManager key used to open the window</param>
    /// <param name="timeoutSeconds">Maximum time to wait before giving up (seconds).</param>
    /// <returns>An enumerator that yields until the window is active or the timeout is reached.</returns>
    public static IEnumerator OpenWindowAndWaitByKey(string windowKey, float timeoutSeconds = 5f)
    {
        var wm = FindWindowManager();
        Assert.IsNotNull(wm, "WindowManager not found in scene.");

        var w = wm.OpenWindow(windowKey);
        Assert.IsNotNull(w, $"OpenWindow returned null for key '{windowKey}'.");

        yield return WaitUntil(() => w != null && w.gameObject.activeInHierarchy, timeoutSeconds);
    }

    /// <summary>
    /// Finds the first active instance of a window/component of type <typeparamref name="T"/> in the scene.
    /// </summary>
    /// <typeparam name="T">Component type to look for.</typeparam>
    /// <returns>The first found component, or <c>null</c> if none exists.</returns>
    public static T FindWindowOfType<T>() where T : Component
    {
        return UnityEngine.Object.FindFirstObjectByType<T>(FindObjectsInactive.Exclude);
    }
    // ---------- 1) Scene / System ----------

    /// <summary>
    /// Loads a scene by name in <see cref="LoadSceneMode.Single"/> and yields one frame.
    /// </summary>
    /// <param name="sceneName">The scene name to load.</param>
    /// <returns>Enumerator that yields until the first frame after the load request.</returns>
    public static IEnumerator LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        yield return null;
    }

    /// <summary>
    /// Ensures an <see cref="EventSystem"/> exists in the scene; creates one if missing.
    /// </summary>
    public static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem").AddComponent<EventSystem>();
            es.gameObject.AddComponent<StandaloneInputModule>();
        }
    }

    // ---------- 2) WindowManager control ----------

    /// <summary>
    /// Tries to locate a <c>WindowManager</c> in the scene.
    /// </summary>
    /// <returns>The first WindowManager found; otherwise null.</returns>
    public static WindowManager FindWindowManager()
    {
        // Prefer lookup by name first; fallback to type search if not found
        var go = GameObject.Find("WindowManager");
        if (go != null) return go.GetComponent<WindowManager>();
        return Object.FindFirstObjectByType<WindowManager>();
    }

    /// <summary>
    /// Opens a window by name through <c>WindowManager</c> and yields one frame.
    /// </summary>
    /// <param name="windowName">The window name registered in WindowManager.</param>
    /// <param name="wm">Optional WindowManager instance; if null it will be looked up.</param>
    /// <returns>Enumerator that yields one frame after the open request.</returns>
    public static IEnumerator OpenWindow(string windowName, WindowManager wm = null)
    {
        wm ??= FindWindowManager();
        if (wm == null)
        {
            throw new InvalidOperationException("WindowManager not found in scene.");
        }
        wm.OpenWindow(windowName);
        yield return null;
    }

    /// <summary>
    /// Closes a window by name through <c>WindowManager</c> and yields one frame.
    /// </summary>
    /// <param name="windowName">The window name registered in WindowManager.</param>
    /// <param name="wm">Optional WindowManager instance; if null it will be looked up.</param>
    /// <returns>Enumerator that yields one frame after the close request.</returns>
    public static IEnumerator CloseWindow(string windowName, WindowManager wm = null)
    {
        wm ??= FindWindowManager();
        if (wm == null) throw new InvalidOperationException("WindowManager not found in scene.");

        Component instance = null;
        if (windowName == WindowKeys.CreateNoteKey)
            instance = FindWindowOfType<CreateNoteWindow>();
        else if (windowName == WindowKeys.NotesKey)
            instance = FindWindowOfType<NoteWindow>();
        else
            instance = Object.FindFirstObjectByType<Window>(FindObjectsInactive.Exclude);

        if (instance != null && instance.gameObject.activeInHierarchy) wm.CloseWindow((Window)instance);
        yield return null;
    }

    /// <summary>
    /// Returns whether a window root GameObject is active in the scene.
    /// </summary>
    /// <param name="windowRootName">The root GameObject name of the window.</param>
    /// <returns>True if the window root is active; otherwise, false.</returns>
    public static bool IsWindowActive(string windowRootName)
    {
        var go = GameObject.Find(windowRootName);
        return go != null && go.activeInHierarchy;
    }

    // ---------- 3) UI lookup and interaction ----------

    /// <summary>
    /// Finds a component by a scene path (using <c>GameObject.Find</c>) and returns the requested type.
    /// </summary>
    /// <typeparam name="T">The component type to search for.</typeparam>
    /// <param name="path">A scene path or object name resolvable by <c>GameObject.Find</c>.</param>
    /// <returns>The found component of type <typeparamref name="T"/>.</returns>
    public static T Find<T>(string path) where T : Component
    {
        var go = GameObject.Find(path);
        return go != null ? go.GetComponent<T>() : null;
    }

    /// <summary>
    /// Finds the first descendant component of type <typeparamref name="T"/> under a root by name.
    /// </summary>
    /// <typeparam name="T">The component type to search for.</typeparam>
    /// <param name="root">The root transform to search under.</param>
    /// <param name="childName">The child object name to match.</param>
    /// <returns>The component of type <typeparamref name="T"/> if found; otherwise <c>null</c>.</returns>
    public static T FindInChildrenByName<T>(Transform root, string childName) where T : Component
    {
        if (root == null || string.IsNullOrEmpty(childName)) return null;
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == childName) return t.GetComponent<T>();
        }
        return null;
    }

    /// <summary>
    /// Invokes a UI button click via its <see cref="Button.onClick"/> event.
    /// </summary>
    /// <param name="button">The button to invoke.</param>
    public static void Click(Button button) => button?.onClick?.Invoke();

    /// <summary>
    /// Sets text into a <see cref="TMP_InputField"/> and triggers <see cref="TMP_InputField.onEndEdit"/>.
    /// </summary>
    /// <param name="field">The input field to modify.</param>
    /// <param name="text">The text to assign.</param>
    public static void Type(TMP_InputField field, string text)
    {
        if (field == null) return;
        field.text = text;
        field.onEndEdit?.Invoke(text);
    }

    // ---------- 4) Waiting and misc ----------

    /// <summary>
    /// Waits until a condition is met or a timeout (in seconds) is reached.
    /// </summary>
    /// <param name="predicate">The condition to wait for.</param>
    /// <param name="timeoutSeconds">The timeout in seconds.</param>
    /// <returns>Enumerator that yields until the condition is true or timed out.</returns>
    public static IEnumerator WaitUntil(Func<bool> predicate, float timeoutSeconds = 5f)
    {
        float until = Time.realtimeSinceStartup + timeoutSeconds;
        while (!predicate() && Time.realtimeSinceStartup < until)
            yield return null;
    }

    /// <summary>
    /// Yields a specific number of frames.
    /// </summary>
    /// <param name="frames">The number of frames to wait for.</param>
    /// <returns>Enumerator that yields <paramref name="frames"/> frames.</returns>
    public static IEnumerator WaitFrames(int frames)
    {
        for (int i = 0; i < frames; i++)
            yield return null;
    }

    /// <summary>
    /// Returns the number of direct children under a transform.
    /// </summary>
    /// <param name="parent">The parent transform.</param>
    /// <returns>The direct child count.</returns>
    public static int ChildCount(Transform parent) => parent != null ? parent.childCount : 0;
}
#endif