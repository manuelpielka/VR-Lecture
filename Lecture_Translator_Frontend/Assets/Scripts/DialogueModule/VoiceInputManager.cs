using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class is used to handle microphone input from the user through unity’s microphone system.
/// </summary>
public class VoiceInputManager : MonoBehaviour
{
    /// <summary>
    /// Reference to the DialogueController to send the voice recorded text to in order to send it to the LLM.
    /// </summary>
    [SerializeField] private DialogueController dialogueController;

    /// <summary>
    /// The image component of the microphone button.
    /// </summary>
    [SerializeField] private Image micImage;

    /// <summary>
    /// The name of the microphone device that is used to record audio.
    /// </summary>
    private string micDevice;

    /// <summary>
    /// The recorded audio is stored in this variable.
    /// </summary>
    private AudioSource audioSource;

    /// <summary>
    /// The sample rate of the microphone recording.
    /// </summary>
    private const int sampleRate = 16000;

    /// <summary>
    /// The size of the microphone's buffer in seconds.
    /// </summary>
    private const int bufferSize = 10;

    /// <summary>
    /// Is the microphone currently recording?
    /// </summary>
    private bool isRecording = false;

    /// <summary>
    /// The chunk size of the recorded audio.
    /// </summary>
    private const int chunkSize = 1600; // 100ms at 16kHz

    /// <summary>
    /// The last sample position of the audio chunk.
    /// </summary>
    private int lastSamplePosition = 0;

    /// <summary>
    /// The byte buffer of the audio chunks.
    /// </summary>
    private float[] audioBuffer = new float[chunkSize];

    /// <summary>
    /// The amount of time recorded.
    /// </summary>
    private float time = 0f;

    /// <summary>
    /// The polling rate of the audio stream.
    /// </summary>
    private float pollingRate = 0.05f;

    /// <summary>
    /// The start method called by unity.
    /// </summary>
    private void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            foreach (var device in Microphone.devices)
            {
                if (device.Contains("Oculus"))
                {
                    micDevice = device;
                    audioSource = GetComponent<AudioSource>();
                    Debug.Log("Using microphone: " + micDevice);
                }
            }

            if (micDevice != null) return;

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

        if (!isRecording)
        {
            // Start recording
            micImage.color = Color.red;

            dialogueController.StartAudioStream();

            audioSource.clip = Microphone.Start(micDevice, true, bufferSize, sampleRate);

            isRecording = true;

            StartCoroutine(StreamMicrophone());
        }
        else
        {
            // End recording
            micImage.color = Color.white;

            Microphone.End(micDevice);

            isRecording = false;
        }
    }

    /// <summary>
    /// This method records audio chunks, converts them to the correct format and sends them to the DialogueController.
    /// </summary>
    IEnumerator StreamMicrophone()
    {
        time = 0f;

        while (isRecording)
        {
            int currentPosition = Microphone.GetPosition(micDevice);
            int samplesAvailable = currentPosition - lastSamplePosition;
            if (samplesAvailable < 0) samplesAvailable += audioSource.clip.samples;

            if (samplesAvailable >= chunkSize)
            {
                audioSource.clip.GetData(audioBuffer, lastSamplePosition);
                lastSamplePosition = (lastSamplePosition + chunkSize) % audioSource.clip.samples;

                byte[] pcmChunk = FloatToPCM16(audioBuffer);

                float startTime = time;
                float endTime = startTime + ((float)pcmChunk.Length / (2 * sampleRate));
                time = endTime;

                dialogueController.StreamAudio(pcmChunk, startTime, endTime);
            }

            yield return new WaitForSeconds(pollingRate);
        }
    }

    /// <summary>
    /// This method converts a recorded audio chunk from a float array to a PCM16 formatted byte array.
    /// </summary>
    /// <param name="floatBuffer">The recorded audio chunk as a float array.</param>
    private byte[] FloatToPCM16(float[] floatBuffer)
    {
        byte[] pcm = new byte[floatBuffer.Length * 2];
        for (int i = 0; i < floatBuffer.Length; i++)
        {
            short val = (short)Mathf.Clamp(floatBuffer[i] * 32767, -32768, 32767);
            pcm[i * 2] = (byte)(val & 0xff);
            pcm[i * 2 + 1] = (byte)((val >> 8) & 0xff);
        }
        return pcm;
    }
}
