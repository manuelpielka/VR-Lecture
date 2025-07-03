using UnityEngine;
using System.Collections.Generic;


/// <summary>
/// This class controls all Window object’s prefabs and currently opened Window objects after instantiating them.
/// </summary>
public class WindowManager : MonoBehaviour
{
    /// <summary>
    /// A list of all the currently open windows displayed in the 3D environment.
    /// </summary>
    private List<Window> activeWindows = new();


    private List<GameObject> windowPrefabsList = new(); //Populated in unity editor. Must have same order as in WindowKeys!!


    /// <summary>
    /// A dictionary of all the prefabs of the windows with a key. These prefabs get instantiated when a new window is opened.
    /// </summary>
    private Dictionary<string, GameObject> windowPrefabs = new Dictionary<string, GameObject>();


    /// <summary>
    /// Reference to the WindowKeys class for accessing keys for specific lectures.
    /// </summary>
    private WindowKeys windowKeys;


    void Awake()
    {
        LoadWindowPrefabs();
    }


    private void LoadWindowPrefabs()
    {
        List<string> keys = windowKeys.AllKeys;

        for (int i = 0; i < keys.Count && i < windowPrefabsList.Count; i++)
        {
            string key = keys[i];
            GameObject prefab = windowPrefabsList[i];

            if (key != null && prefab != null && !windowPrefabs.ContainsKey(key))
            {
                windowPrefabs[key] = prefab;
            }
            else
            {
                Debug.LogError($"Window prefab for key '{key}' is not set in the WindowManager.");
            }
        }
    }


    /// <summary>
    /// Creates a new instance of a prefab with the given prefabKey.
    /// </summary>
    /// <param name="prefabKey">The key for the associated prefab.</param>
    /// <returns>The window after creating it.</returns>
    public Window CreateWindow(string prefabKey)
    {
        if (windowPrefabs.ContainsKey(prefabKey))
        {
            GameObject prefab = windowPrefabs[prefabKey];

            if (prefab == null)
            {
                Debug.LogError($"Window prefab with key '{prefabKey}' is not set in the WindowManager.");
                return null;
            }

            GameObject instance = Instantiate(prefab);
            Window window = instance.GetComponent<Window>();

            if (window != null)
            {
                instance.transform.SetParent(transform, false);
                instance.SetActive(true);


                window.WindowManager = this;
                window.Prefab = prefab;
                return window;
            }
            else
            {
                Debug.LogError($"Prefab with key '{prefabKey}' does not have a Window component.");
                return null;
            }
        }

        Debug.LogError($"WindowManager does not store this prefabKey: '{prefabKey}'.");
        return null;

    }



    /// <summary>
    /// Calls CreateWindow() with the given windowKey to instantiate a new Window object from prefab and add it to the list of active windows.
    /// </summary>
    /// <param name="windowKey">The key for the associated prefab.</param>
    public void OpenWindow(string windowKey)
    {
        Window window = CreateWindow(windowKey);
        if (window != null)
        {
            activeWindows.Add(window);
        }
    }



    /// <summary>
    /// Removes the closed window from the list of active windows.
    /// </summary>
    /// <param name="window"> The window that was closed and that is to be removed from the active windows list.</param>
    public void CloseWindow(Window window)
    {
        if (activeWindows.Contains(window))
        {
            activeWindows.Remove(window);
        }
    }
}
