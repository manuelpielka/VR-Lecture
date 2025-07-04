using System.Collections;
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
    private VirtualAvatar virtualAvatar;

    /// <summary>
    /// The api url of the lecture translator's api
    /// </summary>
    private string apiUrl = "/ltapi/start_dialog"; // TODO: Add real IP

    /// <summary>
    /// This method is used to send the connected LLM a prompt via text.
    /// </summary>
    public void SendPrompt(string prompt)
    {
        string json = "{ \"bot_stream\": \"" + prompt + "\"}";

        StartCoroutine(PostRequest(json));
    }

    /// <summary>
    /// This method is used to send the connected LLM a prompt via audio.
    /// </summary>
    public void SendPrompt(AudioClip clip)
    {
        
    }

    /// <summary>
    /// Sends a post request to the api server.
    /// </summary>
    IEnumerator PostRequest(string json)
    {
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] byteJson = new System.Text.UTF8Encoding().GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(byteJson);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        ReceiveAnswer(request.downloadHandler.text);
    }

    /// <summary>
    /// This method is called when the api answers the post request
    /// </summary>
    public void ReceiveAnswer(string answer)
    {
        virtualAvatar.PlayTalkingAnimation();
        //dialogueUI.DisplayLLMAnswer(answer);
    }
}
