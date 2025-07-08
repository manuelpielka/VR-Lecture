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
    public void MoveTo()
    {
        //WIP
    }

    public void SetPlaybackSpeed()
    {
        //WIP
    }

    public int GetCurrentTime()
    {
        //WIP
        return 0;
    }

    public int GetVideoLength()
    {
        //WIP
        return 0;
    }
}
