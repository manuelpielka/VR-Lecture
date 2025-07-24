using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// This class manages the tutorial steps and controls the flow of the tutorial.
/// It starts the tutorial and tracks the current step.
/// </summary>
public class TutorialManager : MonoBehaviour

{
    /// <summary>
    /// List of all tutorial steps.
    /// </summary>
    private List<ITutorialStep> steps = new List<ITutorialStep>();

    /// <summary>
    /// Current step index in the tutorial.
    /// </summary>
    private int currentStepIndex;

    /// <summary>
    /// Overlay container that holds the tutorial overlays.
    /// </summary>
    [SerializeField]
    private GameObject OverlayContainer;


    /// <summary>
    /// Reference to the WelcomeStepTutorial.
    /// </summary>
    [SerializeField]
    private WelcomeStepTutorial welcomeStep;

    /// <summary>
    /// Reference to the BrowseLectureStepTutorial.
    /// </summary>
    [SerializeField]
    private BrowseLectureStepTutorial browseLectureStep;

    /// <summary>
    /// Reference to the PlaybackControlsStepTutorial.
    /// </summary>
    [SerializeField]
    private PlaybackControlsStepTutorial playbackControlsStep;

    /// <summary>
    /// Reference to the OpenTranscriptStepTutorial.
    /// </summary>
    [SerializeField]
    private OpenTranscriptStepTutorial openTranscriptStep;

    /// <summary>
    /// Reference to the AskAvatarStepTutorial.
    /// </summary>
    [SerializeField]
    private AskAvatarStepTutorial askAvatarStep;

    /// <summary>
    /// Reference to the EndStepTutorial.
    /// </summary>
    [SerializeField]
    private EndStepTutorial endStep;



    /// <summary>
    /// Initializes the tutorial manager and checks if the tutorial has already been completed.
    /// </summary>
    void Start()
    {
        if (UserPreferencesManager.LoadTutorialCompleted())
        {
            Debug.Log("Tutorial already completed. Skipping tutorial.");
            return;
        }

        InitializeSteps();
        StartTutorial();

    }


    /// <summary>
    /// Starts the tutorial by initializing the current step index and showing the overlay container.
    /// It also starts the first step.
    /// </summary>
    public void StartTutorial()
    {

        currentStepIndex = 0;
        OverlayContainer.SetActive(true);

        steps[currentStepIndex].StepCompleted += StartNextStep;
        steps[currentStepIndex].StartStep();

    }


    /// <summary>
    /// Ends the tutorial and removes the overlay container.
    /// </summary>
    private void EndTutorial()
    {
        UserPreferencesManager.SaveTutorialCompleted(true);
        OverlayContainer.SetActive(false);
    }


    /// <summary>
    /// Resets the tutorial by deleting the saved preference for tutorial completion and restarts it.
    /// </summary>
    public void ResetTutorial()
    {
        PlayerPrefs.DeleteKey("UserPref_TutorialCompleted");
        PlayerPrefs.Save();

        InitializeSteps();
        StartTutorial();
        Debug.Log("Tutorial reset");

    }


    /// <summary>
    /// Starts the next step in the tutorial.
    /// It unsubscribes from the previous step's completion event, increments the current step index.
    /// </summary>
    private void StartNextStep()
    {
        steps[currentStepIndex].StepCompleted -= StartNextStep;

        currentStepIndex++;

        if (currentStepIndex < steps.Count)
        {
            steps[currentStepIndex].StepCompleted += StartNextStep;
            steps[currentStepIndex].StartStep();

        }
        else
        {
            EndTutorial();
        }
    }


    /// <summary>
    /// Initializes the list of tutorial steps with the references to the individual step classes.
    /// </summary>
    private void InitializeSteps()
    {
        steps.Clear();

        steps.Add(welcomeStep);
        steps.Add(browseLectureStep);
        steps.Add(playbackControlsStep);
        steps.Add(openTranscriptStep);
        steps.Add(askAvatarStep);
        steps.Add(endStep);
    }


    /// <summary>
    /// Gets the position for the tutorial step overlay based on the camera's position and forward direction.
    /// </summary>
    public static Vector3 GetStepPosition()
    {
        Vector3 forward = Camera.main.transform.forward;
        Vector3 position = Camera.main.transform.position + forward * 2f;
        position.y -= 0.3f;

        return position;
    }


    /// <summary>
    /// Gets the forward direction for the tutorial step overlay based on the camera's forward direction.
    /// </summary>
    public static Vector3 GetStepForward()
    {
        Vector3 forward = Camera.main.transform.forward;

        return forward;
    }




}
