using System.Collections.Generic;

public class Lecture
{
    private string name;
    private string videoSource;
    private string transcriptSource;
    private List<string> transcriptLanguages;
    private float lastPlayTime = 0;
    private bool downloaded = false;

    public Lecture(string name, string videoSource, string transcriptSource, List<string> transcriptLanguages)
    {
        this.name = name;
        this.videoSource = videoSource;
        this.transcriptSource = transcriptSource;
        this.transcriptLanguages = transcriptLanguages;
    }

    public string GetName()
    {
        return name;
    }
    public string GetVideoSource()
    {
        return videoSource;
    }
    public void SetVideoSource(string source)
    {
        videoSource = source;
    }
    public string GetTranscriptSource()
    {
        return transcriptSource;
    }
    public void SetTranscriptSource(string source)
    {
        transcriptSource = source;
    }
    public List<string> GetTranscriptLanguages()
    {
        return transcriptLanguages;
    }
    public float GetLastPlayTime()
    {
        return lastPlayTime;
    }
    public void SetLastPlayTime(int time)
    {
        lastPlayTime = time;
    }
    public bool IsDownloaded()
    {
        return downloaded;
    }
    public void SetDownloaded(bool value)
    {
        downloaded = value;
    }
}
