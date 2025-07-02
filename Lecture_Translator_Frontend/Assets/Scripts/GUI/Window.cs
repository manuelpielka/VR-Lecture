using UnityEngine;

/// <summary>
/// This class is the base class of all Windows. A Window is a GUI screen that can be moved around in the VR environment.
/// </summary>
public class Window : MonoBehaviour
{
    
    /// <summary>
    /// Indicates whether or not the dark mode is currently active.
    /// </summary>
    private bool darkMode = false;

    /// <summary>
    /// The prefab of this window that gets instantiated when a new window is opened.
    /// </summary>
    private GameObject prefab;

    /// <summary>
    /// Reference to the window manager to open and close Window objects.
    /// </summary>
    private WindowManager windowManager;

    /// <summary>
    /// Reference to the tutorial manager to start the tutorial.
    /// </summary>
    private TutorialManager tutorialManager;



    /// <summary>
    /// Closes this window by destroying the GameObject and calling the CloseWindow method in the window manager.
    /// </summary>
    private void Close()
    {
        if (windowManager == null)
        {
            return;
        }
        Destroy(gameObject);
        windowManager.CloseWindow(this);
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

}
