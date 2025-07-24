using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections.Generic;

public static class Login
{
    private const string URL_REGEX = "^https?:\\/\\/(?:www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b(?:[-a-zA-Z0-9()@:%_\\+.~#?&\\/=]*)$";
    private const string REQUEST_TOKEN_URL = "https://lecture-translator.kit.edu/gettoken";

    public static async Task<string> GetToken(string username, string password)
    {
        string loginOptionPage = await PostRequest(REQUEST_TOKEN_URL);
        

        return null;
    }

    private static async Task<string> PostRequest(string url)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(null);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        await request.SendWebRequest();

        Dictionary<string, string> headers = request.GetResponseHeaders();
        foreach (string headercode in headers.Keys)
        {
            Debug.Log(headercode + ": " + headers[headercode]);
        }


        return request.downloadHandler.text;
        
    }
}