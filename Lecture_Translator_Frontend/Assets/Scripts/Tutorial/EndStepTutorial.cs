using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// This class represents the playback controls step of the tutorial.
/// It is the sixth and final step and it informas the user about the end of the tutorial.
/// </summary>

public class EndStepTutorial : MonoBehaviour, ITutorialStep

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
    /// Button that is used to finish the tutorial.
    /// </summary>
    private Button FinishButton;



    /// <summary>
    /// Starts the welcome step of the tutorial.
    /// Shows the overlay and sets up next button and window listener.
    /// </summary>
    public void StartStep()
    {

        overlayInstance = Instantiate(OverlayPrefab);
        overlayInstance.transform.position = TutorialManager.GetStepPosition();
        overlayInstance.transform.rotation = Quaternion.LookRotation(TutorialManager.GetStepForward());

        FinishButton = overlayInstance.GetComponentInChildren<Button>();
        FinishButton.onClick.AddListener(() =>
        {
            StepCompleted?.Invoke();
            EndStep();
        });

    }


    /// <summary>
    /// Ends the step by destroying the overlay instance.
    /// </summary>
    public void EndStep()
    {
        if (overlayInstance != null)
        {
            Destroy(overlayInstance);
        }
    }

}
