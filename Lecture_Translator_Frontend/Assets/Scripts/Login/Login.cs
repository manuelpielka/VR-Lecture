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
    /// <summary>
    /// The url of the post request.
    /// </summary>
    private const string URL = "https://lecture-translator.kit.edu/ltarchive/ls";

    /// <summary>
    /// The token inputted by the user.
    /// </summary>
    public static string token = "";

    /// <summary>
    /// The username at the end of the token.
    /// </summary>
    public static string username = "";

    /// <summary>
    /// The client which handles post requests.
    /// </summary>
    public static IApiClient apiclient;

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

        apiclient = apiclient ?? new UnityApiClient(token);

        string response = await PostRequest(URL, "");

        Debug.Log(response);

        return !response.Contains("Not authorized");
    }

    /// <summary>
    /// Makes post request for checking if the token is valid.
    /// </summary>
    /// <param name="url"> Url of the request. </param>
    /// <returns> Text data from the api. </returns>
    private static async Task<string> PostRequest(string url, string json)
    {
        return await apiclient.PostRequest(url, json);
    }
}