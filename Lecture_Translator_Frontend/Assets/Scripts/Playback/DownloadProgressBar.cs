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

    public void CompleteProgress()
    {
        gameObject.SetActive(false);
    }
}
