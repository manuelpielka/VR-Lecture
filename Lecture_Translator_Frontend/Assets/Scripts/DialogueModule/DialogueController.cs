using System;
using System.Collections;
using System.Text;
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
    //private DialogueSystemWindow dialogueUI;

    /// <summary>
    /// Reference to the VirtualAvatar class to play the talking animation when receiving a response from the LLM.
    /// </summary>
    [SerializeField] private VirtualAvatar virtualAvatar;

    /// <summary>
    /// The api url of the lecture translator's api
    /// </summary>
    private string apiUrl = "/ltapi/start_dialog";

    private const string mainUrl = "https://lecture-translator.kit.edu";
    private const string devUrl = "https://lt2srv-backup.iar.kit.edu";

    private string sessionId = "";
    private string streamId = "";

    private const string token = "";

    void Start()
    {
        string json = "{ \"bot\": \"bot\"}";

        StartCoroutine(PostRequest(devUrl + apiUrl, json));
    }

    /// <summary>
    /// This method is used to send the connected LLM a prompt via text.
    /// </summary>
    /// <param name="prompt">The prompt to send to the LLM.</param>
    public void SendPrompt(string prompt)
    {
        string json = "{ \"bot\": \"bot\"}";

        StartCoroutine(PostRequest(sessionId + "/" + streamId + mainUrl + "/append", json));
    }

    public void SendStart()
    {
        string json = "{ \"controll\": \"START\"}, { \"bot\": \"bot\"}";

        StartCoroutine(PostRequest(sessionId + "/" + streamId + mainUrl + "/append", json));
    }

    public void SendEnd()
    {
        string json = "{ \"controll\": \"END\"}";

        StartCoroutine(PostRequest(sessionId + "/" + streamId + mainUrl + "/append", json));
    }

    /// <summary>
    /// This method is used to send the connected LLM a prompt via audio.
    /// </summary>
    /// <param name="pcmChunk">The recorded audio chunk to send.</param>
    /// <param name="start">The start time of the recorded audio chunk.</param>
    /// <param name="end">The end time of the recorded audio chunk.</param>
    public void StreamAudio(byte[] pcmChunk, float start, float end)
    {
        string base64String = Convert.ToBase64String(pcmChunk);

        byte[] utf8Bytes = Encoding.UTF8.GetBytes(base64String);

        string utf8String = Encoding.UTF8.GetString(utf8Bytes);

        string json = "{\"b64_enc_pcm_s16le\": \"" + utf8String + "\", \"start\": " + start + ",\"end\":" + end + "}"; // TODO: check formatting (LTConnection.java -> normalJsonStringToLtJsonString()

        StartCoroutine(PostRequest(sessionId + "/" + streamId + mainUrl + "/append", json));
    }

    /// <summary>
    /// Sends a post request to the api server.
    /// </summary>
    /// <param name="url">The url of the api server.</param>
    /// <param name="json">The json data to send in the request.</param>
    IEnumerator PostRequest(string url, string json)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] byteJson = new UTF8Encoding().GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(byteJson);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Cookie", "_forward_auth=" + token);

        yield return request.SendWebRequest();

        ReceiveAnswer(request.downloadHandler.text);
    }

    /// <summary>
    /// This method is called when the api answers the post request
    /// </summary>
    /// <param name="answer">The answer sent by the api.</param>
    public void ReceiveAnswer(string answer)
    {
        if (sessionId == "")
        {
            string[] ids = answer.Split(" ");
            sessionId = ids[0];
            streamId = ids[1];
        }

        virtualAvatar.PlayTalkingAnimation();
        Debug.Log("RESPONSE FROM LT_API: " + answer);
        //dialogueUI.DisplayLLMAnswer(answer);
    }
}
