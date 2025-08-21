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
            Debug.Log(lecture.GetTranscriptLanguages().Count);
            foreach (string language in lecture.GetTranscriptLanguages())
            {
                Debug.Log("Attempting language: " + language);
                string transcriptRaw = await LectureDownloader.StreamVTT(lecture, language);
                Debug.Log("Downloaded");
                Transcript transcript = new Transcript(transcriptRaw);
                Debug.Log("Parsed");
                transcripts.Add(language, transcript);
                Debug.Log("Added to index");
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
        return transcripts[language];
    }

    public List<string> GetAvailableLanguages()
    {
        return lecture.GetTranscriptLanguages();
    }
}