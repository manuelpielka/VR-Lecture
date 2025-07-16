using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System;

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
    private const string SUBTITLES = "vtt";
    private const string SEPERATOR = "%252F";

    public static async Task<Lecture> DownloadMetaData(string path)
    {
        //for READ_DIRECTORY
        //string jsonBody = "\"{\\\"directory\\\":\\\"Other\\\",\\\"groups\\\":[\\\"admin\\\",\\\"kitemployee\\\",\\\"kitall\\\"]}\"";

        //for META, LANGUAGES, THUMBNAIL
        //string jsonBody = "\"{\\\"directory\\\":\\\"Other/offline_test\\\"}\"";

        //for SUBTITLES
        //string jsonBody = "\"{\\\"directory\\\":\\\"Other/offline_test\\\",\\\"language\\\":\\\"English\\\"}\"";

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

    public static void DownloadLecture(Lecture lecture)
    {
        //needs to be implmented
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

}

//classes for Json deserialization
[Serializable]
class MetaDataResponse
{
    public string title;
    public string presenter; 
}

