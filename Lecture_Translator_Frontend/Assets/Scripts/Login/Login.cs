using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

public static class Login
{
    private const string URL_REGEX = @"/dex/auth/shib\?client_id=[\S]+&amp;redirect_uri=[\S]+&amp;response_type=code&amp;scope=[\S]+&amp;state=[\S]+";
    private const string REQUEST_TOKEN_URL = "https://lecture-translator.kit.edu/ltarchive/ls";
    private const string SERVER_URL = "https://lecture-translator.kit.edu";

    public static string token = "";
    public static string username = "";

    public static async Task<string> GetToken(string username, string password)
    {
        string loginOptionPage = await PostRequest(REQUEST_TOKEN_URL);
        Debug.Log(loginOptionPage);
        MatchCollection options = Regex.Matches(loginOptionPage, URL_REGEX);
        string shibbolethLink = SERVER_URL + options[0].Value;
        Debug.Log(shibbolethLink);
        string shibbolethPage = await GetRequest(shibbolethLink);
        Debug.Log(shibbolethPage);


        return null;
    }

    public static async Task<bool> SetToken(string newToken)
    {
        token = newToken;
        username = newToken.Split("|")[2];

        Debug.Log("Set token to : " + token);
        Debug.Log("Set username to: " + username);

        string response = await PostRequest(REQUEST_TOKEN_URL);

        Debug.Log(response);

        return !response.Contains("Not authorized");
    }

    private static async Task<string> PostRequest(string url)
    {
        string jsonBody = $"\"{{\\\"directory\\\":\\\"/\\\",\\\"groups\\\":[\\\"admin\\\",\\\"kitemployee\\\",\\\"kitall\\\",\\\"all\\\",\\\"basic\\\",\\\"presenter\\\",\\\"collector\\\"]}}\"";
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] byteJson = new UTF8Encoding().GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(byteJson);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Cookie", "_forward_auth=" + token);
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