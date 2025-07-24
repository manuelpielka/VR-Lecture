using UnityEngine;
using TMPro;

namespace GUI
{
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

        [SerializeField] private GameObject loadingPanel;

        public bool loading = true;

        public string llmAnswer = "";

        private void Start()
        {
            virtualAvatar = VirtualAvatar.instance;
            virtualAvatar.EnableAvatar();
            WindowManager = WindowManager.instance;
        }

        private void Update()
        {
            if (!loading && loadingPanel.activeSelf) loadingPanel.SetActive(false);

            if (llmAnswer == "") return;

            JsonLLMResponse llmresponse = JsonUtility.FromJson<JsonLLMResponse>(llmAnswer);

            if (llmresponse.sender.Contains("bot"))
            {
                if (llmresponse != null && llmresponse.seq != "" && llmresponse.seq != aiTextBox.text) // Have to do this because you have to update ui on a main thread
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

        private void OnDestroy()
        {
            virtualAvatar.DisableAvatar();
        }

        public string GetUserPrompt()
        {
            return userTextBox.text;
        }

        /// <summary>
        /// Button handler for the microphone button that calls a method in the VoiceInputManager class.
        /// </summary>
        public void MicrophoneBtnPressed()
        {
            voiceInputManager.ToggleVoiceRecording();
        }

        /// <summary>
        /// Input field handler for when the user has finished writing their prompt which is then sent to the DialogueController class.
        /// </summary>
        public void TextInputEnded(string text)
        {
            if (text == "") return;
            dialogueController.SendPrompt(text);
            userTextBox.text = text;
        }
    }
}
