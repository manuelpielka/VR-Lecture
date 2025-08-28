using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

public class PlaybackManager : MonoBehaviour
{
    [SerializeField] private DownloadProgressBar downloadProgressBar;
    private Lecture lecture;
    public VideoPlayer VideoPlayer;

    public async Task AssignLecture(Lecture lecture)
    {
        this.lecture = lecture;

        if (!lecture.IsDownloaded())
        {
            Task<string> download = LectureDownloader.DownloadLecture(lecture, downloadProgressBar);
            VideoPlayer.url = await download;
        }
        else
        {
            VideoPlayer.url = lecture.GetVideoSource();
            downloadProgressBar.CompleteProgress();
        }
    }

    public void Play()
    {
        VideoPlayer.Play();
    }

    public void Pause()
    {
        VideoPlayer.Pause();
    }
    public void MoveTo(double time)
    {
        VideoPlayer.time = time;
    }

    public void SetPlaybackSpeed(float factor)
    {
        VideoPlayer.playbackSpeed = factor;
    }

    public double GetCurrentTime()
    {
        return VideoPlayer != null ? VideoPlayer.time: 0;
    }

    public double GetVideoLength()
    {
        return VideoPlayer.length;
    }
}
