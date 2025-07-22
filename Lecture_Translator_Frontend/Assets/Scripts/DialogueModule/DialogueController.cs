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
public class DialogueController : MonoBehaviour
{
    /// <summary>
    /// Reference to the DialogueSystemWindow class to display the LLM’s response.
    /// </summary>
    [SerializeField] private GUI.DialogueSystemWindow dialogueUI;

    /// <summary>
    /// Reference to the VirtualAvatar class to play the talking animation when receiving a response from the LLM.
    /// </summary>
    private VirtualAvatar virtualAvatar;

    /// <summary>
    /// The api url of the lecture translator's api
    /// </summary>
    private string apiUrl = "/webapi/start_dialog";

    private string appendUrl = "/webapi/append";

    private const string mainUrl = "https://lecture-translator.kit.edu";
    private const string devUrl = "https://lt2srv-backup.iar.kit.edu";

    private string sessionId = "";
    private string streamId = "";
    private string streamIdText = "";

    private string token = Environment.GetEnvironmentVariable("MY_API_TOKEN");

    private CancellationToken ct;
    private string sseUrl = "";

    private Coroutine sseCoroutine;
    private bool sseRunning = false;

    async void Start()
    {
        virtualAvatar = VirtualAvatar.instance;

        

        string json = $"\"{{\\\"bot\\\":\\\"bot\\\"}}\"";

        string answer = await PostRequest(devUrl + apiUrl, json);

        ReceiveAnswer(answer);

        
    }

    private async Task ConnectToSSE()
    {
        string sseUrl = devUrl + "/webapi/stream?channel=" + sessionId;
        sseRunning = true;
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sseUrl);
            request.Accept = "text/event-stream";

            print("SSE STARTED!");

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                
                while (!reader.EndOfStream && !ct.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync();
                    if (!string.IsNullOrEmpty(line) && line.StartsWith("data:"))
                    {
                        string data = line.Substring(5).Trim();
                        Debug.Log("SSE Data Received: " + data);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("SSE Error: " + ex.Message);
        }
        print("SSE Stopped!");
        sseRunning = false;
    }

    /// <summary>
    /// This method is used to send the connected LLM a prompt via text.
    /// </summary>
    /// <param name="prompt">The prompt to send to the LLM.</param>
    public async void SendPrompt(string prompt)
    {
        if (!sseRunning)
            ConnectToSSE();

        await SendStart();

        string json = $"\"{{\\\"seq\\\":\\\"{prompt}\\\"}}\"";

        print("Sending prompt: " + prompt);

        ReceiveAnswer(await PostRequest(devUrl + "/webapi/" + sessionId + "/" + streamIdText + "/append", json));
    }

    public async void StartSSE()
    {
        //if(!sseRunning)
            //ConnectToSSE();

        print("Requesting Worker Information");

        string json = $"\"{{\\\"controll\\\":\\\"INFORMATION\\\"}}\"";

        print(await PostRequest(devUrl + "/webapi/" + sessionId + "/" + streamId + "/append", json));

        await SendStart();
    }

    public async Task SendStart()
    {
        print("Send start");

        string json = $"\"{{\\\"controll\\\":\\\"START\\\"}}\"";

        print("Start response: " + await PostRequest(devUrl + "/webapi/" + sessionId + "/" + streamId + "/append", json));
    }

    public async Task SendEnd()
    {
        print("Send end");
        string json = "{ \"controll\": \"END\"}";

        print(await PostRequest(devUrl + "/webapi/" + sessionId + "/" + streamId + "/append", json));
    }

    /// <summary>
    /// This method is used to send the connected LLM a prompt via audio.
    /// </summary>
    /// <param name="pcmChunk">The recorded audio chunk to send.</param>
    /// <param name="start">The start time of the recorded audio chunk.</param>
    /// <param name="end">The end time of the recorded audio chunk.</param>
    public async void StreamAudio(byte[] pcmChunk, float start, float end)
    {
        await SendStart();
        string base64String = Convert.ToBase64String(pcmChunk);

        byte[] utf8Bytes = Encoding.UTF8.GetBytes(base64String);

        string utf8String = Encoding.UTF8.GetString(utf8Bytes);

        string json = "{\"b64_enc_pcm_s16le\": \"" + utf8String + "\", \"start\": " + start + ",\"end\":" + end + "}"; // TODO: check formatting (LTConnection.java -> normalJsonStringToLtJsonString()

        await PostRequest(devUrl + "/webapi/" + sessionId + "/" + streamId + "/append", json);
        await SendEnd();
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
    /// This method is called when the api answers the post request
    /// </summary>
    /// <param name="answer">The answer sent by the api.</param>
    public void ReceiveAnswer(string answer)
    {
        Debug.Log("RESPONSE FROM LT_API: " + answer);
        if (sessionId == "")
        {
            string[] ids = answer.Split(" ");
            sessionId = ids[0];
            streamId = ids[1];
            streamIdText = ids[3];
        }

        virtualAvatar.PlayTalkingAnimation();
        dialogueUI.DisplayLLMAnswer(answer);
    }
}
