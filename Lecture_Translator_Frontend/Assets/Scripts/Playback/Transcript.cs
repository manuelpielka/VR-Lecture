using System;
using System.Collections.Generic;

public class Transcript
{
    private string fullText;
    private List<string> subtitleLines;

    public Transcript(string fullText)
    {
        //this will probably need to be parsed to remove timestamps etc.
        //it depends on the format of the downloaded transcript
        this.fullText = fullText;
        //create subtitles from the full text, will need to be parsed, not implemented for now
        subtitleLines.Add(fullText);
    }

    public string getFullText()
    {
        return fullText;
    }

    public string getLine(int time)
    {
        //needs to be implemented, might need to change the subtitle list into some other data format to make this work
        //if they are in order you could just iterate over it until you hit the first one for which the timestamp is lower than the time, then display the one before
        //imperfect solution though because it would display lines for longer than it sometimes shoulds
        return "";
    }
}