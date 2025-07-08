using UnityEngine;
using System.Collections.Generic;

public class TranscriptManager : MonoBehaviour
{
    private Lecture lecture;
    private Dictionary<string, Transcript> transcripts;

    public void AssignLecture(Lecture lecture)
    {
        this.lecture = lecture;
        //logic for downloading and parsing transcripts goes here
    }

    public Transcript GetTranscript(string language)
    {
        return transcripts[language];
    }
}