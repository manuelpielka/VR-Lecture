using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class is the base class of all Windows. A Window is a GUI screen that can be moved around in the VR environment.
/// </summary>
public class Window : MonoBehaviour
{

    /// <summary>
    /// Indicates whether or not the dark mode is currently active.
    /// </summary>
    public bool DarkMode { get; set; }
    /// <summary>
    /// The prefab of this window that gets instantiated when a new window is opened.
    /// </summary>
    public GameObject Prefab { get; set; }

    /// <summary>
    /// Reference to the window manager to open and close Window objects.
    /// </summary>
    public WindowManager WindowManager { get; set; }

    /// <summary>
    /// Reference to the tutorial manager to start the tutorial.
    /// </summary>
    public TutorialManager TutorialManager { get; set; }

    private void Start()
    {
        WindowManager = WindowManager.instance;

        RefreshTheme();
    }

    /// <summary>
    /// Closes this window by destroying the GameObject and calling the CloseWindow method in the window manager.
    /// </summary>
    public void Close()
    {
        if (WindowManager == null)
        {
            return;
        }

        WindowManager.CloseWindow(this);
        Destroy(gameObject);
        
    }


    /// <summary>
    /// Changes the position of the attached Transform Component.
    /// </summary>
    public void Move(Vector3 newPosition)
    {
        transform.position = newPosition;
    }


    /// <summary>
    /// Changes the size of the attached Transform Component.
    /// </summary>
    public void Resize(Vector2 newSize)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            return;
        }

        rectTransform.sizeDelta = newSize;
    }

    public void RefreshTheme()
    {
        bool isDark = DisplayModeController.Instance?.IsDarkModeEnabled() ?? false;

        foreach (var tmp in GetComponentsInChildren<TextMeshProUGUI>(true))
            tmp.color = isDark ? Color.white : Color.black;

        foreach (var img in GetComponentsInChildren<Image>(true))
            img.color = isDark ? Color.black : Color.white;
    }
}
