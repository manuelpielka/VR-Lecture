using UnityEngine;

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
        if (DisplayModeController.Instance != null)
        {
            bool isDark = DisplayModeController.Instance.IsDarkModeEnabled();

            ColorTheme currentTheme = isDark
            ? DisplayModeController.Instance.darkTheme
            : DisplayModeController.Instance.lightTheme;

            ThemedElement[] themedElements = GetComponentsInChildren<ThemedElement>(true);

            foreach (ThemedElement element in themedElements)
            {
                element.ApplyTheme(currentTheme, isDark);
            }
        }
        else
        {
            Debug.LogWarning("DisplayModeController.Instance is null!");
        }
       
    }
}
