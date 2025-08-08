using UnityEngine;
using UnityEngine.UI;

public class DownloadProgressBar : MonoBehaviour
{
    [SerializeField] private Slider progressBar;

    public void SetProgress(float progress)
    {
        progressBar.value = progress;
    }

    public void CompleteProgress()
    {
        gameObject.SetActive(false);
    }
}
