using UnityEngine;
using UnityEngine.UI;
using System;



public class OpenTranscriptStepTutorial : MonoBehaviour, ITutorialStep
{
    public event Action StepCompleted;

    [SerializeField]
    private GameObject OverlayPrefab;
    private GameObject overlayInstance;
    
    private Button nextButton;



    public void StartStep()
    { 
        Debug.Log("OpenTranscriptStepTutorial started");

        overlayInstance = Instantiate(OverlayPrefab);
        Debug.Log("Instantied overlay: " + overlayInstance.name);

        Vector3 forward = Camera.main.transform.forward;
        Vector3 position = Camera.main.transform.position + forward * 2f;
        position.y -= 0.3f;

        overlayInstance.transform.position = position;
        overlayInstance.transform.rotation = Quaternion.LookRotation(forward);

        nextButton = overlayInstance.GetComponentInChildren<Button>();
        nextButton.onClick.AddListener(() =>
        {
            Debug.Log("Next button clicked");
            StepCompleted?.Invoke();
            EndStep();
        });
          
    }


    public void EndStep()
    {
        if (overlayInstance != null)
        {
            Destroy(overlayInstance);
        }
    }

    
    void Start() { }
    void Update() { }
}
