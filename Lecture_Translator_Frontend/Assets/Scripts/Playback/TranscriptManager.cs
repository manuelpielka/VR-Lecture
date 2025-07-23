using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TranscriptManager : MonoBehaviour
{
    private Lecture lecture;
    private Dictionary<string, Transcript> transcripts = new Dictionary<string, Transcript>();

    public async void AssignLecture(Lecture lecture)
    {
        this.lecture = lecture;

        if (!lecture.IsDownloaded())
        {
            Dictionary<string, Task<string>> DownloadTaskList = new Dictionary<string, Task<string>>();

            //Download in pararllel to minimize wait times
            foreach (string language in lecture.GetTranscriptLanguages())
            {
                DownloadTaskList.Add(language, LectureDownloader.StreamVTT(lecture, language));
            }

            //Process responses
            foreach (string language in DownloadTaskList.Keys)
            {
                string transcriptRaw = await DownloadTaskList[language];
                Debug.Log(transcriptRaw);
                Transcript transcript = new Transcript(transcriptRaw);
                transcripts.Add(language, transcript);
            }
        }
        else
        {
            //TODO: needs to be implemented
        }

    }

    public Transcript GetTranscript(string language)
    {
        return transcripts[language];
    }
}