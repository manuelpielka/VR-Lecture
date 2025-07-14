using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VRInteractionManager : MonoBehaviour
{
    // A list of input components (e.g., VR hand controllers).
    //private List<XRBaseInteractor> Interactors;

    // A list of interactable objects in the scene (e.g., UI buttons or the avatar).
    //private List<XRBaseIbteractable> Interactables;

    // The selected object currently being interacted with (e.g., the button being pressed or the object being grabbed).
    private GameObject SelectObject;


    // Registers the given object so that it can be interacted with in VR (e.g., buttons, sliders, or other UI components).
    public void RegisterInteractable(GameObject targetObject)
    {

    }

    // Unregisters the object, making it no longer interactable in the VR scene (e.g., when a UI window is closed or the scene is switched).
    public void UnregisterInteractable(GameObject targetObject) {
        
    }


    // Processes a generic input action (e.g., trigger press or button click) to determine the appropriate interaction behavior.
    public void HandleInteraction(InputAction input)
    {
        
    }

    // Handles logic for selecting or grabbing the given object in the VR environment. (e.g., clicking a UI button to open the notes window, selecting the summarization tool, or initiating a chat with the AI avatar).
    public void HandleSelect(GameObject targetObject)
    {
        
    }


    // Completes the interaction when the player releases an input (e.g., releasing a button or dropping an object).
    public void HandleRelease()
    {

    }





    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    //Runs every frame to handle ongoing interactions.
    void Update()
    {
        
    }
}
