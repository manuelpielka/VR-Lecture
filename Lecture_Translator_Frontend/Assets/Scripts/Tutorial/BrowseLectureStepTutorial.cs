using UnityEngine;
using UnityEngine.UI;
using System;



public class BrowseLectureStepTutorial : MonoBehaviour, ITutorialStep

{
    public event Action StepCompleted;

    [SerializeField]
    private GameObject OverlayPrefab;
    private GameObject overlayInstance;
    
    private Button nextButton;



    public void StartStep()
    {
        if (WindowManager.instance.IsWindowOpen(WindowKeys.LectureBrowserKey))
        {
            StepCompleted?.Invoke();
            EndStep();
            return;
        }

        overlayInstance = Instantiate(OverlayPrefab);
        Vector3 forward = Camera.main.transform.forward;
        Vector3 position = Camera.main.transform.position + forward * 2f;
        position.y -= 0.3f;

        overlayInstance.transform.position = position;
        overlayInstance.transform.rotation = Quaternion.LookRotation(forward);

        Debug.Log("Browse Lecture Step Tutorial Started");

        nextButton = overlayInstance.GetComponentInChildren<Button>();
        nextButton.onClick.AddListener(() =>
        {
            StepCompleted?.Invoke();
            EndStep();
        });
        
        WindowManager.WindowOpened += HandleWindowOpened;
          
    }

    private void HandleWindowOpened(string windowKey)
    {
        if(windowKey == WindowKeys.LectureBrowserKey)
        {
            StepCompleted?.Invoke();
            EndStep();
        }
    }

    public void EndStep()
    {
        if (overlayInstance != null)
        {
            Destroy(overlayInstance);
        }

        WindowManager.WindowOpened -= HandleWindowOpened;

        Debug.Log("Browse Lecture Step Tutorial Ended");
    }

    
    void Start() { }
    void Update() { }
}
