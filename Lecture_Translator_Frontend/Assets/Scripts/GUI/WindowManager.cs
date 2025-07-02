using UnityEngine;
using System.Collections.Generic;

public class WindowManager : MonoBehaviour
{
    private List<Window> activeWindows = new();
    private Dictionary<string, GameObject> windowPrefabs = new Dictionary<string, GameObject>();

    private WindowKeys windowKeys;

    public Window CreateWindow(string prefabKey)
    {
        return;
    }

    public void OpenWindow(string prefabKey)
    {

    }
    
    public void CloseWindow(Window window)
    {
        
    }
}
