using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class Login
{
    private const string URL_REGEX = @"/dex/auth/shib\?client_id=[\S]+&amp;redirect_uri=[\S]+&amp;response_type=code&amp;scope=[\S]+&amp;state=[\S]+";
    private const string REQUEST_TOKEN_URL = "https://lecture-translator.kit.edu/gettoken";
    private const string SERVER_URL = "https://lecture-translator.kit.edu";

    public static async Task<string> GetToken(string username, string password)
    {
        string loginOptionPage = await PostRequest(REQUEST_TOKEN_URL);
        Debug.Log(loginOptionPage);
        MatchCollection options = Regex.Matches(loginOptionPage, URL_REGEX);
        Debug.Log(SERVER_URL + options[0].Value);
        string shibbolethPage = await GetRequest(SERVER_URL + options[0].Value);
        Debug.Log(shibbolethPage);


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
    
    private static async Task<string> GetRequest(string url)
    {
        UnityWebRequest request = new UnityWebRequest(url, "GET");
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