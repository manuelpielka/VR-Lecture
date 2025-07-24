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
    /// The api url of the lecture translator's api.
    /// </summary>
    private string apiUrl = "/webapi/start_dialog";

    /// <summary>
    /// The main url of the lecture translator.
    /// </summary>
    private const string mainUrl = "https://lecture-translator.kit.edu";

    /// <summary>
    /// The dev url of the lecture translator.
    /// </summary>
    private const string devUrl = "https://lt2srv-backup.iar.kit.edu";

    /// <summary>
    /// The string that represents a "/" in the lecture translator.
    /// </summary>
    private const string SEPERATOR = "%252F";

    /// <summary>
    /// The session id of the current session of the lecture translator.
    /// </summary>
    private string sessionId = "";

    /// <summary>
    /// The stream id of the current session for audio streaming to the lecture translator.
    /// </summary>
    private string streamId = "";

    /// <summary>
    /// The stream id of the current session for text streaming to the lecture translator.
    /// </summary>
    private string streamIdText = "";

    /// <summary>
    /// The token to access the api.
    /// </summary>
    private string token = "";

    /// <summary>
    /// The sse client that handles receiving data from the lecture translator.
    /// </summary>
    private SSEClient sseClient;

    /// <summary>
    /// The content directory of the current lecture. Sent to the api for llm context.
    /// </summary>
    private string contentDirectory = "";

    /// <summary>
    /// The Start method called by unity.
    /// </summary>
    async void Start()
    {
        string json = $"\"{{\\\"bot\\\":\\\"bot\\\"}}\"";

        string answer = await PostRequest(mainUrl + apiUrl, json);

        string[] ids = answer.Split(" ");
        sessionId = ids[0];
        streamId = ids[1];
        streamIdText = ids[3];
        print(answer);
        print(sessionId);
        print("GRAPH: " + await PostRequest(mainUrl + "/webapi/" + sessionId + "/getgraph", ""));

        await SendStart(streamId);

        sseClient = new SSEClient(mainUrl + "/webapi/stream?channel=" + sessionId, this);
        sseClient.InitSse();
    }

    /// <summary>
    /// The OnDestroy method called by unity once this GameObject is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        sseClient.Disconnect();

        SendEnd(streamId);
        SendEnd(streamIdText);
    }

    /// <summary>
    /// Event received when the sse connection is opened.
    /// </summary>
    public async void OnSSEConnectionOpened()
    {
        Debug.Log("SSE Connection Opened");
        dialogueUI.loading = false;
    }

    /// <summary>
    /// Event received when the sse connection is closed.
    /// </summary>
    public void OnSSEConnectionClosed()
    {
        Debug.Log("SSE Connection Closed");
        dialogueUI.loading = true;
    }

    /// <summary>
    /// Event received when the sse connection receives a message from the api.
    /// </summary>
    public void OnSSEEventReceived(string eventName, string data)
    {
        Debug.Log($"Event Received: {eventName} => {data}");

        dialogueUI.llmAnswer = data;
    }

    /// <summary>
    /// Event received when the sse connection has an error.
    /// </summary>
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

        print(await PostRequest(mainUrl + "/webapi/" + sessionId + "/" + streamIdText + "/append", json));
    }

    /// <summary>
    /// Starts the audio stream by requesting worker information.
    /// </summary>
    public async void StartAudioStream()
    {
        print("Requesting Worker Information");

        string json = $"\"{{\\\"controll\\\":\\\"INFORMATION\\\"}}\"";

        print(await PostRequest(mainUrl + "/webapi/" + sessionId + "/" + streamId + "/append", json));
    }

    /// <summary>
    /// Sends the start signal to the api with the content directory as context.
    /// </summary>
    /// <param name="stream"> Which stream id to send the signal to. </param>
    /// <returns> An awaitable task. </returns>
    public async Task SendStart(string stream)
    {
        print("Send start");

        string json = $"\"{{\\\"controll\\\":\\\"START\\\", \\\"content_directory\\\":\\\"{contentDirectory}\\\"}}\"";

        await PostRequest(mainUrl + "/webapi/" + sessionId + "/" + stream + "/append", json);
    }

    /// <summary>
    /// Sends the end signal to the api.
    /// </summary>
    /// <param name="stream"> Which stream id to send the signal to. </param>
    /// <returns> An awaitable task. </returns>
    public async Task SendEnd(string stream)
    {
        print("Send end");

        string json = "\"{\\\"controll\\\":\\\"END\\\"}\"";

        await PostRequest(mainUrl + "/webapi/" + sessionId + "/" + stream + "/append", json);
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

        await PostRequest(mainUrl + "/webapi/" + sessionId + "/" + streamId + "/append", json);
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

    /// <summary>
    /// Sets the content directory in the correct format.
    /// </summary>
    /// <param name="dir"> The directory of the current lecture. </param>
    public void SetContentDirectory(string dir)
    {
        contentDirectory = "/logs/archive/" + dir.Replace("/", SEPERATOR);
    }
}
