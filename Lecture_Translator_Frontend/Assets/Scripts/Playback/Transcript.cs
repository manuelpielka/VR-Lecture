using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Transcript
{
    private const string TIMESTAMP_REGEX = "[0-9]+:[0-9][0-9]:[0-9][0-9].[0-9][0-9][0-9] --> [0-9]+:[0-9][0-9]:[0-9][0-9].[0-9][0-9][0-9]";
    private const string REGEX_FIRST_PART = "[0-9]+:[0-9][0-9]:[0-9][0-9].[0-9][0-9][0-9] --> ";
    private string fullText;
    private Dictionary<float, string> subtitleLines = new Dictionary<float, string>();
    public Transcript(string fullText)
    {
        //Remove Metadata and Linebreaks
        string cleanedText = fullText.Replace("WEBVTT","").Replace("\n","").Replace("\r","");

        //Remove Timestamps for full Text
        this.fullText = Regex.Replace(cleanedText, TIMESTAMP_REGEX, "");

        //create subtitles from the cleaned Text
        string[] rawLines = Regex.Split(cleanedText, TIMESTAMP_REGEX);

        IList<Match> matches = Regex.Matches(cleanedText, TIMESTAMP_REGEX);
        List<float> timestamps = new List<float>();
        foreach (Match match in matches) {
            //turn upper limit string to seconds as float
            string value = match.Value;
            value = Regex.Replace(value, REGEX_FIRST_PART, "");
            value = value.Replace(".", ",");
            string[] times = value.Split(":");
            float result = float.Parse(times[0]) * 3600 + float.Parse(times[1]) * 60 + float.Parse(times[2]);

            timestamps.Add(result);
        }

        for (int i = 0; i < timestamps.Count; i++)
        {
            //rawlines starts with an empty line due to splitting, so i+1
            subtitleLines.Add(timestamps[i],rawLines[i+1]);
        }
        
    }

    public string getFullText()
    {
        return fullText;
    }

    public string getLine(float time)
    {
        //loop over all end timestamps (keys) and determine the one closest to the current time
        float currentKey = 0;
        float currentDelta = Mathf.Infinity;
        foreach (float key in subtitleLines.Keys)
        {
            float delta = key - time;
            if (delta > currentDelta && delta >= 0)
            {
                currentKey = key;
                currentDelta = delta;
            }
        }
        return subtitleLines[currentKey];
    }
}