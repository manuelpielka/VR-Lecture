using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

/// <summary>
/// This class controls all Window object’s prefabs and currently opened Window objects after instantiating them.
/// </summary>
public class WindowManager : MonoBehaviour
{
    /// <summary>
    /// The singleton instance of the WindowManager.
    /// </summary>
    public static WindowManager instance;

    /// <summary>
    /// Event that is triggered when a window is opened.
    /// </summary>
    public static event Action<string> WindowOpened;

    /// <summary>
    /// A list of all the currently open windows displayed in the 3D environment.
    /// </summary>
    private List<Window> activeWindows = new();

    /// <summary>
    /// A bool storing info about whether the main menu is currently open or not.
    /// </summary>
    private bool mainMenuOpen = false;

    /// <summary>
    /// A list of all the prefabs of the windows with a key.
    /// </summary>
    [SerializeField] private List<GameObject> windowPrefabsList; //Populated in unity editor. Must have same order as in WindowKeys!!

    /// <summary>
    /// Input action reference for opening the main menu.
    /// </summary>
    [SerializeField] private InputActionReference openMainMenu;


    /// <summary>
    /// A dictionary of all the prefabs of the windows with a key. These prefabs get instantiated when a new window is opened.
    /// </summary>
    private Dictionary<string, GameObject> windowPrefabs = new Dictionary<string, GameObject>();

    /// <summary>
    /// Initializes the WindowManager and loads all window prefabs.
    /// </summary>
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        // Prevent multiple WindowManager instances from existing
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        LoadWindowPrefabs();
    }

    /// <summary>
    /// Subscribes to input and enables it.
    /// </summary>
    void OnEnable()
    {
        openMainMenu.action.performed += OpenMainMenu;
        openMainMenu.action.Enable();
    }

    /// <summary>
    /// Unsubscribes from input and disables input.
    /// </summary>
    void OnDisable()
    {
        openMainMenu.action.performed -= OpenMainMenu;
        openMainMenu.action.Disable();
    }

    /// <summary>
    /// Opens the main menu window when the corresponding input action is triggered.
    /// </summary>
    /// <param name="context">The input action context.</param>
    private void OpenMainMenu(InputAction.CallbackContext context)
    {
        if (!mainMenuOpen)
        {
            mainMenuOpen = true;
            OpenWindow(WindowKeys.MainMenuKey);

        }
        else
        {
            foreach (Window window in activeWindows)
            {
                if (window.Key == WindowKeys.MainMenuKey)
                {
                    mainMenuOpen = false;
                    window.Close();
                    return;
                }
            }
        }

    }

    /// <summary>
    /// Lods all window prefabs to a dictionary for easy access.
    /// </summary>
    private void LoadWindowPrefabs()
    {
        List<string> keys = WindowKeys.AllKeys;

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

            DontDestroyOnLoad(instance);

            if (window != null)
            {
                instance.transform.SetParent(transform, false);
                instance.SetActive(true);

                Camera camera = Camera.main;
                if (camera != null)
                {
                    Vector3 forward = camera.transform.forward;
                    Vector3 position = camera.transform.position + forward * 2f;
                    position.y -= 0.3f;

                    instance.transform.position = position;
                    instance.transform.rotation = Quaternion.LookRotation(forward);
                }
                else
                {
                    Debug.LogWarning("No main camera found. Window positing issues may occur.");
                }


                window.WindowManager = this;
                window.Prefab = prefab;
                window.Key = prefabKey; //Needed so OpenWindow can detect and prevent duplicate windows
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
    /// <returns>The window after opening it.</returns>
    public Window OpenWindow(string windowKey)
    {
        //Check if the given window is already open 
        var existing = activeWindows.Find(w => w.Key == windowKey);
        if (existing != null) return existing;

        Window window = CreateWindow(windowKey);
        if (window != null)
        {
            activeWindows.Add(window);

            WindowOpened?.Invoke(windowKey);
        }
        return window;
    }

    /// <summary>
    /// Checks if a window with the given key is currently open.
    /// </summary>
    /// <param name="windowKey">The key for the associated prefab.</param>
    public bool IsWindowOpen(string windowKey)
    {
        return activeWindows.Exists(w => w.Key == windowKey);
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

        if (window.Key == WindowKeys.MainMenuKey)
        {
            mainMenuOpen = false;
        }
    }
}
