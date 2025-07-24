using UnityEngine;
using UnityEngine.InputSystem;

public class DebugTutorial : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            FindFirstObjectByType<TutorialManager>()?.ResetTutorial();
            Debug.Log("T key pressed. Resetting tutorial.");
        }
        
    }
}
