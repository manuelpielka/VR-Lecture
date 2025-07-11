using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour

{
    private List<ITutorialStep> steps = new List<ITutorialStep>();
    private int currentStepIndex;
    private bool isTutorialCompleted;

    [SerializeField]
    private GameObject OverlayContainer;


    public void StartTutorial()
    {
    }

    private void UpdateTutorial()
    {
    }

    private void StartNextStep()
    {
    }

    private void EndCurrentStep()
    {
    }

    private void EndTutorial()
    {
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
