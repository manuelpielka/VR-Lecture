using UnityEngine;
using UnityEngine.Video;

public class PlaybackManager : MonoBehaviour
{
    private Lecture lecture;
    public VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer.url = lecture.GetVideoSource();

        if (lecture.IsDownloaded())
        {
            videoPlayer.source = VideoSource.Url;
        }
        else
        {
            videoPlayer.source = VideoSource.VideoClip;
        }

    }

    public void AssignLecture(Lecture lecture)
    {
        this.lecture = lecture;
    }

    public void Play()
    {
        videoPlayer.Play();
    }

    public void Pause()
    {
        videoPlayer.Pause();
    }
    public void MoveTo(double time)
    {
        videoPlayer.time = time;
    }

    public void SetPlaybackSpeed(float factor)
    {
        videoPlayer.playbackSpeed = factor;
    }

    public double GetCurrentTime()
    {
        return videoPlayer.time;
    }

    public double GetVideoLength()
    {
        return videoPlayer.length;
    }
}
