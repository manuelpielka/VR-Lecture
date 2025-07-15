using System;
using System.Collections;
using System.Text;
using System.Net.Http;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

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


    private static string sessionId = "";
    private static string streamId = "";
    private static string token = "";

    public static async Task<Lecture> DownloadMetaData(string path)
    {
        string jsonBody = "\"{\\\"directory\\\":\\\".\\\",\\\"groups\\\":[\\\"admin\\\",\\\"kitemployee\\\",\\\"kitall\\\"]}\"";
        string answer = await PostRequest(SERVER_URL + READ_DIRECTORY, jsonBody);
        Debug.Log(answer);
        return null;
    }

    public static void DownloadLecture(Lecture lecture)
    {
        //needs to be implmented
    }


    /// Sends a post request to the api server.
    /// </summary>
    /// <param name="url">The url of the api server.</param>
    /// <param name="json">The json data to send in the request.</param>
    static async Task<string> PostRequest(string url, string json)
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
