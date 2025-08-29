using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Http;
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
    /// The api url of the lecture translator's api.
    /// </summary>
    private string startDialogURL = "/webapi/start_dialog";

    /// <summary>
    /// The main url of the lecture translator.
    /// </summary>
    private const string mainUrl = "https://lecture-translator.kit.edu";

    /// <summary>
    /// The dev url of the lecture translator.
    /// </summary>
    private const string devUrl = "https://lt2srv-backup.iar.kit.edu";

    /// <summary>
    /// The url that is being used.
    /// </summary>
    private string url = mainUrl;

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
    /// The sse client that handles receiving data from the lecture translator.
    /// </summary>
    private SSEClient sseClient;

    /// <summary>
    /// The content directory of the current lecture. Sent to the api for llm context.
    /// </summary>
    private string contentDirectory = "";

    /// <summary>
    /// Action that is invoked when the sse connection is opened / closed.
    /// </summary>
    public Action<bool> OnLoadingChanged;

    /// <summary>
    /// Action that is invoked when the sse connection receives data from the api.
    /// </summary>
    public Action<string> OnResponseReceived;

    /// <summary>
    /// Action that is invoked when the token is invalid.
    /// </summary>
    public Action<bool> OnInvalidTokenError;

    /// <summary>
    /// Action that is invoked when there is no response from the api.
    /// </summary>
    public Action<bool> OnNoResponseError;

    /// <summary>
    /// Check if the loading value changed this frame (for multithreading purposes)
    /// </summary>
    private bool loadingChanged = false;
    /// <summary>
    /// Check if the response value changed this frame (for multithreading purposes)
    /// </summary>
    private bool responseReceived = false;

    /// <summary>
    /// Whether or not the SSE connection is connected.
    /// </summary>
    private bool loading = true;

    /// <summary>
    /// The last response the api sent via SSE.
    /// </summary>
    private string response = "";

    /// <summary>
    /// Flag if the sendstart was sent to the api.
    /// </summary>
    private bool started = false;

    /// <summary>
    /// The client for connecting to sse.
    /// </summary>
    private HttpClient client;

    /// <summary>
    /// The client for connecting to the api.
    /// </summary>
    private IApiClient apiclient;

    /// <summary>
    /// The Start method called by unity.
    /// </summary>
    async void Start()
    {
        apiclient = apiclient ?? new UnityApiClient(Login.token);

        string json = $"\"{{\\\"bot\\\":\\\"bot\\\"}}\"";

        string answer = await PostRequest(url + startDialogURL, json);

        if (answer == "Not authorized\n")
        {
            print("Invalid token!");
            OnInvalidTokenError?.Invoke(true);
            return;
        }

        if (answer == "")
        {
            print("No response!");
            OnNoResponseError?.Invoke(true);
            return;
        }

        print(answer);
        string[] ids = answer.Split(" ");
        sessionId = ids[0];
        streamId = ids[1];
        streamIdText = ids[3];
        print(sessionId);
        print("GRAPH: " + await PostRequest(url + "/webapi/" + sessionId + "/getgraph", ""));

        await SendStart(streamId);
        started = true;

        sseClient = new SSEClient(url + "/webapi/stream?channel=" + sessionId, this, client);
        sseClient.InitSse();
    }

    /// <summary>
    /// The Update method called by Unity every frame.
    /// </summary>
    private void Update()
    {
        if (loadingChanged) // we need this because you have to invoke actions from a main thread
        {
            OnLoadingChanged?.Invoke(loading);
            loadingChanged = false;
        }
        if (responseReceived)
        {
            OnResponseReceived?.Invoke(response);
            responseReceived = false;
        }
    }

    /// <summary>
    /// The OnDestroy method called by unity once this GameObject is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (!loading)
        {
            sseClient.Disconnect();
        }
        if (started)
        {
            SendEnd(streamId);
            SendEnd(streamIdText);
        }
    }

    /// <summary>
    /// Event received when the sse connection is opened.
    /// </summary>
    public async void OnSSEConnectionOpened()
    {
        Debug.Log("SSE Connection Opened");

        loading = false;
        loadingChanged = true;
    }

    /// <summary>
    /// Event received when the sse connection is closed.
    /// </summary>
    public void OnSSEConnectionClosed()
    {
        Debug.Log("SSE Connection Closed");

        loading = true;
        loadingChanged = true;
    }

    /// <summary>
    /// Event received when the sse connection receives a message from the api.
    /// </summary>
    public void OnSSEEventReceived(string eventName, string data)
    {
        Debug.Log($"Event Received: {eventName} => {data}");

        response = data;
        responseReceived = true;
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
        string json = $"\"{{\\\"seq\\\":\\\"{prompt}\\\",\\\"user\\\":\\\"{Login.username}\\\",\\\"context\\\":\\\"a\\\"}}\"";

        print("Sending prompt: " + prompt);

        print(await PostRequest(url + "/webapi/" + sessionId + "/" + streamIdText + "/append", json));
    }

    /// <summary>
    /// Starts the audio stream by requesting worker information.
    /// </summary>
    public async void StartAudioStream()
    {
        print("Requesting Worker Information");

        string json = $"\"{{\\\"controll\\\":\\\"INFORMATION\\\"}}\"";

        print(await PostRequest(url + "/webapi/" + sessionId + "/" + streamId + "/append", json));
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

        await PostRequest(url + "/webapi/" + sessionId + "/" + stream + "/append", json);
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

        await PostRequest(url + "/webapi/" + sessionId + "/" + stream + "/append", json);
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

        await PostRequest(url + "/webapi/" + sessionId + "/" + streamId + "/append", json);
    }

    /// <summary>
    /// Sends a post request to the api server.
    /// </summary>
    /// <param name="url">The url of the api server.</param>
    /// <param name="json">The json data to send in the request.</param>
    private async Task<string> PostRequest(string url, string json)
    {
        return await apiclient.PostRequest(url, json);
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
