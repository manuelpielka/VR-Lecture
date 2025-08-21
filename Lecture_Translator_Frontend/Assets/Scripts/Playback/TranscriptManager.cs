using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

public class TranscriptManager : MonoBehaviour
{
    private Lecture lecture;
    private Dictionary<string, Transcript> transcripts = new Dictionary<string, Transcript>();

    public async Task AssignLecture(Lecture lecture)
    {
        this.lecture = lecture;

        if (!lecture.IsDownloaded())
        {
            foreach (string language in lecture.GetTranscriptLanguages())
            {
                string transcriptRaw = await LectureDownloader.StreamVTT(lecture, language);
                Transcript transcript = new Transcript(transcriptRaw);
                transcripts.Add(language, transcript);
                Debug.Log("Language added: " + language);
            }
        }
        else
        {
            foreach (string language in lecture.GetTranscriptLanguages())
            {
                string transcriptRaw = File.ReadAllText("./" + lecture.GetTranscriptSource() + "/" + language + ".vtt");
                Transcript transcript = new Transcript(transcriptRaw);
                transcripts.Add(language, transcript);
            }
        }

    }

    public Transcript GetTranscript(string language)
    {
        Debug.Log("Requested Language: " + language);
        return transcripts[language];
    }

    public List<string> GetAvailableLanguages()
    {
        return lecture.GetTranscriptLanguages();
    }
}