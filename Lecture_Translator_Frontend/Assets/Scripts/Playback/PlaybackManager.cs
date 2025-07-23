using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

public class PlaybackManager : MonoBehaviour
{
    private Lecture lecture;
    public VideoPlayer VideoPlayer;

    public async Task AssignLecture(Lecture lecture)
    {
        this.lecture = lecture;

        if (!lecture.IsDownloaded())
        {
            Task<string> download = LectureDownloader.DownloadLecture(lecture);
            VideoPlayer.url = await download;
        }
        else
        {
            VideoPlayer.url = lecture.GetVideoSource();
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
        return VideoPlayer.time;
    }

    public double GetVideoLength()
    {
        return VideoPlayer.length;
    }
}
