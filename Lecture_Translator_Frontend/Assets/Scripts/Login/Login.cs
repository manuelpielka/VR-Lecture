using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// This class handles logging in by inputting a token.
/// </summary>
public static class Login
{
    private const string REQUEST_TOKEN_URL = "https://lecture-translator.kit.edu/ltarchive/ls";

    public static string token = "";
    public static string username = "";

    /// <summary>
    /// Sets the token.
    /// </summary>
    /// <param name="newToken"> The new token. </param>
    /// <returns> Whether or not the token is valid. </returns>
    public static async Task<bool> SetToken(string newToken)
    {
        token = newToken;

        if (!newToken.Contains("|")) return false;

        username = newToken.Split("|")[2];

        Debug.Log("Set token to : " + token);
        Debug.Log("Set username to: " + username);

        string response = await PostRequest(REQUEST_TOKEN_URL);

        Debug.Log(response);

        return !response.Contains("Not authorized");
    }

    /// <summary>
    /// Makes post request for checking if the token is valid.
    /// </summary>
    /// <param name="url"> Url of the request. </param>
    /// <returns> Text data from the api. </returns>
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
}