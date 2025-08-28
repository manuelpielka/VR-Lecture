using UnityEngine;
using TMPro;

namespace GUI
{
    /// <summary>
    /// This is the json format the LLM response is sent in.
    /// </summary>
    [System.Serializable]
    public class JsonLLMResponse
    {
        public string seq;
        public string user;
        public string image_des;
        public string session;
        public string sender;
        public string message_id;
        public string context;
        public string num_subscribers;
    }

    /// <summary>
    /// This Window is used to talk to an AI by getting user input either from text or the
    /// microphone and then displaying the AI’s response in a text box.
    /// </summary>
    public class DialogueSystemWindow : Window
    {
        /// <summary>
        /// Reference to the VoiceInputManager class to start and stop microphone recording.
        /// </summary>
        [SerializeField] private VoiceInputManager voiceInputManager;

        /// <summary>
        /// Reference to the DialogueController class to send a prompt to the AI.
        /// </summary>
        [SerializeField] private DialogueController dialogueController;

        /// <summary>
        /// Reference to the VirtualAvatar class to enable and disable the virtual avatar.
        /// </summary>
        private VirtualAvatar virtualAvatar;

        /// <summary>
        /// The text box for displaying the user’s prompt.
        /// </summary>
        [SerializeField] private TextMeshProUGUI userTextBox;

        /// <summary>
        /// The text box for displaying the LLM’s answer.
        /// </summary>
        [SerializeField] private TextMeshProUGUI aiTextBox;

        /// <summary>
        /// The input field to write the prompt into.
        /// </summary>
        [SerializeField] private TMP_InputField inputField;

        /// <summary>
        /// The GameObject of the loading panel.
        /// </summary>
        [SerializeField] private GameObject loadingPanel;

        /// <summary>
        /// The Gameobject of the token error panel.
        /// </summary>
        [SerializeField] private GameObject errorTokenPanel;

        /// <summary>
        /// The Gameobject of the response error panel.
        /// </summary>
        [SerializeField] private GameObject errorResponsePanel;

        /// <summary>
        /// Whether or not the loading panel is active.
        /// </summary>
        public bool loading = true;

        /// <summary>
        /// The llm answer gets sent here when it is received and then parsed and displayed.
        /// </summary>
        public string llmAnswer = "";

        /// <summary>
        /// Whether or not the microphone is currently recording.
        /// </summary>
        private bool isRecording = false;

        /// <summary>
        /// Called by unity when the object is enabled.
        /// </summary>
        private void OnEnable()
        {
            dialogueController.OnLoadingChanged += SetLoading;
            dialogueController.OnResponseReceived += SetLLMAnswer;
            dialogueController.OnInvalidTokenError += errorTokenPanel.SetActive;
            dialogueController.OnNoResponseError += errorResponsePanel.SetActive;
        }

        /// <summary>
        /// Called by unity when the object is disabled.
        /// </summary>
        private void OnDisable()
        {
            dialogueController.OnLoadingChanged -= SetLoading;
            dialogueController.OnResponseReceived -= SetLLMAnswer;
            dialogueController.OnInvalidTokenError -= errorTokenPanel.SetActive;
            dialogueController.OnNoResponseError -= errorResponsePanel.SetActive;
        }

        /// <summary>
        /// The start method called by unity.
        /// </summary>
        private void Start()
        {
            virtualAvatar = VirtualAvatar.instance;
            virtualAvatar.EnableAvatar();
            WindowManager = WindowManager.instance;
        }

        /// <summary>
        /// The OnDestroy method called by unity once this GameObject is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            virtualAvatar.DisableAvatar();
        }

        /// <summary>
        /// Getter for the user prompt.
        /// </summary>
        /// <returns> The prompt of the user currently in the text box. </returns>
        public string GetUserPrompt()
        {
            return userTextBox.text;
        }

        /// <summary>
        /// Button handler for the microphone button that calls a method in the VoiceInputManager class.
        /// </summary>
        public void MicrophoneBtnPressed()
        {
            isRecording = !isRecording;
            voiceInputManager.ToggleVoiceRecording();
            //if (!isRecording) dialogueController.SendPrompt(userTextBox.text);
        }

        /// <summary>
        /// Input field handler for when the user has finished writing their prompt which is then sent to the DialogueController class.
        /// </summary>
        public void TextInputEnded(string text)
        {
            inputField.text = "";
            if (text == "") return;
            dialogueController.SendPrompt(text);
            userTextBox.text = text;
        }

        /// <summary>
        /// Sets the loading panel to the bool value.
        /// </summary>
        /// <param name="value"> The new value. </param>
        private void SetLoading(bool value)
        {
            loading = value;
            loadingPanel.SetActive(loading);
        }

        /// <summary>
        /// Sets the llmAnswer string and converts the json data correctly.
        /// </summary>
        /// <param name="value"> The new value. </param>
        private void SetLLMAnswer(string value)
        {
            llmAnswer = value;

            if (llmAnswer == "") return;

            JsonLLMResponse llmresponse = JsonUtility.FromJson<JsonLLMResponse>(llmAnswer);

            if (llmresponse.sender.Contains("bot"))
            {
                if (llmresponse != null && llmresponse.seq != "" && llmresponse.seq != aiTextBox.text)
                {
                    aiTextBox.text = llmresponse.seq;
                    virtualAvatar.PlayTalkingAnimation();
                }
            }
            else if (llmresponse.sender.Contains("asr"))
            {
                if (llmresponse != null && llmresponse.seq != "" && llmresponse.seq != aiTextBox.text)
                {
                    userTextBox.text = llmresponse.seq;
                }
            }
        }
    }
}
