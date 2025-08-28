using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.Networking;

public class DialogueSystemTests
{
    private GUI.DialogueSystemWindow dialogueSystemWindow;

    [SetUp]
    public void Setup()
    {
        SceneManager.LoadScene("Library Hall");
    }

    [TearDown]
    public void TearDown()
    {
        if (dialogueSystemWindow != null)
        {
            var closeButton = dialogueSystemWindow.transform.Find("Canvas/Panel/Close/CloseButton").GetComponent<Button>();
            closeButton.onClick.Invoke();
        }
    }

    [UnityTest]
    public IEnumerator VirtualAvatar_Test()
    {
        var windowManager = GameObject.Find("WindowManager").GetComponent<WindowManager>();
        yield return null;
        dialogueSystemWindow = windowManager.OpenWindow("DialogueWindow").GetComponent<GUI.DialogueSystemWindow>();

        var virtualAvatar = VirtualAvatar.instance;

        yield return null;

        Assert.IsTrue(virtualAvatar.transform.GetChild(0).gameObject.activeSelf);

        yield return null;

        virtualAvatar.PlayTalkingAnimation();

        yield return null;

        var closeButton = dialogueSystemWindow.transform.Find("Canvas/Panel/Close/CloseButton").GetComponent<Button>();
        closeButton.onClick.Invoke();

        yield return null;

        Assert.IsTrue(!virtualAvatar.transform.GetChild(0).gameObject.activeSelf);
    }

    [UnityTest]
    public IEnumerator NoToken_Test()
    {
        var windowManager = GameObject.Find("WindowManager").GetComponent<WindowManager>();
        yield return null;
        dialogueSystemWindow = windowManager.OpenWindow("DialogueWindow").GetComponent<GUI.DialogueSystemWindow>();

        yield return new WaitForSeconds(1f);

        Assert.IsTrue(dialogueSystemWindow.transform.Find("Canvas/ErrorTokenPanel").gameObject.activeSelf);
    }

    [UnityTest]
    public IEnumerator NoApi_Test()
    {
        var windowManager = GameObject.Find("WindowManager").GetComponent<WindowManager>();
        yield return null;
        dialogueSystemWindow = windowManager.OpenWindow("DialogueWindow").GetComponent<GUI.DialogueSystemWindow>();

        var manager = dialogueSystemWindow.GetComponent<DialogueController>();

        var field = typeof(DialogueController).GetField("url", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(manager, "");

        yield return new WaitForSeconds(3f);

        Assert.IsTrue(dialogueSystemWindow.transform.Find("Canvas/ErrorResponsePanel").gameObject.activeSelf);
    }

    [UnityTest]
    public IEnumerator SSE_Test()
    {
        var windowManager = GameObject.Find("WindowManager").GetComponent<WindowManager>();
        yield return null;
        dialogueSystemWindow = windowManager.OpenWindow("DialogueWindow").GetComponent<GUI.DialogueSystemWindow>();

        var manager = dialogueSystemWindow.GetComponent<DialogueController>();

        var json = new GUI.JsonLLMResponse();
        json.seq = "Hello";
        json.sender = "bot";
        

        var handler = new FakeHttpMessageHandler("data: " + JsonUtility.ToJson(json) + "\nevent: message\n\n");
        var httpClient = new HttpClient(handler);

        var field = typeof(DialogueController).GetField("client", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(manager, httpClient);

        var field2 = typeof(DialogueController).GetField("apiclient", BindingFlags.NonPublic | BindingFlags.Instance);
        field2.SetValue(manager, new FakeApiClient());

        yield return null;

        var aiTextBox = dialogueSystemWindow.transform.Find("Canvas/Panel/Panel/AiText").GetComponent<TMPro.TextMeshProUGUI>();

        Assert.IsTrue(aiTextBox.text == "Hello");
    }

    [UnityTest]
    public IEnumerator SSEError_Test()
    {
        var windowManager = GameObject.Find("WindowManager").GetComponent<WindowManager>();
        yield return null;
        dialogueSystemWindow = windowManager.OpenWindow("DialogueWindow").GetComponent<GUI.DialogueSystemWindow>();

        var manager = dialogueSystemWindow.GetComponent<DialogueController>();

        var json = new GUI.JsonLLMResponse();
        json.seq = "Hello";
        json.sender = "bot";


        var handler = new FakeHttpMessageHandler("data: " + JsonUtility.ToJson(json) + "\nevent: message\n\n");
        handler.error = true;
        LogAssert.Expect(LogType.Error, "SSE Error: Failed to connect. Status: UnavailableForLegalReasons");
        var httpClient = new HttpClient(handler);

        var field = typeof(DialogueController).GetField("client", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(manager, httpClient);

        var field2 = typeof(DialogueController).GetField("apiclient", BindingFlags.NonPublic | BindingFlags.Instance);
        field2.SetValue(manager, new FakeApiClient());

        yield return null;

        var aiTextBox = dialogueSystemWindow.transform.Find("Canvas/Panel/Panel/AiText").GetComponent<TMPro.TextMeshProUGUI>();

        Assert.IsTrue(aiTextBox.text != "Hello");
    }

    [UnityTest]
    public IEnumerator SendPrompt_Test()
    {
        var windowManager = GameObject.Find("WindowManager").GetComponent<WindowManager>();
        yield return null;
        dialogueSystemWindow = windowManager.OpenWindow("DialogueWindow").GetComponent<GUI.DialogueSystemWindow>();

        var manager = dialogueSystemWindow.GetComponent<DialogueController>();

        var apiClient = new FakeApiClient();

        var field = typeof(DialogueController).GetField("apiclient", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(manager, apiClient);

        manager.SetContentDirectory("lecture1");

        yield return null;

        var input_field = dialogueSystemWindow.transform.Find("Canvas/Panel/Panel/TextInput").GetComponent<TMPro.TMP_InputField>();

        input_field.text = "Test prompt.";

        yield return null;

        input_field.onEndEdit.Invoke("Test prompt.");

        yield return null;

        Assert.IsTrue(dialogueSystemWindow.GetUserPrompt() == "Test prompt."); // Is the user text box updated?
        Assert.IsTrue(apiClient.GetSendPromptCount() == 1); // Was a send prompt api call called?

        yield return null;

        input_field.onEndEdit.Invoke("Test prompt.");

        yield return null;

        Assert.IsTrue(apiClient.GetSendPromptCount() == 2);
    }

    [UnityTest]
    public IEnumerator StreamAudio_Test()
    {
        var windowManager = GameObject.Find("WindowManager").GetComponent<WindowManager>();
        yield return null;
        dialogueSystemWindow = windowManager.OpenWindow("DialogueWindow").GetComponent<GUI.DialogueSystemWindow>();

        var manager = dialogueSystemWindow.GetComponent<DialogueController>();

        var apiClient = new FakeApiClient();

        var field = typeof(DialogueController).GetField("apiclient", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(manager, apiClient);

        yield return null;

        var micButton = dialogueSystemWindow.transform.Find("Canvas/Panel/Mic/MicButton").GetComponent<Button>();

        yield return null;

        micButton.onClick.Invoke(); // Start recording

        yield return new WaitForSeconds(1f); // record for 1 second (should be 6 stream audio requests)

        micButton.onClick.Invoke(); // Stop recording

        yield return null;
        Debug.Log(apiClient.GetStreamAudioCount());
        Assert.GreaterOrEqual(apiClient.GetStreamAudioCount(), 1); // at least 1 request went through -> this works
    }
}

public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly string _responseData;
    public bool error = false;

    public FakeHttpMessageHandler(string responseData)
    {
        _responseData = responseData;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(_responseData));
        var response = new HttpResponseMessage(error ? System.Net.HttpStatusCode.UnavailableForLegalReasons : System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(stream)
        };
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/event-stream");
        return Task.FromResult(response);
    }
}

public class FakeApiClient : IApiClient
{
    private int sendPromptCount = 0;
    private int streamAudioCount = 0;

    public Task<string> PostRequest(string url, string json)
    {
        if (url.Contains("start_dialog"))
        {
            return Task.FromResult("1 0  1 2");
        }
        if (url.Contains("getgraph"))
        {
            return Task.FromResult("{\"user: 0\": [\"asr: 0\", \"log: v1\"], \"user: 1\": [\"bot: 1\", \"log: v1\"], \"user: 2\": [\"bot: 0\", \"log: v1\"], \"asr: 0\": [\"bot: 1\", \"log: v1\", \"api\"], \"bot: 1\": [\"api\", \"log: v1\"], \"bot: 0\": [\"api\", \"log: v1\"]}");
        }
        if (url.Contains("append"))
        {
            if (json.Contains("seq"))
            {
                // sendprompt
                sendPromptCount++;
            }
            else if (json.Contains("b64_enc_pcm_s16le"))
            {
                streamAudioCount++;
            }
            return Task.FromResult("OK");
        }

        return Task.FromResult("OK");
    }

    public int GetSendPromptCount()
    {
        return sendPromptCount;
    }

    public int GetStreamAudioCount()
    {
        return streamAudioCount;
    }
}
