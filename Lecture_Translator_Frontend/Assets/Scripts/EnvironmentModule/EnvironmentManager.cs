using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Class <c>EnvironmentManager</c>manages additive switching between environment scenes.
/// </summary>
public class EnvironmentManager : MonoBehaviour
{
    /// <summary>
    /// Global access to the only instance.
    /// </summary>
    public static EnvironmentManager Instance { get; private set; }

    /// <summary>
    /// Default environment scene name (must exactly match the entry in Build Settings).
    /// Used to detect the starting scene when the app boots into it, and as the target when you programmatically reset back to the default environment.
    /// </summary>
    [Header("Startup")]
    [Tooltip("Scene that should be treated as the default environment.")]
    [SerializeField] private string defaultEnvironmentScene = "Library hall";

    /// <summary>
    /// Tracks the currently active environment scene name.
    /// </summary>
    private string currentSceneName;

    /// <summary>
    /// Read-only current environment scene name.
    /// </summary>
    public string CurrentSceneName => currentSceneName;

    /// <summary>
    /// Returns all scene names from Build Settings (including index 0).
    /// Intended for building dropdowns so users can switch and also switch back.
    /// </summary>
    /// <returns>A list of scene names in build-index order (including index 0)</returns>
    public List<string> GetEnvironmentSceneNames()
    {
        var names = new List<string>();
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = Path.GetFileNameWithoutExtension(path);
            names.Add(name);
        }
        return names;
    }

    /// <summary>
    /// Switchs environments by scene name.
    /// Validates the request and starts the internal switch routine.
    /// </summary>
    /// <param name="sceneName">Exact scene name as listed in Build Settings.</param>
    public void LoadEnvironment(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        if (sceneName == currentSceneName) return;

        if (!IsSceneInBuildSettings(sceneName))
        {
            Debug.LogWarning($"EnvironmentManager: Scene '{sceneName}' is not in Build Settings.");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(SwitchEnvironmentCoroutine(sceneName));
    }

    /// <summary>
    /// Loads the target environment additively, makes it active so RenderSettings apply,
    /// then unloads the previous environment. Used internally by LoadEnvironment().
    /// </summary>
    /// <param name="targetScene">Scene name to load.</param>
    /// <returns>Coroutine enumerator.</returns>
    private IEnumerator SwitchEnvironmentCoroutine(string targetScene)
    {
        string previous = currentSceneName;

        // Load target additively and wait until finished
        var load = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
        load.allowSceneActivation = false;

        // Wait until the scene data is fully loaded (progress reaches 0.9)
        while (load.progress < 0.9f)
        {
            yield return null;
        }

        // Allow activation; Awake/OnEnable/Start in the new scene will run now
        load.allowSceneActivation = true;
        while (!load.isDone) yield return null;

        // Make the new environment the active scene so its RenderSettings apply
        Scene newScene = SceneManager.GetSceneByName(targetScene);
        if (newScene.IsValid())
        {
            SceneManager.SetActiveScene(newScene);
        }

        // Give one frame to settle before unloading the previous environment
        yield return null;

        // Unload previous environment (if any)
        if (!string.IsNullOrEmpty(previous))
        {
            Scene prevScene = SceneManager.GetSceneByName(previous);
            if (prevScene.IsValid() && prevScene.isLoaded)
            {
                AsyncOperation unload = SceneManager.UnloadSceneAsync(prevScene);
                while (!unload.isDone) yield return null;
            }
        }
        currentSceneName = targetScene;
    }

    /// <summary>
    /// Ensures a single, persistent instance and records the starting scene if the app booted directly into the default environment.
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        var active = SceneManager.GetActiveScene().name;

        // Accept either the serialized default name OR whatever is at build index 0
        string index0Name = null;
        if (SceneManager.sceneCountInBuildSettings > 0)
        {
            var index0Path = SceneUtility.GetScenePathByBuildIndex(0);
            index0Name = Path.GetFileNameWithoutExtension(index0Path);
        }

        if (active == defaultEnvironmentScene ||(!string.IsNullOrEmpty(index0Name) && active == index0Name))
        {
            currentSceneName = active;
        }
    }

    /// <summary>
    /// Checks whether a scene exists in Build Settings.
    /// </summary>
    /// <param name="sceneName">Scene name to look for.</param>
    /// <returns></returns>
    private bool IsSceneInBuildSettings(string sceneName)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = Path.GetFileNameWithoutExtension(path);
            if (name == sceneName) return true;
        }
        return false;
    }

    // EnvironmentManager.cs (inside the class)
#if UNITY_EDITOR
    [ExcludeFromCodeCoverage]
    [ContextMenu("Switch To Room")]
    private void _SwitchToRoom()
    {
        LoadEnvironment("Room");
    }

    [ExcludeFromCodeCoverage]
    [ContextMenu("Switch To Library hall")]
    private void _SwitchToLibraryHall()
    {
        LoadEnvironment("Library hall");
    }
#endif
}
