using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour

{
    private List<ITutorialStep> steps = new List<ITutorialStep>();
    private int currentStepIndex;
    private bool isTutorialCompleted;

    [SerializeField]
    private GameObject OverlayContainer;



    [SerializeField]
    private WelcomeStepTutorial welcomeStep;


    [SerializeField]
    private BrowseLectureStepTutorial browseLectureStep;

    [SerializeField]
    private PlaybackControlsStepTutorial playbackControlsStep;


    [SerializeField]
    private OpenTranscriptStepTutorial openTranscriptStep;


    [SerializeField]
    private AskAvatarStepTutorial askAvatarStep;


    [SerializeField]
    private EndStepTutorial endStep;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
        Debug.Log("TutorialManager Start called");

        InitializeSteps();
        StartTutorial();
        
    }

    void nextStep()
    {
        var currentStep = steps[currentStepIndex];
        currentStep.StepCompleted += OnStepCompleted;
        currentStep.StartStep();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartTutorial()
    {

        Debug.Log("Starting Tutorial");

        isTutorialCompleted = false;
        currentStepIndex = 0;
        OverlayContainer.SetActive(true);
        
        steps[currentStepIndex].StepCompleted += OnStepCompleted;
        steps[currentStepIndex].StartStep();

    }

    private void EndTutorial()
    {
        isTutorialCompleted = true;
        OverlayContainer.SetActive(false);
        Debug.Log("Tutorial completed");
    }

    public void ResetTutorial()
    {
    }

    public int GetCurrentStepIndex()
    {
        return currentStepIndex;
    }

    public bool IsTutorialCompleted()
    {
        return isTutorialCompleted;
    }

    private void OnStepCompleted()
    {
        steps[currentStepIndex].StepCompleted -= OnStepCompleted;

        Debug.Log("Step completed: " + currentStepIndex);
        currentStepIndex++;

        if (currentStepIndex < steps.Count)
        {
            steps[currentStepIndex].StepCompleted += OnStepCompleted;
            steps[currentStepIndex].StartStep();

        }
        else
        {
            EndTutorial();
        }
    }

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


}
