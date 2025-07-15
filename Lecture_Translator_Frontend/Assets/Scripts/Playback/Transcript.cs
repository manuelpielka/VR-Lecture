using System;
using System.Collections.Generic;
using UnityEngine;

public class Transcript
{
    private string fullText;
    private Dictionary<float, string> subtitleLines;
    public Transcript(string fullText)
    {
        //this will probably need to be parsed to remove timestamps etc.
        //it depends on the format of the downloaded transcript
        this.fullText = fullText;
        //create subtitles from the full text, will need to be parsed, not implemented for now
        subtitleLines.Add(0,fullText);
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