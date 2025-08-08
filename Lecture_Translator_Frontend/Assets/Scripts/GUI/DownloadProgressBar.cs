using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DownloadProgressBar : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    private UnityWebRequest request;

    void Update()
    {
        if (request != null)
        {
            Debug.Log("Current Progress: " + request.downloadProgress);
            progressBar.value = request.downloadProgress;
            if (request.isDone)
            {
                CompleteProgress();
            }
        }
    }

    public void SetRequest(UnityWebRequest request)
    {
        this.request = request;
    }

    private void CompleteProgress()
    {
        gameObject.SetActive(false);
    }
}
