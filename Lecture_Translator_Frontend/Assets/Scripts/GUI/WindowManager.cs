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

    /// <summary>
    /// A dictionary of all the prefabs of the windows with a key. These prefabs get instantiated when a new window is opened.
    /// </summary>
    private Dictionary<string, GameObject> windowPrefabs = new Dictionary<string, GameObject>();

    /// <summary>
    /// Reference to the WindowKeys class for accessing keys for specific lectures.
    /// </summary>
    private WindowKeys windowKeys;

    //TODO: change return type to Window
    public void CreateWindow(string prefabKey)
    {

    }

    public void OpenWindow(string windowKey)
    {

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
