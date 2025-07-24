using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// This class is used to send prompts to the LLM and receive the responses.
/// </summary>
public class DialogueController : MonoBehaviour, ISSEHandler
{
    /// <summary>
    /// Reference to the DialogueSystemWindow class to display the LLM’s response.
    /// </summary>
    [SerializeField] private GUI.DialogueSystemWindow dialogueUI;

    /// <summary>
    /// The api url of the lecture translator's api
    /// </summary>
    private string apiUrl = "/webapi/start_dialog";

    private const string mainUrl = "https://lecture-translator.kit.edu";
    private const string devUrl = "https://lt2srv-backup.iar.kit.edu";

    private string sessionId = "";
    private string streamId = "";
    private string streamIdText = "";

    private string token = Environment.GetEnvironmentVariable("MY_API_TOKEN");

    private SSEClient sseClient;

    private string contentDirectory = "/logs/archive/%252F%252Fhome%252Fuevjj%2540student.kit.edu%252Fa";

    async void Start()
    {
        string json = $"\"{{\\\"bot\\\":\\\"bot\\\"}}\"";

        string answer = await PostRequest(devUrl + apiUrl, json);

        string[] ids = answer.Split(" ");
        sessionId = ids[0];
        streamId = ids[1];
        streamIdText = ids[3];

        print(sessionId);
        print("GRAPH: " + await PostRequest(devUrl + "/webapi/" + sessionId + "/getgraph", ""));

        await SendStart(streamId);

        sseClient = new SSEClient(devUrl + "/webapi/stream?channel=" + sessionId, this);
        sseClient.InitSse();
    }

    private void OnDestroy()
    {
        sseClient.Disconnect();

        SendEnd(streamId);
        SendEnd(streamIdText);
    }

    public async void OnSSEConnectionOpened()
    {
        Debug.Log("SSE Connection Opened");
        dialogueUI.loading = false;
    }

    public void OnSSEConnectionClosed()
    {
        Debug.Log("SSE Connection Closed");
    }

    public void OnSSEEventReceived(string eventName, string data)
    {
        Debug.Log($"Event Received: {eventName} => {data}");

        dialogueUI.llmAnswer = data;
    }

    public void OnSSEError(Exception ex)
    {
        Debug.LogError("SSE Error: " + ex.Message);
    }

    /// <summary>
    /// This method is used to send the connected LLM a prompt via text.
    /// </summary>
    /// <param name="prompt">The prompt to send to the LLM.</param>
    public async void SendPrompt(string prompt)
    {
        string json = $"\"{{\\\"seq\\\":\\\"{prompt}\\\",\\\"user\\\":\\\"uevjj@student.kit.edu\\\",\\\"context\\\":\\\"a\\\"}}\"";

        print("Sending prompt: " + prompt);

        print(await PostRequest(devUrl + "/webapi/" + sessionId + "/" + streamIdText + "/append", json));
    }

    public async void StartAudioStream()
    {
        print("Requesting Worker Information");

        string json = $"\"{{\\\"controll\\\":\\\"INFORMATION\\\"}}\"";

        print(await PostRequest(devUrl + "/webapi/" + sessionId + "/" + streamId + "/append", json));
    }

    public async Task SendStart(string stream)
    {
        print("Send start");

        string json = $"\"{{\\\"controll\\\":\\\"START\\\", \\\"content_directory\\\":\\\"{contentDirectory}\\\"}}\"";

        await PostRequest(devUrl + "/webapi/" + sessionId + "/" + stream + "/append", json);
    }

    public async Task SendEnd(string stream)
    {
        print("Send end");

        string json = "\"{\\\"controll\\\":\\\"END\\\"}\"";

        await PostRequest(devUrl + "/webapi/" + sessionId + "/" + stream + "/append", json);
    }

    /// <summary>
    /// This method is used to send the connected LLM a prompt via audio.
    /// </summary>
    /// <param name="pcmChunk">The recorded audio chunk to send.</param>
    /// <param name="start">The start time of the recorded audio chunk.</param>
    /// <param name="end">The end time of the recorded audio chunk.</param>
    public async void StreamAudio(byte[] pcmChunk, float start, float end)
    {
        string base64String = Convert.ToBase64String(pcmChunk);

        byte[] utf8Bytes = Encoding.UTF8.GetBytes(base64String);

        string utf8String = Encoding.UTF8.GetString(utf8Bytes);

        string json = "\"{\\\"b64_enc_pcm_s16le\\\": \\\"" + utf8String + "\\\", \\\"start\\\": \\\"" + start.ToString() + "\\\",\\\"end\\\":\\\"" + end.ToString() + "\\\"}\"";

        await PostRequest(devUrl + "/webapi/" + sessionId + "/" + streamId + "/append", json);
    }

    /// <summary>
    /// Sends a post request to the api server.
    /// </summary>
    /// <param name="url">The url of the api server.</param>
    /// <param name="json">The json data to send in the request.</param>
    private async Task<string> PostRequest(string url, string json)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] byteJson = new UTF8Encoding().GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(byteJson);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Cookie", "_forward_auth=" + token);

        print("Sending request to: " + url);

        await request.SendWebRequest();

        return request.downloadHandler.text;
    }

    public void SetContentDirectory(string dir)
    {
        contentDirectory = dir;
    }
}
