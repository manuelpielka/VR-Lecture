using UnityEngine;

/// <summary>
/// This class is used to handle microphone input from the user through unity’s microphone system.
/// </summary>
public class VoiceInputManager : MonoBehaviour
{
    /// <summary>
    /// Reference to the DialogueController to send the voice recorded text to in order to send it to the LLM.
    /// </summary>
    private DialogueController dialogueController;

    /// <summary>
    /// Reference to the DialogueSystemWindow to send the voice recorded text to in order to display it in the GUI.
    /// </summary>
    //private DialogueSystemWindow dialogueUI;

    /// <summary>
    /// The name of the microphone device that is used to record audio.
    /// </summary>
    private string micDevice;

    /// <summary>
    /// The recorded audio is stored in this variable.
    /// </summary>
    private AudioSource audioSource;

    /// <summary>
    /// The start method called by unity.
    /// </summary>
    private void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            micDevice = Microphone.devices[0]; // TODO: Maybe choose device later?
            audioSource = GetComponent<AudioSource>();
            Debug.Log("Using microphone: " + micDevice);
        }
        else
        {
            Debug.LogError("No microphone devices found.");
        }
    }

    /// <summary>
    /// This method toggles the recording of the microphone and sends the recorded audio to the DialogueController.
    /// </summary>
    public void ToggleVoiceRecording()
    {
        if (micDevice == null) Debug.LogError("No microphone detected. Cannot record audio!");

        if (!Microphone.IsRecording(micDevice))
        {
            // Start recording
            audioSource.clip = Microphone.Start(micDevice, false, 10, 44100);
        }
        else
        {
            // End recording
            Microphone.End(micDevice);
            dialogueController.SendPrompt(audioSource.clip);
        }
    }
}
