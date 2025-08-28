using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;

public class UnityApiClient : IApiClient
{
    private readonly string token;

    public UnityApiClient(string token)
    {
        this.token = token;
    }

    public async Task<string> PostRequest(string url, string json)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] byteJson = new UTF8Encoding().GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(byteJson);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Cookie", "_forward_auth=" + token);

        Debug.Log("Sending request to: " + url);

        await request.SendWebRequest();

        return request.downloadHandler.text;
    }
}