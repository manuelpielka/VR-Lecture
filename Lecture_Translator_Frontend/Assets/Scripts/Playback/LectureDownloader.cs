using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.IO;

public static class LectureDownloader
{
    private const string SERVER_URL = "https://lecture-translator.kit.edu/ltarchive/";
    private const string READ_DIRECTORY = "ls";
    private const string MESSAGES = "messages";
    private const string LANGUAGES = "languages";
    private const string MEDIATYPE = "mediatype";
    private const string MEDIA = "media";
    private const string META = "meta";
    private const string THUMBNAIL = "thumb";
    private const string VTT = "vtt";
    private const string SEPERATOR = "%252F";

    private const string DATA_DIRECTORY = "./Data/";

    public static async Task<Lecture> DownloadMetaData(string path)
    {
        //for READ_DIRECTORY
        //string jsonBody = "\"{\\\"directory\\\":\\\"Other\\\",\\\"groups\\\":[\\\"admin\\\",\\\"kitemployee\\\",\\\"kitall\\\"]}\"";

        //for META, LANGUAGES, THUMBNAIL
        //string jsonBody = "\"{\\\"directory\\\":\\\"Other/offline_test\\\"}\"";

        //for SUBTITLES
        //string jsonBody = "\"{\\\"directory\\\":\\\"Other/offline_test\\\",\\\"language\\\":\\\"English\\\"}\"";

        //seems that transcripts are stored as a messages.json file

        string jsonBody = $"\"{{\\\"directory\\\":\\\"{path}\\\"}}\"";

        Task<string> metaDataTask = PostRequest(SERVER_URL + META, jsonBody);
        Task<string> languagesTask = PostRequest(SERVER_URL + LANGUAGES, jsonBody);

        string videoSource = "https://lecture-translator.kit.edu/archivemedia/" + path.Replace("/", SEPERATOR);
        string transcriptSource = path;

        string metaDataRaw = await metaDataTask;
        MetaDataResponse metaDataResponse = JsonUtility.FromJson<MetaDataResponse>(metaDataRaw);
        string name = metaDataResponse.title;

        string langaugesRaw = await languagesTask;
        //Before: ["Multilingual", "German", "Dutch", "Italian", "Portuguese", "Spanish", "French", "English", "English Summary"]
        langaugesRaw = langaugesRaw.Replace("[", "");
        langaugesRaw = langaugesRaw.Replace("]", "");
        langaugesRaw = langaugesRaw.Replace("\"", "");
        //After: Multilingual, German, Dutch, Italian, Portuguese, Spanish, French, English, English Summary

        List<string> languages = new List<string>(langaugesRaw.Split(", "));
        Lecture lecture = new Lecture(name, videoSource, transcriptSource, languages);

        return lecture;
    }

    public static Task<string> StreamVTT(Lecture lecture, string language)
    {
        string source = lecture.GetTranscriptSource();
        string jsonBody = $"\"{{\\\"directory\\\":\\\"{source}\\\",\\\"language\\\":\\\"{language}\\\"}}\"";
        return PostRequest(SERVER_URL + VTT, jsonBody);
    }

    public static void DownloadVTT(Lecture lecture)
    {
        //not yet implemented
    }

    public static async Task<string> DownloadLecture(Lecture lecture)
    {
        Debug.Log(DATA_DIRECTORY + lecture.GetTranscriptSource());
        string targetPath = DATA_DIRECTORY + lecture.GetTranscriptSource() + ".mp4";
        await PostRequestFile(lecture.GetVideoSource(), "", targetPath);
        return targetPath;
    }


    /// Sends a post request to the api server.
    /// </summary>
    /// <param name="url">The url of the api server.</param>
    /// <param name="json">The json data to send in the request.</param>
    private static async Task<string> PostRequest(string url, string json)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] byteJson = new UTF8Encoding().GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(byteJson);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        await request.SendWebRequest();


        return request.downloadHandler.text;

    }

    private static async Task PostRequestFile(string url, string json, string targetPath)
    {
        Debug.Log(Application.dataPath);
        Debug.Log(Path.GetFullPath("."));
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] byteJson = new UTF8Encoding().GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(byteJson);
        request.downloadHandler = new DownloadHandlerFile(targetPath);
        //TODO: Fetch the Token!!!
        request.SetRequestHeader("Cookie", "_forward_auth=ACc-mvsPDcCk14a8hxZSR7Gneas513l0cJLCqiM8js8=|1753884610|ulvqv@student.kit.edu");
        await request.SendWebRequest();
        


        
        //Log the headers
        /*
        Dictionary<string, string> headers = request.GetResponseHeaders();
        foreach (string headercode in headers.Keys)
        {
            Debug.Log(headercode + ": " + headers[headercode]);
        }
        */
    }

}

//classes for Json deserialization
[Serializable]
class MetaDataResponse
{
    public string title;
    public string presenter; 
}

