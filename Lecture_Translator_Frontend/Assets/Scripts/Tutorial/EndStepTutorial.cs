using UnityEngine;
using UnityEngine.UI;
using System;



public class EndStepTutorial : MonoBehaviour, ITutorialStep

{
    public event Action StepCompleted;

    [SerializeField]
    private GameObject OverlayPrefab;
    private GameObject overlayInstance;
    
    private Button FinishButton;



    public void StartStep()
    { 
        overlayInstance = Instantiate(OverlayPrefab);

        Vector3 forward = Camera.main.transform.forward;
        Vector3 position = Camera.main.transform.position + forward * 2f;
        position.y -= 0.3f;

        overlayInstance.transform.position = position;
        overlayInstance.transform.rotation = Quaternion.LookRotation(forward);

        FinishButton = overlayInstance.GetComponentInChildren<Button>();
        FinishButton.onClick.AddListener(() =>
        {
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
