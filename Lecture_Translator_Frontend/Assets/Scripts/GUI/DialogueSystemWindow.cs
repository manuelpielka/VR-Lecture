using UnityEngine;
using TMPro;

namespace GUI
{
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

        private void Start()
        {
            virtualAvatar = VirtualAvatar.instance;
            virtualAvatar.EnableAvatar();
            WindowManager = WindowManager.instance;
        }

        private void OnDestroy()
        {
            virtualAvatar.DisableAvatar();
        }

        /// <summary>
        /// Button handler for the microphone button that calls a method in the VoiceInputManager class.
        /// </summary>
        public void MicrophoneBtnPressed()
        {
            voiceInputManager.ToggleVoiceRecording();
            userTextBox.text = "*recorded audio*";
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

        /// <summary>
        /// Method that gets called from the VoiceInputManager class when the LLM has finished their response.
        /// </summary>
        public void DisplayLLMAnswer(string answer)
        {
            aiTextBox.text = answer;
        }
    }
}
