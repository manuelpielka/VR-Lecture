using UnityEngine;
using UnityEngine.UI;
using System;


/// <summary>
/// This class represents the open transcript step of the tutorial.
/// It is the fourth step and it gives the user information on opening the transcript.
/// </summary>

public class OpenTranscriptStepTutorial : MonoBehaviour, ITutorialStep
{
    /// <summary>
    /// Event that is trrigered when the step is completed.
    /// </summary>
    public event Action StepCompleted;

    /// <summary>
    /// Prefab for the overlay that will be instantiated during the step.
    /// </summary>
    [SerializeField]
    private GameObject OverlayPrefab;

    /// <summary>
    /// Instance of the overlay that is created during the step.
    /// </summary>
    private GameObject overlayInstance;
    
    /// <summary>
    /// Button that is used to proceed to the next step.
    /// </summary>
    private Button nextButton;



    /// <summary>
    /// Starts the welcome step of the tutorial.
    /// Shows the overlay and sets up next button and window listener.
    /// </summary>
    public void StartStep()
    {
        if (WindowManager.instance.IsWindowOpen(WindowKeys.TranscriptKey))
        {
            StepCompleted?.Invoke();
            EndStep();
            return;
        }

        overlayInstance = Instantiate(OverlayPrefab);
        overlayInstance.transform.position = TutorialManager.GetStepPosition();
        overlayInstance.transform.rotation = Quaternion.LookRotation(TutorialManager.GetStepForward());

        nextButton = overlayInstance.GetComponentInChildren<Button>();
        nextButton.onClick.AddListener(() =>
        {
            StepCompleted?.Invoke();
            EndStep();
        });

        WindowManager.WindowOpened += HandleWindowOpened;

    }


    /// <summary>
    /// Completes the step when the transcript window is opened.
    /// </summary>
    /// <param name="windowKey">The key of the opened window.</param>
    /// 
    private void HandleWindowOpened(string windowKey)
    {
        if (windowKey == WindowKeys.TranscriptKey)
        {
            StepCompleted?.Invoke();
            EndStep();
        }
    }


    /// <summary>
    /// Ends the step by destroying the overlay instance and removing the window listener.
    /// </summary>
    public void EndStep()
    {
        if (overlayInstance != null)
        {
            Destroy(overlayInstance);
        }

        WindowManager.WindowOpened -= HandleWindowOpened;

    }

}
