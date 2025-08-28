#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode tests for <see cref="EnvironmentManager"/> using the active Build Profile.
/// Assumes "Library hall" is index 0 and "Room" is index 1 in the active profile’s Scene List.
/// </summary>
public class EnvironmentManagerPlayModeTests
{
    /// <summary>
    /// The default scene name (Build Profiles index 0).
    /// </summary>
    private const string DefaultSceneName = "Library hall";
    
    /// <summary>
    /// A non-default scene name (Build Profiles index 1).
    /// </summary>
    private const string OtherSceneName = "Room";

    /// <summary>
    /// Verifies required scenes exist in the currently active Build Profile.
    /// </summary>
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Guard: make sure these two scenes are actually in the active build (by name)
        Assert.That(Application.CanStreamedLevelBeLoaded(DefaultSceneName),
            $"Scene '{DefaultSceneName}' is not in the active Build Profile.");
        Assert.That(Application.CanStreamedLevelBeLoaded(OtherSceneName),
            $"Scene '{OtherSceneName}' is not in the active Build Profile.");
    }

    /// <summary>
    /// Destroys any <see cref="EnvironmentManager"/> instances created during a test.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        // Clean up any EnvironmentManager instances created during a test
        foreach (var em in Object.FindObjectsByType<EnvironmentManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(em.gameObject);
        }
    }

    /// <summary>
    /// Loads a scene by name in <see cref="LoadSceneMode.Single"/> and waits for completion.
    /// </summary>
    /// <param name="sceneName">he scene name to load.</param>
    /// <returns>Enumerator that yields until the scene is fully loaded and active.</returns>
    private static IEnumerator LoadSingleAndWaitByName(string sceneName)
    {
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while (!op.isDone) yield return null;
        Assert.AreEqual(sceneName, SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Creates a fresh <see cref="EnvironmentManager"/> and yields one frame for <c>Awake</c>.
    /// </summary>
    /// <returns>Enumerator that yields one frame and asserts instance presence.</returns>
    private static IEnumerator CreateManagerAndWait()
    {
        new GameObject("EnvMgr").AddComponent<EnvironmentManager>();
        yield return null; // let Awake run
        Assert.IsNotNull(EnvironmentManager.Instance);
    }

    // -------------------- Tests --------------------

    /// <summary>
    /// <c>Awake</c> must not set <c>CurrentSceneName</c> when the active scene is not the default.
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    public IEnumerator Awake_Leaves_Null_When_Active_Is_Not_Default()
    {
        // Ensure we load a non-default scene
        yield return LoadSingleAndWaitByName("Room");
        Assert.AreEqual("Room", SceneManager.GetActiveScene().name);

        var go = new GameObject("EnvMgr");
        var mgr = go.AddComponent<EnvironmentManager>();
        yield return null;

        Assert.IsNull(mgr.CurrentSceneName);
    }

    /// <summary>
    /// Only one <see cref="EnvironmentManager"/> instance should exist; duplicates destroy themselves.
    /// </summary>
    [UnityTest]
    public IEnumerator Singleton_Destroys_Duplicate_Instance()
    {
        yield return LoadSingleAndWaitByName(DefaultSceneName);
        yield return CreateManagerAndWait();

        new GameObject("EnvMgr2").AddComponent<EnvironmentManager>(); // should self-destruct
        yield return null;

        var all = Object.FindObjectsByType<EnvironmentManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Assert.AreEqual(1, all.Length);
        Assert.AreSame(EnvironmentManager.Instance, all[0]);
    }

    /// <summary>
    /// <see cref="EnvironmentManager.LoadEnvironment(string)"/> should early-out for null, empty, or same name.
    /// </summary>
    [UnityTest]
    public IEnumerator LoadEnvironment_Ignores_Null_And_SameName()
    {
        yield return LoadSingleAndWaitByName(DefaultSceneName);
        yield return CreateManagerAndWait();

        int before = SceneManager.sceneCount;

        EnvironmentManager.Instance.LoadEnvironment(null);
        EnvironmentManager.Instance.LoadEnvironment(string.Empty);
        yield return null;

        EnvironmentManager.Instance.LoadEnvironment(SceneManager.GetActiveScene().name);
        yield return null;

        Assert.AreEqual(before, SceneManager.sceneCount);
    }

    /// <summary>
    /// When the target scene is not in Build Settings, the manager should log a warning and not switch.
    /// </summary>
    [UnityTest]
    public IEnumerator LoadEnvironment_Warns_When_NotInBuildSettings()
    {
        yield return LoadSingleAndWaitByName(DefaultSceneName);
        yield return CreateManagerAndWait();

        LogAssert.Expect(LogType.Warning,
            new System.Text.RegularExpressions.Regex("not in Build Settings"));

        EnvironmentManager.Instance.LoadEnvironment("DoesNotExist");
        yield return null;

        Assert.AreEqual(DefaultSceneName, SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Switching environments should load the target scene, set it active, and unload the previous one.
    /// </summary>
    [UnityTest]
    public IEnumerator SwitchEnvironment_Loads_Target_Sets_Active_And_Unloads_Previous()
    {
        yield return LoadSingleAndWaitByName(DefaultSceneName);
        yield return CreateManagerAndWait();

        EnvironmentManager.Instance.LoadEnvironment(OtherSceneName);

        // Wait until the manager reports the new scene as current
        float timeout = Time.realtimeSinceStartup + 10f;
        while (EnvironmentManager.Instance.CurrentSceneName != OtherSceneName &&
               Time.realtimeSinceStartup < timeout)
        {
            yield return null;
        }

        Assert.AreEqual(OtherSceneName, EnvironmentManager.Instance.CurrentSceneName);
        Assert.AreEqual(OtherSceneName, SceneManager.GetActiveScene().name);

        var prev = SceneManager.GetSceneByName(DefaultSceneName);
        Assert.IsFalse(prev.isLoaded);
    }

    /// <summary>
    /// GetEnvironmentSceneNames should return Build Settings scenes in order (first = default).
    /// </summary>
    [UnityTest]
    public IEnumerator GetEnvironmentSceneNames_Returns_BuildSettings_Order()
    {
        yield return LoadSingleAndWaitByName(DefaultSceneName);
        yield return CreateManagerAndWait();

        var names = EnvironmentManager.Instance.GetEnvironmentSceneNames();
        CollectionAssert.Contains(names, DefaultSceneName);
        CollectionAssert.Contains(names, OtherSceneName);
        Assert.AreEqual(DefaultSceneName, names[0]);
    }
}
#endif